using DreamAquascape.Web.ViewModels.ContestEntry;
using Microsoft.AspNetCore.Mvc;

namespace DreamAquascape.Web.Controllers
{
    /// <summary>
    /// Manages user submissions to contests, including uploading images and viewing entry details. Allows users to participate in active contests.
    /// </summary>
    public class ContestEntriesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create(int contestId)
        {
            var model = new CreateContestViewModel { ContestId = contestId };
            return View(model);
        }

        [HttpPost]
        public IActionResult Create(int contestId, string title, string description, IFormFile[] imageFiles)
        {
            //// Handle file upload
            //string imageUrl = null;
            //if (imageFile != null && imageFile.Length > 0)
            //{
            //    var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
            //    var filePath = Path.Combine(_env.WebRootPath, "images/entries", fileName);
            //    using (var stream = new FileStream(filePath, FileMode.Create))
            //    {
            //        imageFile.CopyTo(stream);
            //    }
            //    imageUrl = "/images/entries/" + fileName;
            //}

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
