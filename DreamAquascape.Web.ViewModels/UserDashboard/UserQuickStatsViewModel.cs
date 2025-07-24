namespace DreamAquascape.Web.ViewModels.UserDashboard
{
    public class UserQuickStatsViewModel
    {
        public int TotalContestsParticipated { get; set; }
        public int TotalEntriesSubmitted { get; set; }
        public int TotalVotesCast { get; set; }
        public int TotalVotesReceived { get; set; }
        public int ContestsWon { get; set; }

        // Current activity
        public int ActiveContests { get; set; }
        public int PendingVotes { get; set; } // Contests user is following but hasn't voted in
        public int SubmissionsInProgress { get; set; } // Contests user can still submit to

        // Performance metrics
        public double AverageVotesPerEntry { get; set; }
        public double WinRate { get; set; } // Percentage of contests won
        public int CurrentStreak { get; set; } // Days of consecutive activity

        // Recent achievements
        public List<string> RecentAchievements { get; set; } = new List<string>();
    }
}
