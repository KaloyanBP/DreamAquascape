namespace DreamAquascape.Data.Repository.Interfaces
{
    public interface IRepository<TEntity, TKey>
    {
        TEntity? GetById(TKey id);

        TEntity? SingleOrDefault(Func<TEntity, bool> predicate);

        TEntity? FirstOrDefault(Func<TEntity, bool> predicate);

        IEnumerable<TEntity> GetAll();

        int Count();

        IQueryable<TEntity> GetAllAttached();

        void Add(TEntity item);

        void AddRange(IEnumerable<TEntity> items);

        bool Delete(TEntity entity, DateTime? deletedAt, string? deletedBy = null);

        bool HardDelete(TEntity entity);

        bool Update(TEntity item);

        void SaveChanges();
    }
}
