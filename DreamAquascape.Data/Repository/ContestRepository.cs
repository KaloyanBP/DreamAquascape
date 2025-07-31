using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DreamAquascape.Data.Repository
{
    public class ContestRepository: BaseRepository<Contest, int>, IContestRepository   
    {
        public ContestRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Contest>> GetActiveContestsAsync()
        {
            var now = DateTime.UtcNow;
            return await DbSet
                .Where(c => c.IsActive && !c.IsDeleted && c.SubmissionStartDate <= now && c.SubmissionEndDate >= now)
                .OrderByDescending(c => c.SubmissionStartDate)
                .ToListAsync();
        }

        public async Task<Contest?> GetContestDetailsAsync(int contestId)
        {
            var contest = await DbSet
                .Include(c => c.Entries)
                    .ThenInclude(e => e.Votes)
                .Include(c => c.Entries)
                    .ThenInclude(e => e.EntryImages)
                .Include(c => c.Prizes)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == contestId && c.IsActive && !c.IsDeleted);
            return contest;
        }
    }
}
