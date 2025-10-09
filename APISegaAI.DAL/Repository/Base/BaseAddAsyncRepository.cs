namespace APISegaAI.DAL.Repository.Base
{
    public class BaseAddAsyncRepository<TEntity> : BaseDbRepository<TEntity, BaseAddAsyncRepository<TEntity>>, IBaseAddAsyncRepository<TEntity>  where TEntity : class
    {
        public BaseAddAsyncRepository(SegaAIContext db, ILogger<BaseAddAsyncRepository<TEntity>> logger) : base(db, logger) {}
        
        public async Task AddAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await dbSet.AddAsync(entity);
        }
    }
}