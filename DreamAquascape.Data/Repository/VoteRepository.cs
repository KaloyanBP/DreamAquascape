using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;

namespace DreamAquascape.Data.Repository
{
    public class VoteRepository: BaseRepository<Vote, int>, IVoteRepository   
    {
        public VoteRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
