namespace APISegaAI.DAL.Interfaces
{
    /// <summary>
    /// для работы с сущностью DataPoolItem.
    /// Управляет отдельными элементами тестовых данных в datapool.
    /// </summary>
    public interface IDataPoolItemRepository: IBaseAddAsyncRepository<DataPoolItem>
    {
        /// <summary>
        /// Получает элементы для указанного datapool.
        /// </summary>
        /// <param name="dataPoolId">Идентификатор datapool.</param>
        /// <returns>Список элементов данных.</returns>
        Task<List<DataPoolItem>> GetByDataPoolIdAsync(string dataPoolId);
    }
}