using DreamAquascape.Data.Repository.Interfaces;

namespace DreamAquascape.Data.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Repository properties
        IContestRepository ContestRepository { get; }
        IContestEntryRepository ContestEntryRepository { get; }
        IContestWinnerRepository ContestWinnerRepository { get; }
        IContestCategoryRepository ContestCategoryRepository { get; }
        IVoteRepository VoteRepository { get; }
        IEntryImageRepository EntryImageRepository { get; }
        IPrizeRepository PrizeRepository { get; }
        IUserRepository UserRepository { get; }

        // Transaction methods
        Task<int> SaveChangesAsync();
        int SaveChanges();

        // Transaction management
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
