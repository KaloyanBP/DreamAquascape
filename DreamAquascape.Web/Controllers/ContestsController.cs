using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.Contest;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DreamAquascape.Web.Controllers
{
    /// <summary>
    /// Handles listing, viewing, creating, editing, and archiving aquascaping contests. Manages contest lifecycle and displays contest details.
    /// </summary>
    public class ContestsController : Controller
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IContestService _contestService;

        public ContestsController(IFileUploadService fileUploadService, IContestService contestService)
        {
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _contestService = contestService ?? throw new ArgumentNullException(nameof(contestService));
        }

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

        [HttpGet]
        public IActionResult Details(int id)
        {
            // Simulate fetching contest details from a database or service
            var contest = new ContestDetailsViewModel
            {
                Id = id,
                Title = "Aquascape Contest 2023",
                Description = "Showcase your best aquascaping skills! In the event of a tie, the entry submitted earliest will be declared the winner.",
                StartDate = new DateTime(2023, 6, 1),
                EndDate = new DateTime(2023, 12, 31),
                IsActive = true,
                Prize = new PrizeViewModel
                {
                    Name = "Aquascaping Kit",
                    Description = "Includes plants, substrate, and tools."
                },
                Entries = new List<ContestEntryViewModel>
                {
                    new ContestEntryViewModel
                    {
                        Id = 1,
                        UserName = "Aquascaper123",
                        Description = "My first aquascape!",
                        EntryImages = new List<string>() { 
                            "https://www.2hraquarist.com/cdn/shop/articles/chonlatee_jaturonrusmee2018_1000x.jpg?v=1567494592",
                            "https://www.2hraquarist.com/cdn/shop/articles/Fernando_Ferreira2_1000x.jpg?v=1582643596",
                            "https://marcusfishtanks.com/cdn/shop/articles/Cover_6cbc74d7-cb0c-4a26-8b52-5ffaabbf5235.jpg?v=1733482162"
                            },
                        VoteCount = 10
                    },
                    new ContestEntryViewModel
                    {
                        Id = 2,
                        UserName = "NatureLover",
                        Description = "Inspired by nature.",
                        EntryImages = new List<string>() {
                            "https://www.plantedwell.com/wp-content/uploads/2021/01/bonsai-tank-aquascaping.jpg.webp",
                            "https://www.plantedwell.com/wp-content/uploads/2021/01/fluval-edge-bonsai-mini-aquascape.jpg.webp",
                            "https://www.plantedwell.com/wp-content/uploads/2021/01/iwagumi-mini-aquascape.jpg.webp"
                            },
                        VoteCount = 5
                    },
                    new ContestEntryViewModel
                    {
                        Id = 3,
                        UserName = "Avatar Inspired world",
                        Description = "Inspired by movie.",
                        EntryImages = new List<string>() {
                            "https://www.plantedwell.com/wp-content/uploads/2021/01/nano-bonsai-aquascape.jpg.webp",
                            "https://www.plantedwell.com/wp-content/uploads/2021/01/small-bonsai-driftwood-aquascaping.jpg.webp",
                            },
                        VoteCount = 5
                    }
                },
                CanVote = true,
                CanSubmitEntry = true
            };

            return View(contest);
        }

        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                var viewModel = new CreateContestViewModel
                {
                    SubmissionStartDate = DateTime.Now.AddDays(1),
                    SubmissionEndDate = DateTime.Now.AddDays(8),
                    VotingStartDate = DateTime.Now.AddDays(8).AddHours(1),
                    VotingEndDate = DateTime.Now.AddDays(15),
                };

                return View(viewModel);
            }
            catch (Exception _ex)
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            string title,
            string description,
            IFormFile imageFile,
            DateTime SubmissionStartDate,
            DateTime SubmissionEndDate,
            DateTime VotingStartDate,
            DateTime VotingEndDate,
            DateTime? ResultDate)
        {
            try
            {
                var imageUrl = await _fileUploadService.SaveContestImageAsync(imageFile);

                var viewModel = new CreateContestViewModel
                {
                    Title = title,
                    Description = description,
                    ImageFileUrl = imageUrl,
                    SubmissionStartDate = SubmissionStartDate,
                    SubmissionEndDate = SubmissionEndDate,
                    VotingStartDate = VotingStartDate,
                    VotingEndDate = VotingEndDate,
                    ResultDate = ResultDate
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
                var createdContest = await _contestService.SubmitContestAsync(viewModel, userId);

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
