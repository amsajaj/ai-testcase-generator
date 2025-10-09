namespace APISegaAI.Service.Interfaces
{
    // для работы с историей операций
    public interface IHistoryService
    {
        // Добавление записи в историю
        Task AddHistoryEntryAsync(string testCaseId, string action, string user, string? details = null);

        // Получение истории по ID тест-кейса
        Task<List<HistoryEntry>> GetHistoryByTestCaseIdAsync(string testCaseId);
    }
}