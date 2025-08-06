using DreamAquascape.Data.Models;
using DreamAquascape.Web.ViewModels.Contest;

namespace DreamAquascape.Services.Core.Interfaces
{
    public interface IContestService
    {
        Task<IEnumerable<ContestItemViewModel>> GetActiveContestsAsync();
        Task<ContestListViewModel> GetFilteredContestsAsync(ContestFilterViewModel filters);
        Task<ContestDetailsViewModel?> GetContestWithEntriesAsync(int contestId, string? currentUserId = null);
        Task<Contest> SubmitContestAsync(CreateContestViewModel dto, PrizeViewModel prizeDto, string createdBy);

        // Winner determination
        Task<ContestWinner?> DetermineAndSetWinnerAsync(int contestId);
        Task<List<ContestWinner>> ProcessEndedContestsAsync();

        // Admin management
        Task<ContestListViewModel> GetFilteredContestsAsync(string searchTerm = "", string status = "", int page = 1, int pageSize = 10);
        Task<bool> ToggleContestStatusAsync(int contestId);
        Task<bool> DeleteContestAsync(int contestId);
        Task<EditContestViewModel?> GetContestForEditAsync(int contestId);
        Task<bool> UpdateContestAsync(EditContestViewModel model, string? newImageUrl = null, string? newPrizeImageUrl = null);
    }
}
