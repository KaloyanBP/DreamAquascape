using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.AdminDashboard;
using Microsoft.Extensions.Logging;

namespace DreamAquascape.Services.Core.AdminDashboard
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminDashboardService> _logger;

        public AdminDashboardService(
            IUnitOfWork unitOfWork,
            ILogger<AdminDashboardService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DashboardStatsViewModel> GetDashboardStatsAsync()
        {
            var now = DateTime.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);

            // Basic counts using repositories
            var totalContests = await _unitOfWork.ContestRepository.CountAsync();
            var activeContests = await _unitOfWork.ContestRepository.GetActiveContestCountAsync();
            var totalEntries = await _unitOfWork.ContestEntryRepository.CountAsync();
            var totalVotes = await _unitOfWork.VoteRepository.CountAsync();
            var totalUsers = await GetTotalUniqueParticipantsAsync();

            var pendingEntries = await _unitOfWork.ContestEntryRepository.GetPendingEntriesCountAsync(now);
            var contestsEndingSoon = await _unitOfWork.ContestRepository.GetContestsEndingSoonCountAsync(now, now.AddDays(7));
            var totalPrizeValue = await _unitOfWork.PrizeRepository.GetTotalActivePrizeValueAsync();

            // Performance metrics
            var averageEntriesPerContest = await _unitOfWork.ContestRepository.GetAverageEntriesPerContestAsync();
            var averageVotesPerEntry = await _unitOfWork.ContestEntryRepository.GetAverageVotesPerEntryAsync();
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
            var entryParticipants = await _unitOfWork.ContestEntryRepository.GetAllParticipantIdsAsync();
            var voteParticipants = await _unitOfWork.VoteRepository.GetAllVoterIdsAsync();

            var totalUsers = entryParticipants.Union(voteParticipants).Distinct().Count();
            return totalUsers;
        }

        private async Task<double> GetUserEngagementRateAsync(DateTime fromDate)
        {
            var totalUsers = await GetTotalUniqueParticipantsAsync();

            if (totalUsers == 0)
                return 0;

            // Get unique users who participated since the specified date
            var activeEntryParticipants = await _unitOfWork.ContestEntryRepository.GetParticipantIdsSinceAsync(fromDate);
            var activeVoteParticipants = await _unitOfWork.VoteRepository.GetVoterIdsSinceAsync(fromDate);

            var activeUsers = activeEntryParticipants.Union(activeVoteParticipants).Distinct().Count();
            var engagementRate = (double)activeUsers / totalUsers * 100;

            return engagementRate;
        }
    }
}
