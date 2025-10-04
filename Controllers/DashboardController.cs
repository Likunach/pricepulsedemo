using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using PricePulse.DAOs.Interfaces;
using PricePulse.ViewModels;

namespace PricePulse.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IOwnProductDAO _ownProductDAO;
        private readonly IPriceDAO _priceDAO;
        private readonly ICompetitorProductDAO _competitorProductDAO;
        private readonly IUserDAO _userDAO;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IOwnProductDAO ownProductDAO,
            IPriceDAO priceDAO,
            ICompetitorProductDAO competitorProductDAO,
            IUserDAO userDAO,
            ILogger<DashboardController> logger)
        {
            _ownProductDAO = ownProductDAO;
            _priceDAO = priceDAO;
            _competitorProductDAO = competitorProductDAO;
            _userDAO = userDAO;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
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

                var model = new DashboardViewModel
                {
                    TotalProductsTracked = await _ownProductDAO.CountAsync(op => op.UserId == user.UserId),
                    ActiveAlerts = await GetActiveAlertsCount(user.UserId),
                    AveragePriceChange = await GetAveragePriceChange(user.UserId),
                    RecentPriceChanges = await GetRecentPriceChanges(user.UserId),
                    PriceAlerts = await GetPriceAlerts(user.UserId)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard for user");
                TempData["Error"] = "An error occurred while loading the dashboard. Please try again.";
                return View(new DashboardViewModel());
            }
        }

        private Task<int> GetActiveAlertsCount(int userId)
        {
            // TODO: Implement actual alert counting logic
            // For now, return a mock value
            return Task.FromResult(32);
        }

        private Task<decimal> GetAveragePriceChange(int userId)
        {
            // TODO: Implement actual price change calculation
            // For now, return a mock value
            return Task.FromResult(-5.2m);
        }

        private Task<List<PriceChangeViewModel>> GetRecentPriceChanges(int userId)
        {
            // TODO: Implement actual recent price changes logic
            // For now, return mock data
            return Task.FromResult(new List<PriceChangeViewModel>
            {
                new PriceChangeViewModel { ProductName = "Smart Watch", CurrentPrice = 199.99m, PreviousPrice = 219.99m, ChangeDate = DateTime.Now.AddDays(-1) },
                new PriceChangeViewModel { ProductName = "Wireless Headphones", CurrentPrice = 149.99m, PreviousPrice = 159.99m, ChangeDate = DateTime.Now.AddDays(-2) },
                new PriceChangeViewModel { ProductName = "Gaming Mouse", CurrentPrice = 79.99m, PreviousPrice = 89.99m, ChangeDate = DateTime.Now.AddDays(-3) },
                new PriceChangeViewModel { ProductName = "Portable Charger", CurrentPrice = 29.99m, PreviousPrice = 34.99m, ChangeDate = DateTime.Now.AddDays(-4) },
                new PriceChangeViewModel { ProductName = "Bluetooth Speaker", CurrentPrice = 59.99m, PreviousPrice = 64.99m, ChangeDate = DateTime.Now.AddDays(-5) }
            });
        }

        private Task<List<PriceAlertViewModel>> GetPriceAlerts(int userId)
        {
            // TODO: Implement actual price alerts logic
            // For now, return mock data
            return Task.FromResult(new List<PriceAlertViewModel>
            {
                new PriceAlertViewModel { ProductName = "Smart Watch", TargetPrice = 180.00m, CurrentPrice = 199.99m, Status = "Active" },
                new PriceAlertViewModel { ProductName = "Wireless Headphones", TargetPrice = 140.00m, CurrentPrice = 149.99m, Status = "Active" },
                new PriceAlertViewModel { ProductName = "Gaming Mouse", TargetPrice = 70.00m, CurrentPrice = 79.99m, Status = "Active" }
            });
        }
    }
}
