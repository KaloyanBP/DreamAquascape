using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;

namespace DreamAquascape.Data.Repository
{
    public class EntryImageRepository : BaseRepository<EntryImage, int>, IEntryImageRepository   
    {
        public EntryImageRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
