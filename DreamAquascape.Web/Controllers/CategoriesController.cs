using DreamAquascape.Data.Models;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.AdminDashboard.ContestCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using System.Data;
using static DreamAquascape.Data.Common.EntityConstants;

namespace DreamAquascape.Web.Controllers
{
    //[Route("Admin/Categories")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : BaseController
    {
        private readonly IAdminDashboardService _dashboardService;
        private readonly IContestService _contestService;
        private readonly IContestQueryService _contestQueryService;
        private readonly IContestCategoryService _contestCategoryService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public CategoriesController(
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
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            try
            {
                var (categories, totalCount) = await _contestCategoryService.GetAllCategoriesAsync(page, pageSize);

                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contest categories");
                TempData["Error"] = "Failed to load contest categories.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ContestCategoryCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContestCategoryCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Check for uniqueness
                if (!await _contestCategoryService.IsCategoryNameUniqueAsync(model.Name))
                {
                    ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
                    return View(model);
                }

                var categoryId = await _contestCategoryService.CreateCategoryAsync(model);
                TempData["Success"] = $"Contest category '{model.Name}' created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contest category");
                TempData["Error"] = "Failed to create contest category.";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var category = await _contestCategoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    TempData["Error"] = "Contest category not found.";
                    return RedirectToAction("Index");
                }

                var model = new ContestCategoryEditViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contest category for editing");
                TempData["Error"] = "Failed to load contest category.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContestCategoryEditViewModel model)
        {
            if (id != model.Id)
            {
                TempData["Error"] = "Invalid category ID.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Check for uniqueness (excluding current category)
                if (!await _contestCategoryService.IsCategoryNameUniqueAsync(model.Name, id))
                {
                    ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");
                    return View(model);
                }

                var success = await _contestCategoryService.UpdateCategoryAsync(id, model);
                if (success)
                {
                    TempData["Success"] = $"Contest category '{model.Name}' updated successfully.";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Contest category not found.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contest category");
                TempData["Error"] = "Failed to update contest category.";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var category = await _contestCategoryService.GetCategoryWithContestsAsync(id);
                if (category == null)
                {
                    TempData["Error"] = "Contest category not found.";
                    return RedirectToAction("Index");
                }

                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contest category details");
                TempData["Error"] = "Failed to load contest category details.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _contestCategoryService.DeleteCategoryAsync(id);
                if (success)
                {
                    TempData["Success"] = "Contest category deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Contest category not found.";
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete contest category with associated contests");
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contest category");
                TempData["Error"] = "Failed to delete contest category.";
            }

            return RedirectToAction("Index");
        }
    }
}
