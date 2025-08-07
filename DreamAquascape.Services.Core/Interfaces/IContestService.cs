using DreamAquascape.Data.Models;
using DreamAquascape.Web.ViewModels.Contest;

namespace DreamAquascape.Services.Core.Interfaces
{
    public interface IContestService
    {
        Task<Contest> SubmitContestAsync(CreateContestViewModel dto, PrizeViewModel prizeDto, string createdBy);

        // Winner determination
        Task<ContestWinner?> DetermineAndSetWinnerAsync(int contestId);
        Task<List<ContestWinner>> ProcessEndedContestsAsync();

        // Admin management
        Task<bool> ToggleContestActiveStatusAsync(int contestId);
        Task<bool> DeleteContestAsync(int contestId);
        Task<bool> UpdateContestAsync(EditContestViewModel model);
    }
}
