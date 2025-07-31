using DreamAquascape.Data.Models;

namespace DreamAquascape.Data.Repository.Interfaces
{
    public interface IPrizeRepository : IRepository<Prize, int>, IAsyncRepository<Prize, int>
    {
    }
}
