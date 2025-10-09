namespace APISegaAI.DAL.Interfaces
{
    /// <summary>
    /// для работы с сущностью TestCase.
    /// Предоставляет методы для создания, получения, обновления и удаления тест-кейсов,
    /// а также фильтрации по статусу и поиску по номеру.
    /// </summary>
    public interface ITestCaseRepository : IBaseRepository<TestCase>, IBaseGetByIdAsyncRepository<TestCase>, IBaseAddAsyncRepository<TestCase>
    {
        /// <summary>
        /// Получает тест-кейс по номеру.
        /// </summary>
        /// <param name="number">Номер тест-кейса (например, TC-001).</param>
        /// <returns>Тест-кейс или null, если не найден.</returns>
        Task<TestCase?> GetByNumberAsync(string number);

        /// <summary>
        /// Получает список тест-кейсов с фильтрацией по статусу.
        /// </summary>
        /// <param name="status">Статус тест-кейса (опционально).</param>
        /// <returns>Список тест-кейсов.</returns>
        Task<List<TestCase>> GetAllAsync(TestCaseStatus? status = null);
    }
}