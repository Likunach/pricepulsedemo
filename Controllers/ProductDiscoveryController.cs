using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using PricePulse.DAOs.Interfaces;
using PricePulse.Services;
using PricePulse.ViewModels;
using PricePulse.Models;

namespace PricePulse.Controllers
{
    public class ProductDiscoveryController : Controller
    {
        private readonly IOpenAIService _openAIService;
        private readonly IUserDAO _userDAO;
        private readonly ICompanyProfileDAO _companyDAO;
        private readonly IOwnProductDAO _ownProductDAO;
        private readonly ICompetitorDAO _competitorDAO;
        private readonly ICompetitorProductDAO _competitorProductDAO;
        private readonly IPriceDAO _priceDAO;
        private readonly ILogger<ProductDiscoveryController> _logger;

        public ProductDiscoveryController(
            IOpenAIService openAIService,
            IUserDAO userDAO,
            ICompanyProfileDAO companyDAO,
            IOwnProductDAO ownProductDAO,
            ICompetitorDAO competitorDAO,
            ICompetitorProductDAO competitorProductDAO,
            IPriceDAO priceDAO,
            ILogger<ProductDiscoveryController> logger)
        {
            _openAIService = openAIService;
            _userDAO = userDAO;
            _companyDAO = companyDAO;
            _ownProductDAO = ownProductDAO;
            _competitorDAO = competitorDAO;
            _competitorProductDAO = competitorProductDAO;
            _priceDAO = priceDAO;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index(string? competitorDomain = null)
        {
            Console.WriteLine("=== INDEX GET CALLED ===");
            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                Console.WriteLine("=== USER NOT AUTHENTICATED, REDIRECTING ===");
                return RedirectToAction("Login", "Account");
            }

            Console.WriteLine("=== RETURNING INDEX VIEW ===");
            
            var model = new ProductDiscoveryViewModel();
            if (!string.IsNullOrEmpty(competitorDomain))
            {
                ViewBag.CompetitorDomain = competitorDomain;
                ViewBag.Message = $"Analyzing products for competitor: {competitorDomain}";
            }
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DiscoverCompetitors(ProductDiscoveryViewModel model)
        {
            Console.WriteLine("=== DISCOVER COMPETITORS POST CALLED ===");
            Console.WriteLine($"=== ModelState.IsValid: {ModelState.IsValid} ===");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== MODEL STATE INVALID, RETURNING TO INDEX ===");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"=== VALIDATION ERROR: {error.ErrorMessage} ===");
                }
                return View("Index", model);
            }

            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Normalize URL to include scheme if missing
                if (!string.IsNullOrWhiteSpace(model.WebsiteUrl))
                {
                    model.WebsiteUrl = NormalizeUrl(model.WebsiteUrl);
                }

                Console.WriteLine($"=== STARTING COMPETITOR DISCOVERY FOR: {model.WebsiteUrl} ===");
                _logger.LogInformation("Starting competitor discovery for {Url}", model.WebsiteUrl);
                var startTime = DateTime.UtcNow;

                var discoveredCompetitors = await _openAIService.DiscoverCompetitorsAsync(
                    model.WebsiteUrl, 
                    model.CompanyLocation ?? "United States"
                );

                var endTime = DateTime.UtcNow;
                var duration = (endTime - startTime).TotalSeconds;

                Console.WriteLine($"=== COMPETITOR DISCOVERY COMPLETED IN {duration:F1} SECONDS ===");
                Console.WriteLine($"=== FOUND {discoveredCompetitors.Count} COMPETITORS ===");
                
                // Debug: Log first few competitors
                for (int i = 0; i < Math.Min(3, discoveredCompetitors.Count); i++)
                {
                    var comp = discoveredCompetitors[i];
                    Console.WriteLine($"=== COMPETITOR {i + 1}: {comp.CompanyName} - {comp.WebsiteUrl} ===");
                }

                var resultModel = new CompetitorDiscoveryResultViewModel
                {
                    WebsiteUrl = model.WebsiteUrl,
                    CompanyLocation = model.CompanyLocation ?? "United States",
                    DiscoveredCompetitors = discoveredCompetitors,
                    DiscoveryTime = duration,
                    DiscoveredAt = endTime
                };

