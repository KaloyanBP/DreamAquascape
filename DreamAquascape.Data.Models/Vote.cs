using Microsoft.AspNetCore.Identity;

namespace DreamAquascape.Data.Models
{
    public class Vote
    {
        public int Id { get; set; }

        public int ContestId { get; set; }

        public int ContestEntryId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public virtual IdentityUser User { get; set; } = null!;

        public DateTime VotedAt { get; set; }

        public string? IpAddress { get; set; } // For additional fraud prevention

        public virtual Contest Contest { get; set; } = null!;

        public virtual ContestEntry ContestEntry { get; set; } = null!;
    }
}
