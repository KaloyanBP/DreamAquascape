using static Azure.Core.HttpHeader;

namespace DreamAquascape.Data.Repository
{
    using Interfaces;
    using Microsoft.EntityFrameworkCore;
    using System.Linq.Expressions;
    using System.Reflection;
    using static Common.ExceptionMessages;
    using static GCommon.ApplicationConstants;

    public abstract class BaseRepository<TEntity, TKey>
        : IRepository<TEntity, TKey>, IAsyncRepository<TEntity, TKey>
        where TEntity : class
    {
        protected readonly ApplicationDbContext DbContext;
        protected readonly DbSet<TEntity> DbSet;

        protected BaseRepository(ApplicationDbContext dbContext)
        {
            this.DbContext = dbContext;
            this.DbSet = this.DbContext.Set<TEntity>();
        }

        public TEntity? GetById(TKey id)
        {
            return this.DbSet
                .Find(id);
        }

        public ValueTask<TEntity?> GetByIdAsync(TKey id)
        {
            return this.DbSet
                .FindAsync(id);
        }

        public TEntity? SingleOrDefault(Func<TEntity, bool> predicate)
        {
            return this.DbSet
                .SingleOrDefault(predicate);
        }

        public Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return this.DbSet
                .SingleOrDefaultAsync(predicate);
        }

        public TEntity? FirstOrDefault(Func<TEntity, bool> predicate)
        {
            return this.DbSet
                .FirstOrDefault(predicate);
        }

        public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return this.DbSet
                .FirstOrDefaultAsync(predicate);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return this.DbSet
                .ToArray();
        }

        public int Count()
        {
            return this.DbSet
                .Count();
        }

        public Task<int> CountAsync()
        {
            return this.DbSet
                .CountAsync();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            TEntity[] entities = await this.DbSet
                .ToArrayAsync();

            return entities;
        }

        public IQueryable<TEntity> GetAllAttached()
        {
            return this.DbSet
                .AsQueryable();
        }

        public void Add(TEntity item)
        {
            this.DbSet.Add(item);
        }

        public async Task AddAsync(TEntity item)
        {
            await this.DbSet.AddAsync(item);
        }

        public void AddRange(IEnumerable<TEntity> items)
        {
            this.DbSet.AddRange(items);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> items)
        {
            await this.DbSet.AddRangeAsync(items);
        }

        public bool Delete(TEntity entity)
        {
            this.PerformSoftDeleteOfEntity(entity);

            return this.Update(entity);
        }

        public Task<bool> DeleteAsync(TEntity entity)
        {
            this.PerformSoftDeleteOfEntity(entity);

            return this.UpdateAsync(entity);
        }

        public bool HardDelete(TEntity entity)
        {
            this.DbSet.Remove(entity);
            return true;
        }

        public async Task<bool> HardDeleteAsync(TEntity entity)
        {
            this.DbSet.Remove(entity);
            return true;
        }

        public bool Update(TEntity item)
        {
            try
            {
                this.DbSet.Attach(item);
                this.DbSet.Entry(item).State = EntityState.Modified;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(TEntity item)
        {
            try
            {
                this.DbSet.Attach(item);
                this.DbSet.Entry(item).State = EntityState.Modified;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SaveChanges()
        {
            this.DbContext.SaveChanges();
        }

        public async Task SaveChangesAsync()
        {
            await this.DbContext.SaveChangesAsync();
        }

        private void PerformSoftDeleteOfEntity(TEntity entity)
        {
            PropertyInfo? isDeletedProperty =
                this.GetIsDeletedProperty(entity);
            if (isDeletedProperty == null)
            {
                throw new InvalidOperationException(SoftDeleteOnNonSoftDeletableEntity);
            }

            isDeletedProperty.SetValue(entity, true);
        }

        private PropertyInfo? GetIsDeletedProperty(TEntity entity)
        {
            return typeof(TEntity)
                .GetProperties()
                .FirstOrDefault(pi => pi.PropertyType == typeof(bool) &&
                                                 pi.Name == IsDeletedPropertyName);
        }
    }
}
