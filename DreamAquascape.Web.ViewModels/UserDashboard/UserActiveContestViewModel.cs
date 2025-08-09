using DreamAquascape.GCommon;

namespace DreamAquascape.Web.ViewModels.UserDashboard
{
    public class UserActiveContestViewModel
    {
        public int ContestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageFileUrl { get; set; }

        public DateTime SubmissionStartDate { get; set; }
        public DateTime SubmissionEndDate { get; set; }
        public DateTime VotingStartDate { get; set; }
        public DateTime VotingEndDate { get; set; }

        public string ContestStatus { get; set; } = string.Empty; // "Accepting Submissions", "Voting", "Ended"
        public int DaysRemaining { get; set; }
        public ContestPhases Phase { get; set; } // "Submission", "Voting", "Results"

        // User's participation status in this contest
        public bool HasSubmitted { get; set; }
        public bool HasVoted { get; set; }
        public int? UserEntryId { get; set; }
        public int TotalEntries { get; set; }
        public int TotalVotes { get; set; }

        // Prize information
        public string? PrizeName { get; set; }
        public decimal? PrizeValue { get; set; }

        public List<string> Categories { get; set; } = new List<string>();
    }
}
