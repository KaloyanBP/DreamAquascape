using DreamAquascape.Data.Models;
using DreamAquascape.Web.ViewModels.UserDashboard;

namespace DreamAquascape.Services.Core.Interfaces
{
    public interface IUserDashboardService
    {
        Task<UserQuickStatsViewModel> GetUserQuickStatsAsync(string userId);

        Task<List<UserActiveContestViewModel>> GetUserActiveContestsAsync(string userId);

        Task<List<UserSubmissionViewModel>> GetUserSubmissionsAsync(string userId, int page = 1, int pageSize = 10);

        Task<List<UserVotingHistoryViewModel>> GetUserVotingHistoryAsync(string userId, int page = 1, int pageSize = 10);
    
        Task<ApplicationUser?> GetUserDetails(string userId);
    }
}
