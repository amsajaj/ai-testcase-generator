namespace APISegaAI.DAL.Interfaces
{
    /// <summary>
    /// для работы с сущностью DataPool.
    /// Управляет тестовыми данными (datapool), связанными с тест-кейсами.
    /// </summary>
    public interface IDataPoolRepository: IBaseAddAsyncRepository<DataPool>, IBaseGetByIdAsyncRepository<DataPool>
    {
        /// <summary>
        /// Получает datapool, связанный с тест-кейсом.
        /// </summary>
        /// <param name="testCaseId">Идентификатор тест-кейса.</param>
        /// <returns>Datapool или null, если не найден.</returns>
        Task<DataPool?> GetByTestCaseIdAsync(string testCaseId);
    }
}