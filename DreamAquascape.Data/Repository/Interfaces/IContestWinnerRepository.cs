using DreamAquascape.Data.Models;

namespace DreamAquascape.Data.Repository.Interfaces
{
    public interface IContestWinnerRepository : IRepository<ContestWinner, int>, IAsyncRepository<ContestWinner, int>
    {
        // Winner queries
        Task<ContestWinner?> GetPrimaryWinnerForContestAsync(int contestId);
        Task<IEnumerable<ContestWinner>> GetWinnersForContestAsync(int contestId);
        Task<IEnumerable<ContestWinner>> GetWinnersByUserAsync(string userId);
        Task<ContestWinner?> GetWinnerByEntryAsync(int entryId);

        // Winner validation
        Task<bool> HasContestWinnerAsync(int contestId);
        Task<bool> IsEntryWinnerAsync(int entryId);
        Task<bool> HasUserWonContestAsync(string userId, int contestId);

        // Winner management
        Task<ContestWinner> CreateWinnerAsync(int contestId, int entryId, int position = 1, string? notes = null);
        Task<ContestWinner?> DetermineWinnerByVotesAsync(int contestId);

        // User dashboard methods
        Task<int> GetContestsWonByUserAsync(string userId);
    }
}
