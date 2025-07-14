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
    }
}
