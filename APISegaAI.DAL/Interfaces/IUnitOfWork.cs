namespace APISegaAI.DAL.Interfaces
{
    /// <summary>
    /// для координации работы репозиториев и управления транзакциями.
    /// Обеспечивает доступ ко всем репозиториям и сохраняет изменения в базе данных.
    /// </summary>
    public interface IUnitOfWork
    {
        ITestCaseRepository TestCases { get; }
        ITestStepRepository TestSteps { get; }
        IInputDataRepository InputData { get; }
        IHistoryEntryRepository HistoryEntries { get; }
        IDataPoolRepository DataPools { get; }
        IDataPoolItemRepository DataPoolItems { get; }

        /// <summary>
        /// Сохраняет все изменения в базе данных.
        /// </summary>
        /// <returns>Количество затронутых строк.</returns>
        Task<int> SaveChangesAsync();
    }
}