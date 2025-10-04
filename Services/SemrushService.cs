using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using PricePulse.Models;
using PricePulse.ViewModels;

namespace PricePulse.Services
{
    public interface ISemrushService
    {
        Task<List<CompetitorInfo>> GetCompetitorsAsync(string domain);
        Task<List<CompetitorInfo>> GetCompetitorsWithDetailsAsync(string domain);
    }

    public class SemrushService : ISemrushService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SemrushService> _logger;
        private readonly IMemoryCache _cache;

        public SemrushService(HttpClient httpClient, IConfiguration configuration, ILogger<SemrushService> logger, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
        }

        public async Task<List<CompetitorInfo>> GetCompetitorsAsync(string domain)
        {
            var cacheKey = $"competitors_{domain}";
            
            // For demo purposes, let's skip cache to ensure sample data is returned
            // if (_cache.TryGetValue(cacheKey, out List<CompetitorInfo>? cachedCompetitors))
            // {
            //     Console.WriteLine($"=== CACHE HIT for competitors of {domain} ===");
            //     return cachedCompetitors ?? new List<CompetitorInfo>();
            // }

            try
            {
                Console.WriteLine($"=== FETCHING COMPETITORS for {domain} ===");
                
                var apiKey = _configuration["Semrush:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("Semrush API key not configured, returning sample competitors");
                    return GetSampleCompetitors(domain);
                }

                // Get competitors using Semrush API
                var competitors = await GetCompetitorsFromSemrush(domain, apiKey);
                
                // If no competitors found and we got a 403 error, use sample data
                if (competitors.Count == 0)
                {
                    _logger.LogWarning("No competitors found from API, returning sample competitors for demo");
                    competitors = GetSampleCompetitors(domain);
                }
                
                // Cache for 24 hours
                _cache.Set(cacheKey, competitors, TimeSpan.FromHours(24));
                Console.WriteLine($"=== CACHED {competitors.Count} competitors for {domain} ===");
                
                return competitors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching competitors for domain {Domain}, returning sample competitors", domain);
                return GetSampleCompetitors(domain);
            }
        }

        public async Task<List<CompetitorInfo>> GetCompetitorsWithDetailsAsync(string domain)
        {
            var competitors = await GetCompetitorsAsync(domain);
            
            // Enhance with additional details
            foreach (var competitor in competitors)
            {
                try
                {
                    var details = await GetCompetitorDetails(competitor.Domain);
                    competitor.Traffic = details.Traffic;
                    competitor.Keywords = details.Keywords;
                    competitor.Backlinks = details.Backlinks;
                    competitor.OrganicTraffic = details.OrganicTraffic;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get details for competitor {Domain}", competitor.Domain);
                }
            }

            return competitors;
        }