                TempData["Success"] = $"Successfully discovered {discoveredCompetitors.Count} competitors in {duration:F1} seconds!";
                return View("CompetitorResults", resultModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR IN COMPETITOR DISCOVERY: {ex.Message} ===");
                _logger.LogError(ex, "Error during competitor discovery for {Url}", model.WebsiteUrl);
                TempData["Error"] = "An error occurred during competitor discovery. Please try again.";
                return View("Index", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveCompetitors(List<DiscoveredCompetitor> competitors)
        {
            try
            {
                if (competitors == null || !competitors.Any())
                {
                    TempData["Error"] = "No competitors selected to save.";
                    return RedirectToAction("Index");
                }

                // Save competitors to database
                foreach (var competitor in competitors)
                {
                    var competitorEntity = new Competitor
                    {
                        CompetitorName = competitor.CompanyName.Length > 50 ? competitor.CompanyName.Substring(0, 47) + "..." : competitor.CompanyName,
                        CompetitorWebsite = competitor.WebsiteUrl.Length > 50 ? competitor.WebsiteUrl.Substring(0, 47) + "..." : competitor.WebsiteUrl,
                        CompetitorCompanyProfile = $"{competitor.Description} | {competitor.CompetitionReason}".Length > 50 
                            ? $"{competitor.Description} | {competitor.CompetitionReason}".Substring(0, 47) + "..." 
                            : $"{competitor.Description} | {competitor.CompetitionReason}",
                        CompanyProfileId = 1 // TODO: Get from current user's company profile
                    };
                    
                    Console.WriteLine($"=== SAVING COMPETITOR: {competitor.CompanyName} - {competitor.WebsiteUrl} ===");
                    await _competitorDAO.CreateAsync(competitorEntity);
                    Console.WriteLine($"=== SAVED COMPETITOR: {competitor.CompanyName} ===");
                }

                TempData["Success"] = $"Successfully saved {competitors.Count} competitors!";
                return RedirectToAction("Index", "Competitor");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving competitors");
                TempData["Error"] = "An error occurred while saving competitors. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DiscoverProducts(ProductDiscoveryViewModel model)
        {
            Console.WriteLine("=== DISCOVER PRODUCTS POST CALLED ===");
            Console.WriteLine($"=== ModelState.IsValid: {ModelState.IsValid} ===");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== MODEL STATE INVALID, RETURNING TO INDEX ===");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"=== VALIDATION ERROR: {error.ErrorMessage} ===");
                }
                return View("Index", model);
            }

            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Normalize URL to include scheme if missing
                if (!string.IsNullOrWhiteSpace(model.WebsiteUrl))
                {
                    model.WebsiteUrl = NormalizeUrl(model.WebsiteUrl);
                }

                Console.WriteLine($"=== STARTING PRODUCT DISCOVERY FOR: {model.WebsiteUrl} ===");
                _logger.LogInformation("Starting product discovery for {Url}", model.WebsiteUrl);
                var startTime = DateTime.UtcNow;

                var discoveredProducts = await _openAIService.DiscoverProductsAsync(
                    model.WebsiteUrl, 
                    model.CompanyLocation ?? "United States");

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation("Product discovery completed in {Duration}ms, found {Count} products", 
                    duration.TotalMilliseconds, discoveredProducts?.Count ?? 0);
                
                if (discoveredProducts == null)
                {
                    _logger.LogWarning("OpenAI service returned null products for URL: {Url}", model.WebsiteUrl);
                    TempData["Error"] = "The AI service returned no data. Please try again or check your website URL.";
                    return View("Index", model);
                }

                model.DiscoveredProducts = discoveredProducts;
                model.SearchPrompt = $"Identify all products on {model.WebsiteUrl} and find top 20 retailers that sell the same products within {model.CompanyLocation ?? "United States"}";

                TempData["Success"] = $"Successfully discovered {discoveredProducts.Count} products in {duration.TotalSeconds:F1} seconds!";
                return View("Results", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discovering products for URL: {Url}", model.WebsiteUrl);
                TempData["Error"] = $"An error occurred while discovering products: {ex.Message}. Please try again.";
                return View("Index", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmProducts(ProductDiscoveryViewModel model)
        {
            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                var user = await _userDAO.GetByEmailAsync(email);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var companies = await _companyDAO.ListCompaniesForUserAsync(user.UserId);
                var company = companies.FirstOrDefault();
                if (company == null)
                {
                    TempData["Error"] = "Please create a company profile first.";
                    return RedirectToAction("Companies", "Account");
                }

                await SaveDiscoveredProducts(model.DiscoveredProducts ?? new List<DiscoveredProduct>(), company.CompanyProfileId, user.UserId);

                TempData["Message"] = "Products and competitor data have been saved successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving discovered products");
                TempData["Error"] = "An error occurred while saving the products. Please try again.";
                return View("Results", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ModifySearch(ProductDiscoveryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Results", model);
            }

            try
            {
                var discoveredProducts = await _openAIService.DiscoverProductsWithModifiedPromptAsync(
                    model.WebsiteUrl,
                    model.CompanyLocation ?? "United States",
                    model.ModifiedPrompt ?? "");

                model.DiscoveredProducts = discoveredProducts;
                model.SearchPrompt = model.ModifiedPrompt;

                return View("Results", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with modified search for URL: {Url}", model.WebsiteUrl);
                TempData["Error"] = "An error occurred with the modified search. Please try again.";
                return View("Results", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyProducts()
        {
            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                var user = await _userDAO.GetByEmailAsync(email);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var companies = await _companyDAO.ListCompaniesForUserAsync(user.UserId);
                var company = companies.FirstOrDefault();
                if (company == null)
                {
                    TempData["Error"] = "Please create a company profile first.";
                    return RedirectToAction("Companies", "Account");
                }

                var ownProducts = await _ownProductDAO.GetByCompanyProfileIdAsync(company.CompanyProfileId);
                var model = new MyProductsViewModel
                {
                    OwnProducts = ownProducts.ToList(),
                    CompanyName = company.CompanyName ?? "Your Company"
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading saved products");
                TempData["Error"] = "An error occurred while loading your products. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult ManualEntry()
        {
            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToAction("Login", "Account");
            }

            return View(new ManualProductEntryViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ManualEntry(ManualProductEntryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                var user = await _userDAO.GetByEmailAsync(email);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var companies = await _companyDAO.ListCompaniesForUserAsync(user.UserId);
                var company = companies.FirstOrDefault();
                if (company == null)
                {
                    TempData["Error"] = "Please create a company profile first.";
                    return RedirectToAction("Companies", "Account");
                }

                await SaveManualProductEntry(model, company.CompanyProfileId, user.UserId);

                TempData["Message"] = "Product and competitor data have been saved successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving manual product entry");
                TempData["Error"] = "An error occurred while saving the product. Please try again.";
                return View(model);
            }
        }

        private async Task SaveDiscoveredProducts(List<DiscoveredProduct> products, int companyProfileId, int userId)
        {
            foreach (var product in products)
            {
                // Check if product already exists
                var existingProduct = await _ownProductDAO.FindFirstAsync(p => 
                    p.CompanyProfileId == companyProfileId && 
                    p.ProductName == product.ProductName);
                
                if (existingProduct != null)
                {
                    _logger.LogInformation("Product {ProductName} already exists, skipping", product.ProductName);
                    continue; // Skip duplicate products
                }

                // Create own product
                var ownProduct = new Models.OwnProduct
                {
                    CompanyProfileId = companyProfileId,
                    UserId = userId,
                    ProductName = product.ProductName,
                    ProductWebsiteUrl = product.OurPrice?.ToString()
                };
                var createdOwnProduct = await _ownProductDAO.CreateAsync(ownProduct);

                // Save initial price for the product
                if (product.OurPrice.HasValue)
                {
                    var initialPrice = new Models.Price
                    {
                        OwnProductId = createdOwnProduct.OwnProductId,
                        PriceValue = product.OurPrice.Value,
                        Currency = "USD",
                        PriceDate = DateTime.Now
                    };
                    await _priceDAO.CreateAsync(initialPrice);
                }

                // Create competitors and their products
                foreach (var competitorPrice in product.CompetitorPrices)
                {
                    // Find or create competitor
                    var existingCompetitor = await _competitorDAO.FindFirstAsync(c => 
                        c.CompanyProfileId == companyProfileId && 
                        c.CompetitorName == competitorPrice.RetailerName);

                    Models.Competitor competitor;
                    if (existingCompetitor == null)
                    {
                        competitor = new Models.Competitor
                        {
                            CompanyProfileId = companyProfileId,
                            CompetitorName = competitorPrice.RetailerName,
                            CompetitorWebsite = competitorPrice.Url
                        };
                        competitor = await _competitorDAO.CreateAsync(competitor);
                    }
                    else
                    {
                        competitor = existingCompetitor;
                    }

                    // Create competitor product
                    var competitorProduct = new Models.CompetitorProduct
                    {
                        OwnProductId = createdOwnProduct.OwnProductId,
                        CompetitorId = competitor.CompetitorId,
                        CProductName = product.ProductName,
                        CProductWebsiteUrl = competitorPrice.Url
                    };
                    await _competitorProductDAO.CreateAsync(competitorProduct);
                }
            }
        }

        private async Task SaveManualProductEntry(ManualProductEntryViewModel model, int companyProfileId, int userId)
        {
            // Create own product
            var ownProduct = new Models.OwnProduct
            {
                CompanyProfileId = companyProfileId,
                UserId = userId,
                ProductName = model.OwnProductName,
                ProductWebsiteUrl = model.OwnProductUrl
            };
            var createdOwnProduct = await _ownProductDAO.CreateAsync(ownProduct);

            // Create price entry for own product if price is provided
            if (model.CurrentPrice.HasValue)
            {
                var ownProductPrice = new Models.Price
                {
                    OwnProductId = createdOwnProduct.OwnProductId,
                    PriceValue = model.CurrentPrice.Value,
                    Currency = model.Currency,
                    PriceDate = DateTime.Now
                };
                await _priceDAO.CreateAsync(ownProductPrice);
            }

            // Create competitor
            var competitor = new Models.Competitor
            {
                CompanyProfileId = companyProfileId,
                CompetitorName = "Manual Entry Competitor",
                CompetitorWebsite = model.CompetitorProductUrl
            };
            competitor = await _competitorDAO.CreateAsync(competitor);

            // Create competitor product
            var competitorProduct = new Models.CompetitorProduct
            {
                OwnProductId = createdOwnProduct.OwnProductId,
                CompetitorId = competitor.CompetitorId,
                CProductName = model.CompetitorProductName,
                CProductWebsiteUrl = model.CompetitorProductUrl
            };
            await _competitorProductDAO.CreateAsync(competitorProduct);

            // Handle additional competitor URLs if provided
            var additionalUrls = new[] { model.CompetitorProductUrl2, model.CompetitorProductUrl3, model.CompetitorProductUrl4, model.CompetitorProductUrl5 }
                .Where(url => !string.IsNullOrEmpty(url))
                .ToList();

            foreach (var url in additionalUrls)
            {
                var additionalCompetitor = new Models.Competitor
                {
                    CompanyProfileId = companyProfileId,
                    CompetitorName = $"Manual Entry Competitor - {url}",
                    CompetitorWebsite = url
                };
                additionalCompetitor = await _competitorDAO.CreateAsync(additionalCompetitor);

                var additionalCompetitorProduct = new Models.CompetitorProduct
                {
                    OwnProductId = createdOwnProduct.OwnProductId,
                    CompetitorId = additionalCompetitor.CompetitorId,
                    CProductName = "Manual Entry Product",
                    CProductWebsiteUrl = url
                };
                await _competitorProductDAO.CreateAsync(additionalCompetitorProduct);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPriceHistory(int productId)
        {
            try
            {
                _logger.LogInformation("Getting price history for product {ProductId}", productId);
                var priceHistory = await _priceDAO.GetByOwnProductIdAsync(productId);
                _logger.LogInformation("Found {Count} price records for product {ProductId}", priceHistory.Count(), productId);
                
                var priceData = priceHistory
                    .OrderByDescending(p => p.PriceDate)
                    .Select(p => new
                    {
                        priceValue = p.PriceValue?.ToString("F2"),
                        priceDate = p.PriceDate,
                        currency = p.Currency
                    })
                    .ToList();

                _logger.LogInformation("Returning {Count} price records", priceData.Count);
                return Json(priceData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting price history for product {ProductId}", productId);
                return Json(new List<object>());
            }
        }

        private string NormalizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            url = url.Trim();

            // If it already has a scheme, return as is
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }

            // Handle different formats
            if (url.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            {
                return "https://" + url;
            }

            // For domain names like "apple.com", add "www." and "https://"
            if (url.Contains(".") && !url.Contains("/"))
            {
                return "https://www." + url;
            }

            // For paths like "apple.com/products", add "https://www."
            if (url.Contains(".") && url.Contains("/"))
            {
                return "https://www." + url;
            }

            // Default fallback
            return "https://" + url;
        }
    }
}
