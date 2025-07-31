using DreamAquascape.Data.Models;

namespace DreamAquascape.Data.Repository.Interfaces
{
    public interface IContestRepository : IRepository<Contest, int>, IAsyncRepository<Contest, int>
    {
        Task<IEnumerable<Contest>> GetActiveContestsAsync();
        Task<Contest?> GetContestDetailsAsync(int contestId);
    }
}
