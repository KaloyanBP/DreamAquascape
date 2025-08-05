using DreamAquascape.Data;
using DreamAquascape.Data.Models;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.UserDashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DreamAquascape.Services.Core
{
    public class UserDashboardService : IUserDashboardService
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

            // Get user entries and votes
            var userEntries = await _context.ContestEntries
                .Where(e => e.ParticipantId == userId && !e.IsDeleted)
                .Include(e => e.Contest)
                .ToListAsync();

            var userVotes = await _context.Votes
                .Where(v => v.UserId == userId)
                .Include(v => v.ContestEntry)
                    .ThenInclude(e => e.Contest)
                .ToListAsync();

            // Basic counts
            var totalContestsParticipated = userEntries.Select(e => e.ContestId)
                .Union(userVotes.Select(v => v.ContestEntry.ContestId))
                .Distinct()
                .Count();

            var totalEntriesSubmitted = userEntries.Count;

            var totalVotesCast = userVotes.Count;

            var totalVotesReceived = await _context.Votes
                .Where(v => v.ContestEntry.ParticipantId == userId)
                .CountAsync();

            var contestsWon = await _context.ContestWinners
                .Where(cw => cw.ContestEntry.ParticipantId == userId)
                .CountAsync();

            // Current activity - contests where user has participated and are still active
            var participatedContestIds = userEntries.Select(e => e.ContestId)
                .Union(userVotes.Select(v => v.ContestEntry.ContestId))
                .Distinct()
                .ToList();

            var activeContests = await _context.Contests
                .Where(c => c.IsActive && !c.IsDeleted &&
                           c.SubmissionStartDate <= now && c.VotingEndDate >= now &&
                           participatedContestIds.Contains(c.Id))
                .CountAsync();

            var submissionsInProgress = await _context.Contests
                .Where(c => c.IsActive && !c.IsDeleted &&
                           c.SubmissionStartDate <= now && c.SubmissionEndDate >= now &&
                           !c.Entries.Any(e => e.ParticipantId == userId && !e.IsDeleted))
                .CountAsync();

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
            var activeContests = await _context.Contests
                .Where(c => c.IsActive && !c.IsDeleted &&
                           c.SubmissionStartDate <= now && c.VotingEndDate >= now)
                .Include(c => c.Categories)
                .Include(c => c.Prizes)
                .Include(c => c.Entries.Where(e => !e.IsDeleted))
                .ToListAsync();

            var result = new List<UserActiveContestViewModel>();

            foreach (var contest in activeContests)
            {
                // Check user participation directly from entries and votes
                var userEntry = contest.Entries.FirstOrDefault(e => e.ParticipantId == userId);

                var userVote = await _context.Votes
                    .Where(v => v.UserId == userId && v.ContestEntry.ContestId == contest.Id)
                    .FirstOrDefaultAsync();

                // Get total votes for this contest
                var totalVotes = await _context.Votes
                    .Where(v => v.ContestEntry.ContestId == contest.Id)
                    .CountAsync();

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
            var skip = (page - 1) * pageSize;

            var entries = await _context.ContestEntries
                .Where(e => e.ParticipantId == userId && !e.IsDeleted)
                .Include(e => e.Contest)
                .Include(e => e.EntryImages)
                .Include(e => e.Votes)
                .OrderByDescending(e => e.SubmittedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var result = new List<UserSubmissionViewModel>();

            foreach (var entry in entries)
            {
                var contest = entry.Contest;
                var totalContestVotes = await _context.Votes
                    .Where(v => v.ContestEntry.ContestId == contest.Id)
                    .CountAsync();

                var votesReceived = entry.Votes.Count;
                var votePercentage = totalContestVotes > 0 ? (double)votesReceived / totalContestVotes * 100 : 0;

                // Calculate ranking
                var contestEntries = await _context.ContestEntries
                    .Where(e => e.ContestId == contest.Id && !e.IsDeleted)
                    .Include(e => e.Votes)
                    .ToListAsync();

                var ranking = contestEntries
                    .OrderByDescending(e => e.Votes.Count)
                    .Select((e, index) => new { Entry = e, Rank = index + 1 })
                    .FirstOrDefault(x => x.Entry.Id == entry.Id)?.Rank ?? contestEntries.Count;

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
            var skip = (page - 1) * pageSize;

            var votes = await _context.Votes
                .Where(v => v.UserId == userId)
                .Include(v => v.ContestEntry)
                    .ThenInclude(e => e.Contest)
                .Include(v => v.ContestEntry)
                    .ThenInclude(e => e.EntryImages)
                .Include(v => v.ContestEntry)
                    .ThenInclude(e => e.Participant)
                .OrderByDescending(v => v.VotedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

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

                    // Calculate final ranking
                    var contestEntries = await _context.ContestEntries
                        .Where(e => e.ContestId == contest.Id && !e.IsDeleted)
                        .Include(e => e.Votes)
                        .ToListAsync();

                    entryFinalRanking = contestEntries
                        .OrderByDescending(e => e.Votes.Count)
                        .Select((e, index) => new { Entry = e, Rank = index + 1 })
                        .FirstOrDefault(x => x.Entry.Id == entry.Id)?.Rank;
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
            var user = await _context.Users
                .OfType<ApplicationUser>() // Ensure the query is filtered to ApplicationUser
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return null;
            }

            return user;
        }
    }
}
