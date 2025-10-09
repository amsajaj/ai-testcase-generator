namespace APISegaAI.DAL.Interfaces
{
    /// <summary>
    /// для работы с сущностью TestStep.
    /// Управляет шагами тест-кейсов, включая добавление и удаление.
    /// </summary>
    public interface ITestStepRepository : IBaseRepository<TestStep>, IBaseAddAsyncRepository<TestStep>
    {
        /// <summary>
        /// Получает шаги для указанного тест-кейса.
        /// </summary>
        /// <param name="testCaseId">Идентификатор тест-кейса.</param>
        /// <returns>Список шагов.</returns>
        Task<List<TestStep>> GetByTestCaseIdAsync(string testCaseId);
    }
}