namespace DreamAquascape.Web.ViewModels.Contest
{
    public class ContestDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        // Prize info
        public PrizeViewModel? Prize { get; set; }

        // Entries for this contest
        public List<ContestEntryViewModel> Entries { get; set; } = new();

        // Winner info (if contest is archived)
        public int? WinnerEntryId { get; set; }

        // UI flags
        public bool CanVote { get; set; }
        public bool CanSubmitEntry { get; set; }
    }

    public class ContestEntryViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> EntryImages { get; set; } = new List<string>();
        public int VoteCount { get; set; }
    }
}
