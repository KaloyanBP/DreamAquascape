using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.Services.Core.Infrastructure;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.ContestEntry;
using DreamAquascape.Web.ViewModels.UserDashboard;
using Microsoft.Extensions.Logging;

namespace DreamAquascape.Services.Core
{
    /// <summary>
    /// Service for contest entry query operations
    /// </summary>
    public class ContestEntryQueryService : IContestEntryQueryService
    {
        private readonly ILogger<ContestEntryQueryService> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUnitOfWork _unitOfWork;

        public ContestEntryQueryService(
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider,
            ILogger<ContestEntryQueryService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentException(nameof(dateTimeProvider));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<ContestEntryDetailsViewModel?> GetContestEntryDetailsAsync(int contestId, int entryId, string? currentUserId = null)
        {
            _logger.LogInformation("Getting contest entry details for entryId: {EntryId}, contestId: {ContestId}, userId: {UserId}",
                entryId, contestId, currentUserId ?? "anonymous");

            try
            {
                var entry = await _unitOfWork.ContestEntryRepository.GetEntryWithAllDataAsync(contestId, entryId);

                if (entry == null)
                {
                    _logger.LogWarning("Contest entry not found: entryId {EntryId}, contestId {ContestId}", entryId, contestId);
                    return null;
                }

                var contest = entry.Contest;
                var now = _dateTimeProvider.UtcNow;

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

                var result = new ContestEntryDetailsViewModel
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

                _logger.LogInformation("Successfully retrieved contest entry details for entryId: {EntryId}", entryId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contest entry details for entryId: {EntryId}, contestId: {ContestId}", entryId, contestId);
                throw;
            }
        }

        public async Task<EditContestEntryViewModel?> GetContestEntryForEditAsync(int contestId, int entryId, string currentUserId)
        {
            _logger.LogInformation("Getting contest entry for edit: entryId {EntryId}, contestId {ContestId}, userId {UserId}",
                entryId, contestId, currentUserId);

            try
            {
                // Get the entry and verify ownership
                var entry = await _unitOfWork.ContestEntryRepository.GetEntryWithAllDataAsync(contestId, entryId);

                if (entry == null)
                {
                    _logger.LogWarning("Contest entry not found for edit: entryId {EntryId}, contestId {ContestId}", entryId, contestId);
                    return null;
                }

                // Verify ownership
                if (entry.ParticipantId != currentUserId)
                {
                    _logger.LogWarning("User {UserId} attempted to edit entry {EntryId} they don't own", currentUserId, entryId);
                    return null;
                }

                // Check if editing is still allowed
                var now = _dateTimeProvider.UtcNow;
                if (now > entry.Contest.SubmissionEndDate || !entry.Contest.IsActive)
                {
                    _logger.LogWarning("Edit attempt after submission deadline for entry {EntryId}", entryId);
                    return null;
                }

                var result = new EditContestEntryViewModel
                {
                    Id = entry.Id,
                    ContestId = entry.ContestId,
                    Title = entry.Title,
                    Description = entry.Description,
                    ExistingImages = entry.EntryImages
                        .Where(img => !img.IsDeleted)
                        .OrderBy(img => img.DisplayOrder)
                        .Select(img => new ExistingImageViewModel
                        {
                            Id = img.Id,
                            ImageUrl = img.ImageUrl,
                            MarkedForRemoval = false
                        }).ToList(),
                    ContestTitle = entry.Contest.Title,
                    CanEdit = true,
                    SubmissionEndDate = entry.Contest.SubmissionEndDate,
                };

                _logger.LogInformation("Successfully retrieved contest entry for edit: entryId {EntryId}", entryId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contest entry for edit: entryId {EntryId}, contestId {ContestId}", entryId, contestId);
                throw;
            }
        }

        public async Task<int> GetEntryCountByContestAsync(int contestId)
        {
            _logger.LogInformation("Getting entry count for contest {ContestId}", contestId);

            try
            {
                var count = await _unitOfWork.ContestEntryRepository.GetEntryCountByContestAsync(contestId);
                _logger.LogInformation("Contest {ContestId} has {Count} entries", contestId, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entry count for contest {ContestId}", contestId);
                throw;
            }
        }

        public async Task<int> GetVoteCountByEntryAsync(int entryId)
        {
            _logger.LogInformation("Getting vote count for entry {EntryId}", entryId);

            try
            {
                var count = await _unitOfWork.ContestEntryRepository.GetVoteCountByEntryAsync(entryId);
                _logger.LogInformation("Entry {EntryId} has {Count} votes", entryId, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vote count for entry {EntryId}", entryId);
                throw;
            }
        }

        public async Task<Dictionary<int, int>> GetVoteCountsByContestAsync(int contestId)
        {
            _logger.LogInformation("Getting vote counts for all entries in contest {ContestId}", contestId);

            try
            {
                var voteCounts = await _unitOfWork.ContestEntryRepository.GetVoteCountsByContestAsync(contestId);
                _logger.LogInformation("Retrieved vote counts for {EntryCount} entries in contest {ContestId}",
                    voteCounts.Count, contestId);
                return voteCounts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vote counts for contest {ContestId}", contestId);
                throw;
            }
        }
    }
}
