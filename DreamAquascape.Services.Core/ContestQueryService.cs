using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.GCommon.Infrastructure;
using DreamAquascape.Web.ViewModels.Contest;
using DreamAquascape.Data.Models;
using Microsoft.Extensions.Logging;
using DreamAquascape.Services.Common.Extensions;

namespace DreamAquascape.Services.Core
{
    /// <summary>
    /// Service for contest query operations
    /// </summary>
    public class ContestQueryService : IContestQueryService
    {
        private readonly ILogger<ContestQueryService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ContestQueryService(
            IUnitOfWork unitOfWork,
            ILogger<ContestQueryService> logger,
            IDateTimeProvider dateTimeProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public async Task<ContestListViewModel> GetFilteredContestsAsync(ContestFilterViewModel filters)
        {
            _logger.LogInformation("Getting filtered contests with filters: {Filters}", filters);

            try
            {
                // Get filtered contests and total count from repository
                var (contests, totalCount) = await _unitOfWork.ContestRepository.GetFilteredContestsAsync(filters);

                // Map contests to view models
                var contestViewModels = contests.Select(c => new ContestItemViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    ImageUrl = c.ImageFileUrl ?? "",
                    StartDate = c.SubmissionStartDate,
                    EndDate = c.VotingEndDate,
                    SubmissionStartDate = c.SubmissionStartDate,
                    SubmissionEndDate = c.SubmissionEndDate,
                    VotingStartDate = c.VotingStartDate,
                    VotingEndDate = c.VotingEndDate,
                    IsActive = c.IsActive,
                    EntryCount = c.Entries.Count(e => !e.IsDeleted),
                    VoteCount = c.Entries.SelectMany(e => e.Votes).Count(),
                    TotalEntries = c.Entries.Count(e => !e.IsDeleted),
                    TotalVotes = c.Entries.SelectMany(e => e.Votes).Count(),
                    Prizes = c.Prizes.Select(p => new PrizeViewModel
                    {
                        MonetaryValue = p.MonetaryValue,
                        Name = p.Name,
                        Description = p.Description
                    }).ToList(),
                    Categories = c.Categories.Select(cc => cc.Category.Name).ToList()
                }).ToList();

                // Get contest statistics from repository
                var stats = await GetContestStatsAsync();

                var result = new ContestListViewModel
                {
                    Contests = contestViewModels,
                    Filters = filters,
                    Stats = stats,
                    Pagination = new PaginationViewModel
                    {
                        CurrentPage = filters.Page,
                        PageSize = filters.PageSize,
                        TotalItems = totalCount,
                        TotalPages = (int)Math.Ceiling((double)totalCount / filters.PageSize)
                    }
                };

                _logger.LogInformation("Successfully retrieved {Count} contests out of {Total} total",
                    contestViewModels.Count, totalCount);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting filtered contests");
                throw;
            }
        }

