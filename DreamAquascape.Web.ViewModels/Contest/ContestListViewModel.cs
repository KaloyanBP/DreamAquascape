namespace DreamAquascape.Web.ViewModels.Contest
{
    public class ContestListViewModel
    {
        public IEnumerable<ContestItemViewModel> Contests { get; set; } = new List<ContestItemViewModel>();
        public ContestFilterViewModel Filters { get; set; } = new ContestFilterViewModel();
        public ContestStatsViewModel Stats { get; set; } = new ContestStatsViewModel();
        public PaginationViewModel Pagination { get; set; } = new PaginationViewModel();
    }

    public class ContestStatsViewModel
    {
        public int TotalContests { get; set; }
        public int ActiveContests { get; set; }
        public int SubmissionPhase { get; set; }
        public int VotingPhase { get; set; }
        public int EndedContests { get; set; }
        public int ArchivedContests { get; set; }
    }

    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
