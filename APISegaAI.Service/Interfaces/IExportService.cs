namespace APISegaAI.Service.Interfaces
{
    // для экспорта тест-кейсов
    public interface IExportService
    {
        // Экспорт тест-кейса в Excel
        Task<byte[]> ExportToExcelAsync(string testCaseId);

        // Экспорт тестовых данных в CSV
        Task<byte[]> ExportToCsvAsync(string dataPoolId);

        // Экспорт тест-кейса в Zephyr Scale через REST API
        Task ExportToZephyrAsync(string testCaseId);

        Task<byte[]> ExportTestCodeAsync(string testCaseId);
    }
}