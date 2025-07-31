using DreamAquascape.Data.Models;

namespace DreamAquascape.Data.Repository.Interfaces
{
    public interface IEntryImageRepository : IRepository<EntryImage, int>, IAsyncRepository<EntryImage, int>
    {
    }
}
