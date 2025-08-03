using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels;
using DreamAquascape.Web.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DreamAquascape.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAdminDashboardService _dashboardService;

        public HomeController(
            IAdminDashboardService dashboardService,
            ILogger<HomeController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var dashboardStat = await _dashboardService.GetDashboardStatsAsync();
            var model = new HomeIndexViewModel
            {
                ActiveContests = dashboardStat.ActiveContests,
                TotalEntries = dashboardStat.TotalEntries,
                TotalUsers = dashboardStat.TotalUsers,
                TotalVotes = dashboardStat.TotalVotes,
            };
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
