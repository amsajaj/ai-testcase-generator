namespace APISegaAI.DAL.Repository
{
    public class DataPoolItemRepository : BaseAddAsyncRepository<DataPoolItem>, IDataPoolItemRepository
    {
        public DataPoolItemRepository(SegaAIContext db, ILogger<DataPoolItemRepository> logger) : base (db, logger){}

        public async Task<List<DataPoolItem>> GetByDataPoolIdAsync(string dataPoolId)
        {
            if (string.IsNullOrEmpty(dataPoolId))
                throw new ArgumentException("ID пула данных не может быть пустым или null", nameof(dataPoolId));

            return await _db.DataPoolItems
                .AsNoTracking()
                .Where(dpi => dpi.DataPoolId == dataPoolId)
                .ToListAsync();
        }
    }
}