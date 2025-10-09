namespace APISegaAI.DAL.Repository
{
    public class InputDataRepository : BaseRepository<InputData>, IInputDataRepository
    {
        public InputDataRepository(SegaAIContext db, ILogger<InputDataRepository> logger) : base (db, logger){}

        public async Task<InputData?> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("ID не может быть пустым или null", nameof(id));

            return await _db.InputData
                .AsNoTracking()
                .Include(id => id.TestCase)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<InputData?> GetByTestCaseIdAsync(string testCaseId)
        {
            if (string.IsNullOrEmpty(testCaseId))
                throw new ArgumentException("TestCaseId не может быть пустым или null", nameof(testCaseId));

            return await _db.InputData
                .AsNoTracking()
                .Include(id => id.TestCase)
                .FirstOrDefaultAsync(id => id.TestCaseId == testCaseId);
        }

        public async Task UpdateAsync(InputData entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var existing = await _db.InputData
                .FirstOrDefaultAsync(id => id.Id == entity.Id);

            if (existing == null)
                throw new InvalidOperationException($"Входные данные с ID {entity.Id} не найдены");

            // Обновление полей
            _db.Entry(existing).CurrentValues.SetValues(entity);
        }
    }
}