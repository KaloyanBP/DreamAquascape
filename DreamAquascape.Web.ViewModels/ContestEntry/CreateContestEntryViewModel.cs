using System.ComponentModel.DataAnnotations;

namespace DreamAquascape.Web.ViewModels.ContestEntry
{
    public class CreateContestViewModel
    {
        [Required]
        public int ContestId { get; set; }

        [Required(ErrorMessage = "Entry title is required.")]
        [StringLength(100, ErrorMessage = "Title must be under 100 characters.")]
        [Display(Name = "Entry Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description must be under 1000 characters.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        public List<string> EntryImages { get; set; } = new List<string>();
    }
}
