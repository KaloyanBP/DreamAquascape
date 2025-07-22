namespace DreamAquascape.Web.ViewModels.AdminDashboard
{    
    public class DashboardStatsViewModel
    {
        public int TotalContests { get; set; }
        public int ActiveContests { get; set; }
        public int TotalEntries { get; set; }
        public int TotalVotes { get; set; }
        public int TotalUsers { get; set; }
        public int PendingEntries { get; set; }
        public int ContestsEndingSoon { get; set; }
        public decimal TotalPrizeValue { get; set; }

        // Performance metrics
        public double AverageEntriesPerContest { get; set; }
        public double AverageVotesPerEntry { get; set; }
        public double UserEngagementRate { get; set; }
    }
}
