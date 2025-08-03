using Microsoft.AspNetCore.Mvc;
using DreamAquascape.Services.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using DreamAquascape.Data.Models;
using DreamAquascape.Web.ViewModels.Contest;

namespace DreamAquascape.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly IAdminDashboardService _dashboardService;
        private readonly IContestService _contestService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IAdminDashboardService dashboardService,
            IContestService contestService,
            UserManager<ApplicationUser> userManager,
            ILogger<AdminController> logger)
        {
            _dashboardService = dashboardService;
            _contestService = contestService;
            _userManager = userManager;
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

        [HttpGet]
        public async Task<IActionResult> Users(int page = 1, int pageSize = 20)
        {
            try
            {
                var users = _userManager.Users
                    .OrderByDescending(u => u.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalUsers = _userManager.Users.Count();

                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                TempData["Error"] = "Failed to load users.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Contests(ContestFilterViewModel filters)
        {
            try
            {
                // Set defaults if not provided
                if (filters == null)
                {
                    filters = new ContestFilterViewModel();
                }

                if (filters.Page <= 0)
                {
                    filters.Page = 1;
                }

                if (filters.PageSize <= 0)
                {
                    filters.PageSize = 10;
                }

                var contests = await _contestService.GetFilteredContestsAsync(filters);

                // Pass current filter values to ViewBag for pagination links
                ViewBag.Search = filters.Search;
                ViewBag.Status = (int)filters.Status;
                ViewBag.SortBy = (int)filters.SortBy;
                ViewBag.Page = filters.Page;
                ViewBag.PageSize = filters.PageSize;

                // Pass result count for the filter component
                ViewBag.ResultCount = contests.Contests.Count();

                // Pass the filters to the view for the partial (simplified for MVP)
                ViewBag.ContestStats = new
                {
                    TotalContests = contests.Pagination.TotalItems
                };

                return View(contests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contests for admin");
                TempData["Error"] = "Failed to load contests.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleContestStatus(int contestId)
        {
            try
            {
                var success = await _contestService.ToggleContestStatusAsync(contestId);
                if (success)
                {
                    TempData["Success"] = "Contest status updated successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to update contest status.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling contest status for contest {ContestId}", contestId);
                TempData["Error"] = "An error occurred while updating contest status.";
            }

            return RedirectToAction("Contests");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteContest(int contestId)
        {
            try
            {
                var success = await _contestService.DeleteContestAsync(contestId);
                if (success)
                {
                    TempData["Success"] = "Contest deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to delete contest.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contest {ContestId}", contestId);
                TempData["Error"] = "An error occurred while deleting the contest.";
            }

            return RedirectToAction("Contests");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Users");
                }

                user.LockoutEnabled = !user.LockoutEnabled;
                if (user.LockoutEnabled)
                {
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                }
                else
                {
                    user.LockoutEnd = null;
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["Success"] = $"User {user.UserName} status updated successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to update user status.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status for user {UserId}", userId);
                TempData["Error"] = "An error occurred while updating user status.";
            }

            return RedirectToAction("Users");
        }
    }
}
