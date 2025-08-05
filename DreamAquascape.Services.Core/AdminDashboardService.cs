using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.AdminDashboard;
using Microsoft.Extensions.Logging;

namespace DreamAquascape.Services.Core.AdminDashboard
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IContestRepository _contestRepository;
        private readonly IContestEntryRepository _contestEntryRepository;
        private readonly IVoteRepository _voteRepository;
        private readonly IPrizeRepository _prizeRepository;
        private readonly ILogger<AdminDashboardService> _logger;

        public AdminDashboardService(
            IContestRepository contestRepository,
            IContestEntryRepository contestEntryRepository,
            IVoteRepository voteRepository,
            IPrizeRepository prizeRepository,
            IContestWinnerRepository contestWinnerRepository,
            ILogger<AdminDashboardService> logger)
        {
            _contestRepository = contestRepository ?? throw new ArgumentNullException(nameof(contestRepository));
            _contestEntryRepository = contestEntryRepository ?? throw new ArgumentNullException(nameof(contestEntryRepository));
            _voteRepository = voteRepository ?? throw new ArgumentNullException(nameof(voteRepository));
            _prizeRepository = prizeRepository ?? throw new ArgumentNullException(nameof(prizeRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DashboardStatsViewModel> GetDashboardStatsAsync()
        {
            var now = DateTime.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);

            // Basic counts using repositories
            var totalContests = await _contestRepository.CountAsync();
            var activeContests = await _contestRepository.GetActiveContestCountAsync();
            var totalEntries = await _contestEntryRepository.CountAsync();
            var totalVotes = await _voteRepository.CountAsync();
            var totalUsers = await GetTotalUniqueParticipantsAsync();

            var pendingEntries = await _contestEntryRepository.GetPendingEntriesCountAsync(now);
            var contestsEndingSoon = await _contestRepository.GetContestsEndingSoonCountAsync(now, now.AddDays(7));
            var totalPrizeValue = await _prizeRepository.GetTotalActivePrizeValueAsync();

            // Performance metrics
            var averageEntriesPerContest = await _contestRepository.GetAverageEntriesPerContestAsync();
            var averageVotesPerEntry = await _contestEntryRepository.GetAverageVotesPerEntryAsync();
            var userEngagementRate = await GetUserEngagementRateAsync(thirtyDaysAgo);

            return new DashboardStatsViewModel
            {
                TotalContests = totalContests,
                ActiveContests = activeContests,
                TotalEntries = totalEntries,
                TotalVotes = totalVotes,
                TotalUsers = totalUsers,
                PendingEntries = pendingEntries,
                ContestsEndingSoon = contestsEndingSoon,
                TotalPrizeValue = totalPrizeValue,
                AverageEntriesPerContest = averageEntriesPerContest,
                AverageVotesPerEntry = averageVotesPerEntry,
                UserEngagementRate = userEngagementRate,
            };
        }

        private async Task<int> GetTotalUniqueParticipantsAsync()
        {
            // Get all unique user IDs who have either submitted entries or cast votes
            var entryParticipants = await _contestEntryRepository.GetAllParticipantIdsAsync();
            var voteParticipants = await _voteRepository.GetAllVoterIdsAsync();

            var totalUsers = entryParticipants.Union(voteParticipants).Distinct().Count();
            return totalUsers;
        }

        private async Task<double> GetUserEngagementRateAsync(DateTime fromDate)
        {
            var totalUsers = await GetTotalUniqueParticipantsAsync();

            if (totalUsers == 0)
                return 0;

            // Get unique users who participated since the specified date
            var activeEntryParticipants = await _contestEntryRepository.GetParticipantIdsSinceAsync(fromDate);
            var activeVoteParticipants = await _voteRepository.GetVoterIdsSinceAsync(fromDate);

            var activeUsers = activeEntryParticipants.Union(activeVoteParticipants).Distinct().Count();
            var engagementRate = (double)activeUsers / totalUsers * 100;

            return engagementRate;
        }
    }
}
