using DreamAquascape.Web.ViewModels.ContestEntry;

namespace DreamAquascape.Services.Core.Interfaces
{
    public interface IContestEntryService
    {
        // Entry CRUD operations
        Task<ContestEntryDetailsViewModel?> GetContestEntryDetailsAsync(int contestId, int entryId, string? currentUserId = null);
        Task<EditContestEntryViewModel?> GetContestEntryForEditAsync(int contestId, int entryId, string currentUserId);
        Task<Data.Models.ContestEntry> SubmitEntryAsync(CreateContestEntryViewModel dto, string userId, string userName);
        Task<bool> UpdateContestEntryAsync(EditContestEntryViewModel model, string currentUserId);
        Task<bool> DeleteEntryAsync(int contestId, int entryId, string currentUserId);

        // Entry queries
        Task<bool> CanUserSubmitEntryAsync(int contestId, string userId);
        Task<bool> CanUserVoteAsync(int contestId, string userId);
        Task<bool> CanUserEditEntryAsync(int contestId, int entryId, string userId);

        // Entry statistics
        Task<int> GetEntryCountByContestAsync(int contestId);
        Task<int> GetVoteCountByEntryAsync(int entryId);
        Task<Dictionary<int, int>> GetVoteCountsByContestAsync(int contestId);
    }
}
