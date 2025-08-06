using DreamAquascape.Data.Models;

namespace DreamAquascape.Services.Core.Interfaces
{
    /// <summary>
    /// Service for handling all voting operations
    /// </summary>
    public interface IVotingService
    {
        /// <summary>
        /// Cast a vote for a contest entry
        /// </summary>
        Task<Vote> CastVoteAsync(int contestId, int entryId, string userId, string? ipAddress = null);

        /// <summary>
        /// Remove a vote from a contest
        /// </summary>
        Task RemoveVoteAsync(int contestId, string userId);

        /// <summary>
        /// Get user's vote in a specific contest
        /// </summary>
        Task<Vote?> GetUserVoteInContestAsync(string userId, int contestId);

        /// <summary>
        /// Check if user has voted in a specific contest
        /// </summary>
        Task<bool> HasUserVotedInContestAsync(string userId, int contestId);
    }
}
