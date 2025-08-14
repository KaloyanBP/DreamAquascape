using DreamAquascape.Services.Common.Exceptions;
using DreamAquascape.Services.Core;
using DreamAquascape.Services.Core.Infrastructure;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.ContestEntry;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DreamAquascape.Web.Controllers
{
    /// <summary>
    /// Manages user submissions to contests, including uploading images and viewing entry details. Allows users to participate in active contests.
    /// </summary>
    [Route("Contests/{contestId:int}/Entries")]
    public class ContestEntriesController : BaseController
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IContestEntryService _contestEntryService;
        private readonly IContestEntryQueryService _contestEntryQueryService;

        public ContestEntriesController(
            IFileUploadService fileUploadService,
            IContestEntryService contestEntryService,
            IContestEntryQueryService contestEntryQueryService)
        {
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _contestEntryService = contestEntryService ?? throw new ArgumentNullException(nameof(contestEntryService));
            _contestEntryQueryService = contestEntryQueryService ?? throw new ArgumentNullException(nameof(contestEntryQueryService));
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("{entryId:int}")]
        public async Task<IActionResult> Details(int contestId, int entryId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var entryDetails = await _contestEntryQueryService.GetContestEntryDetailsAsync(contestId, entryId, currentUserId);

            if (entryDetails == null)
            {
                return NotFound("Contest entry not found.");
            }

            return View(entryDetails);
        }

        [HttpGet("Create")]
        public IActionResult Create(int contestId)
        {
            var model = new CreateContestEntryViewModel { ContestId = contestId };
            return View(model);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(int contestId, string title, string description, IFormFile[] imageFiles)
        {
            var model = new CreateContestEntryViewModel
            {
                ContestId = contestId,
                Title = title,
                Description = description
            };

            try
            {
                // Handle file upload
                var imageUrls = await _fileUploadService.SaveMultipleEntryImagesAsync(imageFiles);
                model.EntryImages = imageUrls;

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;

                var entry = await _contestEntryService.SubmitEntryAsync(model, userId, userName);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }

            return RedirectToAction("Index", "Contests");
        }

        [HttpGet("{entryId:int}/Edit")]
        public async Task<IActionResult> Edit(int contestId, int entryId)
        {
            var currentUserId = GetUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }

            var model = await _contestEntryQueryService.GetContestEntryForEditAsync(contestId, entryId, currentUserId);
            if (model == null)
            {
                return NotFound("Contest entry not found or you don't have permission to edit it.");
            }

            if (!model.CanEdit)
            {
                TempData["ErrorMessage"] = "This contest entry can no longer be edited. The submission period has ended.";
                return RedirectToAction("Details", new { contestId, entryId });
            }

            return View(model);
        }

        [HttpPost("{entryId:int}/Edit")]
        public async Task<IActionResult> Edit(int contestId, int entryId, EditContestEntryViewModel model, IFormFile[]? newImageFiles)
        {
            var currentUserId = GetUserId();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                // Reload the model data if validation fails
                var reloadedModel = await _contestEntryQueryService.GetContestEntryForEditAsync(contestId, entryId, currentUserId);
                if (reloadedModel != null)
                {
                    model.ExistingImages = reloadedModel.ExistingImages;
                    model.ContestTitle = reloadedModel.ContestTitle;
                    model.SubmissionEndDate = reloadedModel.SubmissionEndDate;
                    model.CanEdit = reloadedModel.CanEdit;
                }
                return View(model);
            }

            try
            {
                // Handle new image uploads
                if (newImageFiles?.Any() == true)
                {
                    var newImageUrls = await _fileUploadService.SaveMultipleEntryImagesAsync(newImageFiles);
                    model.NewImages = newImageUrls.ToList();
                }

                var success = await _contestEntryService.UpdateContestEntryAsync(model, currentUserId);
                if (success)
                {
                    TempData["SuccessMessage"] = "Your entry has been updated successfully!";
                    return RedirectToAction("Details", new { contestId, entryId });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update your entry. Please try again.";
                    return RedirectToAction("Edit", new { contestId, entryId });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while updating your entry. Please try again.");

                // Reload the model data
                var reloadedModel = await _contestEntryQueryService.GetContestEntryForEditAsync(contestId, entryId, currentUserId);
                if (reloadedModel != null)
                {
                    model.ExistingImages = reloadedModel.ExistingImages;
                    model.ContestTitle = reloadedModel.ContestTitle;
                    model.SubmissionEndDate = reloadedModel.SubmissionEndDate;
                    model.CanEdit = reloadedModel.CanEdit;
                }

                return View(model);
            }
        }
    }
}
