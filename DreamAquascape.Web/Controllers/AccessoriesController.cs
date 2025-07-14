using DreamAquascape.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DreamAquascape.Web.Controllers
{
    /// <summary>
    /// Handles the shopping section: lists aquascaping accessories, displays product details, and manages product creation/editing for admins.
    /// </summary>
    public class AccessoriesController : Controller
    {
        public IActionResult Index()
        {
            var accessories = new List<AccessoryViewModel>
            {
                new AccessoryViewModel("Aquarium Filter", "https://m.media-amazon.com/images/I/71SBubjVqJL._AC_SX679_.jpg", "High Efficiency Filter", "Keeps your aquarium water clean and clear.", 49.99m, 5),
                new AccessoryViewModel("LED Aquarium Light", "https://m.media-amazon.com/images/I/71mcQryH1fL._AC_SX679_.jpg", "Bright LED Light", "Enhances the beauty of your aquarium with vibrant lighting.", 29.99m, 10),
                new AccessoryViewModel("Aquarium Heater", "https://m.media-amazon.com/images/I/71UvI8Et-OL.__AC_SX300_SY300_QL70_ML2_.jpg", "Reliable Heater", "Maintains optimal water temperature for your fish.", 39.99m, 3),
                new AccessoryViewModel("Air Pump", "https://m.media-amazon.com/images/I/611Fh6VDP-L._AC_SX679_.jpg", "Silent Air Pump", "Provides essential oxygen for your aquatic life.", 19.99m, 7),
                new AccessoryViewModel("Aquarium Thermometer", "https://m.media-amazon.com/images/I/71PHJlJwpZL._AC_SX679_.jpg", "Digital Thermometer", "Accurately monitors water temperature.", 9.99m, 15),
                new AccessoryViewModel("Gravel Cleaner", "https://m.media-amazon.com/images/I/81zEY1VMUbL.__AC_SX300_SY300_QL70_ML2_.jpg", "Easy Gravel Cleaner", "Effortlessly removes debris from substrate.", 14.99m, 8),
                new AccessoryViewModel("Aquarium Net", "https://m.media-amazon.com/images/I/71QwQb6pQwL._AC_SX679_.jpg", "Fine Mesh Net", "Safely transfers fish and removes debris.", 5.99m, 20),
                new AccessoryViewModel("Aquarium Background", "https://m.media-amazon.com/images/I/615xtDizmWL.__AC_SX300_SY300_QL70_ML2_.jpg", "Scenic Background", "Enhances the visual appeal of your aquarium.", 12.99m, 6),
                new AccessoryViewModel("Water Conditioner", "https://m.media-amazon.com/images/I/71QwQb6pQwL._AC_SX679_.jpg", "Premium Water Conditioner", "Removes harmful chemicals from tap water.", 8.99m, 12)
            };
            return View(accessories);
        }
    }
}
