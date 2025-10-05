using Microsoft.AspNetCore.Mvc;
using PricePulse.Services;
using PricePulse.ViewModels;
using PricePulse.Models;
using PricePulse.DAOs.Interfaces;
using System.Linq;
using System.Security.Claims;

namespace PricePulse.Controllers
{
    public class CompetitorController : Controller
    {
        private readonly ICompetitorProductAnalysisService _analysisService;
        private readonly ILogger<CompetitorController> _logger;
        private readonly ICompetitorDAO _competitorDAO;
        private readonly IUserDAO _userDAO;
        private readonly ICompanyProfileDAO _companyDAO;
        private readonly ICompetitorProductAnalysisDAO _competitorProductAnalysisDAO;

        public CompetitorController(
            ICompetitorProductAnalysisService analysisService,
            ILogger<CompetitorController> logger,
            ICompetitorDAO competitorDAO,
            IUserDAO userDAO,
            ICompanyProfileDAO companyDAO,
            ICompetitorProductAnalysisDAO competitorProductAnalysisDAO)
        {
            _analysisService = analysisService;
            _logger = logger;
            _competitorDAO = competitorDAO;
            _userDAO = userDAO;
            _companyDAO = companyDAO;
            _competitorProductAnalysisDAO = competitorProductAnalysisDAO;
        }

        public async Task<IActionResult> Index()
        {
            var model = new CompetitorDiscoveryViewModel();
            
            // Check if user is authenticated
            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Get current user's company profile
                var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                var user = await _userDAO.GetByEmailAsync(email);
                if (user == null)
                {
                    TempData["Error"] = "User not found. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                var companies = await _companyDAO.ListCompaniesForUserAsync(user.UserId);
                var company = companies.FirstOrDefault();
                if (company == null)
                {
                    TempData["Error"] = "No company profile found. Please set up your company first.";
                    return RedirectToAction("Companies", "Account");
                }

                // Load competitors for this user (through their companies)
                Console.WriteLine($"=== LOADING COMPETITORS FOR USER {user.Email} ===");
                var allUserCompetitors = new List<Competitor>();
                foreach (var userCompany in companies)
                {
                    var companyCompetitors = await _competitorDAO.GetByCompanyProfileIdAsync(userCompany.CompanyProfileId);
                    allUserCompetitors.AddRange(companyCompetitors);
                }
                var userCompetitors = allUserCompetitors;
                
                Console.WriteLine($"=== LOADED {userCompetitors.Count()} COMPETITORS FOR USER {user.Email} (Company {company.CompanyProfileId}) ===");
                
                // Debug: List all competitors in the database
                var allCompetitors = await _competitorDAO.GetAllAsync();
                Console.WriteLine($"=== TOTAL COMPETITORS IN DATABASE: {allCompetitors.Count()} ===");
                foreach (var comp in allCompetitors)
                {
                    Console.WriteLine($"=== COMPETITOR: {comp.CompetitorName} - Company {comp.CompanyProfileId} ===");
                }
                
                model.Competitors = userCompetitors.Select(c => new ManualCompetitor
                {
                    Name = c.CompetitorName ?? "Unknown",
                    Website = c.CompetitorWebsite ?? "",
                    Notes = c.CompetitorCompanyProfile ?? "",
                    CompetitorId = c.CompetitorId
                }).ToList();
                
                Console.WriteLine($"=== MAPPED {model.Competitors.Count()} COMPETITORS TO MODEL ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading existing competitors");
                Console.WriteLine($"=== ERROR LOADING COMPETITORS: {ex.Message} ===");
                model.Competitors = new List<ManualCompetitor>();
            }
            
            return View(model);
        }

        [HttpPost]
        public IActionResult DiscoverCompetitors(CompetitorDiscoveryViewModel model)
        {
            Console.WriteLine($"=== STARTING COMPETITOR DISCOVERY for {model.Domain} ===");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== MODEL STATE INVALID ===");
                return View("Index", model);
            }

            try
            {
                // Since we removed Semrush, we'll show manual entry form instead
                model.ShowManualEntry = true;
                model.Competitors = new List<ManualCompetitor> { new ManualCompetitor() };
                model.Message = "Please add your competitors manually below.";
                model.IsSuccess = true;

                TempData["Success"] = model.Message;
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up manual competitor entry for domain {Domain}", model.Domain);
                model.Message = "Error setting up competitor entry. Please try again.";
                model.IsSuccess = false;
                return View("Index", model);
            }
        }

