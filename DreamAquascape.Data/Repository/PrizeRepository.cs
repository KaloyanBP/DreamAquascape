using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.GCommon.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DreamAquascape.Data.Repository
{
    public class PrizeRepository : BaseRepository<Prize, int>, IPrizeRepository
    {
        public PrizeRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext, dateTimeProvider)
        {
        }

        public async Task<decimal> GetTotalActivePrizeValueAsync()
        {
            return await GetAllAttached()
                .Where(p => p.Contest.IsActive && !p.Contest.IsDeleted)
                .SumAsync(p => p.MonetaryValue ?? 0);
        }
    }
}
