using DreamAquascape.Services.Common.Exceptions;
using DreamAquascape.Services.Core;
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
    public class ContestEntriesController : Controller
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IContestService _contestService;

        public ContestEntriesController(IFileUploadService fileUploadService, IContestService contestService)
        { 
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _contestService = contestService ?? throw new ArgumentNullException(nameof(contestService));
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
            var entryDetails = await _contestService.GetContestEntryDetailsAsync(contestId, entryId, currentUserId);

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
            // Handle file upload
            var imageUrls = await _fileUploadService.SaveMultipleEntryImagesAsync(imageFiles);

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;

                var model = new CreateContestEntryViewModel 
                { 
                    ContestId = contestId,
                    Title = title,
                    Description = description,
                    EntryImages = imageUrls
                };
                var entry = await _contestService.SubmitEntryAsync(model, userId, userName);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }

            return RedirectToAction("Index", "Contests");
        }
    }
}
