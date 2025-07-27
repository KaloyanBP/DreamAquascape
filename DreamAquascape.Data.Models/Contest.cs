using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DreamAquascape.Data.Models
{
    public class Contest
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageFileUrl { get; set; }

        public DateTime SubmissionStartDate { get; set; }

        public DateTime SubmissionEndDate { get; set; }

        public DateTime VotingStartDate { get; set; }

        public DateTime VotingEndDate { get; set; }

        public DateTime? ResultDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; } = string.Empty; // User ID or username

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual Prize? Prize { get; set; }

        public int? WinnerEntryId { get; set; }

        public virtual ICollection<ContestWinner> Winners { get; set; } = new HashSet<ContestWinner>();

        public virtual ICollection<ContestEntry> Entries { get; set; } = new HashSet<ContestEntry>();

        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
        
        public virtual ICollection<UserContestParticipation> Participants { get; set; } = new List<UserContestParticipation>();

        // Many-to-many relationships
        public virtual ICollection<ContestsCategories> Categories { get; set; } = new HashSet<ContestsCategories>();
    }
}
