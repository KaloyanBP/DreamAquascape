using DreamAquascape.GCommon.Infrastructure;
using DreamAquascape.Services.Core.Infrastructure;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.Contest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static DreamAquascape.GCommon.ExceptionMessages;

namespace DreamAquascape.Web.Controllers
{
    /// <summary>
    /// Handles listing, viewing, creating, editing, and archiving aquascaping contests. Manages contest lifecycle and displays contest details.
    /// </summary>
    [Route("Contests")]
    public class ContestsController : BaseController
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IContestService _contestService;
        private readonly IContestQueryService _contestQueryService;
        private readonly IContestCategoryService _contestCategoryService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<HomeController> _logger;

        public ContestsController(
            IFileUploadService fileUploadService,
            IContestService contestService,
            IContestQueryService contestQueryService,
            IContestCategoryService contestCategoryService,
            IDateTimeProvider dateTimeProvider,
            ILogger<HomeController> logger)
        {
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _contestService = contestService ?? throw new ArgumentNullException(nameof(contestService));
            _contestQueryService = contestQueryService ?? throw new ArgumentNullException(nameof(contestQueryService));
            _contestCategoryService = contestCategoryService ?? throw new ArgumentNullException(nameof(contestCategoryService));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("")]
        [AllowAnonymous]
        public async Task<IActionResult> Index([FromQuery] ContestFilterViewModel? filters)
        {
            // Initialize filters if null
            filters ??= new ContestFilterViewModel() { };
            filters.ExcludeArchived = true; // Always exclude archived contests by default

            try
            {
                var result = await _contestQueryService.GetFilteredContestsAsync(filters);

                // Set up ViewBag for the partial filter component (simplified for MVP)
                ViewBag.ResultCount = result.Contests.Count();
                ViewBag.ContestStats = new
                {
                    TotalContests = result.Pagination.TotalItems
                };

                // For AJAX requests, return partial view
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_ContestGrid", result);
                }

                return View(result);
            }
            catch (Exception ex)
            {
                // Log error and return fallback
                var emptyResult = new ContestListViewModel
                {
                    Contests = new List<ContestItemViewModel>(),
                    Filters = filters,
                    Stats = new ContestStatsViewModel(),
                    Pagination = new PaginationViewModel()
                };

                return View(emptyResult);
            }
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = GetUserId();

            var contest = await _contestQueryService.GetContestDetailsAsync(id, currentUserId);

            if (contest == null)
            {
                _logger.LogError("Contest with ID {ContestId} not found", id);
                TempData["ErrorMessage"] = "Contest not found. It may have been removed or the link is invalid.";
                TempData["ErrorType"] = "warning";
                return RedirectToAction("Index");
            }

            if (contest.IsActive == false && IsAdminUser() == false)
            {
                _logger.LogWarning("Contest with ID {ContestId} is not active and user is not admin", id);
                TempData["ErrorMessage"] = "This contest is not available. It may have been deactivated, removed, or the link is invalid.";
                TempData["ErrorType"] = "warning";
                return RedirectToAction("Index");
            }

