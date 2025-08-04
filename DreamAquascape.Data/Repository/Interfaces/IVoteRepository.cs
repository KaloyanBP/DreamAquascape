using DreamAquascape.Data.Models;

namespace DreamAquascape.Data.Repository.Interfaces
{
    public interface IVoteRepository : IRepository<Vote, int>, IAsyncRepository<Vote, int>
    {
        Task<Vote?> GetUserVoteInContestAsync(string userId, int contestId);
        Task<bool> HasUserVotedInContestAsync(string userId, int contestId);
    }
}
