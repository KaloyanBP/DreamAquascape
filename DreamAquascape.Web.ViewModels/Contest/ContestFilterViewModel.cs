using System.ComponentModel.DataAnnotations;

namespace DreamAquascape.Web.ViewModels.Contest
{
    public class ContestFilterViewModel
    {
        [Display(Name = "Search")]
        public string? Search { get; set; }

        [Display(Name = "Status")]
        public ContestStatus Status { get; set; } = ContestStatus.Active;

        [Display(Name = "Sort By")]
        public ContestSortBy SortBy { get; set; } = ContestSortBy.Newest;

        [Display(Name = "Page")]
        public int Page { get; set; } = 1;

        [Display(Name = "Page Size")]
        public int PageSize { get; set; } = 12;
    }

    public enum ContestStatus
    {
        All = 0,
        Active = 1,
        Inactive = 2,
        Submission = 3,
        Voting = 4,
        Ended = 5,
        Archived = 6
    }

    public enum ContestSortBy
    {
        Newest = 0,
        Oldest = 1,
        EndingSoon = 2,
        MostEntries = 3,
        Title = 4
    }
}
