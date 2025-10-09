
namespace APISegaAI.DAL.Interfaces
{
    public interface IBaseAddAsyncRepository<T> where T : class
    {
        Task AddAsync(T entity);
    }
}