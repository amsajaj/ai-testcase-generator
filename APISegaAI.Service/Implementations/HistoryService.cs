namespace APISegaAI.Service.Implementations
{
    /// для работы с историей операций над тест-кейсами. 
    public class HistoryService : IHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        
        public HistoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Асинхронно добавляет запись в историю операций для указанного тест-кейса.
        /// </summary>
        /// <param name="testCaseId">Идентификатор тест-кейса, к которому относится запись.</param>
        /// <param name="action">Действие, выполненное над тест-кейсом (например, "Generated", "Edited").</param>
        /// <param name="user">Пользователь или система, выполнившая действие (например, "System" или имя пользователя).</param>
        /// <param name="details">Дополнительные детали операции (опционально).</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если <paramref name="testCaseId"/>, <paramref name="action"/> или <paramref name="user"/> пустые или null.</exception>
        /// <exception cref="InvalidOperationException">Выбрасывается, если тест-кейс с указанным <paramref name="testCaseId"/> не найден.</exception>
        public async Task AddHistoryEntryAsync(string testCaseId, string action, string user, string? details = null)
        {
            if (string.IsNullOrEmpty(testCaseId))
                throw new ArgumentException("ID тест-кейса не может быть пустым или null", nameof(testCaseId));
            if (string.IsNullOrEmpty(action))
                throw new ArgumentException("Действие не может быть пустым или null", nameof(action));
            if (string.IsNullOrEmpty(user))
                throw new ArgumentException("Пользователь не может быть пустым или null", nameof(user));

            // Проверка существования тест-кейса
            var testCase = await _unitOfWork.TestCases.GetByIdAsync(testCaseId);
            if (testCase == null)
                throw new InvalidOperationException($"Тест-кейс с ID {testCaseId} не найден");

            var historyEntry = new HistoryEntry
            {
                Id = Guid.NewGuid().ToString(),
                TestCaseId = testCaseId,
                Action = action,
                User = user,
                Details = details ?? string.Empty,
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.HistoryEntries.AddAsync(historyEntry);
            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Асинхронно получает список записей истории для указанного тест-кейса.
        /// </summary>
        /// <param name="testCaseId">Идентификатор тест-кейса, для которого требуется получить историю.</param>
        /// <returns>Список записей истории <see cref="HistoryEntry"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если <paramref name="testCaseId"/> пустой или null.</exception>
        public async Task<List<HistoryEntry>> GetHistoryByTestCaseIdAsync(string testCaseId)
        {
            if (string.IsNullOrEmpty(testCaseId))
                throw new ArgumentException("ID тест-кейса не может быть пустым или null", nameof(testCaseId));

            return await _unitOfWork.HistoryEntries.GetByTestCaseIdAsync(testCaseId);
        }
    }
}