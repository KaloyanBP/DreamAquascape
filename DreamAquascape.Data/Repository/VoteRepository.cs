using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.GCommon.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DreamAquascape.Data.Repository
{
    public class VoteRepository : BaseRepository<Vote, int>, IVoteRepository
    {
        public VoteRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext, dateTimeProvider)
        {
        }

        public async Task<Vote?> GetUserVoteInContestAsync(string userId, int contestId)
        {
            return await GetAllAttached()
                .Include(v => v.ContestEntry)
                .FirstOrDefaultAsync(v => v.UserId == userId && v.ContestEntry.ContestId == contestId);
        }

        public async Task<bool> HasUserVotedInContestAsync(string userId, int contestId)
        {
            return await GetAllAttached()
                .AnyAsync(v => v.UserId == userId && v.ContestEntry.ContestId == contestId);
        }

        public async Task<Vote?> GetUserVoteForEntryAsync(string userId, int entryId)
        {
            return await FirstOrDefaultAsync(v => v.UserId == userId && v.ContestEntryId == entryId);
        }

        public async Task<IEnumerable<string>> GetAllVoterIdsAsync()
        {
            return await GetAllAttached()
                .Select(v => v.UserId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetVoterIdsSinceAsync(DateTime fromDate)
        {
            return await GetAllAttached()
                .Where(v => v.VotedAt >= fromDate)
                .Select(v => v.UserId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<Vote>> GetUserVotingHistoryAsync(string userId, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            return await GetAllAttached()
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
        }

        public async Task<int> GetTotalVotesForContestAsync(int contestId)
        {
            return await GetAllAttached()
                .Where(v => v.ContestEntry.ContestId == contestId)
                .CountAsync();
        }

        public async Task<Vote?> GetUserVoteForContestAsync(string userId, int contestId)
        {
            return await GetAllAttached()
                .Where(v => v.UserId == userId && v.ContestEntry.ContestId == contestId)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetVotesReceivedByUserAsync(string userId)
        {
            return await GetAllAttached()
                .Where(v => v.ContestEntry.ParticipantId == userId)
                .CountAsync();
        }

        public async Task<int> GetVotesCastByUserAsync(string userId)
        {
            return await GetAllAttached()
                .Where(v => v.UserId == userId)
                .CountAsync();
        }

        public async Task<IEnumerable<int>> GetContestIdsUserVotedInAsync(string userId)
        {
            return await GetAllAttached()
                .Where(v => v.UserId == userId)
                .Select(v => v.ContestEntry.ContestId)
                .Distinct()
                .ToListAsync();
        }
    }
}
