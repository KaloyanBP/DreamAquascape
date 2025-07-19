using System.ComponentModel.DataAnnotations;

namespace DreamAquascape.Data.Models
{    
    public class ContestCategory
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Navigation properties
        public virtual ICollection<ContestsCategories> Contests { get; set; } = new HashSet<ContestsCategories>();
    }
}
