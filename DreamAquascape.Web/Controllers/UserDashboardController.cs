using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.UserDashboard;
using Microsoft.AspNetCore.Mvc;
using static DreamAquascape.Data.Common.EntityConstants;

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
                UserName = GetUserName() ?? "AquaFan",
                QuickStats = await _userDashboardService.GetUserQuickStatsAsync(GetUserId()!),
                ActiveContests = await _userDashboardService.GetUserActiveContestsAsync(GetUserId()!),
                MySubmissions = await _userDashboardService.GetUserSubmissionsAsync(GetUserId()!),
                VotingHistory = await _userDashboardService.GetUserVotingHistoryAsync(GetUserId()!),
            };

            return View(model);
        }

        [Route("votes")]
        public async Task<IActionResult> Votes(int page = 1, int pageSize = 20)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var votingHistory = await _userDashboardService.GetUserVotingHistoryAsync(userId, page, pageSize);
            return View(votingHistory);
        }

        [Route("entries")]
        public async Task<IActionResult> Entries(int page = 1, int pageSize = 20)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var userSubmissions = await _userDashboardService.GetUserSubmissionsAsync(userId, page, pageSize);
            return View(userSubmissions);
        }
    }
}
