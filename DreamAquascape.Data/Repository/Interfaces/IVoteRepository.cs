using DreamAquascape.Data.Models;

namespace DreamAquascape.Data.Repository.Interfaces
{
    public interface IVoteRepository : IRepository<Vote, int>, IAsyncRepository<Vote, int>
    {
        Task<Vote?> GetUserVoteInContestAsync(string userId, int contestId);
        Task<bool> HasUserVotedInContestAsync(string userId, int contestId);
        Task<Vote?> GetUserVoteForEntryAsync(string userId, int entryId);

        // Dashboard statistics methods
        Task<IEnumerable<string>> GetAllVoterIdsAsync();
        Task<IEnumerable<string>> GetVoterIdsSinceAsync(DateTime fromDate);

        // User dashboard methods
        Task<IEnumerable<Vote>> GetUserVotingHistoryAsync(string userId, int page, int pageSize);
        Task<int> GetTotalVotesForContestAsync(int contestId);
        Task<Vote?> GetUserVoteForContestAsync(string userId, int contestId);
        Task<int> GetVotesReceivedByUserAsync(string userId);
        Task<int> GetVotesCastByUserAsync(string userId);
        Task<IEnumerable<int>> GetContestIdsUserVotedInAsync(string userId);
    }
}
