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

        public async Task<string> GetCompletionAsync(string prompt)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("OpenAI API key not configured");
            }

            var requestBody = new
            {
                model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                max_tokens = 4000,
                temperature = 0.1
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"OpenAI API call failed: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObj = JsonSerializer.Deserialize<OpenAICompletionResponse>(responseContent);
            
            return responseObj?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
        }

        public async Task<List<DiscoveredCompetitor>> DiscoverCompetitorsAsync(string websiteUrl, string companyLocation)
        {
            // Normalize URL to ensure consistent format
            websiteUrl = NormalizeUrl(websiteUrl);
            
            var cacheKey = $"competitors_{websiteUrl}_{companyLocation}";
            
            if (_cache.TryGetValue(cacheKey, out List<DiscoveredCompetitor>? cachedCompetitors))
            {
                Console.WriteLine($"=== CACHE HIT for competitors {websiteUrl} ===");
                return cachedCompetitors ?? new List<DiscoveredCompetitor>();
            }

            // Cost-saving: Check if we've made too many requests recently
            var recentRequestsKey = $"recent_requests_{DateTime.UtcNow:yyyy-MM-dd}";
            if (_cache.TryGetValue(recentRequestsKey, out int requestCount) && requestCount > 10)
            {
                Console.WriteLine($"=== TOO MANY REQUESTS TODAY ({requestCount}), USING FALLBACK ===");
                return CreateFallbackCompetitors(websiteUrl);
            }

            await _semaphore.WaitAsync();
            try
            {
                Console.WriteLine($"=== ACQUIRED SEMAPHORE for competitors {websiteUrl} ===");
                
                var html = await FetchPageForUrlAsync(websiteUrl);
                Console.WriteLine($"=== HTML LENGTH: {html.Length} ===");

                // Limit HTML content to reduce token usage and costs
                var limitedHtml = LimitHtmlContent(html, 50000); // Limit to 50KB
                Console.WriteLine($"=== LIMITED HTML LENGTH: {limitedHtml.Length} ===");

                var prompt = BuildCompetitorDiscoveryPrompt(websiteUrl, companyLocation, limitedHtml);
                Console.WriteLine($"=== PROMPT LENGTH: {prompt.Length} ===");
                
                // Check if prompt is too large (over 150K characters to be safe)
                if (prompt.Length > 150000)
                {
                    Console.WriteLine($"=== PROMPT TOO LARGE ({prompt.Length} chars), USING FALLBACK ===");
                    return CreateFallbackCompetitors(websiteUrl);
                }

                var competitors = await CallOpenAIForCompetitorsAsync(prompt, websiteUrl);
                
                // Track daily request count for cost control
                var todayKey = $"recent_requests_{DateTime.UtcNow:yyyy-MM-dd}";
                var currentCount = _cache.Get<int>(todayKey);
                _cache.Set(todayKey, currentCount + 1, TimeSpan.FromDays(1));
                
                _cache.Set(cacheKey, competitors, TimeSpan.FromHours(6));
                Console.WriteLine($"=== CACHED {competitors.Count} competitors for {websiteUrl} ===");
                
                return competitors;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private string BuildCompetitorDiscoveryPrompt(string websiteUrl, string companyLocation, string html)
        {
            var promptTemplate = File.ReadAllText("Services/CompetitorDiscoveryPrompt.txt");
            var prompt = promptTemplate
                .Replace("$URL", websiteUrl)
                .Replace("$company location", companyLocation);

            return $"{prompt}\n\nWebsite HTML Content:\n{html}";
        }

        private async Task<List<DiscoveredCompetitor>> CallOpenAIForCompetitorsAsync(string prompt, string websiteUrl)
        {
            var requestBody = new
            {
                model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                max_tokens = 2000, // Reduced to save costs
                temperature = 0.1
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            var apiKey = _configuration["OpenAI:ApiKey"];
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                
                // If it's a rate limit error, use fallback instead of throwing
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    Console.WriteLine($"=== RATE LIMIT HIT, USING FALLBACK COMPETITORS ===");
                    return CreateFallbackCompetitors(websiteUrl);
                }
                
                throw new HttpRequestException($"OpenAI API call failed: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObj = JsonSerializer.Deserialize<OpenAICompletionResponse>(responseContent);
            
            if (responseObj?.Choices?.FirstOrDefault()?.Message?.Content != null)
            {
                var messageContent = responseObj.Choices.First().Message.Content;
                
                // Clean the response - remove markdown code blocks if present
                if (messageContent.Contains("```json"))
                {
                    var startIndex = messageContent.IndexOf("```json") + 7;
                    var endIndex = messageContent.LastIndexOf("```");
                    if (endIndex > startIndex)
                    {
                        messageContent = messageContent.Substring(startIndex, endIndex - startIndex).Trim();
                    }
                }
                else if (messageContent.Contains("```"))
                {
                    var startIndex = messageContent.IndexOf("```") + 3;
                    var endIndex = messageContent.LastIndexOf("```");
                    if (endIndex > startIndex)
                    {
                        messageContent = messageContent.Substring(startIndex, endIndex - startIndex).Trim();
                    }
                }
                
                try
                {
                    Console.WriteLine($"=== RAW AI RESPONSE: {messageContent} ===");
                    var competitorResponse = JsonSerializer.Deserialize<CompetitorDiscoveryResponse>(messageContent);
                    var competitors = competitorResponse?.CompetitorAnalysis?.Competitors ?? new List<DiscoveredCompetitor>();
                    Console.WriteLine($"=== PARSED {competitors.Count} COMPETITORS ===");
                    return competitors;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to parse competitor discovery JSON. Content: {Content}", messageContent);
                    Console.WriteLine($"=== JSON PARSE ERROR: {ex.Message} ===");
                    Console.WriteLine($"=== FALLING BACK TO HARDCODED COMPETITORS ===");
                    
                    // Fallback: Create some basic competitors based on the website
                    return CreateFallbackCompetitors(websiteUrl);
                }
            }
            
            return new List<DiscoveredCompetitor>();
        }

        private List<DiscoveredCompetitor> CreateFallbackCompetitors(string websiteUrl)
        {
            var competitors = new List<DiscoveredCompetitor>();
            
            // Create some basic competitors based on common patterns
            if (websiteUrl.Contains("apple.com"))
            {
                competitors.AddRange(new[]
                {
                    new DiscoveredCompetitor
                    {
                        CompanyName = "Samsung",
                        WebsiteUrl = "https://www.samsung.com",
                        Description = "South Korean multinational electronics company",
                        KeyProductsServices = "Galaxy smartphones, tablets, TVs, home appliances",
                        CompetitionReason = "Direct competitor in smartphones, tablets, and consumer electronics",
                        CompanyType = "enterprise",
                        MarketPosition = "leader"
                    },
                    new DiscoveredCompetitor
                    {
                        CompanyName = "Google",
                        WebsiteUrl = "https://www.google.com",
                        Description = "American multinational technology company",
                        KeyProductsServices = "Android OS, Pixel phones, Google Cloud, AI services",
                        CompetitionReason = "Competes in mobile operating systems, cloud services, and AI",
                        CompanyType = "enterprise",
                        MarketPosition = "leader"
                    },
                    new DiscoveredCompetitor
                    {
                        CompanyName = "Microsoft",
                        WebsiteUrl = "https://www.microsoft.com",
                        Description = "American multinational technology corporation",
                        KeyProductsServices = "Windows OS, Office 365, Azure cloud, Surface devices",
                        CompetitionReason = "Competes in software, cloud services, and productivity tools",
                        CompanyType = "enterprise",
                        MarketPosition = "leader"
                    },
                    new DiscoveredCompetitor
                    {
                        CompanyName = "Sony",
                        WebsiteUrl = "https://www.sony.com",
                        Description = "Japanese multinational conglomerate",
                        KeyProductsServices = "PlayStation, Xperia phones, cameras, audio equipment",
                        CompetitionReason = "Competes in consumer electronics, gaming, and entertainment",
                        CompanyType = "enterprise",
                        MarketPosition = "leader"
                    },
                    new DiscoveredCompetitor
                    {
                        CompanyName = "Amazon",
                        WebsiteUrl = "https://www.amazon.com",
                        Description = "American multinational technology company",
                        KeyProductsServices = "Echo devices, Fire tablets, AWS cloud, e-commerce",
                        CompetitionReason = "Competes in smart home devices, cloud services, and e-commerce",
                        CompanyType = "enterprise",
                        MarketPosition = "leader"
                    }
                });
            }
            else
            {
                // Generic competitors for other websites
                competitors.AddRange(new[]
                {
                    new DiscoveredCompetitor
                    {
                        CompanyName = "Competitor 1",
                        WebsiteUrl = "https://competitor1.com",
                        Description = "Direct competitor in the same market",
                        KeyProductsServices = "Similar products and services",
                        CompetitionReason = "Similar products and target market",
                        CompanyType = "enterprise",
                        MarketPosition = "challenger"
                    },
                    new DiscoveredCompetitor
                    {
                        CompanyName = "Competitor 2",
                        WebsiteUrl = "https://competitor2.com",
                        Description = "Another competitor in the industry",
                        KeyProductsServices = "Competing products and solutions",
                        CompetitionReason = "Competes for the same customer base",
                        CompanyType = "mid-size",
                        MarketPosition = "follower"
                    }
                });
            }
            
            return competitors;
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

        private string LimitHtmlContent(string html, int maxLength)
        {
            if (string.IsNullOrEmpty(html) || html.Length <= maxLength)
                return html;

            // Try to find a good breaking point (end of a tag or sentence)
            var limited = html.Substring(0, maxLength);
            var lastTag = limited.LastIndexOf('>');
            var lastSentence = limited.LastIndexOf('.');
            var lastSpace = limited.LastIndexOf(' ');

            // Use the best breaking point
            var breakPoint = Math.Max(lastTag, Math.Max(lastSentence, lastSpace));
            if (breakPoint > maxLength * 0.8) // Only if we don't lose too much content
            {
                return html.Substring(0, breakPoint + 1);
            }

            return limited + "...";
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

    public class OpenAICompletionResponse
    {
        [JsonPropertyName("choices")]
        public List<OpenAIChoice>? Choices { get; set; }
    }

    public class OpenAICompetitorResponse
    {
        [JsonPropertyName("choices")]
        public List<OpenAIChoice>? Choices { get; set; }
    }

    public class CompetitorDiscoveryResponse
    {
        [JsonPropertyName("competitor_analysis")]
        public CompetitorDiscoveryData? CompetitorAnalysis { get; set; }
    }

    public class CompetitorDiscoveryData
    {
        [JsonPropertyName("total_competitors_found")]
        public int TotalCompetitorsFound { get; set; }
        
        [JsonPropertyName("competitors")]
        public List<DiscoveredCompetitor>? Competitors { get; set; }
    }

}