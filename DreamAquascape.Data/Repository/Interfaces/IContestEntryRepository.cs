using DreamAquascape.Data.Models;
using System.Xml;

namespace DreamAquascape.Data.Repository.Interfaces
{
    public interface IContestEntryRepository : IRepository<ContestEntry, int>, IAsyncRepository<ContestEntry, int>
    {
        // Existing methods
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

        Task<ContestEntry?> GetContestEntryByIdAsync(int entryId);

        // New method for detailed entry view
        Task<ContestEntry?> GetEntryDetailsWithAllDataAsync(int contestId, int entryId);
        Task<IEnumerable<ContestEntry>> GetAllEntriesInContestAsync(int contestId);

        // Dashboard statistics methods
        Task<int> GetTotalEntryCountAsync();
        Task<int> GetPendingEntriesCountAsync(DateTime now);
        Task<double> GetAverageVotesPerEntryAsync();
        Task<IEnumerable<string>> GetAllParticipantIdsAsync();
        Task<IEnumerable<string>> GetParticipantIdsSinceAsync(DateTime fromDate);

        Task<IEnumerable<ContestEntry>> GetUserEntriesAsync(string userId);
        Task<IEnumerable<ContestEntry>> GetUserSubmissionsWithFullDataAsync(string userId, int page, int pageSize);
        Task<IEnumerable<int>> GetContestIdsUserEnteredAsync(string userId);
        Task<int> GetTotalEntriesSubmittedByUserAsync(string userId);
    }
}
