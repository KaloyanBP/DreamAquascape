namespace DreamAquascape.Web.ViewModels.UserDashboard
{
    public class UserSubmissionViewModel
    {
        public int EntryId { get; set; }
        public int ContestId { get; set; }
        public string ContestTitle { get; set; } = string.Empty;
        public string EntryTitle { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; } = string.Empty; // "Active", "Contest Ended", "Winner"

        // Performance metrics
        public int VotesReceived { get; set; }
        public int TotalContestVotes { get; set; }
        public double VotePercentage { get; set; }
        public int Ranking { get; set; } // Position in contest (1 = winner)
        public bool IsWinner { get; set; }

        // Images
        public List<EntryImageViewModel> Images { get; set; } = new List<EntryImageViewModel>();

        // Contest status
        public bool CanEdit { get; set; } // True if submission period is still active
        public DateTime? ContestEndDate { get; set; }
    }
}
