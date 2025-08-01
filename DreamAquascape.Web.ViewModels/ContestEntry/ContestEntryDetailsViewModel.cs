using DreamAquascape.Web.ViewModels.UserDashboard;

namespace DreamAquascape.Web.ViewModels.ContestEntry
{
    public class ContestEntryDetailsViewModel
    {
        // Entry Information
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime SubmittedAt { get; set; }
        public bool IsActive { get; set; }

        // Participant Information
        public string ParticipantId { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;

        // Contest Information
        public int ContestId { get; set; }
        public string ContestTitle { get; set; } = string.Empty;
        public string? ContestDescription { get; set; }
        public DateTime ContestSubmissionStartDate { get; set; }
        public DateTime ContestSubmissionEndDate { get; set; }
        public DateTime ContestVotingStartDate { get; set; }
        public DateTime ContestVotingEndDate { get; set; }
        public string ContestPhase { get; set; } = string.Empty; // "Submission", "Voting", "Results"
        public bool IsContestActive { get; set; }

        // Entry Images
        public List<EntryImageViewModel> Images { get; set; } = new List<EntryImageViewModel>();

        // Voting Information
        public int VoteCount { get; set; }
        public List<VoteDetailViewModel> Votes { get; set; } = new List<VoteDetailViewModel>();

        // User Context (for current user)
        public bool IsOwnEntry { get; set; }
        public bool CanUserVote { get; set; }
        public bool HasUserVoted { get; set; }
        public bool CanEdit { get; set; }

        // Competition Information
        public int EntryRanking { get; set; }
        public int TotalEntriesInContest { get; set; }
        public double VotePercentage { get; set; }
        public bool IsWinner { get; set; }
        public int? WinnerPosition { get; set; } // 1st, 2nd, 3rd place etc.

        // Statistics
        public DateTime? LastVoteDate { get; set; }
        public DateTime? FirstVoteDate { get; set; }
        
        // Related Entries (other entries in the same contest)
        public List<RelatedEntryViewModel> RelatedEntries { get; set; } = new List<RelatedEntryViewModel>();
    }

    public class VoteDetailViewModel
    {
        public int Id { get; set; }
        public string VoterName { get; set; } = string.Empty;
        public DateTime VotedAt { get; set; }
        public bool IsAnonymous { get; set; } = true; // For privacy, might want to keep voter names anonymous
    }

    public class RelatedEntryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public string? ThumbnailImageUrl { get; set; }
        public int VoteCount { get; set; }
        public bool IsWinner { get; set; }
    }
}
