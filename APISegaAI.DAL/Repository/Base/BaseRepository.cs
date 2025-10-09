namespace APISegaAI.DAL.Repository.Base
{
    public class BaseRepository<TEntity> : BaseAddAsyncRepository<TEntity>  where TEntity : class
    {
        public BaseRepository(SegaAIContext db, ILogger<BaseRepository<TEntity>> logger) : base(db, logger) {}

        public async Task<bool> DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("ID не может быть пустым или null", nameof(id));

            var entity = await dbSet.FindAsync(id);
            if (entity == null)
                return false;

            dbSet.Remove(entity);

            return true; 
        }
    }
}