
namespace APISegaAI.DAL.Repository
{
    public class HistoryEntryRepository : BaseAddAsyncRepository<HistoryEntry>, IHistoryEntryRepository
    {
        public HistoryEntryRepository(SegaAIContext db, ILogger<HistoryEntryRepository> logger) : base (db, logger){}

        public async Task<List<HistoryEntry>> GetByTestCaseIdAsync(string testCaseId)
        {
            if (string.IsNullOrEmpty(testCaseId))
            throw new ArgumentException("TestCaseId не может быть пустым или null", nameof(testCaseId));

            return await _db.HistoryEntries
                .AsNoTracking()
                .Where(he => he.TestCaseId == testCaseId)
                .OrderBy(he => he.Timestamp) // Сортировка по времени для удобства просмотра истории
                .ToListAsync();
        }
    }
}