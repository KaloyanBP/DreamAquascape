using DreamAquascape.Data;
using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.GCommon;
using DreamAquascape.GCommon.Infrastructure;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.UserDashboard;
using Microsoft.Extensions.Logging;

namespace DreamAquascape.Services.Core
{
    public class UserDashboardService : IUserDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserDashboardService> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;

        public UserDashboardService(
            IUnitOfWork unitOfWork,
            ILogger<UserDashboardService> logger,
            IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger;
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public async Task<UserQuickStatsViewModel> GetUserQuickStatsAsync(string userId)
        {
            var now = _dateTimeProvider.UtcNow;

            // Get user participation data using repositories
            var userContestIdsFromEntries = await _unitOfWork.ContestEntryRepository.GetContestIdsUserEnteredAsync(userId);
            var userContestIdsFromVotes = await _unitOfWork.VoteRepository.GetContestIdsUserVotedInAsync(userId);

            // Basic counts
            var totalContestsParticipated = userContestIdsFromEntries
                .Union(userContestIdsFromVotes)
                .Distinct()
                .Count();

            var totalEntriesSubmitted = await _unitOfWork.ContestEntryRepository.GetTotalEntriesSubmittedByUserAsync(userId);

            var totalVotesCast = await _unitOfWork.VoteRepository.GetVotesCastByUserAsync(userId);

            var totalVotesReceived = await _unitOfWork.VoteRepository.GetVotesReceivedByUserAsync(userId);

            var contestsWon = await _unitOfWork.ContestWinnerRepository.GetContestsWonByUserAsync(userId);

            // Current activity - contests where user has participated and are still active
            var activeContests = await _unitOfWork.ContestRepository.GetActiveContestsCountForUserAsync(userId, now);

            var submissionsInProgress = await _unitOfWork.ContestRepository.GetSubmissionsInProgressCountForUserAsync(userId, now);

            // Performance metrics
            var averageVotesPerEntry = totalEntriesSubmitted > 0 ? (double)totalVotesReceived / totalEntriesSubmitted : 0;
            var winRate = totalContestsParticipated > 0 ? (double)contestsWon / totalContestsParticipated * 100 : 0;

            var currentStreak = 0; // TODO: Calculate actual streak

            return new UserQuickStatsViewModel
            {
                TotalContestsParticipated = totalContestsParticipated,
                TotalEntriesSubmitted = totalEntriesSubmitted,
                TotalVotesCast = totalVotesCast,
                TotalVotesReceived = totalVotesReceived,
                ContestsWon = contestsWon,
                ActiveContests = activeContests,
                SubmissionsInProgress = submissionsInProgress,
                AverageVotesPerEntry = averageVotesPerEntry,
                WinRate = winRate,
                CurrentStreak = currentStreak
            };
        }

        public async Task<List<UserActiveContestViewModel>> GetUserActiveContestsAsync(string userId)
        {
            var now = _dateTimeProvider.UtcNow;

            // Get contests that are currently active (accepting submissions or voting)
            var activeContests = await _unitOfWork.ContestRepository.GetActiveContestsWithFullDataAsync(now);

            var result = new List<UserActiveContestViewModel>();

            foreach (var contest in activeContests)
            {
                // Check user participation directly from entries and votes
                var userEntry = contest.Entries.FirstOrDefault(e => e.ParticipantId == userId);

                var userVote = await _unitOfWork.VoteRepository.GetUserVoteForContestAsync(userId, contest.Id);

                // Get total votes for this contest
                var totalVotes = await _unitOfWork.VoteRepository.GetTotalVotesForContestAsync(contest.Id);

                // Determine contest phase and status
                ContestPhases phase;
                string status;
                int daysRemaining;

                if (now < contest.SubmissionEndDate)
                {
                    phase = ContestPhases.Submission;
                    status = "Accepting Submissions";
                    daysRemaining = (int)(contest.SubmissionEndDate - now).TotalDays;
                }
                else if (now < contest.VotingEndDate)
                {
                    phase = ContestPhases.Voting;
                    status = "Voting Period";
                    daysRemaining = (int)(contest.VotingEndDate - now).TotalDays;
                }
                else if (contest.VotingEndDate < now && contest.Winners.Any() == false)
                {
                    phase = ContestPhases.ResultsPending;
                    status = "Results Pending";
                    daysRemaining = 0;
                }
                else
                {
                    phase = ContestPhases.Results;
                    status = "Ended";
                    daysRemaining = 0;
                }

                result.Add(new UserActiveContestViewModel
                {
                    ContestId = contest.Id,
                    Title = contest.Title,
                    Description = contest.Description,
                    ImageFileUrl = contest.ImageFileUrl,
                    SubmissionStartDate = contest.SubmissionStartDate,
                    SubmissionEndDate = contest.SubmissionEndDate,
                    VotingStartDate = contest.VotingStartDate,
                    VotingEndDate = contest.VotingEndDate,
                    Phase = phase,
                    DaysRemaining = daysRemaining,
                    HasSubmitted = userEntry != null,
                    HasVoted = userVote != null,
                    UserEntryId = userEntry?.Id,
                    TotalEntries = contest.Entries.Count,
                    TotalVotes = totalVotes,
                    PrizeName = contest.PrimaryPrize?.Name,
                    PrizeValue = contest.PrimaryPrize?.MonetaryValue,
                    Categories = contest.Categories.Select(c => c.Category.Name).ToList(),
                });
            }

            return result.OrderBy(c => c.DaysRemaining).ToList();
        }

        public async Task<List<UserSubmissionViewModel>> GetUserSubmissionsAsync(string userId, int page = 1, int pageSize = 10)
        {
            var entries = await _unitOfWork.ContestEntryRepository.GetUserSubmissionsWithFullDataAsync(userId, page, pageSize);
            var result = new List<UserSubmissionViewModel>();

            foreach (var entry in entries)
            {
                var contest = entry.Contest;
                var totalContestVotes = await _unitOfWork.VoteRepository.GetTotalVotesForContestAsync(contest.Id);

                var votesReceived = entry.Votes.Count;
                var votePercentage = totalContestVotes > 0 ? (double)votesReceived / totalContestVotes * 100 : 0;

                // Calculate ranking using repository
                var ranking = await _unitOfWork.ContestEntryRepository.GetEntryRankingInContestAsync(contest.Id, entry.Id);

                var isWinner = contest.PrimaryWinner?.ContestEntryId == entry.Id;

                // Determine status
                var now = _dateTimeProvider.UtcNow;
                string status;
                if (contest.VotingEndDate < now)
                {
                    status = isWinner ? "Winner" : "Contest Ended";
                }
                else
                {
                    status = "Active";
                }

                var canEdit = contest.SubmissionEndDate > now && contest.IsActive;

                result.Add(new UserSubmissionViewModel
                {
                    EntryId = entry.Id,
                    ContestId = contest.Id,
                    ContestTitle = contest.Title,
                    EntryTitle = entry.Title,
                    Description = entry.Description,
                    SubmittedAt = entry.SubmittedAt,
                    Status = status,
                    VotesReceived = votesReceived,
                    TotalContestVotes = totalContestVotes,
                    VotePercentage = votePercentage,
                    Ranking = ranking,
                    IsWinner = isWinner,
                    CanEdit = canEdit,
                    ContestEndDate = contest.VotingEndDate,
                    Images = entry.EntryImages
                        .OrderBy(i => i.DisplayOrder)
                        .Select(i => new EntryImageViewModel
                        {
                            Id = i.Id,
                            ImageUrl = i.ImageUrl,
                            Caption = i.Caption,
                            DisplayOrder = i.DisplayOrder,
                            UploadedAt = i.UploadedAt
                        }).ToList(),
                });
            }

            return result;
        }

        public async Task<List<UserVotingHistoryViewModel>> GetUserVotingHistoryAsync(string userId, int page = 1, int pageSize = 10)
        {
            var votes = await _unitOfWork.VoteRepository.GetUserVotingHistoryAsync(userId, page, pageSize);
            var result = new List<UserVotingHistoryViewModel>();

            foreach (var vote in votes)
            {
                var contest = vote.ContestEntry.Contest;
                var entry = vote.ContestEntry;
                var now = _dateTimeProvider.UtcNow;

                var canChangeVote = contest.VotingEndDate > now && contest.IsActive;

                string contestStatus;
                if (contest.VotingEndDate < now)
                {
                    contestStatus = "Ended";
                }
                else if (contest.VotingStartDate <= now)
                {
                    contestStatus = "Voting";
                }
                else
                {
                    contestStatus = "Upcoming";
                }

                // Check if the entry won
                bool? entryWon = null;
                int? entryFinalRanking = null;

                if (contest.VotingEndDate < now)
                {
                    entryWon = contest.PrimaryWinner?.ContestEntryId == entry.Id;

                    // Calculate final ranking using repository
                    entryFinalRanking = await _unitOfWork.ContestEntryRepository.GetEntryRankingInContestAsync(contest.Id, entry.Id);
                }

                result.Add(new UserVotingHistoryViewModel
                {
                    VoteId = vote.Id,
                    ContestId = contest.Id,
                    ContestTitle = contest.Title,
                    EntryId = entry.Id,
                    EntryTitle = entry.Title,
                    EntryOwner = entry.Participant?.UserName,
                    VotedAt = vote.VotedAt,
                    CanChangeVote = canChangeVote,
                    ContestVotingStatus = contestStatus,
                    VotingEndDate = contest.VotingEndDate,
                    EntryImageUrl = entry.EntryImages.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl,
                    EntryDescription = entry.Description,
                    EntryWon = entryWon,
                    EntryFinalRanking = entryFinalRanking
                });
            }

            return result;
        }

        public async Task<ApplicationUser?> GetUserDetails(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("GetUserDetails called with null or empty userId");
                return null;
            }

            var user = await _unitOfWork.UserRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return null;
            }

            return user;
        }
    }
}
