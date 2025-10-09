namespace APISegaAI.Service.Interfaces
{
    // для управления тест-кейсами 
    public interface ITestCaseService
    {
        // Генерация тест-кейса на основе входных данных через LLM 
        Task<(TestCase TestCase, string? Recommendation)> GenerateTestCaseAsync(string inputData, string llmModel);
        //Task<(TestCase TestCase, string? Recommendation)> GenerateTestCaseAsync(string inputData, string llmModel);

        // Обновление шагов тест-кейса
        Task UpdateTestCaseStepsAsync(string testCaseId, List<TestStep> updatedSteps);

        // Получение тест-кейса по ID
        Task<TestCase?> GetTestCaseByIdAsync(string id);

        // Получение тест-кейса по номеру
        Task<TestCase?> GetTestCaseByNumberAsync(string number);

        // Получение всех тест-кейсов с фильтрацией по статусу
        Task<List<TestCase>> GetAllTestCasesAsync(TestCaseStatus? status = null);

        // Удаление тест-кейса
        Task<bool> DeleteTestCaseAsync(string id);

        // Генерация изменений для существующего тест-кейса на основе новых входных данных
        Task<(TestCase TestCase, string? Recommendation)> UpdateTestCaseWithChangesAsync(string testCaseId, string changesInput, string llmModel);
    }
}