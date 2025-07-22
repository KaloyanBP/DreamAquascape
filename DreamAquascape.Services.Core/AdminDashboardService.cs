using DreamAquascape.Data;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.AdminDashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AquascapingContest.AdminDashboard.Services
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

            // User participation (assuming you have user tracking)
            var totalUsers = await _context.UserContestParticipations
                .Select(p => p.UserId)
                .Distinct()
                .CountAsync();

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

            var userEngagementRate = totalUsers > 0
                ? (double)await _context.UserContestParticipations
                    .Where(p => p.ParticipationDate >= thirtyDaysAgo)
                    .Select(p => p.UserId)
                    .Distinct()
                    .CountAsync() / totalUsers * 100
                : 0;

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
    }
}
