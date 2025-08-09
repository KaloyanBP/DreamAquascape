using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.Services.Common.Exceptions;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.ContestEntry;
using DreamAquascape.Web.ViewModels.UserDashboard;
using Microsoft.Extensions.Logging;
using DreamAquascape.GCommon;

namespace DreamAquascape.Services.Core
{
    public class ContestEntryService : IContestEntryService
    {
        private readonly ILogger<ContestEntryService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public ContestEntryService(
            ILogger<ContestEntryService> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<ContestEntry> SubmitEntryAsync(CreateContestEntryViewModel dto, string userId, string userName)
        {
            bool transactionStarted = false;
            try
            {
                var contest = await _unitOfWork.ContestRepository.GetByIdAsync(dto.ContestId);

                if (contest == null || !contest.IsActive || contest.IsDeleted)
                    throw new NotFoundException(ExceptionMessages.ContestNotFoundErrorMessage);

                if (DateTime.UtcNow < contest.SubmissionStartDate || DateTime.UtcNow > contest.SubmissionEndDate)
                    throw new InvalidOperationException("Contest submission period is not active");

                // Check if user already has an entry
                var hasExistingEntry = await _unitOfWork.ContestEntryRepository.UserHasEntryInContestAsync(dto.ContestId, userId);

                if (hasExistingEntry)
                    throw new InvalidOperationException("User already has an entry in this contest");

                // Start transaction only when we're about to make changes
                await _unitOfWork.BeginTransactionAsync();
                transactionStarted = true;

                // Create the contest entry
                var entry = new ContestEntry
                {
                    ContestId = dto.ContestId,
                    ParticipantId = userId,
                    Title = dto.Title,
                    Description = dto.Description,
                    SubmittedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false,
                    EntryImages = GetEntryImages(dto.EntryImages)
                };

                await _unitOfWork.ContestEntryRepository.AddAsync(entry);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Entry {EntryId} submitted by user {UserId} for contest {ContestId}",
                    entry.Id, userId, dto.ContestId);

                return entry;
            }
            catch (Exception ex)
            {
                if (transactionStarted)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }

                _logger.LogError(ex, "Failed to submit entry for user {UserId} in contest {ContestId}",
                    userId, dto.ContestId);
                throw;
            }
        }

        public async Task<bool> UpdateContestEntryAsync(EditContestEntryViewModel model, string currentUserId)
        {
            var contest = await _unitOfWork.ContestRepository.GetByIdAsync(model.ContestId);

            if (contest == null || contest.IsDeleted) return false;

            var entry = await _unitOfWork.ContestEntryRepository.GetEntryForEditAsync(model.ContestId, model.Id, currentUserId);

            if (entry == null) return false;

            // Check if editing is allowed
            var now = DateTime.UtcNow;
            var canEdit = contest.IsActive &&
                         now >= contest.SubmissionStartDate &&
                         now <= contest.SubmissionEndDate;

            if (!canEdit) return false;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Update entry details
                entry.Title = model.Title;
                entry.Description = model.Description;
                entry.UpdatedAt = now;

                // Handle image removals
                if (model.ImagesToRemove?.Any() == true)
                {
                    var imagesToRemove = entry.EntryImages
                        .Where(img => model.ImagesToRemove.Contains(img.Id))
                        .ToList();

                    foreach (var img in imagesToRemove)
                    {
                        img.IsDeleted = true;
                        await _unitOfWork.EntryImageRepository.UpdateAsync(img);
                    }
                }

                // Add new images
                if (model.NewImages?.Any() == true)
                {
                    foreach (var imageUrl in model.NewImages)
                    {
                        var newImage = new EntryImage
                        {
                            ContestEntryId = entry.Id,
                            ImageUrl = imageUrl,
                            UploadedAt = now
                        };
                        await _unitOfWork.EntryImageRepository.AddAsync(newImage);
                    }
                }

                await _unitOfWork.ContestEntryRepository.UpdateAsync(entry);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Contest entry {EntryId} updated successfully by user {UserId}", model.Id, currentUserId);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating contest entry {EntryId} for user {UserId}", model.Id, currentUserId);
                return false;
            }
        }

        public async Task<bool> DeleteEntryAsync(int contestId, int entryId, string currentUserId)
        {
            var contest = await _unitOfWork.ContestRepository.GetByIdAsync(contestId);

            if (contest == null || contest.IsDeleted) return false;

            var entry = await _unitOfWork.ContestEntryRepository.GetEntryForEditAsync(contestId, entryId, currentUserId);

            if (entry == null) return false;

            // Check if deletion is allowed (same rules as editing)
            var now = DateTime.UtcNow;
            var canDelete = contest.IsActive &&
                           now >= contest.SubmissionStartDate &&
                           now <= contest.SubmissionEndDate;

            if (!canDelete) return false;

            // Soft delete the entry
            entry.IsDeleted = true;
            entry.UpdatedAt = now;

            try
            {
                await _unitOfWork.ContestEntryRepository.UpdateAsync(entry);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Contest entry {EntryId} deleted by user {UserId}", entryId, currentUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contest entry {EntryId} for user {UserId}", entryId, currentUserId);
                return false;
            }
        }

        // Helper methods
        private ICollection<EntryImage> GetEntryImages(List<string> imageUrls)
        {
            if (imageUrls == null || !imageUrls.Any())
                throw new ArgumentException("Entry images cannot be null or empty");

            var entryImages = new List<EntryImage>();
            for (int i = 0; i < imageUrls.Count; i++)
            {
                entryImages.Add(new EntryImage
                {
                    ImageUrl = imageUrls[i],
                    DisplayOrder = i + 1,
                    UploadedAt = DateTime.UtcNow
                });
            }

            return entryImages;
        }
    }
}