            return View(contest);
        }

        [HttpGet("Create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            // Prepopulate example dates for the contest creation form
            DateTime startDate = _dateTimeProvider.UtcNow.AddDays(10);
            DateTime votingDate = startDate.AddDays(8);
            DateTime endDate = startDate.AddDays(15);

            try
            {
                var viewModel = new CreateContestViewModel
                {
                    SubmissionStartDate = startDate,
                    VotingStartDate = votingDate,
                    VotingEndDate = endDate,
                };

                // Load available categories
                ViewBag.AvailableCategories = await _contestCategoryService.GetActiveCategoriesForSelectionAsync();

                return View(viewModel);
            }
            catch (Exception _ex)
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateContestViewModel model, IFormFile imageFile, IFormFile? prizeImageFile)
        {
            try
            {
                model.SubmissionEndDate = model.VotingStartDate; // Ensure submission end date is equal to voting start date
                if (!ValidateContestDates(model.SubmissionStartDate, model.VotingStartDate, model.VotingEndDate))
                {
                    ViewBag.AvailableCategories = await _contestCategoryService.GetActiveCategoriesForSelectionAsync();
                    return View(model);
                }

                var imageUrl = await _fileUploadService.SaveContestImageAsync(imageFile);
                if (string.IsNullOrEmpty(imageUrl))
                {
                    ModelState.AddModelError("ImageFile", "Failed to upload contest image.");
                    return View(model);
                }

                string prizeImageUrl = null;
                if (prizeImageFile != null)
                {
                    prizeImageUrl = await _fileUploadService.SavePrizeImageAsync(prizeImageFile);
                    if (string.IsNullOrEmpty(prizeImageUrl))
                    {
                        ModelState.AddModelError("PrizeImage", "Failed to upload prize image.");
                        return View(model);
                    }
                }

                model.ImageFileUrl = imageUrl;
                model.PrizeImageUrl = prizeImageUrl;


                if (!ModelState.IsValid)
                {
                    // Reload dropdown data
                    ViewBag.AvailableCategories = await _contestCategoryService.GetActiveCategoriesForSelectionAsync();
                    return View(model);
                }

                // Get current user ID
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Submit the contest
                var createdContest = await _contestService.SubmitContestAsync(model, userId);

                TempData["SuccessMessage"] = $"Contest '{createdContest.Title}' has been created successfully!";

                return RedirectToAction("Details", new { id = createdContest.Id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while creating the contest. Please try again.");
                return View(model);
            }
        }

        [HttpGet("Edit/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var contest = await _contestQueryService.GetContestForEditAsync(id);
                if (contest == null)
                {
                    TempData["ErrorMessage"] = ContestNotFoundErrorMessage;
                    return RedirectToAction("Index", "Admin");
                }

                // Load current contest categories
                var currentCategories = await _contestService.GetContestCategoriesAsync(id);
                contest.SelectedCategoryIds = currentCategories.Select(c => c.Id).ToList();

                // Load available categories
                ViewBag.AvailableCategories = await _contestCategoryService.GetActiveCategoriesForSelectionAsync();

                return View(contest);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the contest for editing.";
                return RedirectToAction("Index", "Admin");
            }
        }

        [HttpPost("Edit/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, EditContestViewModel model, IFormFile? newImageFile, IFormFile? newPrizeImageFile)
        {
            try
            {
                if (model == null || id != model.Id)
                {
                    TempData["ErrorMessage"] = "Invalid contest ID.";
                    return RedirectToAction("Index", "Admin");
                }

                if (model.IsEnded)
                {
                    TempData["ErrorMessage"] = "Cannot edit an ended contest.";
                    return RedirectToAction("Details", new { id });
                }


                if (!ModelState.IsValid)
                {
                    // Reload available categories for the view
                    ViewBag.AvailableCategories = await _contestCategoryService.GetActiveCategoriesForSelectionAsync();
                    return View(model);
                }

                // When model.InPorgress is true, ensure dates are not modified
                if (!model.InProgress && !ValidateContestDates(model.SubmissionStartDate, model.VotingStartDate, model.VotingEndDate))
                {
                    ViewBag.AvailableCategories = await _contestCategoryService.GetActiveCategoriesForSelectionAsync();
                    return View(model);
                }

                string? newImageUrl = null;
                string? newPrizeImageUrl = null;

                // Handle image removal logic
                if (model.RemoveCurrentImage)
                {
                    // If user wants to remove current image, set it to null
                    model.NewImageUrl = null;
                    newImageUrl = null;
                }

                // Handle new contest image upload
                if (newImageFile != null)
                {
                    newImageUrl = await _fileUploadService.SaveContestImageAsync(newImageFile);
                    if (string.IsNullOrEmpty(newImageUrl))
                    {
                        ModelState.AddModelError("newImageFile", "Failed to upload contest image.");
                        ViewBag.AvailableCategories = await _contestCategoryService.GetActiveCategoriesForSelectionAsync();
                        return View(model);
                    }
                    model.NewImageUrl = newImageUrl;
                }

                // Handle prize image removal logic
                if (model.RemoveCurrentPrizeImage)
                {
                    // If user wants to remove current prize image, set it to null
                    model.NewPrizeImageUrl = null;
                    newPrizeImageUrl = null;
                }


                // Handle new prize image upload
                if (newPrizeImageFile != null)
                {
                    newPrizeImageUrl = await _fileUploadService.SavePrizeImageAsync(newPrizeImageFile);
                    if (string.IsNullOrEmpty(newPrizeImageUrl))
                    {
                        ModelState.AddModelError("newPrizeImageFile", "Failed to upload prize image.");
                        ViewBag.AvailableCategories = await _contestCategoryService.GetActiveCategoriesForSelectionAsync();
                        return View(model);
                    }
                    model.NewPrizeImageUrl = newPrizeImageUrl;
                }

                model.SubmissionEndDate = model.VotingStartDate; // Ensure submission end date is equal to voting start date
                var success = await _contestService.UpdateContestAsync(model);

                if (success)
                {
                    TempData["SuccessMessage"] = $"Contest '{model.Title}' has been updated successfully!";
                    return RedirectToAction("Details", new { id = model.Id });
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update contest. Please try again.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while updating the contest. Please try again.");
                return View(model);
            }
        }


        #region Private Helper Methods

        /// <summary>
        /// Validates contest date logic and adds appropriate model errors
        /// </summary>
        private bool ValidateContestDates(DateTime submissionStartDate, DateTime votingStartDate, DateTime votingEndDate)
        {
            var isValid = true;
            var now = _dateTimeProvider.UtcNow;

            // Check if submission start is not in the past (allow some tolerance)
            if (submissionStartDate < now.AddMinutes(-1))
            {
                ModelState.AddModelError("SubmissionStartDate", "Submission start date cannot be in the past.");
                isValid = false;
            }

            // Check voting start is after submission start
            if (votingStartDate <= submissionStartDate)
            {
                ModelState.AddModelError("VotingStartDate", "Voting start date must be after submission start date.");
                isValid = false;
            }

            // Check voting end is after voting start
            if (votingEndDate <= votingStartDate)
            {
                ModelState.AddModelError("VotingEndDate", "Voting end date must be after voting start date.");
                isValid = false;
            }

            return isValid;
        }

        #endregion
    }
}
