namespace DreamAquascape.Data.Models
{
    public class EntryImage
    {
        public int Id { get; set; }

        public int ContestEntryId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string? Caption { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public DateTime UploadedAt { get; set; }

        public virtual ContestEntry ContestEntry { get; set; } = null!;

        public bool IsDeleted { get; set; } = false;
    }
}
