namespace APISegaAI.Service.Interfaces
{
    // для взаимодействия с LLM через REST API
    public interface ILlmService
    {
        // Вызов LLM для генерации или проверки данных
        Task<string> CallLlmAsync(string prompt, string model);
    }
}