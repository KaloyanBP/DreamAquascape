using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DreamAquascape.Data.Models
{
    public class Prize
    {
        public int Id { get; set; }

        public int ContestId { get; set; }

        public int Place { get; set; } // 1 = first place, 2 = second place, etc.

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? NavigationUrl { get; set; }

        public string? ImageUrl { get; set; }

        public decimal? MonetaryValue { get; set; }

        public string? SponsorName { get; set; }

        public bool IsDeleted { get; set; }

        public virtual Contest Contest { get; set; } = null!;
    }
}
