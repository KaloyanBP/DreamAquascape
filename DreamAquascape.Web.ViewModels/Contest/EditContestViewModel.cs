using System.ComponentModel.DataAnnotations;

namespace DreamAquascape.Web.ViewModels.Contest
{
    public class EditContestViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Contest title is required.")]
        [StringLength(100, ErrorMessage = "Title must be under 100 characters.")]
        [Display(Name = "Contest Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description must be under 1000 characters.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Current Image")]
        public string? CurrentImageUrl { get; set; }

        [Display(Name = "New Image URL")]
        public string? NewImageUrl { get; set; }

        [Display(Name = "Remove Current Image")]
        public bool RemoveCurrentImage { get; set; }

        [Required(ErrorMessage = "Submission start date is required.")]
        [Display(Name = "Submission Start Date")]
        public DateTime SubmissionStartDate { get; set; }

        [Required(ErrorMessage = "Submission end date is required.")]
        [Display(Name = "Submission End Date")]
        public DateTime SubmissionEndDate { get; set; }

        [Required(ErrorMessage = "Voting start date is required.")]
        [Display(Name = "Voting Start Date")]
        public DateTime VotingStartDate { get; set; }

        [Required(ErrorMessage = "Voting end date is required.")]
        [Display(Name = "Voting End Date")]
        public DateTime VotingEndDate { get; set; }

        [Display(Name = "Result Date")]
        public DateTime? ResultDate { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Is In Progress")]
        public bool InProgress { get; set; }

        [Display(Name = "Is Ended")]
        public bool IsEnded { get; set; }

        // Prize information
        [Required(ErrorMessage = "Prize Name is required.")]
        [Display(Name = "Prize Name")]
        public string? PrizeName { get; set; }

        [Display(Name = "Prize Description")]
        public string? PrizeDescription { get; set; }

        [Display(Name = "Prize Monetary Value")]
        public decimal? PrizeMonetaryValue { get; set; }

        [Display(Name = "Current Prize Image")]
        public string? CurrentPrizeImageUrl { get; set; }

        [Display(Name = "New Prize Image URL")]
        public string? NewPrizeImageUrl { get; set; }

        [Display(Name = "Remove Current Prize Image")]
        public bool RemoveCurrentPrizeImage { get; set; }

        // Category selection
        [Display(Name = "Categories")]
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();
    }
}
