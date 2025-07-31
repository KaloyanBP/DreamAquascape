using DreamAquascape.Data.Models;

namespace DreamAquascape.Data.Repository.Interfaces
{
    public interface IVoteRepository : IRepository<Vote, int>, IAsyncRepository<Vote, int>
    {
    }
}
