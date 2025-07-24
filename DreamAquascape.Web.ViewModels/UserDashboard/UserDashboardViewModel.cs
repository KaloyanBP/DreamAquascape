namespace DreamAquascape.Web.ViewModels.UserDashboard
{
    public class UserDashboardViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        // Active contests user is participating in
        public List<UserActiveContestViewModel> ActiveContests { get; set; } = new List<UserActiveContestViewModel>();

        // User's submissions
        public List<UserSubmissionViewModel> MySubmissions { get; set; } = new List<UserSubmissionViewModel>();

        // Voting history
        public List<UserVotingHistoryViewModel> VotingHistory { get; set; } = new List<UserVotingHistoryViewModel>();

        // Quick stats
        public UserQuickStatsViewModel QuickStats { get; set; } = new UserQuickStatsViewModel();
    }
}
