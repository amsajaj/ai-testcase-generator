namespace APISegaAI.DAL.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task UpdateAsync(T entity);
        Task<bool> DeleteAsync(string id);
    }
}