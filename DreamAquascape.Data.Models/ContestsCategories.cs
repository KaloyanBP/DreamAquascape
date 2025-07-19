using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamAquascape.Data.Models
{
    public class ContestsCategories
    {
        public int ContestId { get; set; }
        public int CategoryId { get; set; }

        // Navigation properties
        public virtual Contest Contest { get; set; } = null!;
        public virtual ContestCategory Category { get; set; } = null!;
    }
}
