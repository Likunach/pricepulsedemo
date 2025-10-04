using Microsoft.AspNetCore.Mvc;
using PricePulse.Services;
using PricePulse.ViewModels;
using PricePulse.Models;

namespace PricePulse.Controllers
{
    public class CompetitorController : Controller
    {
        private readonly ISemrushService _semrushService;
        private readonly ICompetitorProductAnalysisService _analysisService;
        private readonly ILogger<CompetitorController> _logger;

        public CompetitorController(
            ISemrushService semrushService, 
            ICompetitorProductAnalysisService analysisService,
            ILogger<CompetitorController> logger)
        {
            _semrushService = semrushService;
            _analysisService = analysisService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new CompetitorDiscoveryViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DiscoverCompetitors(CompetitorDiscoveryViewModel model)
        {
            Console.WriteLine($"=== STARTING COMPETITOR DISCOVERY for {model.Domain} ===");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== MODEL STATE INVALID ===");
                return View("Index", model);
            }

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                // Get competitors from Semrush
                var competitors = await _semrushService.GetCompetitorsAsync(model.Domain);
                
                stopwatch.Stop();
                Console.WriteLine($"=== COMPETITOR DISCOVERY COMPLETED in {stopwatch.ElapsedMilliseconds}ms, found {competitors.Count} competitors ===");

                model.DiscoveredCompetitors = competitors;
                
                if (competitors.Count > 0)
                {
                    // Check if we're using sample data (when Semrush API units are exhausted)
                    var isSampleData = competitors.Any(c => c.Domain == "samsung.com" || c.Domain == "google.com");
                    if (isSampleData)
                    {
                        model.Message = $"Demo Mode: Showing {competitors.Count} sample competitors for {model.Domain} (Semrush API units exhausted)";
                    }
                    else
                    {
                        model.Message = $"Found {competitors.Count} competitors for {model.Domain}";
                    }
                }
                else
                {
                    model.Message = "No competitors found. You can add them manually.";
                }
                
                model.IsSuccess = true;

                TempData["Success"] = model.Message;
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discovering competitors for domain {Domain}", model.Domain);
                model.Message = "Error discovering competitors. Please try again.";
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
        public IActionResult SaveManualCompetitors(ManualCompetitorEntryViewModel model)
        {
            Console.WriteLine($"=== SAVING MANUAL COMPETITORS for {model.Domain} ===");
            
            if (!ModelState.IsValid)
            {
                return View("Index", new CompetitorDiscoveryViewModel 
                { 
                    Domain = model.Domain, 
                    ShowManualEntry = true,
                    Competitors = model.Competitors.Select(c => new ManualCompetitor 
                    { 
                        Name = c.Name, 
                        Website = c.Website, 
                        ProductListingPage = c.ProductListingPage, 
                        Notes = c.Notes 
                    }).ToList()
                });
            }

            try
            {
                var competitors = model.Competitors.Select(c => new CompetitorInfo
                {
                    Domain = c.Website,
                    Notes = c.Notes,
                    ProductListingPage = c.ProductListingPage,
                    IsConfirmed = true,
                    DiscoveredAt = DateTime.UtcNow
                }).ToList();

                // Here you would save to database
                foreach (var competitor in competitors)
                {
                    Console.WriteLine($"=== SAVED MANUAL COMPETITOR: {competitor.Domain} ===");
                }

                TempData["Success"] = $"Saved {competitors.Count} manual competitors for {model.Domain}";
                return RedirectToAction("List", new { domain = model.Domain });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving manual competitors for domain {Domain}", model.Domain);
                TempData["Error"] = "Error saving manual competitors. Please try again.";
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
        public async Task<IActionResult> RefreshCompetitors(string domain)
        {
            Console.WriteLine($"=== REFRESHING COMPETITORS for {domain} ===");
            
            try
            {
                var competitors = await _semrushService.GetCompetitorsWithDetailsAsync(domain);
                
                // Here you would update the database
                TempData["Success"] = $"Refreshed {competitors.Count} competitors for {domain}";
                
                return RedirectToAction("List", new { domain });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing competitors for domain {Domain}", domain);
                TempData["Error"] = "Error refreshing competitors. Please try again.";
                return RedirectToAction("List", new { domain });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeAllCompetitorProducts(string domain)
        {
            Console.WriteLine($"=== ANALYZING ALL COMPETITOR PRODUCTS for {domain} ===");
            
            try
            {
                // Get confirmed competitors
                var competitors = await _semrushService.GetCompetitorsAsync(domain);
                var confirmedCompetitors = competitors.Where(c => c.IsConfirmed).ToList();
                
                if (!confirmedCompetitors.Any())
                {
                    TempData["Error"] = "No confirmed competitors found. Please confirm competitors first.";
                    return RedirectToAction("List", new { domain });
                }

                // Analyze products for all competitors
                var analysisResults = await _analysisService.AnalyzeCompetitorProductsAsync(domain, confirmedCompetitors);
                
                TempData["Success"] = $"Analyzed products for {analysisResults.Count} competitors. Found {analysisResults.Sum(r => r.TotalProducts)} total products.";
                
                return RedirectToAction("AnalysisResults", new { domain });
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
                var result = await _analysisService.AnalyzeSingleCompetitorAsync(competitorDomain);
                
                if (result.Success)
                {
                    TempData["Success"] = $"Found {result.TotalProducts} products for {competitorDomain}";
                }
                else
                {
                    TempData["Error"] = $"Failed to analyze {competitorDomain}: {result.ErrorMessage}";
                }
                
                return RedirectToAction("CompetitorDetails", new { domain = competitorDomain });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing competitor {Domain}", competitorDomain);
                TempData["Error"] = "Error analyzing competitor. Please try again.";
                return RedirectToAction("List");
            }
        }

        public async Task<IActionResult> AnalysisResults(string domain)
        {
            Console.WriteLine($"=== SHOWING ANALYSIS RESULTS for {domain} ===");
            
            try
            {
                var competitors = await _semrushService.GetCompetitorsAsync(domain);
                var confirmedCompetitors = competitors.Where(c => c.IsConfirmed).ToList();
                
                var analysisResults = new List<CompetitorAnalysisResult>();
                foreach (var competitor in confirmedCompetitors)
                {
                    var result = await _analysisService.AnalyzeSingleCompetitorAsync(competitor.Domain);
                    result.CompetitorInfo = competitor;
                    analysisResults.Add(result);
                }
                
                var model = new CompetitorAnalysisResultsViewModel
                {
                    Domain = domain,
                    AnalysisResults = analysisResults,
                    TotalCompetitors = analysisResults.Count,
                    TotalProducts = analysisResults.Sum(r => r.TotalProducts),
                    ProductsWithPrices = analysisResults.Sum(r => r.ProductsWithPrices),
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
    }
}
