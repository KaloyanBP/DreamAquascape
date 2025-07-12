using System.Reflection;

namespace DreamAquascape.Web.ViewModels
{
    public class AccessoryViewModel
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public decimal Price { get; set; }
        public int OutstandingCount { get; set; }
        public AccessoryViewModel(string name, string imageUrl, string title, string subtitle, decimal price, int outstandingCount)
        {
            Name = name;
            ImageUrl = imageUrl;
            Title = title;
            Subtitle = subtitle;
            Price = price;
            OutstandingCount = outstandingCount;
        }
    }
}
