using Microsoft.AspNetCore.Mvc;
using DreamAquascape.Services.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace DreamAquascape.Web.Controllers
{
    public class AdminController : BaseController
    {
        private readonly IAdminDashboardService _dashboardService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IAdminDashboardService dashboardService,
            ILogger<AdminController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardStats = await _dashboardService.GetDashboardStatsAsync();
                return View(dashboardStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                return View("Error");
            }
        }
    }
}
