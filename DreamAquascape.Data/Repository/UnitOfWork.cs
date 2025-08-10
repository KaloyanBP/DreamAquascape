using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.GCommon.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace DreamAquascape.Data.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private IDbContextTransaction? _transaction;

        // Repository instances
        private IContestRepository? _contestRepository;
        private IContestEntryRepository? _contestEntryRepository;
        private IContestWinnerRepository? _contestWinnerRepository;
        private IContestCategoryRepository? _contestCategoryRepository;
        private IVoteRepository? _voteRepository;
        private IEntryImageRepository? _entryImageRepository;
        private IPrizeRepository? _prizeRepository;
        private IUserRepository? _userRepository;

        public UnitOfWork(ApplicationDbContext context, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }

        // Repository properties with lazy initialization
        public IContestRepository ContestRepository
        {
            get
            {
                _contestRepository ??= new ContestRepository(_context, _dateTimeProvider);
                return _contestRepository;
            }
        }

        public IContestEntryRepository ContestEntryRepository
        {
            get
            {
                _contestEntryRepository ??= new ContestEntryRepository(_context, _dateTimeProvider);
                return _contestEntryRepository;
            }
        }

        public IContestWinnerRepository ContestWinnerRepository
        {
            get
            {
                _contestWinnerRepository ??= new ContestWinnerRepository(_context, _dateTimeProvider);
                return _contestWinnerRepository;
            }
        }

        public IContestCategoryRepository ContestCategoryRepository
        {
            get
            {
                _contestCategoryRepository ??= new ContestCategoryRepository(_context, _dateTimeProvider);
                return _contestCategoryRepository;
            }
        }

        public IVoteRepository VoteRepository
        {
            get
            {
                _voteRepository ??= new VoteRepository(_context, _dateTimeProvider);
                return _voteRepository;
            }
        }

        public IEntryImageRepository EntryImageRepository
        {
            get
            {
                _entryImageRepository ??= new EntryImageRepository(_context, _dateTimeProvider);
                return _entryImageRepository;
            }
        }

        public IPrizeRepository PrizeRepository
        {
            get
            {
                _prizeRepository ??= new PrizeRepository(_context, _dateTimeProvider);
                return _prizeRepository;
            }
        }

        public IUserRepository UserRepository
        {
            get
            {
                _userRepository ??= new UserRepository(_context, _dateTimeProvider);
                return _userRepository;
            }
        }

        // Save methods
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        // Transaction management
        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Transaction already started");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction to commit");
            }

            try
            {
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction to rollback");
            }

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        // Dispose pattern
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    // Note: We don't dispose _context here as it's managed by DI container
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
