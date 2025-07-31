using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;

namespace DreamAquascape.Data.Repository
{
    public class PrizeRepository: BaseRepository<Prize, int>, IPrizeRepository   
    {
        public PrizeRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
