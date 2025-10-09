namespace APISegaAI.Service.Interfaces
{
    // для обработки входных данных
    public interface IInputDataService
    {
        // Сохранение входных данных (функциональные требования, сценарии, URL)
        Task<InputData> SaveInputDataAsync(IFormFile? file, string? textData, string? url, string type);

        // Получение входных данных по ID
        Task<InputData?> GetInputDataByIdAsync(string id);

        // Получение входных данных по ID тест-кейса
        Task<InputData?> GetInputDataByTestCaseIdAsync(string testCaseId);
    }
}