        private async Task<List<CompetitorInfo>> GetCompetitorsFromSemrush(string domain, string apiKey)
        {
            try
            {
                // Semrush API endpoint for competitors
                var url = $"https://api.semrush.com/?type=domain_organic_organic&key={apiKey}&domain={domain}&database=us&display_limit=20&export_columns=domain,common_keywords,organic_keywords,organic_traffic,organic_cost,adwords_keywords,adwords_traffic,adwords_cost";
                
                Console.WriteLine($"=== SEMRUSH API URL: {url} ===");
                
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Semrush API call failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    
                    // If API units are zero or any 403 error, return sample competitors for demo purposes
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || errorContent.Contains("API UNITS BALANCE IS ZERO"))
                    {
                        _logger.LogWarning("Semrush API units exhausted, returning sample competitors for demo");
                        return GetSampleCompetitors(domain);
                    }
                    
                    return new List<CompetitorInfo>();
                }

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"=== SEMRUSH RESPONSE: {content.Substring(0, Math.Min(500, content.Length))}... ===");

                return ParseSemrushResponse(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Semrush API for domain {Domain}", domain);
                return new List<CompetitorInfo>();
            }
        }

        private List<CompetitorInfo> ParseSemrushResponse(string response)
        {
            var competitors = new List<CompetitorInfo>();
            
            try
            {
                var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                
                // Skip header line
                for (int i = 1; i < lines.Length; i++)
                {
                    var columns = lines[i].Split(';');
                    if (columns.Length >= 6)
                    {
                        var competitor = new CompetitorInfo
                        {
                            Domain = columns[0].Trim(),
                            CommonKeywords = int.TryParse(columns[1], out var commonKeywords) ? commonKeywords : 0,
                            OrganicKeywords = int.TryParse(columns[2], out var organicKeywords) ? organicKeywords : 0,
                            OrganicTraffic = int.TryParse(columns[3], out var organicTraffic) ? organicTraffic : 0,
                            OrganicCost = decimal.TryParse(columns[4], out var organicCost) ? organicCost : 0,
                            AdwordsKeywords = int.TryParse(columns[5], out var adwordsKeywords) ? adwordsKeywords : 0,
                            AdwordsTraffic = int.TryParse(columns[6], out var adwordsTraffic) ? adwordsTraffic : 0,
                            AdwordsCost = decimal.TryParse(columns[7], out var adwordsCost) ? adwordsCost : 0,
                            IsConfirmed = false,
                            DiscoveredAt = DateTime.UtcNow
                        };
                        
                        competitors.Add(competitor);
                        Console.WriteLine($"=== FOUND COMPETITOR: {competitor.Domain} ===");
                    }
                }
                
                Console.WriteLine($"=== PARSED {competitors.Count} COMPETITORS ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Semrush response");
            }

            return competitors;
        }

        private async Task<CompetitorDetails> GetCompetitorDetails(string domain)
        {
            try
            {
                var apiKey = _configuration["Semrush:ApiKey"];
                var url = $"https://api.semrush.com/?type=domain_ranks&key={apiKey}&domain={domain}&database=us&export_columns=domain,rank,keywords,traffic,traffic_cost,adwords_keywords,adwords_traffic,adwords_cost";
                
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    
                    if (lines.Length > 1)
                    {
                        var columns = lines[1].Split(';');
                        return new CompetitorDetails
                        {
                            Traffic = int.TryParse(columns[3], out var traffic) ? traffic : 0,
                            Keywords = int.TryParse(columns[2], out var keywords) ? keywords : 0,
                            Backlinks = 0, // Would need separate API call
                            OrganicTraffic = int.TryParse(columns[3], out var organicTraffic) ? organicTraffic : 0
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get details for {Domain}", domain);
            }

            return new CompetitorDetails();
        }

        private List<CompetitorInfo> GetSampleCompetitors(string domain)
        {
            // Return sample competitors for demo purposes when API units are exhausted
            var sampleCompetitors = new List<CompetitorInfo>
            {
                new CompetitorInfo
                {
                    Domain = "samsung.com",
                    CommonKeywords = 1250,
                    OrganicKeywords = 8900,
                    OrganicTraffic = 45000,
                    OrganicCost = 125000,
                    AdwordsKeywords = 320,
                    AdwordsTraffic = 8500,
                    AdwordsCost = 45000,
                    IsConfirmed = false,
                    DiscoveredAt = DateTime.UtcNow
                },
                new CompetitorInfo
                {
                    Domain = "google.com",
                    CommonKeywords = 2100,
                    OrganicKeywords = 15600,
                    OrganicTraffic = 89000,
                    OrganicCost = 280000,
                    AdwordsKeywords = 450,
                    AdwordsTraffic = 12000,
                    AdwordsCost = 65000,
                    IsConfirmed = false,
                    DiscoveredAt = DateTime.UtcNow
                },
                new CompetitorInfo
                {
                    Domain = "microsoft.com",
                    CommonKeywords = 1800,
                    OrganicKeywords = 12300,
                    OrganicTraffic = 67000,
                    OrganicCost = 195000,
                    AdwordsKeywords = 380,
                    AdwordsTraffic = 9800,
                    AdwordsCost = 52000,
                    IsConfirmed = false,
                    DiscoveredAt = DateTime.UtcNow
                },
                new CompetitorInfo
                {
                    Domain = "amazon.com",
                    CommonKeywords = 3200,
                    OrganicKeywords = 25000,
                    OrganicTraffic = 150000,
                    OrganicCost = 450000,
                    AdwordsKeywords = 680,
                    AdwordsTraffic = 18000,
                    AdwordsCost = 95000,
                    IsConfirmed = false,
                    DiscoveredAt = DateTime.UtcNow
                },
                new CompetitorInfo
                {
                    Domain = "tesla.com",
                    CommonKeywords = 450,
                    OrganicKeywords = 3200,
                    OrganicTraffic = 18000,
                    OrganicCost = 55000,
                    AdwordsKeywords = 120,
                    AdwordsTraffic = 3200,
                    AdwordsCost = 18000,
                    IsConfirmed = false,
                    DiscoveredAt = DateTime.UtcNow
                }
            };

            Console.WriteLine($"=== RETURNING {sampleCompetitors.Count} SAMPLE COMPETITORS for {domain} ===");
            return sampleCompetitors;
        }
    }


    public class CompetitorDetails
    {
        public int Traffic { get; set; }
        public int Keywords { get; set; }
        public int Backlinks { get; set; }
        public int OrganicTraffic { get; set; }
    }
}
