using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DreamAquascape.Data.Models
{
    public class ContestEntry
    {
        public int Id { get; set; }

        public int ContestId { get; set; }

        [Comment("Foreign key to the referenced AspNetUser.")]
        public string ParticipantId { get; set; } = null!;

        public virtual ApplicationUser Participant { get; set; } = null!;

        public virtual ContestWinner? Winner { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime SubmittedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual Contest Contest { get; set; } = null!;

        public virtual ICollection<EntryImage> EntryImages { get; set; } = new HashSet<EntryImage>();

        public virtual ICollection<Vote> Votes { get; set; } = new HashSet<Vote>();

        // Computed properties for performance
        public int VoteCount => Votes?.Count ?? 0;
        public bool HasImages => EntryImages?.Any() == true;
    }
}
