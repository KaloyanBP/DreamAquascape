namespace DreamAquascape.Web.ViewModels.AdminDashboard.ContestCategory
{
    public class ContestCategoryWithContestsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public IEnumerable<ContestSummaryViewModel> Contests { get; set; } = new List<ContestSummaryViewModel>();
    }

    public class ContestSummaryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int EntriesCount { get; set; }
    }
}
