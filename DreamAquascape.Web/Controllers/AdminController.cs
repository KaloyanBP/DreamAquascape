using Microsoft.AspNetCore.Mvc;

namespace DreamAquascape.Web.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
