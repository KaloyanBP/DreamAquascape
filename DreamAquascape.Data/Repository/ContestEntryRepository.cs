using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DreamAquascape.Data.Repository
{
    public class ContestEntryRepository: BaseRepository<ContestEntry, int>, IContestEntryRepository   
    {
        public ContestEntryRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
