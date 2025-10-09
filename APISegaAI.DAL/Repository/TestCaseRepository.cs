namespace APISegaAI.DAL.Repository
{
    public class TestCaseRepository : BaseRepository<TestCase>, ITestCaseRepository
    {
        public TestCaseRepository(SegaAIContext db, ILogger<TestCaseRepository> logger) : base (db, logger){}

        public async Task<List<TestCase>> GetAllAsync(TestCaseStatus? status = null)
        {
            var query = _db.TestCases
                .AsNoTracking()
                .Include(tc => tc.Steps)
                .Include(tc => tc.History)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(tc => tc.Status == status.Value);

            return await query.ToListAsync();
        }

        public async Task<TestCase?> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("ID не может быть пустым или null", nameof(id));

            return await _db.TestCases
                .AsNoTracking()
                .Include(tc => tc.Steps)
                .Include(tc => tc.History)
                .FirstOrDefaultAsync(tc => tc.Id == id);
        }

        public async Task<TestCase?> GetByNumberAsync(string number)
        {
            if (string.IsNullOrEmpty(number))
                throw new ArgumentException("Номер не может быть пустым или null", nameof(number));

            return await _db.TestCases
                .AsNoTracking()
                .Include(tc => tc.Steps)
                .Include(tc => tc.History)
                .FirstOrDefaultAsync(tc => tc.Number == number);
        }

        public async Task UpdateAsync(TestCase entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var existing = await _db.TestCases
                .Include(tc => tc.Steps)
                .Include(tc => tc.History)
                .FirstOrDefaultAsync(tc => tc.Id == entity.Id);

            if (existing == null)
                throw new InvalidOperationException($"Тест-кейс с ID {entity.Id} не найден");

            // Обновление основных полей
            _db.Entry(existing).CurrentValues.SetValues(entity);

            // Обновление шагов
            existing.Steps.Clear();
            foreach (var step in entity.Steps)
            {
                step.TestCaseId = existing.Id;
                existing.Steps.Add(step);
            }

            // Обновление истории
            existing.History.Clear();
            foreach (var history in entity.History)
            {
                history.TestCaseId = existing.Id;
                existing.History.Add(history);
            }
        }
    }
}