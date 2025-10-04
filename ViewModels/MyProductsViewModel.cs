using PricePulse.Models;

namespace PricePulse.ViewModels
{
    public class MyProductsViewModel
    {
        public List<OwnProduct> OwnProducts { get; set; } = new();
        public string CompanyName { get; set; } = string.Empty;
        public int TotalProducts => OwnProducts.Count;
    }
}
