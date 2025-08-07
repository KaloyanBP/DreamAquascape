using DreamAquascape.Web.ViewModels.Contest;

namespace DreamAquascape.Services.Core.Interfaces
{
    /// <summary>
    /// Service for contest query operations (read-only)
    /// </summary>
    public interface IContestQueryService
    {
        Task<ContestListViewModel> GetFilteredContestsAsync(ContestFilterViewModel filters);
        Task<ContestDetailsViewModel?> GetContestDetailsAsync(int contestId, string? currentUserId = null);
        Task<EditContestViewModel?> GetContestForEditAsync(int contestId);
        Task<ContestStatsViewModel> GetContestStatsAsync();
    }
}
