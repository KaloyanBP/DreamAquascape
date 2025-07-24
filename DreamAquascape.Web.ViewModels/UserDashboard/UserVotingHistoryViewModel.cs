namespace DreamAquascape.Web.ViewModels.UserDashboard
{
    public class UserVotingHistoryViewModel
    {
        public int VoteId { get; set; }
        public int ContestId { get; set; }
        public string ContestTitle { get; set; } = string.Empty;
        public int EntryId { get; set; }
        public string EntryTitle { get; set; } = string.Empty;
        public string EntryOwner { get; set; } = string.Empty;

        public DateTime VotedAt { get; set; }
        public bool CanChangeVote { get; set; } // True if voting period is still active

        // Contest status
        public string ContestStatus { get; set; } = string.Empty;
        public DateTime? VotingEndDate { get; set; }

        // Entry preview
        public string? EntryImageUrl { get; set; }
        public string? EntryDescription { get; set; }

        // Results (if contest ended)
        public bool? EntryWon { get; set; }
        public int? EntryFinalRanking { get; set; }
    }
}
