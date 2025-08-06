using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.Services.Common.Exceptions;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.ContestEntry;
using DreamAquascape.Web.ViewModels.UserDashboard;
using Microsoft.Extensions.Logging;
using static DreamAquascape.GCommon.ExceptionMessages;

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

        public async Task<ContestEntryDetailsViewModel?> GetContestEntryDetailsAsync(int contestId, int entryId, string? currentUserId = null)
        {
            var entry = await _unitOfWork.ContestEntryRepository.GetEntryWithAllDataAsync(contestId, entryId);

            if (entry == null)
                return null;

            var contest = entry.Contest;
            var now = DateTime.UtcNow;

            // Get ranking from repository
            var entryRanking = await _unitOfWork.ContestEntryRepository.GetEntryRankingInContestAsync(contestId, entryId);

            // Get vote statistics from repository
            var voteStatistics = await _unitOfWork.ContestEntryRepository.GetVoteCountsByContestAsync(contestId);
            var totalVotesInContest = voteStatistics.Sum(v => v.Value);
            var votePercentage = totalVotesInContest > 0 ? (double)entry.Votes.Count / totalVotesInContest * 100 : 0;

            // Get all entries in this contest for related entries and total count
            var allContestEntries = await _unitOfWork.ContestEntryRepository.GetByContestIdWithImagesAsync(contestId);
            var allContestEntriesList = allContestEntries.ToList();

            // Calculate ranking for related entries
            var rankedEntries = allContestEntriesList
                .OrderByDescending(e => e.Votes.Count)
                .ThenBy(e => e.SubmittedAt)
                .ToList();

            // Check if user has voted for this entry
            var userVote = !string.IsNullOrEmpty(currentUserId) ?
                await _unitOfWork.VoteRepository.GetUserVoteForEntryAsync(currentUserId, entryId) :
                null;

            // Determine contest phase
            string contestPhase;
            if (now < contest.SubmissionEndDate)
                contestPhase = "Submission";
            else if (now < contest.VotingEndDate)
                contestPhase = "Voting";
            else
                contestPhase = "Results";

            // Check user permissions
            bool isOwnEntry = !string.IsNullOrEmpty(currentUserId) && entry.ParticipantId == currentUserId;
            bool canUserVote = !string.IsNullOrEmpty(currentUserId) &&
                              now >= contest.VotingStartDate &&
                              now <= contest.VotingEndDate &&
                              !isOwnEntry &&
                              userVote == null;
            bool canEdit = isOwnEntry && now <= contest.SubmissionEndDate && contest.IsActive;

            // Check if entry is winner
            bool isWinner = contest.Winners.Any(w => w.ContestEntryId == entryId);
            int? winnerPosition = contest.Winners.FirstOrDefault(w => w.ContestEntryId == entryId)?.Position;

            return new ContestEntryDetailsViewModel
            {
                // Entry Information
                Id = entry.Id,
                Title = entry.Title,
                Description = entry.Description,
                SubmittedAt = entry.SubmittedAt,
                IsActive = entry.IsActive,

                // Participant Information
                ParticipantId = entry.ParticipantId,
                ParticipantName = entry.Participant.UserName ?? "Unknown",

                // Contest Information
                ContestId = contest.Id,
                ContestTitle = contest.Title,
                ContestDescription = contest.Description,
                ContestSubmissionStartDate = contest.SubmissionStartDate,
                ContestSubmissionEndDate = contest.SubmissionEndDate,
                ContestVotingStartDate = contest.VotingStartDate,
                ContestVotingEndDate = contest.VotingEndDate,
                ContestPhase = contestPhase,
                IsContestActive = contest.IsActive,

                // Entry Images
                Images = entry.EntryImages
                    .Where(img => !img.IsDeleted)
                    .OrderBy(img => img.DisplayOrder)
                    .Select(img => new EntryImageViewModel
                    {
                        Id = img.Id,
                        ImageUrl = img.ImageUrl,
                        Caption = img.Caption,
                        DisplayOrder = img.DisplayOrder,
                        UploadedAt = img.UploadedAt
                    }).ToList(),

                // Voting Information
                VoteCount = entry.Votes.Count,
                Votes = entry.Votes
                    .OrderByDescending(v => v.VotedAt)
                    .Select(v => new VoteDetailViewModel
                    {
                        Id = v.Id,
                        VoterName = v.User.UserName ?? "Anonymous",
                        VotedAt = v.VotedAt,
                        IsAnonymous = true // Keep voter names private by default
                    }).ToList(),

                // User Context
                IsOwnEntry = isOwnEntry,
                CanUserVote = canUserVote,
                HasUserVoted = userVote != null,
                CanEdit = canEdit,

                // Competition Information
                EntryRanking = entryRanking,
                TotalEntriesInContest = allContestEntriesList.Count,
                VotePercentage = votePercentage,
                IsWinner = isWinner,
                WinnerPosition = winnerPosition,

                // Statistics
                LastVoteDate = entry.Votes.Any() ? entry.Votes.Max(v => v.VotedAt) : null,
                FirstVoteDate = entry.Votes.Any() ? entry.Votes.Min(v => v.VotedAt) : null,

                // Related Entries (top 5 other entries from same contest)
                RelatedEntries = rankedEntries
                    .Where(e => e.Id != entryId)
                    .Take(5)
                    .Select(e => new RelatedEntryViewModel
                    {
                        Id = e.Id,
                        Title = e.Title,
                        ParticipantName = e.Participant.UserName ?? "Unknown",
                        ThumbnailImageUrl = e.EntryImages.Where(img => !img.IsDeleted).OrderBy(img => img.DisplayOrder).FirstOrDefault()?.ImageUrl,
                        VoteCount = e.Votes.Count,
                        IsWinner = contest.Winners.Any(w => w.ContestEntryId == e.Id)
                    }).ToList()
            };
        }

        public async Task<EditContestEntryViewModel?> GetContestEntryForEditAsync(int contestId, int entryId, string currentUserId)
        {
            var entry = await _unitOfWork.ContestEntryRepository.GetEntryForEditAsync(contestId, entryId, currentUserId);

            if (entry == null || entry.ContestId != contestId)
                return null;

            var contest = entry.Contest;
            var now = DateTime.UtcNow;
            var canEdit = contest.IsActive &&
                         now >= contest.SubmissionStartDate &&
                         now <= contest.SubmissionEndDate;

            return new EditContestEntryViewModel
            {
                Id = entry.Id,
                ContestId = contestId,
                Title = entry.Title,
                Description = entry.Description,
                ContestTitle = contest.Title,
                SubmissionEndDate = contest.SubmissionEndDate,
                CanEdit = canEdit,
                ExistingImages = entry.EntryImages
                    .Where(img => !img.IsDeleted)
                    .Select(img => new ExistingImageViewModel
                    {
                        Id = img.Id,
                        ImageUrl = img.ImageUrl
                    })
                    .ToList()
            };
        }

        public async Task<ContestEntry> SubmitEntryAsync(CreateContestEntryViewModel dto, string userId, string userName)
        {
            bool transactionStarted = false;
            try
            {
                var contest = await _unitOfWork.ContestRepository.GetByIdAsync(dto.ContestId);

                if (contest == null || !contest.IsActive || contest.IsDeleted)
                    throw new NotFoundException(ContestNotFoundErrorMessage);

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

        // Additional helper methods for permissions and queries...
        public async Task<bool> CanUserSubmitEntryAsync(int contestId, string userId)
        {
            var contest = await _unitOfWork.ContestRepository.GetByIdAsync(contestId);

            if (contest == null || contest.IsDeleted || !contest.IsActive) return false;

            var now = DateTime.UtcNow;
            if (now < contest.SubmissionStartDate || now > contest.SubmissionEndDate)
                return false;

            // Check if user already has an entry
            var hasExistingEntry = await _unitOfWork.ContestEntryRepository.UserHasEntryInContestAsync(contestId, userId);

            return !hasExistingEntry;
        }

        public async Task<bool> CanUserVoteAsync(int contestId, string userId)
        {
            var contest = await _unitOfWork.ContestRepository.GetByIdAsync(contestId);

            if (contest == null || contest.IsDeleted || !contest.IsActive) return false;

            var now = DateTime.UtcNow;
            if (now < contest.VotingStartDate || now > contest.VotingEndDate)
                return false;

            // Check if user has already voted
            var hasVoted = await _unitOfWork.VoteRepository.HasUserVotedInContestAsync(userId, contestId);

            return !hasVoted;
        }

        public async Task<bool> CanUserEditEntryAsync(int contestId, int entryId, string userId)
        {
            var contest = await _unitOfWork.ContestRepository.GetByIdAsync(contestId);

            if (contest == null || !contest.IsActive) return false;

            var entry = await _unitOfWork.ContestEntryRepository.GetUserEntryInContestAsync(contestId, userId);

            if (entry == null || entry.Id != entryId) return false;

            var now = DateTime.UtcNow;
            return now >= contest.SubmissionStartDate && now <= contest.SubmissionEndDate;
        }

        public async Task<int> GetEntryCountByContestAsync(int contestId)
        {
            return await _unitOfWork.ContestEntryRepository.GetEntryCountByContestAsync(contestId);
        }

        public async Task<int> GetVoteCountByEntryAsync(int entryId)
        {
            return await _unitOfWork.ContestEntryRepository.GetVoteCountByEntryAsync(entryId);
        }

        public async Task<Dictionary<int, int>> GetVoteCountsByContestAsync(int contestId)
        {
            return await _unitOfWork.ContestEntryRepository.GetVoteCountsByContestAsync(contestId);
        }
    }
}
