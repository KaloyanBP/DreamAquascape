using Microsoft.AspNetCore.Mvc;

namespace DreamAquascape.Web.Controllers
{
    /// <summary>
    /// Manages contest prizes, including listing available prizes, viewing prize details, and assigning prizes to contest winners.
    /// </summary>
    public class PricesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
