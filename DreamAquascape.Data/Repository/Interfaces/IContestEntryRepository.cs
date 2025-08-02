using DreamAquascape.Data.Models;

namespace DreamAquascape.Data.Repository.Interfaces
{
    public interface IContestEntryRepository : IRepository<ContestEntry, int>, IAsyncRepository<ContestEntry, int>
    {
        Task<ContestEntry?> GetEntryWithAllDataAsync(int contestId, int entryId);
        Task<ContestEntry?> GetEntryForEditAsync(int contestId, int entryId, string userId);

        // Contest-based queries
        Task<IEnumerable<ContestEntry>> GetByContestIdWithImagesAsync(int contestId);
        Task<int> GetEntryCountByContestAsync(int contestId);

        // User-based queries
        Task<ContestEntry?> GetUserEntryInContestAsync(int contestId, string userId);
        Task<bool> UserHasEntryInContestAsync(int contestId, string userId);

        // Statistics methods
        Task<Dictionary<int, int>> GetVoteCountsByContestAsync(int contestId);
        Task<int> GetVoteCountByEntryAsync(int entryId);
        Task<int> GetEntryRankingInContestAsync(int contestId, int entryId);
    }
}
