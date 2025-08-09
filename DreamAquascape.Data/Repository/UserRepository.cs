using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.GCommon.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DreamAquascape.Data.Repository
{
    public class UserRepository : BaseRepository<ApplicationUser, string>, IUserRepository
    {
        public UserRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext, dateTimeProvider)
        {
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await GetAllAttached()
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
