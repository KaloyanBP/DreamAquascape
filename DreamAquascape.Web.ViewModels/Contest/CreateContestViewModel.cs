using System.ComponentModel.DataAnnotations;

namespace DreamAquascape.Web.ViewModels.Contest
{
    public class CreateContestViewModel
    {
        [Required(ErrorMessage = "Entry title is required.")]
        [StringLength(100, ErrorMessage = "Title must be under 100 characters.")]
        [Display(Name = "Contest Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description must be under 1000 characters.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Image is required.")]

        public string ImageFileUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Submission start date is required.")]
        public DateTime SubmissionStartDate { get; set; }

        [Required(ErrorMessage = "Submission end date is required.")]
        public DateTime SubmissionEndDate { get; set; }

        [Required(ErrorMessage = "Voting start date is required.")]
        public DateTime VotingStartDate { get; set; }

        [Required(ErrorMessage = "Voting end date is required.")]
        public DateTime VotingEndDate { get; set; }

        public DateTime? ResultDate { get; set; }

        // Category selection
        [Display(Name = "Categories")]
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();
    }
}
