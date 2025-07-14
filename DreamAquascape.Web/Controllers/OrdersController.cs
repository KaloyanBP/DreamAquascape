using Microsoft.AspNetCore.Mvc;

namespace DreamAquascape.Web.Controllers
{
    /// <summary>
    /// Manages the shopping cart, checkout process, and order history. Handles user purchases and order tracking
    /// </summary>
    public class OrdersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
