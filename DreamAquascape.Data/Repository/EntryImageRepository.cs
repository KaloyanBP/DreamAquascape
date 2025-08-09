using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.GCommon.Infrastructure;

namespace DreamAquascape.Data.Repository
{
    public class EntryImageRepository : BaseRepository<EntryImage, int>, IEntryImageRepository
    {
        public EntryImageRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext, dateTimeProvider)
        {
        }
    }
}