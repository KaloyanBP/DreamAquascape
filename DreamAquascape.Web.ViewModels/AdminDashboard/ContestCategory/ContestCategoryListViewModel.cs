namespace DreamAquascape.Web.ViewModels.AdminDashboard.ContestCategory
{
    public class ContestCategoryListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ContestsCount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
