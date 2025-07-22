using DreamAquascape.Web.ViewModels.AdminDashboard;

namespace DreamAquascape.Services.Core.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<DashboardStatsViewModel> GetDashboardStatsAsync();
    }
}