        public async Task<ContestDetailsViewModel?> GetContestDetailsAsync(int contestId, string? currentUserId = null)
        {
            _logger.LogInformation("Getting contest details for contest {ContestId}, user: {UserId}",
                contestId, currentUserId ?? "anonymous");

            try
            {
                // First, get the contest with all related data
                var contest = await _unitOfWork.ContestRepository.GetContestDetailsAsync(contestId);

                if (contest == null)
                {
                    _logger.LogWarning("Contest {ContestId} not found", contestId);
                    return null;
                }

                // Get user participation data if user is authenticated
                ContestEntry? userEntry = null;
                Vote? userVote = null;

                if (!string.IsNullOrEmpty(currentUserId))
                {
                    userEntry = contest.Entries
                        .FirstOrDefault(e => e.ContestId == contestId && e.ParticipantId == currentUserId && !e.IsDeleted);

                    userVote = contest.Entries?.SelectMany(e => e.Votes)
                        .FirstOrDefault(v => v.UserId == currentUserId &&
                                           v.ContestEntry != null &&
                                           v.ContestEntry.ContestId == contestId);
                }

                var now = _dateTimeProvider.UtcNow;
                var result = new ContestDetailsViewModel
                {
                    Id = contest.Id,
                    Title = contest.Title,
                    Description = contest.Description,
                    ImageUrl = contest.ImageFileUrl,
                    StartDate = contest.SubmissionStartDate,
                    EndDate = contest.VotingEndDate,
                    IsActive = contest.IsActive,

                    // UI flags - determine what user can do
                    CanSubmitEntry = contest.IsActive &&
                                   now >= contest.SubmissionStartDate &&
                                   now <= contest.SubmissionEndDate &&
                                   userEntry == null,

                    CanVote = contest.IsActive &&
                             now >= contest.VotingStartDate &&
                             now <= contest.VotingEndDate &&
                             userVote == null,

                    // User participation status
                    UserHasSubmittedEntry = userEntry != null,
                    UserHasVoted = userVote != null,
                    UserSubmittedEntryId = userEntry?.Id,
                    UserVotedForEntryId = userVote?.ContestEntryId,

                    // Prize information
                    Prize = contest.Prizes.FirstOrDefault() != null ? new PrizeViewModel
                    {
                        Name = contest.Prizes.First().Name,
                        Description = contest.Prizes.First().Description,
                        MonetaryValue = contest.Prizes.First().MonetaryValue,
                        ImageUrl = contest.Prizes.First().ImageUrl
                    } : null,

                    // Winner information (if contest is finished)
                    WinnerEntryId = contest.Winners.FirstOrDefault()?.ContestEntryId,

                    // Entries with full details
                    Entries = contest.Entries
                        .Where(e => !e.IsDeleted)
                        .Select(e => MapToContestEntryViewModel(e, currentUserId, userVote?.ContestEntryId))
                        .OrderByDescending(e => e.VoteCount)
                        .ThenBy(e => e.SubmittedAt)
                        .ToList()
                };

                _logger.LogInformation("Successfully retrieved contest details for {ContestId}", contestId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting contest details for {ContestId}", contestId);
                throw;
            }
        }

        public async Task<EditContestViewModel?> GetContestForEditAsync(int contestId)
        {
            _logger.LogInformation("Getting contest for edit: {ContestId}", contestId);

            try
            {
                var contest = await _unitOfWork.ContestRepository.GetContestForEditAsync(contestId);

                if (contest == null || contest.IsDeleted)
                {
                    _logger.LogWarning("Contest {ContestId} not found or deleted", contestId);
                    return null;
                }

                var prize = contest.PrimaryPrize;

                var result = new EditContestViewModel
                {
                    Id = contest.Id,
                    Title = contest.Title,
                    Description = contest.Description,
                    SubmissionStartDate = contest.SubmissionStartDate,
                    SubmissionEndDate = contest.SubmissionEndDate,
                    VotingStartDate = contest.VotingStartDate,
                    VotingEndDate = contest.VotingEndDate,
                    ResultDate = contest.ResultDate,
                    IsActive = contest.IsActive,
                    InProgress = contest.InProgress(_dateTimeProvider),
                    CurrentImageUrl = contest.ImageFileUrl,

                    // Prize information
                    PrizeName = prize?.Name,
                    PrizeDescription = prize?.Description,
                    PrizeMonetaryValue = prize?.MonetaryValue,
                    CurrentPrizeImageUrl = prize?.ImageUrl
                };

                _logger.LogInformation("Successfully retrieved contest for edit: {ContestId}", contestId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting contest for edit {ContestId}", contestId);
                throw;
            }
        }

        public async Task<ContestStatsViewModel> GetContestStatsAsync()
        {
            _logger.LogInformation("Getting contest statistics");

            try
            {
                var stats = await _unitOfWork.ContestRepository.GetContestStatsAsync();

                _logger.LogInformation("Successfully retrieved contest statistics");
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting contest statistics");
                throw;
            }
        }

        #region Private Helper Methods

        private static ContestEntryViewModel MapToContestEntryViewModel(ContestEntry entry, string? currentUserId, int? userVotedEntryId)
        {
            var winnerId = entry.Contest.Winners.FirstOrDefault()?.ContestEntryId;

            return new ContestEntryViewModel
            {
                Id = entry.Id,
                UserName = entry.Participant?.DisplayName ?? entry.Participant?.UserName ?? string.Empty,
                Title = entry.Title,
                Description = entry.Description,
                SubmittedAt = entry.SubmittedAt,
                VoteCount = entry.Votes.Count,
                EntryImages = entry.EntryImages
                    .OrderBy(img => img.DisplayOrder)
                    .Select(img => img.ImageUrl)
                    .ToList(),

                // User-specific flags
                IsOwnEntry = !string.IsNullOrEmpty(currentUserId) && entry.ParticipantId == currentUserId,
                HasUserVoted = userVotedEntryId == entry.Id,
                CanUserVote = !string.IsNullOrEmpty(currentUserId) &&
                             entry.ParticipantId != currentUserId &&
                             userVotedEntryId == null,

                // Contest status
                IsWinner = winnerId == entry.Id
            };
        }

        #endregion
    }
}
