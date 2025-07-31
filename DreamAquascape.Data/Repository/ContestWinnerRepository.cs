using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;

namespace DreamAquascape.Data.Repository
{
    public class ContestWinnerRepository : BaseRepository<ContestWinner, int>, IContestWinnerRepository   
    {
        public ContestWinnerRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
