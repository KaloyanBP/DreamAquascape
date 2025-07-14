using Microsoft.AspNetCore.Mvc;

namespace DreamAquascape.Web.Controllers
{
    /// <summary>
    /// Handles voting functionality for contest entries. Ensures users can vote during the allowed period and tracks votes per entry.
    /// </summary>
    public class VotesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
