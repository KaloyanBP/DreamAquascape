using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DreamAquascape.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        [StringLength(100)]
        public string? DisplayName { get; set; }
    }
}
