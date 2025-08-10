using DreamAquascape.Data.Common;
using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.AdminDashboard.ContestCategory;
using Microsoft.EntityFrameworkCore;

namespace DreamAquascape.Services.Core
{
    public class ContestCategoryService : IContestCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ContestCategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IEnumerable<ContestCategoryListViewModel> Categories, int TotalCount)> GetAllCategoriesAsync(int page = 1, int pageSize = 10)
        {
            var (categories, totalCount) = await _unitOfWork.ContestCategoryRepository
                .GetCategoriesWithPaginationAsync(page, pageSize);

            var viewModels = categories.Select(c => new ContestCategoryListViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ContestsCount = c.ContestsCategories?.Count ?? 0,
                CreatedOn = c.CreatedAt,
                ModifiedOn = c.UpdatedAt
            });

            return (viewModels, totalCount);
        }

        public async Task<IEnumerable<ContestCategorySelectViewModel>> GetActiveCategoriesForSelectionAsync()
        {
            var categories = await _unitOfWork.ContestCategoryRepository.GetAllActiveCategoriesAsync();

            return categories.Select(c => new ContestCategorySelectViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            });
        }

        public async Task<ContestCategoryDetailsViewModel?> GetCategoryByIdAsync(int id)
        {
            var category = await _unitOfWork.ContestCategoryRepository.GetByIdAsync(id);

            if (category == null || category.IsDeleted)
            {
                return null;
            }

            return new ContestCategoryDetailsViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ContestsCount = category.ContestsCategories?.Count ?? 0,
                CreatedOn = category.CreatedAt,
                ModifiedOn = category.UpdatedAt,
                CreatedBy = category.CreatedBy,
                ModifiedBy = category.ModifiedBy
            };
        }

        public async Task<int> CreateCategoryAsync(ContestCategoryCreateViewModel model)
        {
            // Validate uniqueness
            if (!await IsCategoryNameUniqueAsync(model.Name))
            {
                throw new InvalidOperationException($"A category with the name '{model.Name}' already exists.");
            }

            var category = new ContestCategory
            {
                Name = model.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim()
            };

            await _unitOfWork.ContestCategoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return category.Id;
        }

        public async Task<bool> UpdateCategoryAsync(int id, ContestCategoryEditViewModel model)
        {
            var category = await _unitOfWork.ContestCategoryRepository.GetByIdAsync(id);

            if (category == null || category.IsDeleted)
            {
                return false;
            }

            // Validate uniqueness (excluding current category)
            if (!await IsCategoryNameUniqueAsync(model.Name, id))
            {
                throw new InvalidOperationException($"A category with the name '{model.Name}' already exists.");
            }

            category.Name = model.Name.Trim();
            category.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();

            _unitOfWork.ContestCategoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _unitOfWork.ContestCategoryRepository.GetCategoryWithContestsAsync(id);

            if (category == null || category.IsDeleted)
            {
                return false;
            }

            // Check if category has associated contests
            if (category.ContestsCategories?.Any() == true)
            {
                throw new InvalidOperationException("Cannot delete a category that has associated contests. Please remove the category from all contests first.");
            }

            category.IsDeleted = true;
            _unitOfWork.ContestCategoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsCategoryNameUniqueAsync(string name, int? excludeId = null)
        {
            return await _unitOfWork.ContestCategoryRepository.IsCategoryNameUniqueAsync(name, excludeId);
        }

        public async Task<IEnumerable<ContestCategorySelectViewModel>> GetCategoriesByContestIdAsync(int contestId)
        {
            var categories = await _unitOfWork.ContestCategoryRepository.GetCategoriesByContestAsync(contestId);

            return categories.Select(c => new ContestCategorySelectViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            });
        }

        public async Task<ContestCategoryWithContestsViewModel?> GetCategoryWithContestsAsync(int id)
        {
            var category = await _unitOfWork.ContestCategoryRepository.GetCategoryWithContestsAsync(id);

            if (category == null || category.IsDeleted)
            {
                return null;
            }

            return new ContestCategoryWithContestsViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedOn = category.CreatedAt,
                ModifiedOn = category.UpdatedAt,
                Contests = category.ContestsCategories?.Select(cc => new ContestSummaryViewModel
                {
                    Id = cc.Contest.Id,
                    Title = cc.Contest.Title,
                    StartDate = cc.Contest.SubmissionStartDate,
                    EndDate = cc.Contest.SubmissionEndDate,
                    IsActive = cc.Contest.IsActive,
                    EntriesCount = cc.Contest.Entries?.Count ?? 0
                }) ?? new List<ContestSummaryViewModel>()
            };
        }
    }
}
