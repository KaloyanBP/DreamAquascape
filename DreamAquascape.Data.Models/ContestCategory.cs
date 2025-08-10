using System.ComponentModel.DataAnnotations;

namespace DreamAquascape.Data.Models
{
    public class ContestCategory : SoftDeletableEntity
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? CreatedBy { get; set; }

        public string? ModifiedBy { get; set; }

        // Navigation properties
        public virtual ICollection<ContestsCategories> ContestsCategories { get; set; } = new HashSet<ContestsCategories>();
    }
}
