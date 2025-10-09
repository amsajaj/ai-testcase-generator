namespace APISegaAI.DAL.Interfaces
{
    public interface IBaseGetByIdAsyncRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(string id);
    }
}