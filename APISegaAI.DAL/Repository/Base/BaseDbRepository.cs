namespace APISegaAI.DAL.Repository.Base
{
    public class BaseDbRepository<TEntity , TRepository> where TEntity : class
    {
        protected readonly SegaAIContext _db;
        protected readonly ILogger<TRepository> _logger;
        protected readonly DbSet<TEntity> dbSet;

        public BaseDbRepository(SegaAIContext db, ILogger<TRepository> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.dbSet = _db.Set<TEntity>();
        }
    }
}