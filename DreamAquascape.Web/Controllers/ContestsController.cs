using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.Contest;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DreamAquascape.Web.Controllers
{
    /// <summary>
    /// Handles listing, viewing, creating, editing, and archiving aquascaping contests. Manages contest lifecycle and displays contest details.
    /// </summary>
    [Route("Contests")]
    public class ContestsController : Controller
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IContestService _contestService;

        public ContestsController(IFileUploadService fileUploadService, IContestService contestService)
        {
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _contestService = contestService ?? throw new ArgumentNullException(nameof(contestService));
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            // Return list of active contents
            var contests = await _contestService.GetActiveContestsAsync();
            if (contests == null || !contests.Any())
            {
                return View("NoContests");
            }
            return View(contests);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var contest = await _contestService.GetContestWithEntriesAsync(id, currentUserId);

            if (contest == null)
            {
                return NotFound();
            }

            return View(contest);
        }

        [HttpGet("Create")]
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

        public IActionResult Archive()
        {
            List<ContestItemViewModel> contests = new List<ContestItemViewModel>
            {
                new ContestItemViewModel
                {
                    Id = 2,
                    Title = "Underwater Photography Contest",
                    StartDate = new DateTime(2023, 7, 15),
                    EndDate = new DateTime(2023, 10, 15),
                    IsActive = false,
                    ImageUrl = "https://avonturia.nl/wp-content/uploads/2023/06/Aquascaping-Aquarium-985x1024.png"
                },
                new ContestItemViewModel
                {
                    Id = 3,
                    Title = "April",
                    StartDate = new DateTime(2023, 4, 15),
                    EndDate = new DateTime(2023, 4, 20),
                    IsActive = false,
                    ImageUrl = "https://avonturia.nl/wp-content/uploads/2023/06/Aquascaping-Aquarium-985x1024.png"
                },
                new ContestItemViewModel
                {
                    Id = 4,
                    Title = "May",
                    StartDate = new DateTime(2023, 5, 15),
                    EndDate = new DateTime(2023, 5, 20),
                    IsActive = false,
                    ImageUrl = "https://avonturia.nl/wp-content/uploads/2023/06/Aquascaping-Aquarium-985x1024.png"
                }
            };

            return PartialView("_Archive", contests);
        }
    }
}
