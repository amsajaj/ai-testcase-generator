namespace APISegaAI.DAL.Interfaces
{
    /// <summary>
    /// для работы с сущностью HistoryEntry.
    /// Хранит историю операций с тест-кейсами (генерация, редактирование, экспорт).
    /// </summary>
    public interface IHistoryEntryRepository : IBaseAddAsyncRepository<HistoryEntry>
    {
        /// <summary>
        /// Получает историю для указанного тест-кейса.
        /// </summary>
        /// <param name="testCaseId">Идентификатор тест-кейса.</param>
        /// <returns>Список записей истории.</returns>
        Task<List<HistoryEntry>> GetByTestCaseIdAsync(string testCaseId);
    }
}