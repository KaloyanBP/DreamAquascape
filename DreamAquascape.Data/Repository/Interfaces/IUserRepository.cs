using DreamAquascape.Data.Models;

namespace DreamAquascape.Data.Repository.Interfaces
{
    public interface IUserRepository : IRepository<ApplicationUser, string>, IAsyncRepository<ApplicationUser, string>
    {
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
    }
}
