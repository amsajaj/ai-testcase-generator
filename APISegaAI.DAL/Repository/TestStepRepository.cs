namespace APISegaAI.DAL.Repository
{
    public class TestStepRepository  : BaseRepository<TestStep>,  ITestStepRepository
    {
        public TestStepRepository(SegaAIContext db, ILogger<TestStepRepository> logger) : base (db, logger){}

        public async Task<List<TestStep>> GetByTestCaseIdAsync(string testCaseId)
        {
            if (string.IsNullOrEmpty(testCaseId))
                throw new ArgumentException("TestCaseId не может быть нулевым или пустым.", nameof(testCaseId));

            return await _db.TestSteps
                .Where(ts => ts.TestCaseId == testCaseId)
                .OrderBy(ts => ts.StepNumber)
                .ToListAsync();
        }

        public async Task UpdateAsync(TestStep entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var existing = await _db.TestSteps.FindAsync(entity.Id);
            if (existing == null)
                throw new KeyNotFoundException($"TestStep c Id {entity.Id} не найден.");

            _db.Entry(existing).CurrentValues.SetValues(entity);
        }
    }
}