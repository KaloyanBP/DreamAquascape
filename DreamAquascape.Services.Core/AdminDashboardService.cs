using DreamAquascape.Data;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.AdminDashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DreamAquascape.Services.Core.AdminDashboard
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminDashboardService> _logger;

        public AdminDashboardService(ApplicationDbContext context, ILogger<AdminDashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DashboardStatsViewModel> GetDashboardStatsAsync()
        {
            var now = DateTime.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);

            // Basic counts
            var totalContests = await _context.Contests.CountAsync(c => !c.IsDeleted);
            var activeContests = await _context.Contests.CountAsync(c =>
                c.IsActive && !c.IsDeleted &&
                c.SubmissionStartDate <= now && c.VotingEndDate >= now);

            var totalEntries = await _context.ContestEntries.CountAsync(e => !e.IsDeleted);
            var totalVotes = await _context.Votes.CountAsync();

            var totalUsers = await GetTotalUniqueParticipantsAsync();

            var pendingEntries = await _context.ContestEntries.CountAsync(e =>
                !e.IsDeleted && e.IsActive &&
                e.Contest.SubmissionStartDate <= now && e.Contest.SubmissionEndDate >= now);

            var contestsEndingSoon = await _context.Contests.CountAsync(c =>
                c.IsActive && !c.IsDeleted &&
                c.VotingEndDate <= now.AddDays(7) && c.VotingEndDate >= now);

            var totalPrizeValue = await _context.Prizes
                .Where(p => p.Contest.IsActive && !p.Contest.IsDeleted)
                .SumAsync(p => p.MonetaryValue ?? 0);

            // Performance metrics
            var contestsWithEntries = await _context.Contests
                .Where(c => !c.IsDeleted)
                .Select(c => new { c.Id, EntryCount = c.Entries.Count(e => !e.IsDeleted) })
                .ToListAsync();

            var averageEntriesPerContest = contestsWithEntries.Any()
                ? contestsWithEntries.Average(c => c.EntryCount)
                : 0;

            var entriesWithVotes = await _context.ContestEntries
                .Where(e => !e.IsDeleted)
                .Select(e => new { e.Id, VoteCount = e.Votes.Count() })
                .ToListAsync();

            var averageVotesPerEntry = entriesWithVotes.Any()
                ? entriesWithVotes.Average(e => e.VoteCount)
                : 0;

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
            var totalUsers = await (
                _context.ContestEntries
                    .Select(e => e.ParticipantId)
                    .Union(
                        _context.Votes.Select(v => v.UserId)
                    )
                ).Distinct().CountAsync();

            return totalUsers;
        }

        private async Task<double> GetUserEngagementRateAsync(DateTime fromDate)
        {
            var totalUsers = await GetTotalUniqueParticipantsAsync();

            if (totalUsers == 0)
                return 0;

            // Get unique users who participated since the specified date
            var activeUsers = await (
                _context.ContestEntries
                    .Where(e => e.SubmittedAt >= fromDate)
                    .Select(e => e.ParticipantId)
                    .Union(
                        _context.Votes
                            .Where(v => v.VotedAt >= fromDate)
                            .Select(v => v.UserId)
                    )
                ).Distinct().CountAsync();

            var engagementRate = (double)activeUsers / totalUsers * 100;

            return engagementRate;
        }
    }
}
