using DreamAquascape.Data.Models;

namespace DreamAquascape.Data.Repository.Interfaces
{
    public interface IContestWinnerRepository : IRepository<ContestWinner, int>, IAsyncRepository<ContestWinner, int>
    {
    }
}
