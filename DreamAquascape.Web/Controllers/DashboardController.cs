using Microsoft.AspNetCore.Mvc;

namespace DreamAquascape.Web.Controllers
{
    /// <summary>
    /// Personal contest dashboard
    /// </summary>
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
