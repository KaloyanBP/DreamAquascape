using DreamAquascape.Web.ViewModels.ContestEntry;

namespace DreamAquascape.Services.Core.Interfaces
{
    /// <summary>
    /// Service for contest entry query operations
    /// </summary>
    public interface IContestEntryQueryService
    {
        /// <summary>
        /// Gets detailed information about a specific contest entry
        /// </summary>
        /// <param name="contestId">The contest ID</param>
        /// <param name="entryId">The entry ID</param>
        /// <param name="currentUserId">The current user ID for permission calculations</param>
        /// <returns>Contest entry details or null if not found</returns>
        Task<ContestEntryDetailsViewModel?> GetContestEntryDetailsAsync(int contestId, int entryId, string? currentUserId = null);

        /// <summary>
        /// Gets a contest entry for editing
        /// </summary>
        /// <param name="contestId">The contest ID</param>
        /// <param name="entryId">The entry ID</param>
        /// <param name="currentUserId">The current user ID for ownership validation</param>
        /// <returns>Contest entry edit view model or null if not found/not allowed</returns>
        Task<EditContestEntryViewModel?> GetContestEntryForEditAsync(int contestId, int entryId, string currentUserId);

        /// <summary>
        /// Gets the total number of entries in a contest
        /// </summary>
        /// <param name="contestId">The contest ID</param>
        /// <returns>Number of entries</returns>
        Task<int> GetEntryCountByContestAsync(int contestId);

        /// <summary>
        /// Gets the total number of votes for a specific entry
        /// </summary>
        /// <param name="entryId">The entry ID</param>
        /// <returns>Number of votes</returns>
        Task<int> GetVoteCountByEntryAsync(int entryId);

        /// <summary>
        /// Gets vote counts for all entries in a contest
        /// </summary>
        /// <param name="contestId">The contest ID</param>
        /// <returns>Dictionary mapping entry ID to vote count</returns>
        Task<Dictionary<int, int>> GetVoteCountsByContestAsync(int contestId);
    }
}
