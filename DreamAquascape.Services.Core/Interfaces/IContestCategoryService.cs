using DreamAquascape.Data.Models;
using DreamAquascape.Web.ViewModels.AdminDashboard.ContestCategory;

namespace DreamAquascape.Services.Core.Interfaces
{
    public interface IContestCategoryService
    {
        Task<(IEnumerable<ContestCategoryListViewModel> Categories, int TotalCount)> GetAllCategoriesAsync(int page = 1, int pageSize = 10);

        Task<IEnumerable<ContestCategorySelectViewModel>> GetActiveCategoriesForSelectionAsync();

        Task<ContestCategoryDetailsViewModel?> GetCategoryByIdAsync(int id);

        Task<int> CreateCategoryAsync(ContestCategoryCreateViewModel model);

        Task<bool> UpdateCategoryAsync(int id, ContestCategoryEditViewModel model);

        Task<bool> DeleteCategoryAsync(int id);

        Task<bool> IsCategoryNameUniqueAsync(string name, int? excludeId = null);

        Task<IEnumerable<ContestCategorySelectViewModel>> GetCategoriesByContestIdAsync(int contestId);

        Task<ContestCategoryWithContestsViewModel?> GetCategoryWithContestsAsync(int id);
    }
}
