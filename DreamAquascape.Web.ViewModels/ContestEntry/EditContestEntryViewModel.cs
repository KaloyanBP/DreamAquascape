using System.ComponentModel.DataAnnotations;

namespace DreamAquascape.Web.ViewModels.ContestEntry
{
    public class EditContestEntryViewModel
    {
        public int Id { get; set; }

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

        // Existing images
        public List<ExistingImageViewModel> ExistingImages { get; set; } = new List<ExistingImageViewModel>();

        // New images to upload
        public List<string> NewImages { get; set; } = new List<string>();

        // Images to remove (by ID)
        public List<int> ImagesToRemove { get; set; } = new List<int>();

        // Contest details for validation
        public string ContestTitle { get; set; } = string.Empty;
        public DateTime SubmissionEndDate { get; set; }
        public bool CanEdit { get; set; }
    }

    public class ExistingImageViewModel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool MarkedForRemoval { get; set; }
    }
}
