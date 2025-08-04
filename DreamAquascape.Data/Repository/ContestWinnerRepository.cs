using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DreamAquascape.Data.Repository
{
    public class ContestWinnerRepository : BaseRepository<ContestWinner, int>, IContestWinnerRepository
    {
        public ContestWinnerRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<ContestWinner?> GetPrimaryWinnerForContestAsync(int contestId)
        {
            return await DbSet
                .Include(w => w.ContestEntry)
                    .ThenInclude(e => e.Participant)
                .Include(w => w.ContestEntry)
                    .ThenInclude(e => e.EntryImages)
                .FirstOrDefaultAsync(w => w.ContestId == contestId && w.Position == 1);
        }

        public async Task<IEnumerable<ContestWinner>> GetWinnersForContestAsync(int contestId)
        {
            return await DbSet
                .Include(w => w.ContestEntry)
                    .ThenInclude(e => e.Participant)
                .Include(w => w.ContestEntry)
                    .ThenInclude(e => e.EntryImages)
                .Where(w => w.ContestId == contestId)
                .OrderBy(w => w.Position)
                .ToListAsync();
        }

        public async Task<IEnumerable<ContestWinner>> GetWinnersByUserAsync(string userId)
        {
            return await DbSet
                .Include(w => w.Contest)
                .Include(w => w.ContestEntry)
                    .ThenInclude(e => e.EntryImages)
                .Where(w => w.ContestEntry.ParticipantId == userId)
                .OrderByDescending(w => w.WonAt)
                .ToListAsync();
        }

        public async Task<ContestWinner?> GetWinnerByEntryAsync(int entryId)
        {
            return await DbSet
                .Include(w => w.Contest)
                .FirstOrDefaultAsync(w => w.ContestEntryId == entryId);
        }

        public async Task<bool> HasContestWinnerAsync(int contestId)
        {
            return await DbSet
                .AnyAsync(w => w.ContestId == contestId && w.Position == 1);
        }

        public async Task<bool> IsEntryWinnerAsync(int entryId)
        {
            return await DbSet
                .AnyAsync(w => w.ContestEntryId == entryId);
        }

        public async Task<bool> HasUserWonContestAsync(string userId, int contestId)
        {
            return await DbSet
                .AnyAsync(w => w.ContestId == contestId && w.ContestEntry.ParticipantId == userId);
        }

        public async Task<ContestWinner> CreateWinnerAsync(int contestId, int entryId, int position = 1, string? notes = null)
        {
            var winner = new ContestWinner
            {
                ContestId = contestId,
                ContestEntryId = entryId,
                Position = position,
                WonAt = DateTime.UtcNow,
                AwardTitle = position == 1 ? "Contest Winner" : $"Contest Winner - Position {position}",
                Notes = notes
            };

            await AddAsync(winner); // This will save immediately
            return winner;
        }

        public async Task<ContestWinner?> DetermineWinnerByVotesAsync(int contestId)
        {
            // Get the entry with the most votes
            var winnerEntry = await DbContext.ContestEntries
                .Include(e => e.Votes)
                .Where(e => e.ContestId == contestId && !e.IsDeleted)
                .OrderByDescending(e => e.Votes.Count)
                .ThenBy(e => e.SubmittedAt) // Tie-breaker: earliest submission
                .FirstOrDefaultAsync();

            if (winnerEntry == null)
                return null;

            var notes = $"Won with {winnerEntry.Votes.Count} votes";
            return await CreateWinnerAsync(contestId, winnerEntry.Id, 1, notes);
        }
    }
}