using DreamAquascape.Web.ViewModels.Contest;

namespace DreamAquascape.Web.ViewModels.Home
{
    public class HomeIndexViewModel
    {
        // Statistics
        public int ActiveContests { get; set; }
        public int TotalEntries { get; set; }
        public int TotalVotes { get; set; }
        public int TotalUsers { get; set; }

        // Featured Contests
        public ContestItemViewModel? ActiveContest { get; set; }
        public ContestItemViewModel? VotingContest { get; set; }
        public ContestItemViewModel? UpcomingContest { get; set; }
        public ContestItemViewModel? RecentlyEndedContest { get; set; }

        // Helper method to check if we have featured contests
        public bool HasFeaturedContests => ActiveContest != null || VotingContest != null ||
                                          UpcomingContest != null || RecentlyEndedContest != null;
    }
}
