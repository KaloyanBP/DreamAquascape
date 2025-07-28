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
        public virtual ICollection<Prize> Prizes { get; set; } = new HashSet<Prize>();

        public virtual ICollection<ContestWinner> Winners { get; set; } = new HashSet<ContestWinner>();

        public virtual ICollection<ContestEntry> Entries { get; set; } = new HashSet<ContestEntry>();

        public virtual ICollection<Vote> Votes { get; set; } = new HashSet<Vote>();
        
        public virtual ICollection<UserContestParticipation> Participants { get; set; } = new HashSet<UserContestParticipation>();

        // Many-to-many relationships
        public virtual ICollection<ContestsCategories> Categories { get; set; } = new HashSet<ContestsCategories>();

        // Helpers
        public ContestWinner? PrimaryWinner => Winners.FirstOrDefault(w => w.Position == 1);

        public Prize? PrimaryPrize => Prizes.FirstOrDefault(p => p.Place == 1);
    }
}
