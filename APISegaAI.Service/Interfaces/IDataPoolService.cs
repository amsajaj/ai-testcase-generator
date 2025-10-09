namespace APISegaAI.Service.Interfaces
{
    // для работы с тестовыми данными (datapool)
    public interface IDataPoolService
    {
        // Генерация тестовых данных на основе тест-кейса или входных данных через LLM
        Task<DataPool> GenerateDataPoolAsync(string testCaseJson, string llmModel);

        // Сохранение пользовательских тестовых данных из загруженного файла
        Task<DataPool> SaveUserDataPoolAsync(IFormFile file, string? testCaseId);

        // Получение тестовых данных по ID
        Task<DataPool?> GetDataPoolByIdAsync(string id);

        // Получение тестовых данных по ID тест-кейса
        Task<DataPool?> GetDataPoolByTestCaseIdAsync(string testCaseId);
    }
}