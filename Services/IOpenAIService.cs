using PricePulse.ViewModels;

namespace PricePulse.Services
{
    public interface IOpenAIService
    {
        Task<List<DiscoveredProduct>> DiscoverProductsAsync(string websiteUrl, string companyLocation);
        Task<List<DiscoveredProduct>> DiscoverProductsWithModifiedPromptAsync(string websiteUrl, string companyLocation, string modifiedPrompt);
    }
}
