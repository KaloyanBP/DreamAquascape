using System.ComponentModel.DataAnnotations;

namespace DreamAquascape.Data.Models
{
    public class ContestCategory : SoftDeletableEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Navigation properties
        public virtual ICollection<ContestsCategories> ContestsCategories { get; set; } = new HashSet<ContestsCategories>();
    }
}
