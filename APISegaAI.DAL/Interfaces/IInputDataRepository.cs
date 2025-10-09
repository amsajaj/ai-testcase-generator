namespace APISegaAI.DAL.Interfaces
{
    /// <summary>
    /// для работы с сущностью InputData.
    /// Хранит входные данные (требования, сценарии, URL) для генерации тест-кейсов.
    /// </summary>
    public interface IInputDataRepository : IBaseRepository<InputData>, IBaseGetByIdAsyncRepository<InputData>, IBaseAddAsyncRepository<InputData>
    {
        /// <summary>
        /// Получает входные данные, связанные с тест-кейсом.
        /// </summary>
        /// <param name="testCaseId">Идентификатор тест-кейса.</param>
        /// <returns>Входные данные или null, если не найдены.</returns>
        Task<InputData?> GetByTestCaseIdAsync(string testCaseId);
    }
}