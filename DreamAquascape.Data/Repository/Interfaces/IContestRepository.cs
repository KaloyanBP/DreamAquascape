using DreamAquascape.Data.Models;
using DreamAquascape.Web.ViewModels.Contest;

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

        // New methods for filtered queries and statistics
        Task<(IEnumerable<Contest> contests, int totalCount)> GetFilteredContestsAsync(ContestFilterViewModel filters);
        Task<ContestStatsViewModel> GetContestStatsAsync();

        // Contest creation method
        Task<Contest> CreateContestWithPrizeAsync(Contest contest, Prize prize);

        // Dashboard statistics methods
        Task<int> GetTotalContestCountAsync();
        Task<int> GetActiveContestCountAsync();
        Task<int> GetContestsEndingSoonCountAsync(DateTime now, DateTime endDate);
        Task<double> GetAverageEntriesPerContestAsync();
    }
}
