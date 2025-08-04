using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DreamAquascape.Data.Repository
{
    public class VoteRepository : BaseRepository<Vote, int>, IVoteRepository
    {
        public VoteRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Vote?> GetUserVoteInContestAsync(string userId, int contestId)
        {
            return await DbSet
                .Include(v => v.ContestEntry)
                .FirstOrDefaultAsync(v => v.UserId == userId && v.ContestEntry.ContestId == contestId);
        }

        public async Task<bool> HasUserVotedInContestAsync(string userId, int contestId)
        {
            return await DbSet
                .AnyAsync(v => v.UserId == userId && v.ContestEntry.ContestId == contestId);
        }

        public async Task<Vote?> GetUserVoteForEntryAsync(string userId, int entryId)
        {
            return await DbSet
                .FirstOrDefaultAsync(v => v.UserId == userId && v.ContestEntryId == entryId);
        }
    }
}
