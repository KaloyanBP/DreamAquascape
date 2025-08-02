using DreamAquascape.Data;
using DreamAquascape.Data.Models;
using DreamAquascape.Services.Common.Exceptions;
using DreamAquascape.Web.ViewModels.Contest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DreamAquascape.Services.Core.Interfaces
{
    public interface IContestService
    {
        Task<IEnumerable<ContestItemViewModel>> GetActiveContestsAsync();
        Task<ContestListViewModel> GetFilteredContestsAsync(ContestFilterViewModel filters);
        Task<ContestDetailsViewModel?> GetContestWithEntriesAsync(int contestId, string? currentUserId = null);
        Task<Contest> SubmitContestAsync(CreateContestViewModel dto, PrizeViewModel prizeDto, string createdBy);

        // Voting
        Task<Vote> CastVoteAsync(int contestId, int entryId, string userId, string userName, string? ipAddress = null);
        Task<Vote> ChangeVoteAsync(int contestId, int newEntryId, string userId, string userName);
        Task RemoveVoteAsync(int contestId, string userId);

        // Winner determination
        Task<ContestWinner?> DetermineAndSetWinnerAsync(int contestId);
        Task<List<ContestWinner>> ProcessEndedContestsAsync();
    }
}
