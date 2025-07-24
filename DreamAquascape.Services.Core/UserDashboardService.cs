using DreamAquascape.Data;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.UserDashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DreamAquascape.Services.Core
{
    public class UserDashboardService: IUserDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserDashboardService> _logger;

        public UserDashboardService(ApplicationDbContext context, ILogger<UserDashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserQuickStatsViewModel> GetUserQuickStatsAsync(string userId)
        {
            var now = DateTime.UtcNow;

            // Basic counts
            var totalContestsParticipated = await _context.UserContestParticipations
                .Where(p => p.UserId == userId)
                .Select(p => p.ContestId)
                .Distinct()
                .CountAsync();

            var totalEntriesSubmitted = await _context.ContestEntries
                .CountAsync(e => e.ParticipantId == userId && !e.IsDeleted);

            var totalVotesCast = await _context.Votes
                .CountAsync(v => v.UserId == userId);

            var totalVotesReceived = await _context.Votes
                .Where(v => v.ContestEntry.ParticipantId == userId)
                .CountAsync();

            var contestsWon = await _context.Contests
                .Where(c => c.WinnerEntry != null && c.WinnerEntry.ParticipantId == userId)
                .CountAsync();

            // Current activity
            var activeContests = await _context.Contests
                .Where(c => c.IsActive && !c.IsDeleted &&
                           c.SubmissionStartDate <= now && c.VotingEndDate >= now)
                .Join(_context.UserContestParticipations.Where(p => p.UserId == userId),
                      c => c.Id,
                      p => p.ContestId,
                      (c, p) => c)
                .CountAsync();

            var submissionsInProgress = await _context.Contests
                .Where(c => c.IsActive && !c.IsDeleted &&
                           c.SubmissionStartDate <= now && c.SubmissionEndDate >= now &&
                           !c.Entries.Any(e => e.ParticipantId == userId && !e.IsDeleted))
                .CountAsync();

            // Performance metrics
            var averageVotesPerEntry = totalEntriesSubmitted > 0 ? (double)totalVotesReceived / totalEntriesSubmitted : 0;
            var winRate = totalContestsParticipated > 0 ? (double)contestsWon / totalContestsParticipated * 100 : 0;

            var currentStreak = 0;

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

    }
}
