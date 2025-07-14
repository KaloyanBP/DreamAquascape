using DreamAquascape.Web.ViewModels.Contest;
using Microsoft.AspNetCore.Mvc;

namespace DreamAquascape.Web.Controllers
{
    /// <summary>
    /// Handles listing, viewing, creating, editing, and archiving aquascaping contests. Manages contest lifecycle and displays contest details.
    /// </summary>
    public class ContestsController : Controller
    {
        public IActionResult Index()
        {
            List<ContestItemViewModel> contests = new List<ContestItemViewModel>
            {
                new ContestItemViewModel
                {
                    Id = 1,
                    Title = "Aquascape Contest 2023",
                    StartDate = new DateTime(2023, 6, 1),
                    EndDate = new DateTime(2023, 12, 31),
                    IsActive = true,
                    ImageUrl = "https://charterhouse-aquatics.com/cdn/shop/articles/aquascaping_72.jpg?v=1719156854"
                },
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
            return View(contests.OrderByDescending(x => x.IsActive));
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            // Simulate fetching contest details from a database or service
            var contest = new ContestDetailsViewModel
            {
                Id = id,
                Title = "Aquascape Contest 2023",
                Description = "Showcase your best aquascaping skills!",
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
                        ImageUrl = "https://www.2hraquarist.com/cdn/shop/articles/chonlatee_jaturonrusmee2018_1000x.jpg?v=1567494592",
                        VoteCount = 10
                    },
                    new ContestEntryViewModel
                    {
                        Id = 2,
                        UserName = "NatureLover",
                        Description = "Inspired by nature.",
                        ImageUrl = "https://www.2hraquarist.com/cdn/shop/articles/Fernando_Ferreira2_1000x.jpg?v=1582643596",
                        VoteCount = 5
                    },
                    new ContestEntryViewModel
                    {
                        Id = 3,
                        UserName = "Avatar Inspired world",
                        Description = "Inspired by movie.",
                        ImageUrl = "https://www.2hraquarist.com/cdn/shop/articles/Fernando_Ferreira2_1000x.jpg?v=1582643596",
                        VoteCount = 5
                    }
                },
                CanVote = true,
                CanSubmitEntry = true
            };

            return View(contest);
        }
    }
}
