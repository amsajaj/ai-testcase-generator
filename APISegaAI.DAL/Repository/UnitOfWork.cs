namespace APISegaAI.DAL.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SegaAIContext _db;
        private readonly ILoggerFactory _loggerFactory;
        private ITestCaseRepository? _testCases;
        private ITestStepRepository? _testSteps; 
        private IInputDataRepository? _inputData; 
        private IHistoryEntryRepository? _historyEntries;
        private IDataPoolRepository? _dataPools; 
        private IDataPoolItemRepository? _dataPoolItems;

        public UnitOfWork(SegaAIContext db, ILoggerFactory loggerFactory)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public ITestCaseRepository TestCases => _testCases ??= new TestCaseRepository(_db, _loggerFactory.CreateLogger<TestCaseRepository>());
        
        public ITestStepRepository TestSteps => _testSteps ??= new TestStepRepository(_db, _loggerFactory.CreateLogger<TestStepRepository>());

        public IInputDataRepository InputData => _inputData ??= new InputDataRepository(_db, _loggerFactory.CreateLogger<InputDataRepository>());

        public IHistoryEntryRepository HistoryEntries => _historyEntries ??= new HistoryEntryRepository(_db, _loggerFactory.CreateLogger<HistoryEntryRepository>());

        public IDataPoolRepository DataPools  => _dataPools ??= new DataPoolRepository(_db, _loggerFactory.CreateLogger<DataPoolRepository>());

        public IDataPoolItemRepository DataPoolItems => _dataPoolItems ??= new DataPoolItemRepository(_db, _loggerFactory.CreateLogger<DataPoolItemRepository>());

        public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();
    }
}