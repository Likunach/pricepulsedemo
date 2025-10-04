using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using PricePulse.ViewModels;

namespace PricePulse.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenAIService> _logger;
        private readonly IMemoryCache _cache;
        private readonly SemaphoreSlim _semaphore;

        public OpenAIService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAIService> logger, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
            _semaphore = new SemaphoreSlim(5, 5);
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsAsync(string websiteUrl, string companyLocation)
        {
            var cacheKey = $"products_{websiteUrl}_{companyLocation}";
            
            if (_cache.TryGetValue(cacheKey, out List<DiscoveredProduct>? cachedProducts))
            {
                Console.WriteLine($"=== CACHE HIT for {websiteUrl} ===");
                return cachedProducts ?? new List<DiscoveredProduct>();
            }

            await _semaphore.WaitAsync();
            try
            {
                Console.WriteLine($"=== ACQUIRED SEMAPHORE for {websiteUrl} ===");
                
                var html = await FetchPageForUrlAsync(websiteUrl);
                Console.WriteLine($"=== HTML LENGTH: {html.Length} ===");
                Console.WriteLine($"=== HTML PREVIEW: {html.Substring(0, Math.Min(500, html.Length))}... ===");

                var prompt = BuildDiscoveryPrompt(websiteUrl, companyLocation, html);
                Console.WriteLine($"=== PROMPT LENGTH: {prompt.Length} ===");

                var products = await CallOpenAIAsync(prompt);
                
                _cache.Set(cacheKey, products, TimeSpan.FromMinutes(30));
                Console.WriteLine($"=== CACHED {products.Count} products for {websiteUrl} ===");
                
                return products;
            }
            finally
            {
                _semaphore.Release();
                Console.WriteLine($"=== RELEASED SEMAPHORE for {websiteUrl} ===");
            }
        }

        public async Task<List<DiscoveredProduct>> DiscoverProductsWithModifiedPromptAsync(string websiteUrl, string companyLocation, string modifiedPrompt)
        {
            var html = await FetchPageForUrlAsync(websiteUrl);
            var prompt = BuildModifiedPrompt(websiteUrl, companyLocation, modifiedPrompt, html);
            return await CallOpenAIAsync(prompt);
        }

        private string BuildDiscoveryPrompt(string websiteUrl, string companyLocation, string pageHtml)
        {
            var snippet = pageHtml ?? string.Empty;
            if (snippet.Length > 120000) snippet = snippet.Substring(0, 120000);
            
            return $"Identify all products on {websiteUrl} and find top 20 retailers that sell the same products within {companyLocation}. Return JSON formatted table with following: product name, our price, price for each retailer and URL of the product purchase page of given retailer.\n\n" +
                   "ANALYSIS REQUIREMENTS:\n" +
                   "- Extract ALL products found on the website\n" +
                   "- For each product, identify the current price on this website (our price)\n" +
                   "- Research and find up to 20 major retailers in {companyLocation} that sell the same products\n" +
                   "- Include competitor prices from retailers like Amazon, Best Buy, Walmart, Target, etc.\n" +
                   "- Provide product URLs for each retailer when available\n\n" +
                   "PRICE EXTRACTION:\n" +
                   "- Look for price text like '$999', '$1,299', 'Starting at $599'\n" +
                   "- Look for 'From $X' or 'Starting at $X'\n" +
                   "- Look for price ranges like '$999-$1,299'\n" +
                   "- Extract numbers as DECIMAL (e.g., 999.00 for '$999')\n" +
                   "- If no price found, set to null\n" +
                   "- IMPORTANT: All prices must be numbers, not strings\n\n" +
                   "RETAILER RESEARCH:\n" +
                   "- Focus on major retailers in {companyLocation}\n" +
                   "- Include online retailers (Amazon, eBay, etc.)\n" +
                   "- Include brick-and-mortar chains (Best Buy, Walmart, Target, etc.)\n" +
                   "- Include specialty retailers when relevant\n" +
                   "- Provide product URLs where possible\n\n" +
                   "Return ONLY this JSON format:\n" +
                   "{\n" +
                   "  \"products\": [\n" +
                   "    {\n" +
                   "      \"productName\": \"iPhone 15 Pro 256GB\",\n" +
                   "      \"ourPrice\": 999.00,\n" +
                   "      \"competitorPrices\": [\n" +
                   "        {\n" +
                   "          \"retailerName\": \"Amazon\",\n" +
                   "          \"price\": 989.00,\n" +
                   "          \"url\": \"https://amazon.com/iphone-15-pro-256gb\"\n" +
                   "        },\n" +
                   "        {\n" +
                   "          \"retailerName\": \"Best Buy\",\n" +
                   "          \"price\": 999.00,\n" +
                   "          \"url\": \"https://bestbuy.com/iphone-15-pro\"\n" +
                   "        }\n" +
                   "      ]\n" +
                   "    }\n" +
                   "  ]\n" +
                   "}\n\n" +
                   "HTML Content from {websiteUrl}:\n" + snippet;
        }

        private string BuildModifiedPrompt(string websiteUrl, string companyLocation, string modifiedPrompt, string pageHtml)
        {
            var snippet = pageHtml ?? string.Empty;
            if (snippet.Length > 120000) snippet = snippet.Substring(0, 120000);
            
            return $"Apply these modified parameters: {modifiedPrompt}. From the provided HTML snippet of {websiteUrl} (company location: {companyLocation}), extract products.\n" +
                   "Return ONLY valid JSON per this schema and nothing else:\n" +
                   "{\n" +
                   "  \"products\": [\n" +
                   "    { \"productName\": \"string\", \"ourPrice\": number | null, \"competitorPrices\": [{ \"retailerName\": \"string\", \"price\": number | null, \"url\": \"string\" }] }\n" +
                   "  ]\n" +
                   "}\n" +
                   "HTML:\n" + snippet;
        }

        private async Task<List<DiscoveredProduct>> CallOpenAIAsync(string prompt)
        {
            try
            {
                var apiKey = _configuration["OpenAI:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogError("OpenAI API key not configured");
                    return new List<DiscoveredProduct>();
                }

                var model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini";
                var requestBody = new
                {
                    model = model,
                    response_format = new { type = "json_object" },
                    messages = new object[]
                    {
                        new { role = "system", content = "You are a precise data extractor that returns only valid JSON." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.1,
                    max_tokens = 4000
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
                request.Headers.Add("Authorization", $"Bearer {apiKey}");
                
                var json = JsonSerializer.Serialize(requestBody);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
                var response = await _httpClient.SendAsync(request, cts.Token);

                Console.WriteLine($"=== HTTP RESPONSE RECEIVED: {response.StatusCode} ===");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("OpenAI API call failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    return new List<DiscoveredProduct>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"=== API RESPONSE: {responseContent} ===");

                var products = await ParseDiscoveredProductsAsync(responseContent);
                Console.WriteLine($"=== OPENAI API COMPLETED: {products.Count} products ===");
                
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI API");
                return new List<DiscoveredProduct>();
            }
        }

        private async Task<List<DiscoveredProduct>> ParseDiscoveredProductsAsync(string jsonContent)
        {
            try
            {
                Console.WriteLine($"=== PARSING JSON: {jsonContent.Length} chars ===");
                
                // First, try to extract the content from the API response
                var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(jsonContent);
                var messageContent = openAIResponse?.Choices?.FirstOrDefault()?.Message?.Content;
                
                Console.WriteLine($"=== MESSAGE CONTENT: {messageContent} ===");
                
                if (string.IsNullOrEmpty(messageContent))
                {
                    Console.WriteLine("=== NO MESSAGE CONTENT FOUND ===");
                    return new List<DiscoveredProduct>();
                }

                // Clean and validate JSON before parsing
                var cleanedJson = CleanJsonResponse(messageContent);
                
                if (string.IsNullOrEmpty(cleanedJson))
                {
                    Console.WriteLine("=== CLEANED JSON IS EMPTY ===");
                    return new List<DiscoveredProduct>();
                }

                // Now parse the products from the message content
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                var response = await Task.Run(() => 
                    JsonSerializer.Deserialize<OpenAIProductResponse>(cleanedJson, options));

                var products = response?.Products ?? new List<DiscoveredProduct>();
                Console.WriteLine($"=== PARSED {products.Count} PRODUCTS ===");
                
                foreach (var product in products)
                {
                    Console.WriteLine($"=== PRODUCT: {product.ProductName}, PRICE: {product.OurPrice} ===");
                }
                
                _logger.LogInformation("Successfully parsed {Count} products", products.Count);
                
                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== PARSING ERROR: {ex.Message} ===");
                Console.WriteLine($"=== RAW JSON THAT FAILED: {jsonContent} ===");
                _logger.LogError(ex, "Error parsing discovered products from JSON");
                return new List<DiscoveredProduct>();
            }
        }

        private string CleanJsonResponse(string jsonContent)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonContent))
                    return string.Empty;

                // Remove any markdown code blocks
                jsonContent = jsonContent.Trim();
                if (jsonContent.StartsWith("```json"))
                {
                    jsonContent = jsonContent.Substring(7);
                }
                if (jsonContent.StartsWith("```"))
                {
                    jsonContent = jsonContent.Substring(3);
                }
                if (jsonContent.EndsWith("```"))
                {
                    jsonContent = jsonContent.Substring(0, jsonContent.Length - 3);
                }

                jsonContent = jsonContent.Trim();

                // Find the JSON object boundaries
                var startIndex = jsonContent.IndexOf('{');
                var lastIndex = jsonContent.LastIndexOf('}');
                
                if (startIndex >= 0 && lastIndex > startIndex)
                {
                    jsonContent = jsonContent.Substring(startIndex, lastIndex - startIndex + 1);
                }

                // Try to fix common JSON issues
                jsonContent = jsonContent.Replace("\n", " ").Replace("\r", " ");
                
                // Remove any trailing commas before closing braces/brackets
                jsonContent = System.Text.RegularExpressions.Regex.Replace(jsonContent, @",(\s*[}\]])", "$1");

                Console.WriteLine($"=== CLEANED JSON: {jsonContent} ===");
                return jsonContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR CLEANING JSON: {ex.Message} ===");
                return string.Empty;
            }
        }

        private async Task<string> FetchPageForUrlAsync(string url)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
                
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                request.Headers.Add("Connection", "keep-alive");
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                
                var response = await _httpClient.SendAsync(request, cts.Token);
                var html = await response.Content.ReadAsStringAsync();
                
                return html;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch page HTML for {Url}", url);
                return string.Empty;
            }
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
        }
    }

    public class OpenAIResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        
        [JsonPropertyName("object")]
        public string? Object { get; set; }
        
        [JsonPropertyName("created")]
        public long? Created { get; set; }
        
        [JsonPropertyName("model")]
        public string? Model { get; set; }
        
        [JsonPropertyName("choices")]
        public List<OpenAIChoice>? Choices { get; set; }
        
        [JsonPropertyName("usage")]
        public OpenAIUsage? Usage { get; set; }
        
        [JsonPropertyName("service_tier")]
        public string? ServiceTier { get; set; }
        
        [JsonPropertyName("system_fingerprint")]
        public string? SystemFingerprint { get; set; }
    }

    public class OpenAIChoice
    {
        [JsonPropertyName("index")]
        public int? Index { get; set; }
        
        [JsonPropertyName("message")]
        public OpenAIMessage? Message { get; set; }
        
        [JsonPropertyName("logprobs")]
        public object? Logprobs { get; set; }
        
        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }

    public class OpenAIMessage
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }
        
        [JsonPropertyName("content")]
        public string? Content { get; set; }
        
        [JsonPropertyName("refusal")]
        public object? Refusal { get; set; }
        
        [JsonPropertyName("annotations")]
        public List<object>? Annotations { get; set; }
    }

    public class OpenAIUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int? PromptTokens { get; set; }
        
        [JsonPropertyName("completion_tokens")]
        public int? CompletionTokens { get; set; }
        
        [JsonPropertyName("total_tokens")]
        public int? TotalTokens { get; set; }
    }

    public class OpenAIProductResponse
    {
        [JsonPropertyName("products")]
        public List<DiscoveredProduct>? Products { get; set; }
    }
}