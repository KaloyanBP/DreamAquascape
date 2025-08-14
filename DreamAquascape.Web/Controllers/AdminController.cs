using Microsoft.AspNetCore.Mvc;
using DreamAquascape.Services.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using DreamAquascape.Data.Models;
using DreamAquascape.Web.ViewModels.Contest;
using DreamAquascape.Web.ViewModels.AdminDashboard.ContestCategory;

namespace DreamAquascape.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly IAdminDashboardService _dashboardService;
        private readonly IContestService _contestService;
        private readonly IContestQueryService _contestQueryService;
        private readonly IContestCategoryService _contestCategoryService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IAdminDashboardService dashboardService,
            IContestService contestService,
            IContestQueryService contestQueryService,
            IContestCategoryService contestCategoryService,
            UserManager<ApplicationUser> userManager,
            ILogger<AdminController> logger)
        {
            _dashboardService = dashboardService;
            _contestService = contestService;
            _contestQueryService = contestQueryService;
            _contestCategoryService = contestCategoryService;
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

                var contests = await _contestQueryService.GetFilteredContestsAsync(filters);

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
                var success = await _contestService.ToggleContestActiveStatusAsync(contestId);
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

        //#region Contest Categories Management

        //[HttpGet]
        //public async Task<IActionResult> Categories(int page = 1, int pageSize = 10)
        //{
        //    try
        //    {
        //        var (categories, totalCount) = await _contestCategoryService.GetAllCategoriesAsync(page, pageSize);

        //        ViewBag.CurrentPage = page;
        //        ViewBag.PageSize = pageSize;
        //        ViewBag.TotalCount = totalCount;
        //        ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        //        return View(categories);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error loading contest categories");
        //        TempData["Error"] = "Failed to load contest categories.";
        //        return RedirectToAction("Index");
        //    }
        //}

        //[HttpGet]
        //public IActionResult CreateCategory()
        //{
        //    return View(new ContestCategoryCreateViewModel());
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> CreateCategory(ContestCategoryCreateViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    try
        //    {
        //        // Check for uniqueness
        //        if (!await _contestCategoryService.IsCategoryNameUniqueAsync(model.Name))
        //        {
        //            ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
        //            return View(model);
        //        }

        //        var categoryId = await _contestCategoryService.CreateCategoryAsync(model);
        //        TempData["Success"] = $"Contest category '{model.Name}' created successfully.";
        //        return RedirectToAction("Categories");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating contest category");
        //        TempData["Error"] = "Failed to create contest category.";
        //        return View(model);
        //    }
        //}

        //[HttpGet]
        //public async Task<IActionResult> EditCategory(int id)
        //{
        //    try
        //    {
        //        var category = await _contestCategoryService.GetCategoryByIdAsync(id);
        //        if (category == null)
        //        {
        //            TempData["Error"] = "Contest category not found.";
        //            return RedirectToAction("Categories");
        //        }

        //        var model = new ContestCategoryEditViewModel
        //        {
        //            Id = category.Id,
        //            Name = category.Name,
        //            Description = category.Description
        //        };

        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error loading contest category for editing");
        //        TempData["Error"] = "Failed to load contest category.";
        //        return RedirectToAction("Categories");
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditCategory(int id, ContestCategoryEditViewModel model)
        //{
        //    if (id != model.Id)
        //    {
        //        TempData["Error"] = "Invalid category ID.";
        //        return RedirectToAction("Categories");
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    try
        //    {
        //        // Check for uniqueness (excluding current category)
        //        if (!await _contestCategoryService.IsCategoryNameUniqueAsync(model.Name, id))
        //        {
        //            ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
        //            return View(model);
        //        }

        //        var success = await _contestCategoryService.UpdateCategoryAsync(id, model);
        //        if (success)
        //        {
        //            TempData["Success"] = $"Contest category '{model.Name}' updated successfully.";
        //            return RedirectToAction("Categories");
        //        }
        //        else
        //        {
        //            TempData["Error"] = "Contest category not found.";
        //            return RedirectToAction("Categories");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error updating contest category");
        //        TempData["Error"] = "Failed to update contest category.";
        //        return View(model);
        //    }
        //}

        //[HttpGet]
        //public async Task<IActionResult> CategoryDetails(int id)
        //{
        //    try
        //    {
        //        var category = await _contestCategoryService.GetCategoryWithContestsAsync(id);
        //        if (category == null)
        //        {
        //            TempData["Error"] = "Contest category not found.";
        //            return RedirectToAction("Categories");
        //        }

        //        return View(category);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error loading contest category details");
        //        TempData["Error"] = "Failed to load contest category details.";
        //        return RedirectToAction("Categories");
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteCategory(int id)
        //{
        //    try
        //    {
        //        var success = await _contestCategoryService.DeleteCategoryAsync(id);
        //        if (success)
        //        {
        //            TempData["Success"] = "Contest category deleted successfully.";
        //        }
        //        else
        //        {
        //            TempData["Error"] = "Contest category not found.";
        //        }
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        _logger.LogWarning(ex, "Cannot delete contest category with associated contests");
        //        TempData["Error"] = ex.Message;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error deleting contest category");
        //        TempData["Error"] = "Failed to delete contest category.";
        //    }

        //    return RedirectToAction("Categories");
        //}

        //#endregion
    }
}
