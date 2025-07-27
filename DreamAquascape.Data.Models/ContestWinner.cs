namespace DreamAquascape.Data.Models
{
    public class ContestWinner
    {
        public int ContestId { get; set; }
        public int ContestEntryId { get; set; }
        public int Position { get; set; } // 1 = first place, 2 = 2 second place, etc.

        public DateTime WonAt { get; set; }
        public string? Notes { get; set; }
        public string? AwardTitle { get; set; }

        public Contest Contest { get; set; } = null!;
        public ContestEntry ContestEntry { get; set; } = null!;

        public bool IsDeleted { get; set; } = false;
    }
}
