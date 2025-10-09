namespace APISegaAI.Service.Implementations
{
    /// для управления тест кейсами включая генерацию, обновление и валидацию с использованием LLM. 
    /// обеспечивает взаимодействие с репозиториями через IUnitOfWork и вызовы LLM через ILlmService. 
    public class TestCaseService : ITestCaseService
    {
        private readonly IUnitOfWork _unitOfWork; 
	    private readonly ILlmService _llmService;
        private readonly ILogger _logger;
        private readonly TestCaseGenerator<TestCaseService> _helper;

        public TestCaseService(IUnitOfWork unitOfWork, ILlmService llmService, ILogger<TestCaseService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _helper = new TestCaseGenerator<TestCaseService>(unitOfWork, llmService, logger);
        }

        public async Task<bool> DeleteTestCaseAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Попытка удаления тест-кейса с пустым или null ID");
                throw new ArgumentException("ID не может быть пустым или null", nameof(id));
            }

            _logger.LogInformation("Удаление тест-кейса с ID: {TestCaseId}", id);
            var result = await _unitOfWork.TestCases.DeleteAsync(id);
            if (result)
            {
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Тест-кейс с ID: {TestCaseId} успешно удалён", id);
            }
            else
            {
                _logger.LogWarning("Тест-кейс с ID: {TestCaseId} не найден", id);
            }

            return result;
        }

        /// <summary>
        /// Генерирует новый тест кейс на основе входных данных с использованием указанной модели LLM.
        /// Сохраняет тесткейс в бд записывает историю операции и выполняет валидацию.
        /// При невалидном результате автоматически перегенерирует тест-кейс с уточнённым промптом.
        /// </summary>
        /// <param name="inputData">Входные данные (текст, JSON, URL) для генерации тест-кейса.</param>
        /// <param name="llmModel">Название модели LLM (например, "qwen3-32b-awq").</param>
        /// <returns>Кортеж, содержащий сгенерированный тест-кейс и рекомендацию (если есть).</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если входные данные или модель пустые.</exception>
        /// <exception cref="InvalidOperationException">Выбрасывается при ошибке десериализации или генерации.</exception>
        public async Task<(TestCase TestCase, string? Recommendation)> GenerateTestCaseAsync(string inputData, string llmModel)
        {
            _helper.ValidateInput(inputData, llmModel);
            var testCase = await _helper.GenerateTestCaseInternalAsync(inputData, llmModel);
            await _helper.SaveTestCaseAsync(testCase, llmModel, "Generated", $"Test case and code generated using model {llmModel}");
            return await _helper.ValidateAndRetryAsync(testCase, inputData, llmModel, 
                (data, model) => GenerateTestCaseAsync(data, model));
        }

        public async Task<List<TestCase>> GetAllTestCasesAsync(TestCaseStatus? status = null)
        {
            _logger.LogInformation("Получение списка тест-кейсов с фильтром по статусу: {Status}", status?.ToString() ?? "все");
            var testCases = await _unitOfWork.TestCases.GetAllAsync(status);
            _logger.LogInformation("Получено {Count} тест-кейсов", testCases.Count);
            return testCases;
        }

        public async Task<TestCase?> GetTestCaseByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Попытка получения тест-кейса с пустым или null ID");
                throw new ArgumentException("ID не может быть пустым или null", nameof(id));
            }

            _logger.LogInformation("Получение тест-кейса с ID: {TestCaseId}", id);
            var testCase = await _unitOfWork.TestCases.GetByIdAsync(id);
            if (testCase == null)
            {
                _logger.LogWarning("Тест-кейс с ID: {TestCaseId} не найден", id);
            }
            else
            {
                _logger.LogInformation("Тест-кейс с ID: {TestCaseId} успешно получен", id);
            }

            return testCase;
        }

        public async Task<TestCase?> GetTestCaseByNumberAsync(string number)
        {
            if (string.IsNullOrEmpty(number))
            {
                _logger.LogWarning("Попытка получения тест-кейса с пустым или null номером");
                throw new ArgumentException("Номер не может быть пустым или null", nameof(number));
            }

            _logger.LogInformation("Получение тест-кейса с номером: {TestCaseNumber}", number);
            var testCase = await _unitOfWork.TestCases.GetByNumberAsync(number);
            if (testCase == null)
            {
                _logger.LogWarning("Тест-кейс с номером: {TestCaseNumber} не найден", number);
            }
            else
            {
                _logger.LogInformation("Тест-кейс с номером: {TestCaseNumber} успешно получен", number);
            }

            return testCase;
        }

        /// <summary>
        /// Обновляет шаги указанного тест-кейса в бд.
        /// </summary>
        /// <param name="testCaseId">id тест-кейса.</param>
        /// <param name="updatedSteps">Список новых шагов тест кейса.</param>
        /// <exception cref="ArgumentException">Выбрасывается, если testCaseId или updatedSteps пустые/недопустимые.</exception>
        /// <exception cref="InvalidOperationException">Выбрасывается, если тест-кейс с указанным ID не найден.</exception>
        /// <remarks>
        /// Метод загружает тест кейс по ID, валидирует новые шаги, обновляет коллекцию шагов и добавляет запись в историю операций.
        /// Каждый шаг получает новый ID и нумеруется последовательно (1, 2, 3...).
        /// </remarks>
        public async Task UpdateTestCaseStepsAsync(string testCaseId, List<TestStep> updatedSteps)
        {
            if (string.IsNullOrEmpty(testCaseId))
            {
                _logger.LogWarning("Попытка обновления шагов тест-кейса с пустым или null ID");
                throw new ArgumentException("ID не может быть пустым или null", nameof(testCaseId));
            }

            if (updatedSteps == null || !updatedSteps.Any())
            {
                _logger.LogWarning($"Список шагов для тест-кейса с ID: {testCaseId} пуст или null");
                throw new ArgumentException("Список шагов не может быть пустым или null", nameof(updatedSteps));
            }

            _logger.LogInformation("Обновление шагов для тест-кейса с ID: {TestCaseId}", testCaseId);
            var testCase = await _unitOfWork.TestCases.GetByIdAsync(testCaseId)
                ?? throw new InvalidOperationException($"Тест-кейс с ID {testCaseId} не найден");

            for (int i = 0; i < updatedSteps.Count; i++)
            {
                var step = updatedSteps[i];
                if (string.IsNullOrEmpty(step.Action) || string.IsNullOrEmpty(step.ExpectedResult))
                {
                    _logger.LogWarning($"Шаг #{i + 1} содержит пустые поля Action или ExpectedResult");
                    throw new ArgumentException($"Шаг #{i + 1} содержит пустые поля Action или ExpectedResult");
                }
                step.Id = Guid.NewGuid().ToString();
                step.TestCaseId = testCaseId;
                step.StepNumber = i + 1;
            }

            testCase.Steps = updatedSteps;
            await _unitOfWork.TestCases.UpdateAsync(testCase);
            await _unitOfWork.HistoryEntries.AddAsync(new HistoryEntry
            {
                TestCaseId = testCaseId,
                Action = "StepsUpdated",
                User = "System",
                Details = $"Обновлено {updatedSteps.Count} шагов"
            });
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Шаги для тест-кейса с ID: {TestCaseId} успешно обновлены", testCaseId);
        }

        /// <summary>
        /// Обновляет существующий тест кейс на основе новых изменений с использованием LLM.
        /// Сохраняет обновлённый тесткейс в бд, добавляет запись в историю и выполняет валидацию.
        /// При невалидном результате автоматически перегенерирует тест кейс с уточнённым промптом.
        /// </summary>
        /// <param name="testCaseId">id тест-кейса для обновления.</param>
        /// <param name="changesInput">Новые изменения (например, обновлённые требования).</param>
        /// <param name="llmModel">Название модели LLM (например, "qwen3-32b-awq").</param>
        /// <returns>Кортеж, содержащий обновлённый тест-кейс и рекомендацию (если есть).</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если ID или изменения пустые.</exception>
        /// <exception cref="InvalidOperationException">Выбрасывается, если тест-кейс не найден или при ошибке десериализации.</exception>
        public async Task<(TestCase TestCase, string? Recommendation)> UpdateTestCaseWithChangesAsync(string testCaseId, string changesInput, string llmModel)
        {
            _helper.ValidateInput(changesInput, llmModel);
            if (string.IsNullOrWhiteSpace(testCaseId))
                throw new ArgumentException("ID тест-кейса не может быть пустым", nameof(testCaseId));

            var existingTestCase = await _unitOfWork.TestCases.GetByIdAsync(testCaseId)
                ?? throw new InvalidOperationException($"Тест-кейс с ID {testCaseId} не найден");

            var testCase = await _helper.GenerateTestCaseInternalAsync(changesInput, llmModel, existingTestCase);
            
            testCase.Id = existingTestCase.Id;
            testCase.Status = existingTestCase.Status;
            testCase.CreationDate = existingTestCase.CreationDate;
            if (string.IsNullOrWhiteSpace(testCase.Number))
                testCase.Number = existingTestCase.Number;

            await _helper.SaveTestCaseAsync(testCase, llmModel, "Updated", $"Test case and code updated with changes: {changesInput}");
            return await _helper.ValidateAndRetryAsync(testCase, changesInput, llmModel, 
                (data, model) => UpdateTestCaseWithChangesAsync(testCaseId, data, model));
        }
    }
}