        [HttpPost]
        public IActionResult ShowManualEntry(CompetitorDiscoveryViewModel model)
        {
            Console.WriteLine($"=== SHOWING MANUAL ENTRY for {model.Domain} ===");
            
            model.ShowManualEntry = true;
            model.Competitors = new List<ManualCompetitor> { new ManualCompetitor() };
            
            return View("Index", model);
        }

        [HttpPost]
        public IActionResult AddManualCompetitor(CompetitorDiscoveryViewModel model)
        {
            Console.WriteLine($"=== ADDING MANUAL COMPETITOR for {model.Domain} ===");
            
            if (model.Competitors == null)
            {
                model.Competitors = new List<ManualCompetitor>();
            }

            // Add new empty competitor for entry
            model.Competitors.Add(new ManualCompetitor());
            model.ShowManualEntry = true;
            
            return View("Index", model);
        }

        [HttpPost]
        public IActionResult ConfirmCompetitors(CompetitorConfirmationViewModel model)
        {
            Console.WriteLine($"=== CONFIRMING COMPETITORS for {model.Domain} ===");
            
            try
            {
                var confirmedCompetitors = model.Competitors
                    .Where((c, index) => model.SelectedCompetitorIds.Contains(index))
                    .ToList();

                // Here you would save to database
                // For now, we'll just log the confirmation
                foreach (var competitor in confirmedCompetitors)
                {
                    competitor.IsConfirmed = true;
                    Console.WriteLine($"=== CONFIRMED COMPETITOR: {competitor.Domain} ===");
                }

                TempData["Success"] = $"Confirmed {confirmedCompetitors.Count} competitors for {model.Domain}";
                
                // Redirect to competitor list or back to discovery
                return RedirectToAction("List", new { domain = model.Domain });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming competitors for domain {Domain}", model.Domain);
                TempData["Error"] = "Error confirming competitors. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveManualCompetitors(ManualCompetitorEntryViewModel model)
        {
            Console.WriteLine($"=== SAVING MANUAL COMPETITORS for '{model.Domain}' ===");
            Console.WriteLine($"=== MODEL COMPETITORS COUNT: {model.Competitors?.Count ?? 0} ===");
            Console.WriteLine($"=== MODEL STATE IS VALID: {ModelState.IsValid} ===");
            
            // Debug: Check what's in the request
            Console.WriteLine($"=== REQUEST FORM DATA ===");
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"=== {key}: {Request.Form[key]} ===");
            }
            Console.WriteLine($"=== USER AUTHENTICATED: {User?.Identity?.IsAuthenticated ?? false} ===");
            Console.WriteLine($"=== USER NAME: {User?.Identity?.Name ?? "NULL"} ===");
            
            // Check if user is authenticated
            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.ContentType?.Contains("application/x-www-form-urlencoded") == true)
                {
                    return Json(new { success = false, message = "You must be logged in to save competitors." });
                }
                
