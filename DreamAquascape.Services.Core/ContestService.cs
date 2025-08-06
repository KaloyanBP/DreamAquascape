using DreamAquascape.Data;
using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.Services.Common.Exceptions;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.Contest;
using DreamAquascape.Web.ViewModels.ContestEntry;
using DreamAquascape.Web.ViewModels.UserDashboard;
using static DreamAquascape.GCommon.ExceptionMessages;
using Microsoft.Extensions.Logging;

namespace DreamAquascape.Services.Core
{
    public class ContestService : IContestService
    {
        private readonly ILogger<ContestService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public ContestService(
            IUnitOfWork unitOfWork,
            ILogger<ContestService> logger)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ContestItemViewModel>> GetActiveContestsAsync()
        {
            var activeContests = await _unitOfWork.ContestRepository.GetActiveContestsAsync();

            return activeContests
                .Select(c => new ContestItemViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    ImageUrl = c.ImageFileUrl ?? "",
                    StartDate = c.SubmissionStartDate,
                    EndDate = c.VotingEndDate,
                    IsActive = c.IsActive,
                })
                .ToList();
        }

        public async Task<ContestListViewModel> GetFilteredContestsAsync(ContestFilterViewModel filters)
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
                    Description = p.Description
                }).ToList()
            }).ToList();

            // Get contest statistics from repository
            var stats = await _unitOfWork.ContestRepository.GetContestStatsAsync();

            return new ContestListViewModel
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
        }

        public async Task<ContestDetailsViewModel?> GetContestWithEntriesAsync(int contestId, string? currentUserId = null)
        {
            // First, get the contest with all related data
            Contest? contest = await _unitOfWork.ContestRepository.GetContestDetailsAsync(contestId);

            if (contest == null)
                return null;

            // Get user participation data if user is authenticated
            ContestEntry? userEntry = null;
            Vote? userVote = null;

            if (!string.IsNullOrEmpty(currentUserId))
            {
                userEntry = contest.Entries
                    .FirstOrDefault(e => e.ContestId == contestId && e.ParticipantId == currentUserId && !e.IsDeleted);

                userVote = contest.Entries?.SelectMany(e => e.Votes)
                    .FirstOrDefault(v => v.UserId == currentUserId && v.ContestEntry.ContestId == contestId);
            }

            var now = DateTime.UtcNow;
            return new ContestDetailsViewModel
            {
                Id = contest.Id,
                Title = contest.Title,
                Description = contest.Description,
                StartDate = contest.SubmissionStartDate,
                EndDate = contest.VotingEndDate,
                IsActive = contest.IsActive,

                CanSubmitEntry = !string.IsNullOrEmpty(currentUserId) &&
                               now >= contest.SubmissionStartDate &&
                               now <= contest.SubmissionEndDate &&
                               userEntry == null,

                CanVote = now >= contest.VotingStartDate &&
                         now <= contest.VotingEndDate,

                Prize = contest.PrimaryPrize != null ? new PrizeViewModel
                {
                    Name = contest.PrimaryPrize.Name,
                    Description = contest.PrimaryPrize.Description,
                } : null,

                Entries = contest.Entries
                    .Where(e => !e.IsDeleted)
                    .Select(e => new ContestEntryViewModel
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Description = e.Description,
                        EntryImages = e.EntryImages
                            .OrderBy(img => img.DisplayOrder)
                            .Select(img => img.ImageUrl)
                            .ToList(),

                        CanUserVote = !string.IsNullOrEmpty(currentUserId) &&
                                     now >= contest.VotingStartDate &&
                                     now <= contest.VotingEndDate &&
                                     e.ParticipantId != currentUserId &&
                                     userVote == null,

                        HasUserVoted = userVote?.ContestEntryId == e.Id,
                        IsWinner = contest.PrimaryWinner?.ContestEntryId == e.Id,
                        IsOwnEntry = e.ParticipantId == currentUserId,
                        VoteCount = e.Votes.Count(),
                        SubmittedAt = e.SubmittedAt
                    })
                    .OrderByDescending(e => e.VoteCount)
                    .ThenBy(e => e.SubmittedAt)
                    .ToList(),

                WinnerEntryId = contest.PrimaryWinner?.ContestEntryId,
                UserHasSubmittedEntry = userEntry != null,
                UserHasVoted = userVote != null,
                UserVotedForEntryId = userVote?.ContestEntryId,
                UserSubmittedEntryId = userEntry?.Id
            };
        }

        public async Task<Contest> SubmitContestAsync(CreateContestViewModel dto, PrizeViewModel prizeDto, string createdBy)
        {
            try
            {
                // Validate submission dates
                if (dto.SubmissionStartDate >= dto.SubmissionEndDate)
                    throw new InvalidOperationException("Submission start date must be before end date");
                if (dto.VotingStartDate <= dto.SubmissionStartDate || dto.VotingEndDate <= dto.VotingStartDate)
                    throw new InvalidOperationException("Start voting date must be after submission start date and before voting end date");

                // Create the primary prize
                var primaryPrize = new Prize
                {
                    Name = prizeDto.Name,
                    Description = prizeDto.Description,
                    ImageUrl = prizeDto.ImageUrl,
                    Place = 1
                };

                // Create the contest
                var contest = new Contest
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    ImageFileUrl = dto.ImageFileUrl,
                    SubmissionStartDate = dto.SubmissionStartDate,
                    SubmissionEndDate = dto.SubmissionEndDate,
                    VotingStartDate = dto.VotingStartDate,
                    VotingEndDate = dto.VotingEndDate,
                    ResultDate = dto.ResultDate,
                    CreatedBy = createdBy,
                    IsActive = true,
                    IsDeleted = false
                };

                // Create contest with prize using repository
                var createdContest = await _unitOfWork.ContestRepository.CreateContestWithPrizeAsync(contest, primaryPrize);

                _logger.LogInformation("Contest {ContestId} created by user {UserId}", createdContest.Id, createdBy);
                return createdContest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create contest by user {UserId}", createdBy);
                throw;
            }
        }

        public async Task<Vote> CastVoteAsync(int contestId, int entryId, string userId, string userName, string? ipAddress = null)
        {
            try
            {
                var contest = await _unitOfWork.ContestRepository.GetByIdAsync(contestId);
                if (contest == null || !contest.IsActive || contest.IsDeleted)
                    throw new NotFoundException(String.Format(ContestNotFoundMessage, contestId));

                if (DateTime.UtcNow < contest.VotingStartDate || DateTime.UtcNow > contest.VotingEndDate)
                    throw new InvalidOperationException(ContestVotingPeriodNotActiveMessage);

                // Validate the entry exists and is active
                var entry = await _unitOfWork.ContestEntryRepository.GetByIdAsync(entryId);
                if (entry == null || entry.ContestId != contestId || entry.IsDeleted)
                    throw new NotFoundException(ContestEntryNotFoundMessage);

                // Check if user is trying to vote for their own entry
                if (entry.ParticipantId == userId)
                    throw new InvalidOperationException(UserCannotVoteForOwnEntryMessage);

                // Check if user already voted in this contest
                var hasVoted = await _unitOfWork.VoteRepository.HasUserVotedInContestAsync(userId, contestId);
                if (hasVoted)
                    throw new InvalidOperationException(UserAlreadyVotedInContestMessage);

                // Create the vote
                var vote = new Vote
                {
                    ContestEntryId = entryId,
                    UserId = userId,
                    VotedAt = DateTime.UtcNow,
                    IpAddress = ipAddress
                };

                await _unitOfWork.VoteRepository.AddAsync(vote);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vote cast by user {UserId} for entry {EntryId} in contest {ContestId}",
                    userId, entryId, contestId);

                return vote;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cast vote for user {UserId} in contest {ContestId}",
                    userId, contestId);
                throw;
            }
        }

        public async Task<Vote> ChangeVoteAsync(int contestId, int newEntryId, string userId, string userName)
        {
            try
            {
                // Validate voting period
                var contest = await _unitOfWork.ContestRepository.GetByIdAsync(contestId);
                if (contest == null || !contest.IsActive || contest.IsDeleted)
                    throw new NotFoundException(String.Format(ContestNotFoundMessage, contestId));

                if (DateTime.UtcNow < contest.VotingStartDate || DateTime.UtcNow > contest.VotingEndDate)
                    throw new InvalidOperationException(ContestVotingPeriodNotActiveMessage);

                // Get existing vote
                var existingVote = await _unitOfWork.VoteRepository.GetUserVoteInContestAsync(userId, contestId);
                if (existingVote == null)
                    throw new NotFoundException(NoExistingVoteFoundMessage);

                // Validate new entry
                var newEntry = await _unitOfWork.ContestEntryRepository.GetByIdAsync(newEntryId);
                if (newEntry == null || newEntry.ContestId != contestId || newEntry.IsDeleted)
                    throw new NotFoundException("New entry not found");

                if (newEntry.ParticipantId == userId)
                    throw new InvalidOperationException("Users cannot vote for their own entries");

                // Update the vote
                existingVote.ContestEntryId = newEntryId;
                existingVote.VotedAt = DateTime.UtcNow;

                await _unitOfWork.VoteRepository.UpdateAsync(existingVote);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vote changed by user {UserId} to entry {EntryId} in contest {ContestId}",
                    userId, newEntryId, contestId);

                return existingVote;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to change vote for user {UserId} in contest {ContestId}",
                    userId, contestId);
                throw;
            }
        }

        public async Task RemoveVoteAsync(int contestId, string userId)
        {
            try
            {
                // Validate voting period
                var contest = await _unitOfWork.ContestRepository.GetByIdAsync(contestId);
                if (contest == null || !contest.IsActive || contest.IsDeleted)
                    throw new NotFoundException("Contest not found");

                if (DateTime.UtcNow < contest.VotingStartDate || DateTime.UtcNow > contest.VotingEndDate)
                    throw new InvalidOperationException("Contest voting period is not active");

                // Get existing vote
                var existingVote = await _unitOfWork.VoteRepository.GetUserVoteInContestAsync(userId, contestId);
                if (existingVote == null)
                    throw new NotFoundException("No existing vote found for this user");

                // Remove the vote
                await _unitOfWork.VoteRepository.HardDeleteAsync(existingVote);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vote removed by user {UserId} in contest {ContestId}",
                    userId, contestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove vote for user {UserId} in contest {ContestId}",
                    userId, contestId);
                throw;
            }
        }

        public async Task<ContestEntryDetailsViewModel?> GetContestEntryDetailsAsync(int contestId, int entryId, string? currentUserId = null)
        {
            // Get entry with all related data
            var entry = await _unitOfWork.ContestEntryRepository.GetEntryDetailsWithAllDataAsync(contestId, entryId);

            if (entry == null)
                return null;

            var contest = entry.Contest;
            var now = DateTime.UtcNow;

            // Get all entries in this contest for ranking calculation
            var allContestEntries = await _unitOfWork.ContestEntryRepository.GetAllEntriesInContestAsync(contestId);

            // Calculate ranking
            var rankedEntries = allContestEntries
                .OrderByDescending(e => e.Votes.Count)
                .ThenBy(e => e.SubmittedAt)
                .ToList();

            var entryRanking = rankedEntries.FindIndex(e => e.Id == entryId) + 1;
            var totalVotesInContest = allContestEntries.Sum(e => e.Votes.Count);
            var votePercentage = totalVotesInContest > 0 ? (double)entry.Votes.Count / totalVotesInContest * 100 : 0;

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

                // Statistics
                EntryRanking = entryRanking,
                VotePercentage = votePercentage,
                TotalEntriesInContest = allContestEntries.Count(),

                CanUserVote = canUserVote,
                CanEdit = canEdit,
                IsOwnEntry = isOwnEntry,

                // Winner Information
                IsWinner = isWinner,
                WinnerPosition = winnerPosition
            };
        }

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

        /// <summary>
        /// Automatically determines and sets the winner for a contest when voting ends.
        /// Winner is determined by highest vote count, with ties broken by earliest submission.
        /// </summary>
        /// <param name="contestId">The contest ID to determine winner for</param>
        /// <returns>The contest winner, or null if no entries or winner already exists</returns>
        public async Task<ContestWinner?> DetermineAndSetWinnerAsync(int contestId)
        {
            var contest = await _unitOfWork.ContestRepository.GetContestForWinnerDeterminationAsync(contestId);
            if (contest == null)
            {
                _logger.LogWarning("Contest with ID {ContestId} not found for winner determination", contestId);
                return null;
            }

            // Check if voting has ended
            if (contest.IsVotingOpen)
            {
                _logger.LogInformation("Contest {ContestId} voting is still open, cannot determine winner yet", contestId);
                return null;
            }

            // Check if winner already exists
            if (contest.PrimaryWinner != null)
            {
                _logger.LogInformation("Contest {ContestId} already has a winner: Entry {EntryId}",
                    contestId, contest.PrimaryWinner.ContestEntryId);
                return contest.PrimaryWinner;
            }

            // Check if there are any entries
            if (!contest.Entries.Any())
            {
                _logger.LogInformation("Contest {ContestId} has no entries, cannot determine winner", contestId);
                return null;
            }

            // Find winner: highest vote count, then earliest submission for ties
            var winnerEntry = contest.Entries
                .OrderByDescending(e => e.Votes.Count)
                .ThenBy(e => e.SubmittedAt)
                .First();

            var winner = new ContestWinner
            {
                ContestId = contestId,
                ContestEntryId = winnerEntry.Id,
                Position = 1,
                WonAt = DateTime.UtcNow,
                AwardTitle = "Contest Winner",
                Notes = $"Won with {winnerEntry.Votes.Count} votes"
            };

            await _unitOfWork.ContestWinnerRepository.AddAsync(winner);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Winner determined for contest {ContestId}: Entry {EntryId} with {VoteCount} votes",
                contestId, winnerEntry.Id, winnerEntry.Votes.Count);

            return winner;
        }

        /// <summary>
        /// Checks all contests where voting has recently ended and determines winners automatically.
        /// </summary>
        /// <returns>List of newly determined winners</returns>
        public async Task<List<ContestWinner>> ProcessEndedContestsAsync()
        {
            var newWinners = new List<ContestWinner>();

            var endedContests = await _unitOfWork.ContestRepository.GetEndedContestsWithoutWinnersAsync();

            foreach (var contest in endedContests)
            {
                try
                {
                    var winner = await DetermineAndSetWinnerAsync(contest.Id);
                    if (winner != null)
                    {
                        newWinners.Add(winner);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error determining winner for contest {ContestId}", contest.Id);
                }
            }

            if (newWinners.Any())
            {
                _logger.LogInformation("Processed {Count} ended contests and determined {WinnerCount} new winners",
                    endedContests.Count(), newWinners.Count);
            }

            return newWinners;
        }

        // Admin management methods - Backward compatibility wrapper
        public async Task<ContestListViewModel> GetFilteredContestsAsync(string searchTerm = "", string status = "", int page = 1, int pageSize = 10)
        {
            // Convert parameters to ContestFilterViewModel for consistency
            var filters = new ContestFilterViewModel
            {
                Search = searchTerm,
                Status = status?.ToLower() switch
                {
                    "active" => ContestStatus.Active,
                    "inactive" => ContestStatus.Archived,
                    "submission" => ContestStatus.Submission,
                    "voting" => ContestStatus.Voting,
                    "ended" => ContestStatus.Ended,
                    _ => ContestStatus.All
                },
                Page = page,
                PageSize = pageSize,
                SortBy = ContestSortBy.Newest,
                ExcludeArchived = false
            };

            // Use the main GetFilteredContestsAsync method
            return await GetFilteredContestsAsync(filters);
        }

        public async Task<bool> ToggleContestStatusAsync(int contestId)
        {
            try
            {
                var contest = await _unitOfWork.ContestRepository.GetContestForToggleAsync(contestId);
                if (contest == null)
                    return false;

                contest.IsActive = !contest.IsActive;
                await _unitOfWork.ContestRepository.UpdateAsync(contest);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Contest {ContestId} status toggled to {Status}",
                    contestId, contest.IsActive ? "Active" : "Inactive");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling contest status for contest {ContestId}", contestId);
                return false;
            }
        }

        public async Task<bool> DeleteContestAsync(int contestId)
        {
            try
            {
                var contest = await _unitOfWork.ContestRepository.GetContestForDeleteAsync(contestId);
                if (contest == null)
                    return false;

                // Only allow deletion if no entries exist
                if (contest.Entries.Any(e => !e.IsDeleted))
                {
                    _logger.LogWarning("Cannot delete contest {ContestId} - has existing entries", contestId);
                    return false;
                }

                contest.IsDeleted = true;
                await _unitOfWork.ContestRepository.UpdateAsync(contest);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Contest {ContestId} deleted successfully", contestId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contest {ContestId}", contestId);
                return false;
            }
        }

        public async Task<EditContestViewModel?> GetContestForEditAsync(int contestId)
        {
            try
            {
                var contest = await _unitOfWork.ContestRepository.GetContestForEditAsync(contestId);
                if (contest == null)
                    return null;

                var prize = contest.Prizes.FirstOrDefault();

                return new EditContestViewModel
                {
                    Id = contest.Id,
                    Title = contest.Title,
                    Description = contest.Description,
                    CurrentImageUrl = contest.ImageFileUrl,
                    SubmissionStartDate = contest.SubmissionStartDate,
                    SubmissionEndDate = contest.SubmissionEndDate,
                    VotingStartDate = contest.VotingStartDate,
                    VotingEndDate = contest.VotingEndDate,
                    ResultDate = contest.ResultDate,
                    IsActive = contest.IsActive,
                    PrizeName = prize?.Name,
                    PrizeDescription = prize?.Description,
                    PrizeMonetaryValue = prize?.MonetaryValue,
                    CurrentPrizeImageUrl = prize?.ImageUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contest for edit {ContestId}", contestId);
                return null;
            }
        }

        public async Task<bool> UpdateContestAsync(EditContestViewModel model, string? newImageUrl = null, string? newPrizeImageUrl = null)
        {
            try
            {
                var contest = await _unitOfWork.ContestRepository.GetContestForEditAsync(model.Id);

                if (contest == null)
                    return false;

                await _unitOfWork.BeginTransactionAsync();

                // Update contest properties
                contest.Title = model.Title;
                contest.Description = model.Description;
                contest.SubmissionStartDate = model.SubmissionStartDate;
                contest.SubmissionEndDate = model.SubmissionEndDate;
                contest.VotingStartDate = model.VotingStartDate;
                contest.VotingEndDate = model.VotingEndDate;
                contest.ResultDate = model.ResultDate;
                contest.IsActive = model.IsActive;

                // Update image - use NewImageUrl from model if provided, otherwise use parameter
                if (!string.IsNullOrEmpty(model.NewImageUrl))
                {
                    contest.ImageFileUrl = model.NewImageUrl;
                }
                else if (!string.IsNullOrEmpty(newImageUrl))
                {
                    contest.ImageFileUrl = newImageUrl;
                }

                // Update prize if exists
                var prize = contest.Prizes.FirstOrDefault();
                if (prize != null)
                {
                    if (!string.IsNullOrEmpty(model.PrizeName))
                    {
                        prize.Name = model.PrizeName;
                        prize.Description = model.PrizeDescription ?? "";
                        prize.MonetaryValue = model.PrizeMonetaryValue;

                        // Update prize image - use NewPrizeImageUrl from model if provided, otherwise use parameter
                        if (!string.IsNullOrEmpty(model.NewPrizeImageUrl))
                        {
                            prize.ImageUrl = model.NewPrizeImageUrl;
                        }
                        else if (!string.IsNullOrEmpty(newPrizeImageUrl))
                        {
                            prize.ImageUrl = newPrizeImageUrl;
                        }

                        // Save the updated prize
                        await _unitOfWork.PrizeRepository.UpdateAsync(prize);
                    }

                    // Save contest changes
                    await _unitOfWork.ContestRepository.UpdateAsync(contest);
                }
                else if (!string.IsNullOrEmpty(model.PrizeName))
                {
                    // Create new prize if none exists but prize info provided
                    var prizeImageUrl = !string.IsNullOrEmpty(model.NewPrizeImageUrl) ? model.NewPrizeImageUrl : newPrizeImageUrl;
                    var newPrize = new Prize
                    {
                        Name = model.PrizeName,
                        Description = model.PrizeDescription ?? "",
                        MonetaryValue = model.PrizeMonetaryValue,
                        ImageUrl = prizeImageUrl,
                        ContestId = contest.Id
                    };
                    await _unitOfWork.PrizeRepository.AddAsync(newPrize);
                }
                else
                {
                    await _unitOfWork.ContestRepository.UpdateAsync(contest);
                }

                // Save all changes in a single transaction
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Contest {ContestId} updated successfully", model.Id);

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating contest {ContestId}", model.Id);
                return false;
            }
        }
    }
}
