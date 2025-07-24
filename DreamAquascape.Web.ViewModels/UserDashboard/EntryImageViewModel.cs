namespace DreamAquascape.Web.ViewModels.UserDashboard
{
    public class EntryImageViewModel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
