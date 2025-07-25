using Microsoft.AspNetCore.Mvc;
using DreamAquascape.Web.ViewModels.UserDashboard;
using DreamAquascape.Services.Core.Interfaces;

namespace DreamAquascape.Web.Controllers
{
    /// <summary>
    /// Personal contest dashboard
    /// </summary>
    [Route("user")]
    public class UserDashboardController : BaseController
    {
        private readonly IUserDashboardService _userDashboardService;

        public UserDashboardController(IUserDashboardService userDashboardService)
        {
            _userDashboardService = userDashboardService;
        }

        [Route("dashboard")]
        public async Task<IActionResult> Index()
        {
            var model = new UserDashboardViewModel
            {
                UserName = "AquaFan",
                QuickStats = await _userDashboardService.GetUserQuickStatsAsync(GetUserId()!),
                ActiveContests = await _userDashboardService.GetUserActiveContestsAsync(GetUserId()!),
                MySubmissions = await _userDashboardService.GetUserSubmissionsAsync(GetUserId()!),
                VotingHistory = await _userDashboardService.GetUserVotingHistoryAsync(GetUserId()!),
            };

            return View(model);
        }

    }
}
