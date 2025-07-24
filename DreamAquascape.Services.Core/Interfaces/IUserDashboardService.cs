using DreamAquascape.Web.ViewModels.UserDashboard;

namespace DreamAquascape.Services.Core.Interfaces
{
    public interface IUserDashboardService
    {
        Task<UserQuickStatsViewModel> GetUserQuickStatsAsync(string userId);
    }
}
