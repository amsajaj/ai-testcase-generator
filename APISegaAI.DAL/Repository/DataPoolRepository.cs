
namespace APISegaAI.DAL.Repository
{
    public class DataPoolRepository : BaseAddAsyncRepository<DataPool>, IDataPoolRepository
    {
        public DataPoolRepository(SegaAIContext db, ILogger<DataPoolRepository> logger) : base (db, logger){}

        public async Task<DataPool?> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("ID не может быть пустым или null", nameof(id));

            return await _db.DataPools
                .AsNoTracking()
                .Include(dp => dp.Items)
                .Include(dp => dp.TestCase)
                .FirstOrDefaultAsync(dp => dp.Id == id);
        }

        public async Task<DataPool?> GetByTestCaseIdAsync(string testCaseId)
        {
            if (string.IsNullOrEmpty(testCaseId))
                throw new ArgumentException("TestCaseId не может быть пустым или null", nameof(testCaseId));

            return await _db.DataPools
                .AsNoTracking()
                .Include(dp => dp.Items)
                .Include(dp => dp.TestCase)
                .FirstOrDefaultAsync(dp => dp.TestCaseId == testCaseId);
        }
    }
}