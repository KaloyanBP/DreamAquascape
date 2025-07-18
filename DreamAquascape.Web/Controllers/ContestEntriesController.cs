using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.ContestEntry;
using Microsoft.AspNetCore.Mvc;

namespace DreamAquascape.Web.Controllers
{
    /// <summary>
    /// Manages user submissions to contests, including uploading images and viewing entry details. Allows users to participate in active contests.
    /// </summary>
    public class ContestEntriesController : Controller
    {
        private readonly IFileUploadService _fileUploadService;
        public ContestEntriesController(IFileUploadService fileUploadService)
        { 
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create(int contestId)
        {
            var model = new CreateContestEntryViewModel { ContestId = contestId };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int contestId, string title, string description, IFormFile[] imageFiles)
        {
            // Handle file upload
            var imageUrls = await _fileUploadService.SaveMultipleEntryImagesAsync(imageFiles);

            //var model = new CreateContestEntryViewModel
            //{
            //    Title = title,
            //    Description = description,
            //    ContestId = contestId,
            //    ImageUrl = imageUrl
            //};

            // ToDO: Save model, redirect, etc.
            return RedirectToAction("Index", "Contests");
        }
    }
}
