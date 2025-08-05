using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DreamAquascape.Data.Repository
{
    public class UserRepository : BaseRepository<ApplicationUser, string>, IUserRepository
    {
        public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await GetAllAttached()
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}