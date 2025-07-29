using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DreamAquascape.Data.Models
{
    public class Vote
    {
        public int Id { get; set; }

        public int ContestEntryId { get; set; }

        [Comment("Foreign key to the referenced AspNetUser.")]
        public string UserId { get; set; } = string.Empty;

        public DateTime VotedAt { get; set; }

        public string? IpAddress { get; set; }

        // Navigation properties
        public virtual IdentityUser User { get; set; } = null!;
        public virtual ContestEntry ContestEntry { get; set; } = null!;

        // Computed property to access contest through entry
        public Contest Contest => ContestEntry?.Contest!;
    }
}
