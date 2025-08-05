using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DreamAquascape.Data.Repository
{
    public class ContestEntryRepository : BaseRepository<ContestEntry, int>, IContestEntryRepository
    {
        public ContestEntryRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<ContestEntry?> GetEntryWithAllDataAsync(int contestId, int entryId)
        {
            return await DbSet
                .Include(e => e.EntryImages.Where(img => !img.IsDeleted))
                .Include(e => e.Votes)
                    .ThenInclude(v => v.User)
                .Include(e => e.Contest)
                    .ThenInclude(c => c.Winners)
                .Include(e => e.Participant)
                .FirstOrDefaultAsync(e => e.Id == entryId &&
                                        e.ContestId == contestId &&
                                        !e.IsDeleted);
        }

        public async Task<ContestEntry?> GetEntryForEditAsync(int contestId, int entryId, string userId)
        {
            return await DbSet
                .Include(e => e.Contest)
                .Include(e => e.EntryImages.Where(img => !img.IsDeleted))
                .FirstOrDefaultAsync(e => e.Id == entryId &&
                                        e.ContestId == contestId &&
                                        e.ParticipantId == userId &&
                                        !e.IsDeleted);
        }

        public async Task<IEnumerable<ContestEntry>> GetByContestIdWithImagesAsync(int contestId)
        {
            return await DbSet
                .Include(e => e.EntryImages.Where(img => !img.IsDeleted))
                .Include(e => e.Votes)
                .Include(e => e.Participant)
                .Where(e => e.ContestId == contestId && !e.IsDeleted)
                .OrderByDescending(e => e.Votes.Count)
                .ThenByDescending(e => e.SubmittedAt)
                .ToListAsync();
        }

        public async Task<int> GetEntryCountByContestAsync(int contestId)
        {
            return await DbSet
                .CountAsync(e => e.ContestId == contestId && !e.IsDeleted);
        }

        public async Task<ContestEntry?> GetUserEntryInContestAsync(int contestId, string userId)
        {
            return await DbSet
                .Include(e => e.EntryImages.Where(img => !img.IsDeleted))
                .FirstOrDefaultAsync(e => e.ContestId == contestId &&
                                        e.ParticipantId == userId &&
                                        !e.IsDeleted);
        }

        public async Task<bool> UserHasEntryInContestAsync(int contestId, string userId)
        {
            return await DbSet
                .AnyAsync(e => e.ContestId == contestId &&
                              e.ParticipantId == userId &&
                              !e.IsDeleted);
        }

        public async Task<Dictionary<int, int>> GetVoteCountsByContestAsync(int contestId)
        {
            return await DbSet
                .Where(e => e.ContestId == contestId && !e.IsDeleted)
                .Select(e => new { e.Id, VoteCount = e.Votes.Count })
                .ToDictionaryAsync(x => x.Id, x => x.VoteCount);
        }

        public async Task<int> GetVoteCountByEntryAsync(int entryId)
        {
            var entry = await DbSet
                .Include(e => e.Votes)
                .FirstOrDefaultAsync(e => e.Id == entryId && !e.IsDeleted);

            return entry?.Votes.Count ?? 0;
        }

        public async Task<int> GetEntryRankingInContestAsync(int contestId, int entryId)
        {
            var allEntries = await DbSet
                .Where(e => e.ContestId == contestId && !e.IsDeleted)
                .Include(e => e.Votes)
                .ToListAsync();

            var rankedEntries = allEntries
                .OrderByDescending(e => e.Votes.Count)
                .ThenBy(e => e.SubmittedAt)
                .ToList();

            return rankedEntries.FindIndex(e => e.Id == entryId) + 1;
        }

        public async Task<ContestEntry?> GetContestEntryByIdAsync(int entryId)
        {
            return await DbSet
                .FirstOrDefaultAsync(e => e.Id == entryId && !e.IsDeleted);
        }

        public async Task<ContestEntry?> GetEntryDetailsWithAllDataAsync(int contestId, int entryId)
        {
            return await DbSet
                .Include(e => e.Contest)
                    .ThenInclude(c => c.Winners)
                .Include(e => e.Participant)
                .Include(e => e.EntryImages)
                .Include(e => e.Votes)
                    .ThenInclude(v => v.User)
                .Include(e => e.Winner)
                .FirstOrDefaultAsync(e => e.Id == entryId && e.ContestId == contestId && !e.IsDeleted);
        }

        public async Task<IEnumerable<ContestEntry>> GetAllEntriesInContestAsync(int contestId)
        {
            return await DbSet
                .Where(e => e.ContestId == contestId && !e.IsDeleted)
                .Include(e => e.Votes)
                .Include(e => e.Participant)
                .Include(e => e.EntryImages)
                .ToListAsync();
        }

        public async Task<int> GetTotalEntryCountAsync()
        {
            return await DbSet.CountAsync(e => !e.IsDeleted);
        }

        public async Task<int> GetPendingEntriesCountAsync(DateTime now)
        {
            return await DbSet.CountAsync(e =>
                !e.IsDeleted && e.IsActive &&
                e.Contest.SubmissionStartDate <= now && e.Contest.SubmissionEndDate >= now);
        }

        public async Task<double> GetAverageVotesPerEntryAsync()
        {
            var entriesWithVotes = await DbSet
                .Where(e => !e.IsDeleted)
                .Select(e => new { e.Id, VoteCount = e.Votes.Count() })
                .ToListAsync();

            return entriesWithVotes.Any()
                ? entriesWithVotes.Average(e => e.VoteCount)
                : 0;
        }

        public async Task<IEnumerable<string>> GetAllParticipantIdsAsync()
        {
            return await DbSet
                .Select(e => e.ParticipantId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetParticipantIdsSinceAsync(DateTime fromDate)
        {
            return await DbSet
                .Where(e => e.SubmittedAt >= fromDate)
                .Select(e => e.ParticipantId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<ContestEntry>> GetUserEntriesAsync(string userId)
        {
            return await DbSet
                .Where(e => e.ParticipantId == userId && !e.IsDeleted)
                .Include(e => e.Contest)
                .ToListAsync();
        }

        public async Task<IEnumerable<ContestEntry>> GetUserSubmissionsWithFullDataAsync(string userId, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            return await DbSet
                .Where(e => e.ParticipantId == userId && !e.IsDeleted)
                .Include(e => e.Contest)
                .Include(e => e.EntryImages)
                .Include(e => e.Votes)
                .OrderByDescending(e => e.SubmittedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<int>> GetContestIdsUserEnteredAsync(string userId)
        {
            return await DbSet
                .Where(e => e.ParticipantId == userId && !e.IsDeleted)
                .Select(e => e.ContestId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<int> GetTotalEntriesSubmittedByUserAsync(string userId)
        {
            return await DbSet
                .Where(e => e.ParticipantId == userId && !e.IsDeleted)
                .CountAsync();
        }
    }
}
