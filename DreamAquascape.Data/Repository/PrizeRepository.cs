using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DreamAquascape.Data.Repository
{
    public class PrizeRepository : BaseRepository<Prize, int>, IPrizeRepository
    {
        public PrizeRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<decimal> GetTotalActivePrizeValueAsync()
        {
            return await DbSet
                .Where(p => p.Contest.IsActive && !p.Contest.IsDeleted)
                .SumAsync(p => p.MonetaryValue ?? 0);
        }
    }
}
