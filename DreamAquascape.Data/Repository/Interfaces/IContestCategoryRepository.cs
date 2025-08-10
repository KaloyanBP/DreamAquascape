using DreamAquascape.Data.Models;

namespace DreamAquascape.Data.Repository.Interfaces
{
    public interface IContestCategoryRepository : IRepository<ContestCategory, int>, IAsyncRepository<ContestCategory, int>
    {
        // Basic category queries
        Task<IEnumerable<ContestCategory>> GetAllActiveCategoriesAsync();
        Task<ContestCategory?> GetCategoryWithContestsAsync(int categoryId);
        Task<bool> IsCategoryNameUniqueAsync(string name, int? excludeId = null);

        // Category-Contest relationship queries
        Task<IEnumerable<ContestCategory>> GetCategoriesByContestAsync(int contestId);
        Task<int> GetContestCountByCategoryAsync(int categoryId);

        // Admin management
        Task<bool> IsCategoryInUseAsync(int categoryId);
        Task<(IEnumerable<ContestCategory> categories, int totalCount)> GetCategoriesWithPaginationAsync(
            int page = 1, int pageSize = 20, string? searchTerm = null);
    }
}