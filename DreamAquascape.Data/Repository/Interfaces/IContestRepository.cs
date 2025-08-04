using DreamAquascape.Data.Models;

namespace DreamAquascape.Data.Repository.Interfaces
{
    public interface IContestRepository : IRepository<Contest, int>, IAsyncRepository<Contest, int>
    {
        Task<IEnumerable<Contest>> GetActiveContestsAsync();
        Task<Contest?> GetContestDetailsAsync(int contestId);

        Task<Contest?> GetContestForToggleAsync(int contestId);
        Task<Contest?> GetContestForDeleteAsync(int contestId);
        Task<Contest?> GetContestForEditAsync(int contestId);
        Task<Contest?> GetContestForWinnerDeterminationAsync(int contestId);
        Task<IEnumerable<Contest>> GetEndedContestsWithoutWinnersAsync();
    }
}
