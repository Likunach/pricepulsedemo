using Microsoft.Extensions.Caching.Memory;
using PricePulse.Models;
using PricePulse.ViewModels;
using PricePulse.DAOs.Interfaces;
using System.Text.Json;

namespace PricePulse.Services
{
    public interface ICompetitorProductAnalysisService
    {
        Task<List<CompetitorAnalysisResult>> AnalyzeCompetitorProductsAsync(string domain, List<CompetitorInfo> competitors);
        Task<CompetitorAnalysisResult> AnalyzeSingleCompetitorAsync(string competitorDomain);
        Task<List<DiscoveredProduct>> GetCompetitorProductsAsync(string competitorDomain);
        Task<CompetitorProductAnalysisResult> AnalyzeCompetitorWithRetailersAsync(string competitorDomain, string companyLocation = "United States");
        Task<List<CompetitorProductAnalysis>> GetCompetitorProductsFromDatabaseAsync(string competitorDomain);
        Task SaveCompetitorProductsAsync(string competitorDomain, List<CompetitorProductAnalysis> products);
    }

    public class CompetitorProductAnalysisService : ICompetitorProductAnalysisService
    {
        private readonly IOpenAIService _openAIService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CompetitorProductAnalysisService> _logger;
        private readonly ICompetitorProductAnalysisDAO _competitorProductAnalysisDAO;

