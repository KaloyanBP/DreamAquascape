using DreamAquascape.Web.ViewModels.Contest;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace DreamAquascape.Web.Controllers
{
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
            return View(contests.OrderByDescending( x => x.IsActive));
        }
    }
}
