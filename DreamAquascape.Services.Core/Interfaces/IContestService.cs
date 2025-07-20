using DreamAquascape.Data;
using DreamAquascape.Data.Models;
using DreamAquascape.Services.Common.Exceptions;
using DreamAquascape.Web.ViewModels.Contest;
using DreamAquascape.Web.ViewModels.ContestEntry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DreamAquascape.Services.Core.Interfaces
{
    public interface IContestService
    {
        Task<IEnumerable<ContestItemViewModel>> GetActiveContestsAsync();
        Task<ContestDetailsViewModel?> GetContestWithEntriesAsync(int contestId);
        Task<Contest> SubmitContestAsync(CreateContestViewModel dto, PrizeViewModel prizeDto, string createdBy);
        Task<ContestEntry> SubmitEntryAsync(CreateContestEntryViewModel dto, string userId, string userName);
        Task<Vote> CastVoteAsync(int contestId, int entryId, string userId, string userName, string? ipAddress = null);
    }
}