        public CompetitorProductAnalysisService(
            IOpenAIService openAIService,
            IMemoryCache cache,
            ILogger<CompetitorProductAnalysisService> logger,
            ICompetitorProductAnalysisDAO competitorProductAnalysisDAO)
        {
            _openAIService = openAIService;
            _cache = cache;
            _logger = logger;
            _competitorProductAnalysisDAO = competitorProductAnalysisDAO;
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

        public async Task<CompetitorProductAnalysisResult> AnalyzeCompetitorWithRetailersAsync(string competitorDomain, string companyLocation = "United States")
        {
            var cacheKey = $"competitor_retailers_{competitorDomain}_{companyLocation}";
            
            if (_cache.TryGetValue(cacheKey, out CompetitorProductAnalysisResult? cachedResult))
            {
                Console.WriteLine($"=== CACHE HIT for competitor retailers: {competitorDomain} ===");
                return cachedResult ?? new CompetitorProductAnalysisResult();
            }

            try
            {
                Console.WriteLine($"=== ANALYZING COMPETITOR WITH RETAILERS: {competitorDomain} ===");
                
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                // Create the enhanced prompt for competitor analysis with retailers
                var prompt = $@"Analyze the website at https://{competitorDomain} and identify all products. For each product found, find the top 20 retailers that sell the same or similar products within {companyLocation}.

Return the results in JSON format with the following structure:

{{
  ""competitor_analysis"": {{
    ""competitor_url"": ""https://{competitorDomain}"",
    ""analysis_date"": ""{DateTime.UtcNow:yyyy-MM-dd}"",
    ""total_products_found"": number,
    ""products"": [
      {{
        ""product_name"": ""string"",
        ""product_description"": ""string"", 
        ""competitor_price"": ""string"",
        ""competitor_currency"": ""string"",
        ""product_category"": ""string"",
        ""product_image_url"": ""string"",
        ""competitor_product_url"": ""string"",
        ""retailers"": [
          {{
            ""retailer_name"": ""string"",
            ""retailer_url"": ""string"",
            ""product_url"": ""string"",
            ""price"": ""string"",
            ""currency"": ""string"",
            ""availability"": ""string"",
            ""shipping_info"": ""string"",
            ""rating"": ""string"",
            ""reviews_count"": ""string""
          }}
        ]
      }}
    ]
  }}
}}

## Instructions:
1. Identify ALL products on the competitor website
2. For each product, find retailers selling the same/similar products
3. Focus on retailers within the specified location: {companyLocation}
4. Include major retailers like Amazon, eBay, Walmart, Target, etc.
5. Provide accurate pricing information where available
6. Include product URLs for each retailer
7. If a product is not found at retailers, still include it with empty retailers array
8. Ensure all prices are in the same currency for comparison
9. Include availability status (In Stock, Out of Stock, Limited Stock)
10. Provide shipping information where available";

                var response = await _openAIService.GetCompletionAsync(prompt);
                
                stopwatch.Stop();
                
                var result = new CompetitorProductAnalysisResult
                {
                    CompetitorDomain = competitorDomain,
                    Success = true,
                    AnalysisTime = stopwatch.ElapsedMilliseconds,
                    AnalyzedAt = DateTime.UtcNow,
                    CompanyLocation = companyLocation
                };

                // Parse the JSON response
                try
                {
                    var jsonResponse = JsonSerializer.Deserialize<CompetitorAnalysisResponse>(response);
                    if (jsonResponse?.CompetitorAnalysis != null)
                    {
                        result.TotalProducts = jsonResponse.CompetitorAnalysis.TotalProductsFound;
                        result.Products = jsonResponse.CompetitorAnalysis.Products.Select(p => new CompetitorProductAnalysis
                        {
                            CompetitorDomain = competitorDomain,
                            ProductName = p.ProductName,
                            ProductDescription = p.ProductDescription,
                            CompetitorPrice = p.CompetitorPrice,
                            CompetitorCurrency = p.CompetitorCurrency,
                            ProductCategory = p.ProductCategory,
                            ProductImageUrl = p.ProductImageUrl,
                            CompetitorProductUrl = p.CompetitorProductUrl,
                            DiscoveredAt = DateTime.UtcNow,
                            LastUpdated = DateTime.UtcNow,
                            IsActive = true,
                            Retailers = p.Retailers.Select(r => new CompetitorProductRetailer
                            {
                                RetailerName = r.RetailerName,
                                RetailerUrl = r.RetailerUrl,
                                ProductUrl = r.ProductUrl,
                                Price = r.Price,
                                Currency = r.Currency,
                                Availability = r.Availability,
                                ShippingInfo = r.ShippingInfo,
                                Rating = r.Rating,
                                ReviewsCount = r.ReviewsCount,
                                LastUpdated = DateTime.UtcNow,
                                IsActive = true
                            }).ToList()
                        }).ToList();
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error parsing competitor analysis JSON response");
                    result.Success = false;
                    result.ErrorMessage = "Failed to parse analysis results";
                }

                // Cache for 6 hours
                _cache.Set(cacheKey, result, TimeSpan.FromHours(6));
                
                Console.WriteLine($"=== COMPETITOR RETAILER ANALYSIS COMPLETED: {competitorDomain} - {result.TotalProducts} products in {stopwatch.ElapsedMilliseconds}ms ===");
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing competitor with retailers for {Domain}", competitorDomain);
                return new CompetitorProductAnalysisResult
                {
                    CompetitorDomain = competitorDomain,
                    Success = false,
                    ErrorMessage = ex.Message,
                    Products = new List<CompetitorProductAnalysis>(),
                    AnalyzedAt = DateTime.UtcNow
                };
            }
        }

        public async Task<List<CompetitorProductAnalysis>> GetCompetitorProductsFromDatabaseAsync(string competitorDomain)
        {
            return await _competitorProductAnalysisDAO.GetByCompetitorDomainAsync(competitorDomain);
        }

        public async Task SaveCompetitorProductsAsync(string competitorDomain, List<CompetitorProductAnalysis> products)
        {
            foreach (var product in products)
            {
                product.CompetitorDomain = competitorDomain;
                product.DiscoveredAt = DateTime.UtcNow;
                product.LastUpdated = DateTime.UtcNow;
                product.IsActive = true;

                foreach (var retailer in product.Retailers)
                {
                    retailer.LastUpdated = DateTime.UtcNow;
                    retailer.IsActive = true;
                }

                await _competitorProductAnalysisDAO.CreateAsync(product);
            }
        }
    }

    // Response models for JSON deserialization
    public class CompetitorAnalysisResponse
    {
        public CompetitorAnalysisData CompetitorAnalysis { get; set; } = new();
    }

    public class CompetitorAnalysisData
    {
        public string CompetitorUrl { get; set; } = string.Empty;
        public string AnalysisDate { get; set; } = string.Empty;
        public int TotalProductsFound { get; set; }
        public List<CompetitorProductData> Products { get; set; } = new();
    }

    public class CompetitorProductData
    {
        public string ProductName { get; set; } = string.Empty;
        public string ProductDescription { get; set; } = string.Empty;
        public string CompetitorPrice { get; set; } = string.Empty;
        public string CompetitorCurrency { get; set; } = string.Empty;
        public string ProductCategory { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
        public string CompetitorProductUrl { get; set; } = string.Empty;
        public List<RetailerData> Retailers { get; set; } = new();
    }

    public class RetailerData
    {
        public string RetailerName { get; set; } = string.Empty;
        public string RetailerUrl { get; set; } = string.Empty;
        public string ProductUrl { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string Availability { get; set; } = string.Empty;
        public string ShippingInfo { get; set; } = string.Empty;
        public string Rating { get; set; } = string.Empty;
        public string ReviewsCount { get; set; } = string.Empty;
    }

    public class CompetitorProductAnalysisResult
    {
        public string CompetitorDomain { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<CompetitorProductAnalysis> Products { get; set; } = new();
        public int TotalProducts { get; set; }
        public long AnalysisTime { get; set; }
        public DateTime AnalyzedAt { get; set; }
        public string CompanyLocation { get; set; } = "United States";
    }

}
