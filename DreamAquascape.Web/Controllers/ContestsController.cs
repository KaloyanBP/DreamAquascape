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

        public ContestsController(
            IFileUploadService fileUploadService,
            IContestService contestService,
            IContestQueryService contestQueryService)
        {
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _contestService = contestService ?? throw new ArgumentNullException(nameof(contestService));
            _contestQueryService = contestQueryService ?? throw new ArgumentNullException(nameof(contestQueryService));
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
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var contest = await _contestQueryService.GetContestDetailsAsync(id, currentUserId);

            if (contest == null)
            {
                return NotFound();
            }

            return View(contest);
        }

        [HttpGet("Create")]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            try
            {
                var viewModel = new CreateContestViewModel
                {
                    SubmissionStartDate = DateTime.Now,
                    SubmissionEndDate = DateTime.Now.AddDays(8),
                    VotingStartDate = DateTime.Now.AddMinutes(1),
                    VotingEndDate = DateTime.Now.AddDays(15),
                };

                return View(viewModel);
            }
            catch (Exception _ex)
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost("Create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            string title,
            string description,
            IFormFile imageFile,
            DateTime submissionStartDate,
            DateTime submissionEndDate,
            DateTime votingStartDate,
            DateTime votingEndDate,
            DateTime? resultDate,
            string prizeName,
            string prizeDescription,
            decimal? prizeMonetaryValue,
            IFormFile? prizeImageFile)
        {
            try
            {
                var imageUrl = await _fileUploadService.SaveContestImageAsync(imageFile);
                if (string.IsNullOrEmpty(imageUrl))
                {
                    ModelState.AddModelError("ImageFile", "Failed to upload contest image.");
                    return View();
                }

                string prizeImageUrl = null;
                if (prizeImageFile != null)
                {
                    prizeImageUrl = await _fileUploadService.SavePrizeImageAsync(prizeImageFile);
                    if (string.IsNullOrEmpty(prizeImageUrl))
                    {
                        ModelState.AddModelError("PrizeImage", "Failed to upload prize image.");
                        return View();
                    }
                }

                var prizeViewModel = new PrizeViewModel
                {
                    Name = prizeName,
                    Description = prizeDescription,
                    ImageUrl = prizeImageUrl
                };

                var viewModel = new CreateContestViewModel
                {
                    Title = title,
                    Description = description,
                    ImageFileUrl = imageUrl,
                    SubmissionStartDate = submissionStartDate,
                    SubmissionEndDate = submissionEndDate,
                    VotingStartDate = votingStartDate,
                    VotingEndDate = votingEndDate,
                    ResultDate = resultDate
                };

                if (!ModelState.IsValid)
                {
                    // Reload dropdown data
                    return View(viewModel);
                }

                // Get current user ID
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Submit the contest
                var createdContest = await _contestService.SubmitContestAsync(viewModel, prizeViewModel, userId);

                TempData["SuccessMessage"] = $"Contest '{createdContest.Title}' has been created successfully!";

                return RedirectToAction("Details", new { id = createdContest.Id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while creating the contest. Please try again.");
                return View();
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
                if (id != model.Id)
                {
                    TempData["ErrorMessage"] = "Invalid contest ID.";
                    return RedirectToAction("Index", "Admin");
                }

                if (!ModelState.IsValid)
                {
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
                        return View(model);
                    }
                    model.NewPrizeImageUrl = newPrizeImageUrl;
                }

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
    }
}
