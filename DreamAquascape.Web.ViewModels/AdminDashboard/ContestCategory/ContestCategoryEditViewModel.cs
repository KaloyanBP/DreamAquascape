using System.ComponentModel.DataAnnotations;
using DreamAquascape.Data.Common;

namespace DreamAquascape.Web.ViewModels.AdminDashboard.ContestCategory
{
    public class ContestCategoryEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(EntityConstants.ContestCategory.NameMaxLength, 
            MinimumLength = EntityConstants.ContestCategory.NameMinLength,
            ErrorMessage = "Category name must be between {2} and {1} characters")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(EntityConstants.ContestCategory.DescriptionMaxLength,
            ErrorMessage = "Description cannot exceed {1} characters")]
        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }
    }
}
