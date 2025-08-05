using DreamAquascape.Data;
using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.UserDashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DreamAquascape.Services.Core
{
    public class UserDashboardService : IUserDashboardService
    {
        private readonly IUserRepository _userRepository;
        private readonly IVoteRepository _voteRepository;
        private readonly IContestEntryRepository _contestEntryRepository;
        private readonly IContestRepository _contestRepository;
        private readonly IContestWinnerRepository _contestWinnerRepository;
        private readonly ILogger<UserDashboardService> _logger;

        public UserDashboardService(
            IUserRepository userRepository,
            IVoteRepository voteRepository,
            IContestEntryRepository contestEntryRepository,
            IContestRepository contestRepository,
            IContestWinnerRepository contestWinnerRepository,
            ILogger<UserDashboardService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _voteRepository = voteRepository ?? throw new ArgumentNullException(nameof(voteRepository));
            _contestEntryRepository = contestEntryRepository ?? throw new ArgumentNullException(nameof(contestEntryRepository));
            _contestRepository = contestRepository ?? throw new ArgumentNullException(nameof(contestRepository));
            _contestWinnerRepository = contestWinnerRepository ?? throw new ArgumentNullException(nameof(contestWinnerRepository));
            _logger = logger;
        }

        public async Task<UserQuickStatsViewModel> GetUserQuickStatsAsync(string userId)
        {
            var now = DateTime.UtcNow;

            // Get user participation data using repositories
            var userContestIdsFromEntries = await _contestEntryRepository.GetContestIdsUserEnteredAsync(userId);
            var userContestIdsFromVotes = await _voteRepository.GetContestIdsUserVotedInAsync(userId);

            // Basic counts
            var totalContestsParticipated = userContestIdsFromEntries
                .Union(userContestIdsFromVotes)
                .Distinct()
                .Count();

            var totalEntriesSubmitted = await _contestEntryRepository.GetTotalEntriesSubmittedByUserAsync(userId);

            var totalVotesCast = await _voteRepository.GetVotesCastByUserAsync(userId);

            var totalVotesReceived = await _voteRepository.GetVotesReceivedByUserAsync(userId);

            var contestsWon = await _contestWinnerRepository.GetContestsWonByUserAsync(userId);

            // Current activity - contests where user has participated and are still active
            var activeContests = await _contestRepository.GetActiveContestsCountForUserAsync(userId, now);

            var submissionsInProgress = await _contestRepository.GetSubmissionsInProgressCountForUserAsync(userId, now);

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
            var now = DateTime.UtcNow;

            // Get contests that are currently active (accepting submissions or voting)
            var activeContests = await _contestRepository.GetActiveContestsWithFullDataAsync(now);

            var result = new List<UserActiveContestViewModel>();

            foreach (var contest in activeContests)
            {
                // Check user participation directly from entries and votes
                var userEntry = contest.Entries.FirstOrDefault(e => e.ParticipantId == userId);

                var userVote = await _voteRepository.GetUserVoteForContestAsync(userId, contest.Id);

                // Get total votes for this contest
                var totalVotes = await _voteRepository.GetTotalVotesForContestAsync(contest.Id);

                // Determine contest phase and status
                string phase;
                string status;
                int daysRemaining;

                if (now < contest.SubmissionEndDate)
                {
                    phase = "Submission";
                    status = "Accepting Submissions";
                    daysRemaining = (int)(contest.SubmissionEndDate - now).TotalDays;
                }
                else if (now < contest.VotingEndDate)
                {
                    phase = "Voting";
                    status = "Voting Period";
                    daysRemaining = (int)(contest.VotingEndDate - now).TotalDays;
                }
                else
                {
                    phase = "Results";
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
                    ContestStatus = status,
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
            var entries = await _contestEntryRepository.GetUserSubmissionsWithFullDataAsync(userId, page, pageSize);
            var result = new List<UserSubmissionViewModel>();

            foreach (var entry in entries)
            {
                var contest = entry.Contest;
                var totalContestVotes = await _voteRepository.GetTotalVotesForContestAsync(contest.Id);

                var votesReceived = entry.Votes.Count;
                var votePercentage = totalContestVotes > 0 ? (double)votesReceived / totalContestVotes * 100 : 0;

                // Calculate ranking using repository
                var ranking = await _contestEntryRepository.GetEntryRankingInContestAsync(contest.Id, entry.Id);

                var isWinner = contest.PrimaryWinner?.ContestEntryId == entry.Id;

                // Determine status
                var now = DateTime.UtcNow;
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
            var votes = await _voteRepository.GetUserVotingHistoryAsync(userId, page, pageSize);
            var result = new List<UserVotingHistoryViewModel>();

            foreach (var vote in votes)
            {
                var contest = vote.ContestEntry.Contest;
                var entry = vote.ContestEntry;
                var now = DateTime.UtcNow;

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
                    entryFinalRanking = await _contestEntryRepository.GetEntryRankingInContestAsync(contest.Id, entry.Id);
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
                    ContestStatus = contestStatus,
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

            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return null;
            }

            return user;
        }
    }
}
