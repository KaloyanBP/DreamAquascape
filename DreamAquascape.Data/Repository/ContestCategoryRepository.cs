using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.GCommon.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DreamAquascape.Data.Repository
{
    public class ContestCategoryRepository : BaseRepository<ContestCategory, int>, IContestCategoryRepository
    {
        public ContestCategoryRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
            : base(dbContext, dateTimeProvider)
        {
        }

        public async Task<IEnumerable<ContestCategory>> GetAllActiveCategoriesAsync()
        {
            return await GetAllAttached()
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<ContestCategory?> GetCategoryWithContestsAsync(int categoryId)
        {
            return await GetAllAttached()
                .Include(c => c.ContestsCategories)
                    .ThenInclude(cc => cc.Contest)
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        public async Task<bool> IsCategoryNameUniqueAsync(string name, int? excludeId = null)
        {
            var query = GetAllAttached()
                .Where(c => c.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<IEnumerable<ContestCategory>> GetCategoriesByContestAsync(int contestId)
        {
            return await DbContext.ContestsCategories
                .Include(cc => cc.Category)
                .Where(cc => cc.ContestId == contestId)
                .Select(cc => cc.Category)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<int> GetContestCountByCategoryAsync(int categoryId)
        {
            return await DbContext.ContestsCategories
                .Include(cc => cc.Contest)
                .CountAsync(cc => cc.CategoryId == categoryId &&
                                 cc.Contest.IsActive &&
                                 !cc.Contest.IsDeleted);
        }

        public async Task<bool> IsCategoryInUseAsync(int categoryId)
        {
            return await DbContext.ContestsCategories
                .AnyAsync(cc => cc.CategoryId == categoryId);
        }

        public async Task<(IEnumerable<ContestCategory> categories, int totalCount)> GetCategoriesWithPaginationAsync(
            int page = 1, int pageSize = 20, string? searchTerm = null)
        {
            var query = GetAllAttached()
                .Include(c => c.ContestsCategories)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(search) ||
                                        (c.Description != null && c.Description.ToLower().Contains(search)));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var categories = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (categories, totalCount);
        }
    }
}
