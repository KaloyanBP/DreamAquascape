using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DreamAquascape.Data.Models
{
    public class UserContestParticipation
    {
        public int Id { get; set; }

        public int ContestId { get; set; }

        public string UserId { get; set; } = null!;

        public virtual IdentityUser User { get; set; } = null!;

        public DateTime ParticipationDate { get; set; }

        public bool HasSubmittedEntry { get; set; } = false;

        public bool HasVoted { get; set; } = false;

        public int? VotedForEntryId { get; set; }

        public DateTime? VotedAt { get; set; }

        public int? SubmittedEntryId { get; set; }

        public DateTime? EntrySubmittedAt { get; set; }

        // Navigation properties
        public virtual Contest Contest { get; set; } = null!;

        public virtual ContestEntry? VotedForEntry { get; set; }

        public virtual ContestEntry? SubmittedEntry { get; set; }
    }
}
