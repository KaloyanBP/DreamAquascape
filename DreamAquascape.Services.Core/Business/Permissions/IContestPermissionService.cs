namespace DreamAquascape.Services.Core.Business.Permissions
{
    /// <summary>
    /// Handles user permission checks for various operations
    /// </summary>
    public interface IContestPermissionService
    {
        /// <summary>
        /// Checks if user can vote in a specific contest (hasn't voted yet)
        /// Note: The check for not voting on their own entry is done at the entry level
        /// </summary>
        Task<bool> CanUserVoteInContestAsync(string userId, int contestId);

        /// <summary>
        /// Checks if user has already voted in a specific contest
        /// </summary>
        Task<bool> HasUserVotedInContestAsync(string userId, int contestId);

        /// <summary>
        /// Checks if user owns an entry in a specific contest
        /// </summary>
        Task<bool> DoesUserOwnEntryInContestAsync(string userId, int contestId);

        /// <summary>
        /// Checks if user can edit a specific contest entry
        /// </summary>
        Task<bool> CanUserEditEntryAsync(string userId, int entryId);
    }
}
