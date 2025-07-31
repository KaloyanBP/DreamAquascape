using DreamAquascape.Data.Models;

namespace DreamAquascape.Data.Repository.Interfaces
{
    public interface IContestEntryRepository : IRepository<ContestEntry, int>, IAsyncRepository<ContestEntry, int>
    {
    }
}
