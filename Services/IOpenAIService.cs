using PricePulse.ViewModels;

namespace PricePulse.Services
{
    public interface IOpenAIService
    {
        Task<List<DiscoveredProduct>> DiscoverProductsAsync(string websiteUrl, string companyLocation);
        Task<List<DiscoveredProduct>> DiscoverProductsWithModifiedPromptAsync(string websiteUrl, string companyLocation, string modifiedPrompt);
        Task<List<DiscoveredCompetitor>> DiscoverCompetitorsAsync(string websiteUrl, string companyLocation);
        Task<string> GetCompletionAsync(string prompt);
    }
}
