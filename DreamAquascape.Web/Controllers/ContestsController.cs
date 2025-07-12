using Microsoft.AspNetCore.Mvc;

namespace DreamAquascape.Web.Controllers
{
    public class ContestsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
