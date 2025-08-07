using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.Contest;
using DreamAquascape.Web.ViewModels.ContestEntry;
using DreamAquascape.Web.ViewModels.UserDashboard;
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
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Contest {ContestId} created by user {UserId}", createdContest.Id, createdBy);
                return createdContest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create contest by user {UserId}", createdBy);
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

        public async Task<bool> ToggleContestActiveStatusAsync(int contestId)
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

        public async Task<bool> UpdateContestAsync(EditContestViewModel model)
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

                // Update image - handle removal or replacement
                if (!string.IsNullOrEmpty(model.NewImageUrl))
                {
                    contest.ImageFileUrl = model.NewImageUrl;
                }
                else if (model.RemoveCurrentImage)
                {
                    // Explicitly remove the current image
                    contest.ImageFileUrl = null;
                }

                // Update image - use NewImageUrl from model if provided, otherwise use parameter
                if (!string.IsNullOrEmpty(model.NewImageUrl))
                {
                    contest.ImageFileUrl = model.NewImageUrl;
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

                        // Update prize image - handle removal or replacement
                        if (!string.IsNullOrEmpty(model.NewPrizeImageUrl))
                        {
                            prize.ImageUrl = model.NewPrizeImageUrl;
                        }
                        else if (model.RemoveCurrentPrizeImage)
                        {
                            // Explicitly remove the current prize image
                            prize.ImageUrl = null;
                        }


                        // Update prize image - use NewPrizeImageUrl from model if provided, otherwise use parameter
                        if (!string.IsNullOrEmpty(model.NewPrizeImageUrl))
                        {
                            prize.ImageUrl = model.NewPrizeImageUrl;
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
                    var prizeImageUrl = model.NewPrizeImageUrl;
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