                TempData["Error"] = "You must be logged in to save competitors.";
                return RedirectToAction("Login", "Account");
            }

            // Get current user's company profile
            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            Console.WriteLine($"=== USER EMAIL FROM CLAIMS: {email} ===");
            var user = await _userDAO.GetByEmailAsync(email);
            Console.WriteLine($"=== USER FOUND: {user?.Email ?? "NULL"} - ID: {user?.UserId ?? 0} ===");
            if (user == null)
            {
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.ContentType?.Contains("application/x-www-form-urlencoded") == true)
                {
                    return Json(new { success = false, message = "User not found. Please log in again." });
                }
                
                TempData["Error"] = "User not found. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            var companies = await _companyDAO.ListCompaniesForUserAsync(user.UserId);
            Console.WriteLine($"=== FOUND {companies.Count()} COMPANIES FOR USER {user.UserId} ===");
            foreach (var comp in companies)
            {
                Console.WriteLine($"=== COMPANY: {comp.CompanyName} - ID {comp.CompanyProfileId} ===");
            }
            
            // Use the user's primary company (first one) for saving competitors
            var company = companies.FirstOrDefault();
            if (company == null)
            {
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.ContentType?.Contains("application/x-www-form-urlencoded") == true)
                {
                    return Json(new { success = false, message = "No company profile found. Please set up your company first." });
                }
                
                TempData["Error"] = "No company profile found. Please set up your company first.";
                return RedirectToAction("Companies", "Account");
            }

            Console.WriteLine($"=== SAVING COMPETITORS FOR USER {user.Email} TO PRIMARY COMPANY {company.CompanyProfileId} ({company.CompanyName}) ===");
            
            if (model.Competitors != null)
            {
                foreach (var comp in model.Competitors)
                {
                    Console.WriteLine($"=== COMPETITOR: Name='{comp.Name}', Website='{comp.Website}', Notes='{comp.Notes}' ===");
                    Console.WriteLine($"=== COMPETITOR NAME LENGTH: {comp.Name?.Length ?? 0} ===");
                    Console.WriteLine($"=== COMPETITOR WEBSITE LENGTH: {comp.Website?.Length ?? 0} ===");
                }
            }
            else
            {
                Console.WriteLine("=== MODEL.COMPETITORS IS NULL ===");
            }
            
            // Skip validation for AJAX requests and manually validate
            if (!ModelState.IsValid && !(Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                Request.ContentType?.Contains("application/x-www-form-urlencoded") == true))
            {
                Console.WriteLine("=== MODEL STATE IS INVALID ===");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"=== ERROR: {error.Key} = {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))} ===");
                }
                
                return View("Index", new CompetitorDiscoveryViewModel 
                { 
                    Domain = model.Domain, 
                    ShowManualEntry = true,
                    Competitors = model.Competitors?.Select(c => new ManualCompetitor 
                    { 
                        Name = c.Name, 
                        Website = c.Website, 
                        ProductListingPage = c.ProductListingPage, 
                        Notes = c.Notes 
                    }).ToList() ?? new List<ManualCompetitor>()
                });
            }

            try
            {
                // Use the ManualCompetitor directly, no need to convert to CompetitorInfo
                var competitors = model.Competitors?.ToList() ?? new List<ManualCompetitor>();
                
                // Manual validation for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.ContentType?.Contains("application/x-www-form-urlencoded") == true)
                {
                    if (!competitors.Any())
                    {
                        return Json(new { success = false, message = "No competitors to save." });
                    }
                    
                    foreach (var competitor in competitors)
                    {
                        if (string.IsNullOrEmpty(competitor.Website))
                        {
                            return Json(new { success = false, message = "Website URL is required." });
                        }
                    }
                }

                // Save to database
                int lastCompetitorId = 0;
                foreach (var competitor in competitors)
                {
                    // Use the Name and Website fields from the form
                    var competitorName = !string.IsNullOrEmpty(competitor.Name) ? competitor.Name : "Unknown Competitor";
                    var competitorWebsite = !string.IsNullOrEmpty(competitor.Website) ? competitor.Website : "";
                    
                    var competitorEntity = new Competitor
                    {
                        CompetitorName = competitorName,
                        CompetitorWebsite = competitorWebsite,
                        CompetitorCompanyProfile = competitor.Notes ?? "Manually added competitor",
                        CompanyProfileId = company.CompanyProfileId // Use current user's company profile
                    };
                    
                    Console.WriteLine($"=== COMPETITOR ENTITY CREATED WITH CompanyProfileId: {competitorEntity.CompanyProfileId} ===");
                    
                    Console.WriteLine($"=== SAVING MANUAL COMPETITOR: {competitorName} - {competitorWebsite} for Company {company.CompanyProfileId} ===");
                    await _competitorDAO.CreateAsync(competitorEntity);
                    lastCompetitorId = competitorEntity.CompetitorId; // Store the ID after creation
                    Console.WriteLine($"=== SAVED MANUAL COMPETITOR: {competitorName} with ID {lastCompetitorId} for Company {company.CompanyProfileId} ===");
                }

                // Check if this is an AJAX request
                Console.WriteLine($"=== REQUEST HEADERS: X-Requested-With = {Request.Headers["X-Requested-With"]}, Content-Type = {Request.ContentType} ===");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.ContentType?.Contains("application/x-www-form-urlencoded") == true)
                {
                    Console.WriteLine($"=== RETURNING JSON RESPONSE for AJAX request ===");
                    return Json(new { 
                        success = true, 
                        message = $"Saved {competitors.Count} competitor(s)",
                        competitorId = lastCompetitorId
                    });
                }
                
                TempData["Success"] = $"Saved {competitors.Count} manual competitors for {model.Domain}";
                return RedirectToAction("Index", "Competitor");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving manual competitors for domain {Domain}", model.Domain);
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.ContentType?.Contains("application/x-www-form-urlencoded") == true)
                {
                    return Json(new { success = false, message = "Error saving competitor. Please try again." });
                }
                
                TempData["Error"] = "Error saving manual competitors. Please try again.";
                return RedirectToAction("Index", "Competitor");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCompetitor(string website)
        {
            try
            {
                Console.WriteLine($"=== DELETING COMPETITOR: {website} ===");
                
                // Check if user is authenticated
                if (!(User?.Identity?.IsAuthenticated ?? false))
                {
                    TempData["Error"] = "You must be logged in to delete competitors.";
                    return RedirectToAction("Login", "Account");
                }

                // Get current user's company profile
                var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                var user = await _userDAO.GetByEmailAsync(email);
                if (user == null)
                {
                    TempData["Error"] = "User not found. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                var companies = await _companyDAO.ListCompaniesForUserAsync(user.UserId);
                var company = companies.FirstOrDefault();
                if (company == null)
                {
                    TempData["Error"] = "No company profile found.";
                    return RedirectToAction("Index");
                }
                
                // Find competitor by website and ensure it belongs to current user
                var competitors = await _competitorDAO.GetAllAsync();
                var competitorToDelete = competitors.FirstOrDefault(c => c.CompetitorWebsite == website && c.CompanyProfileId == company.CompanyProfileId);
                
                if (competitorToDelete != null)
                {
                    await _competitorDAO.DeleteAsync(competitorToDelete.CompetitorId);
                    Console.WriteLine($"=== DELETED COMPETITOR: {website} FOR USER {user.Email} ===");
                    TempData["Success"] = $"Competitor {website} deleted successfully.";
                }
                else
                {
                    Console.WriteLine($"=== COMPETITOR NOT FOUND OR NOT OWNED BY USER: {website} ===");
                    TempData["Error"] = "Competitor not found or you don't have permission to delete it.";
                }
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting competitor {Website}", website);
                TempData["Error"] = "Error deleting competitor. Please try again.";
                return RedirectToAction("Index");
            }
        }

        public IActionResult List(string domain)
        {
            Console.WriteLine($"=== SHOWING COMPETITOR LIST for {domain} ===");
            
            var model = new CompetitorListViewModel
            {
                Domain = domain,
                Competitors = new List<CompetitorInfo>(), // Would load from database
                TotalCompetitors = 0,
                LastUpdated = DateTime.UtcNow
            };

            return View(model);
        }


        [HttpPost]
        public IActionResult AnalyzeAllCompetitorProducts(string domain)
        {
            Console.WriteLine($"=== ANALYZING ALL COMPETITOR PRODUCTS for {domain} ===");
            
            try
            {
                // Since we removed Semrush, we'll work with manually added competitors
                // For now, return a message that manual competitor analysis is not yet implemented
                TempData["Error"] = "Competitor analysis requires manually added competitors. Please add competitors first.";
                
                return RedirectToAction("List", new { domain });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing competitor products for domain {Domain}", domain);
                TempData["Error"] = "Error analyzing competitor products. Please try again.";
                return RedirectToAction("List", new { domain });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeSingleCompetitor(string competitorDomain)
        {
            Console.WriteLine($"=== ANALYZING SINGLE COMPETITOR: {competitorDomain} ===");
            
            try
            {
                var result = await _analysisService.AnalyzeCompetitorWithRetailersAsync(competitorDomain, "United States");
                
                if (result.Success)
                {
                    // Save the results to database
                    await _analysisService.SaveCompetitorProductsAsync(competitorDomain, result.Products);
                    TempData["Success"] = $"Found {result.TotalProducts} products for {competitorDomain} with retailer information";
                }
                else
                {
                    TempData["Error"] = $"Failed to analyze {competitorDomain}: {result.ErrorMessage}";
                }
                
                return RedirectToAction("CompetitorProducts", new { domain = competitorDomain });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing competitor {Domain}", competitorDomain);
                TempData["Error"] = "Error analyzing competitor. Please try again.";
                return RedirectToAction("List");
            }
        }

        public IActionResult AnalysisResults(string domain)
        {
            Console.WriteLine($"=== SHOWING ANALYSIS RESULTS for {domain} ===");
            
            try
            {
                // Since we removed Semrush, return empty results for now
                var model = new CompetitorAnalysisResultsViewModel
                {
                    Domain = domain,
                    AnalysisResults = new List<CompetitorAnalysisResult>(),
                    TotalCompetitors = 0,
                    TotalProducts = 0,
                    ProductsWithPrices = 0,
                    LastAnalyzed = DateTime.UtcNow
                };
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading analysis results for domain {Domain}", domain);
                TempData["Error"] = "Error loading analysis results. Please try again.";
                return RedirectToAction("List", new { domain });
            }
        }

        public async Task<IActionResult> Profile(int id)
        {
            Console.WriteLine($"=== SHOWING COMPETITOR PROFILE for ID {id} ===");
            
            try
            {
                // Check if user is authenticated
                if (!(User?.Identity?.IsAuthenticated ?? false))
                {
                    TempData["Error"] = "You must be logged in to view competitor profiles.";
                    return RedirectToAction("Login", "Account");
                }

                // Get current user's company profile
                var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                var user = await _userDAO.GetByEmailAsync(email);
                if (user == null)
                {
                    TempData["Error"] = "User not found. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                var companies = await _companyDAO.ListCompaniesForUserAsync(user.UserId);
                var company = companies.FirstOrDefault();
                if (company == null)
                {
                    TempData["Error"] = "No company profile found.";
                    return RedirectToAction("Index");
                }

                // Get competitor details
                var competitor = await _competitorDAO.GetByIdAsync(id);
                if (competitor == null || competitor.CompanyProfileId != company.CompanyProfileId)
                {
                    TempData["Error"] = "Competitor not found or you don't have permission to view it.";
                    return RedirectToAction("Index");
                }

                // Get historical products for this competitor
                var historicalProducts = await _competitorProductAnalysisDAO.GetByCompetitorDomainAsync(competitor.CompetitorWebsite ?? "");
                
                var model = new CompetitorProfileViewModel
                {
                    CompetitorId = competitor.CompetitorId,
                    CompetitorName = competitor.CompetitorName ?? "Unknown Competitor",
                    CompetitorWebsite = competitor.CompetitorWebsite ?? "",
                    CompetitorDescription = competitor.CompetitorCompanyProfile,
                    AddedDate = DateTime.UtcNow, // We'll need to add this field to the model
                    LastAnalyzed = historicalProducts.Any() ? historicalProducts.Max(p => p.LastUpdated) : DateTime.MinValue,
                    TotalProductsDiscovered = historicalProducts.Count,
                    ActiveProducts = historicalProducts.Count(p => p.IsActive),
                    HistoricalProducts = historicalProducts.OrderByDescending(p => p.LastUpdated).ToList(),
                    IsAnalyzing = false
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading competitor profile for ID {CompetitorId}", id);
                TempData["Error"] = "Error loading competitor profile. Please try again.";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> CompetitorDetails(string domain)
        {
            Console.WriteLine($"=== SHOWING COMPETITOR DETAILS for {domain} ===");
            
            try
            {
                var result = await _analysisService.AnalyzeSingleCompetitorAsync(domain);
                
                var model = new CompetitorDetailsViewModel
                {
                    CompetitorDomain = domain,
                    AnalysisResult = result,
                    Products = result.Products
                };
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading competitor details for {Domain}", domain);
                TempData["Error"] = "Error loading competitor details. Please try again.";
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DiscoverProductsForCompetitor(int competitorId, string companyLocation = "United States")
        {
            Console.WriteLine($"=== DISCOVERING PRODUCTS FOR COMPETITOR ID {competitorId} ===");
            
            try
            {
                // Check if user is authenticated
                if (!(User?.Identity?.IsAuthenticated ?? false))
                {
                    TempData["Error"] = "You must be logged in to discover products.";
                    return RedirectToAction("Login", "Account");
                }

                // Get current user's company profile
                var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                var user = await _userDAO.GetByEmailAsync(email);
                if (user == null)
                {
                    TempData["Error"] = "User not found. Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                var companies = await _companyDAO.ListCompaniesForUserAsync(user.UserId);
                var company = companies.FirstOrDefault();
                if (company == null)
                {
                    TempData["Error"] = "No company profile found.";
                    return RedirectToAction("Index");
                }

                // Get competitor details
                var competitor = await _competitorDAO.GetByIdAsync(competitorId);
                if (competitor == null || competitor.CompanyProfileId != company.CompanyProfileId)
                {
                    TempData["Error"] = "Competitor not found or you don't have permission to analyze it.";
                    return RedirectToAction("Index");
                }

                // Use product discovery pipeline for consistency with Product Discovery pages
                var result = await _analysisService.DiscoverProductsUsingDiscoveryAsync(competitor.CompetitorWebsite ?? "", companyLocation);
                
                if (result.Success)
                {
                    // Save the results to database
                    await _analysisService.SaveCompetitorProductsAsync(competitor.CompetitorWebsite ?? "", result.Products);
                    
                    TempData["Success"] = $"Successfully discovered {result.TotalProducts} products for {competitor.CompetitorName}.";
                    return RedirectToAction("Profile", new { id = competitorId });
                }
                else
                {
                    TempData["Error"] = $"Failed to discover products for {competitor.CompetitorName}: {result.ErrorMessage}";
                    return RedirectToAction("Profile", new { id = competitorId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discovering products for competitor ID {CompetitorId}", competitorId);
                TempData["Error"] = "Error discovering products. Please try again.";
                return RedirectToAction("Profile", new { id = competitorId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeCompetitorWithRetailers(string competitorDomain, string companyLocation = "United States")
        {
            Console.WriteLine($"=== ANALYZING COMPETITOR WITH RETAILERS: {competitorDomain} ===");
            
            try
            {
                var result = await _analysisService.AnalyzeCompetitorWithRetailersAsync(competitorDomain, companyLocation);
                
                if (result.Success)
                {
                    // Save the results to database
                    await _analysisService.SaveCompetitorProductsAsync(competitorDomain, result.Products);
                    
                    TempData["Success"] = $"Analyzed {result.TotalProducts} products for {competitorDomain} with retailer information.";
                    return RedirectToAction("CompetitorProducts", new { domain = competitorDomain });
                }
                else
                {
                    TempData["Error"] = $"Failed to analyze {competitorDomain}: {result.ErrorMessage}";
                    return RedirectToAction("List");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing competitor with retailers for {Domain}", competitorDomain);
                TempData["Error"] = "Error analyzing competitor. Please try again.";
                return RedirectToAction("List");
            }
        }

        public async Task<IActionResult> CompetitorProducts(string domain)
        {
            Console.WriteLine($"=== SHOWING COMPETITOR PRODUCTS for {domain} ===");
            
            try
            {
                var products = await _analysisService.GetCompetitorProductsFromDatabaseAsync(domain);
                
                var model = new CompetitorProductsViewModel
                {
                    CompetitorDomain = domain,
                    CompetitorName = domain,
                    Products = products,
                    TotalProducts = products.Count,
                    ProductsWithRetailers = products.Count(p => p.Retailers.Any()),
                    LastAnalyzed = products.FirstOrDefault()?.LastUpdated ?? DateTime.UtcNow,
                    CompanyLocation = "United States"
                };
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading competitor products for {Domain}", domain);
                TempData["Error"] = "Error loading competitor products. Please try again.";
                return RedirectToAction("List");
            }
        }

        public async Task<IActionResult> CompetitorProductDetails(string domain, int productId)
        {
            Console.WriteLine($"=== SHOWING COMPETITOR PRODUCT DETAILS for {domain}, Product {productId} ===");
            
            try
            {
                var products = await _analysisService.GetCompetitorProductsFromDatabaseAsync(domain);
                var product = products.FirstOrDefault(p => p.Id == productId);
                
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("CompetitorProducts", new { domain });
                }
                
                var retailers = product.Retailers.ToList();
                var prices = retailers.Where(r => decimal.TryParse(r.Price, out _))
                                    .Select(r => decimal.Parse(r.Price!))
                                    .ToList();
                
                var model = new CompetitorProductDetailsViewModel
                {
                    Product = product,
                    Retailers = retailers,
                    CompetitorDomain = domain,
                    CompetitorName = domain,
                    LowestPrice = prices.Any() ? prices.Min() : null,
                    HighestPrice = prices.Any() ? prices.Max() : null,
                    AveragePrice = prices.Any() ? prices.Average() : null,
                    TotalRetailers = retailers.Count
                };
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading competitor product details for {Domain}, Product {ProductId}", domain, productId);
                TempData["Error"] = "Error loading product details. Please try again.";
                return RedirectToAction("CompetitorProducts", new { domain });
            }
        }
    }
}
