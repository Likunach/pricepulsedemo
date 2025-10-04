using Microsoft.Extensions.Caching.Memory;
using PricePulse.Models;
using PricePulse.ViewModels;

namespace PricePulse.Services
{
    public interface ICompetitorProductAnalysisService
    {
        Task<List<CompetitorAnalysisResult>> AnalyzeCompetitorProductsAsync(string domain, List<CompetitorInfo> competitors);
        Task<CompetitorAnalysisResult> AnalyzeSingleCompetitorAsync(string competitorDomain);
        Task<List<DiscoveredProduct>> GetCompetitorProductsAsync(string competitorDomain);
    }

    public class CompetitorProductAnalysisService : ICompetitorProductAnalysisService
    {
        private readonly ISemrushService _semrushService;
        private readonly IOpenAIService _openAIService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CompetitorProductAnalysisService> _logger;

        public CompetitorProductAnalysisService(
            ISemrushService semrushService,
            IOpenAIService openAIService,
            IMemoryCache cache,
            ILogger<CompetitorProductAnalysisService> logger)
        {
            _semrushService = semrushService;
            _openAIService = openAIService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<CompetitorAnalysisResult>> AnalyzeCompetitorProductsAsync(string domain, List<CompetitorInfo> competitors)
        {
            Console.WriteLine($"=== STARTING COMPETITOR PRODUCT ANALYSIS for {domain} ===");
            
            var results = new List<CompetitorAnalysisResult>();
            var semaphore = new SemaphoreSlim(3, 3); // Limit to 3 concurrent analyses
            
            var tasks = competitors.Select(async competitor =>
            {
                await semaphore.WaitAsync();
                try
                {
                    Console.WriteLine($"=== ANALYZING COMPETITOR: {competitor.Domain} ===");
                    var result = await AnalyzeSingleCompetitorAsync(competitor.Domain);
                    result.CompetitorInfo = competitor;
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error analyzing competitor {Domain}", competitor.Domain);
                    return new CompetitorAnalysisResult
                    {
                        CompetitorInfo = competitor,
                        Success = false,
                        ErrorMessage = ex.Message,
                        Products = new List<DiscoveredProduct>()
                    };
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var analysisResults = await Task.WhenAll(tasks);
            results.AddRange(analysisResults);

            Console.WriteLine($"=== COMPLETED COMPETITOR ANALYSIS: {results.Count} results ===");
            return results;
        }

        public async Task<CompetitorAnalysisResult> AnalyzeSingleCompetitorAsync(string competitorDomain)
        {
            var cacheKey = $"competitor_products_{competitorDomain}";
            
            if (_cache.TryGetValue(cacheKey, out CompetitorAnalysisResult? cachedResult))
            {
                Console.WriteLine($"=== CACHE HIT for competitor products: {competitorDomain} ===");
                return cachedResult ?? new CompetitorAnalysisResult();
            }

            try
            {
                Console.WriteLine($"=== ANALYZING COMPETITOR PRODUCTS: {competitorDomain} ===");
                
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                // Use OpenAI service to discover products from competitor website
                var products = await _openAIService.DiscoverProductsAsync($"https://{competitorDomain}", "Unknown");
                
                stopwatch.Stop();
                
                var result = new CompetitorAnalysisResult
                {
                    CompetitorDomain = competitorDomain,
                    Success = true,
                    Products = products,
                    AnalysisTime = stopwatch.ElapsedMilliseconds,
                    AnalyzedAt = DateTime.UtcNow
                };

                // Cache for 6 hours
                _cache.Set(cacheKey, result, TimeSpan.FromHours(6));
                
                Console.WriteLine($"=== COMPETITOR ANALYSIS COMPLETED: {competitorDomain} - {products.Count} products in {stopwatch.ElapsedMilliseconds}ms ===");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing competitor products for {Domain}", competitorDomain);
                return new CompetitorAnalysisResult
                {
                    CompetitorDomain = competitorDomain,
                    Success = false,
                    ErrorMessage = ex.Message,
                    Products = new List<DiscoveredProduct>(),
                    AnalyzedAt = DateTime.UtcNow
                };
            }
        }

        public async Task<List<DiscoveredProduct>> GetCompetitorProductsAsync(string competitorDomain)
        {
            var result = await AnalyzeSingleCompetitorAsync(competitorDomain);
            return result.Products ?? new List<DiscoveredProduct>();
        }
    }

}
