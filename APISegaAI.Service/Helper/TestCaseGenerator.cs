namespace APISegaAI.Service.Helper
{
    public class TestCaseGenerator<T>
    {
        private readonly IUnitOfWork _unitOfWork; 
        private readonly ILlmService _llmService;
        private readonly ILogger _logger;

        public TestCaseGenerator(IUnitOfWork unitOfWork, ILlmService llmService, ILogger<T> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _llmService = llmService ?? throw new ArgumentNullException(nameof(llmService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void ValidateInput(string inputData, string llmModel)
        {
            if (string.IsNullOrWhiteSpace(inputData))
                throw new ArgumentException("Входные данные не могут быть пустыми", nameof(inputData));
            if (string.IsNullOrWhiteSpace(llmModel))
                throw new ArgumentException("Модель LLM не указана", nameof(llmModel));
        }

        public async Task<TestCase> GenerateTestCaseInternalAsync(string inputData, string llmModel, TestCase? existingTestCase = null)
        {
            string prompt = CreatePrompt(inputData, existingTestCase);
            string llmResponse = await _llmService.CallLlmAsync(prompt, llmModel);
            var responseJson = JsonSerializer.Deserialize<JsonElement>(llmResponse);
            var testCaseJson = responseJson.GetProperty("testCase").GetRawText();
            var testCode = responseJson.GetProperty("testCode").GetString();
            
            var testCase = JsonSerializer.Deserialize<TestCase>(testCaseJson, JsonSerializerConfig.Default)
                ?? throw new InvalidOperationException("Не удалось десериализовать ответ LLM в TestCase");
            
            testCase.Id = Guid.NewGuid().ToString();
            testCase.Status = TestCaseStatus.Development;
            testCase.TestCode = testCode ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(testCase.Number))
                testCase.Number = $"TC-{DateTime.UtcNow.Ticks}";
            if (string.IsNullOrWhiteSpace(testCase.Author))
                testCase.Author = "AI Generated";

            return testCase;
        }

        public async Task SaveTestCaseAsync(TestCase testCase, string llmModel, string action, string details)
        {
            await _unitOfWork.TestCases.AddAsync(testCase);
            await _unitOfWork.HistoryEntries.AddAsync(new HistoryEntry
            {
                TestCaseId = testCase.Id,
                Action = action,
                User = "System",
                Details = details
            });
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<(TestCase TestCase, string? Recommendation)> ValidateAndRetryAsync(
            TestCase testCase, 
            string inputData, 
            string llmModel,
            Func<string, string, Task<(TestCase, string?)>> retryAction)
        {
            var (isValid, recommendation) = await ValidateTestCaseAsync(testCase, inputData, llmModel);
            if (!isValid)
            {
                return await retryAction(inputData + $" (улучшите детализацию, учтите: {recommendation})", llmModel);
            }
            return (testCase, recommendation);
        }

        /// <summary>
        /// Проверяет тест кейс на соответствие входным данным, используя правила в коде и LLM.
        /// Возвращает результат проверки и рекомендации для улучшения, если тест кейс невалиден.
        /// </summary>
        /// <param name="testCase">Тест кейс для проверки.</param>
        /// <param name="inputData">Входные данные для сравнения.</param>
        /// <param name="llmModel">Название модели LLM (например, "qwen3-32b-awq").</param>
        /// <returns>Кортеж, содержащий флаг валидности (true/false) и рекомендацию (если есть).</returns>
        /// <exception cref="ArgumentNullException">Выбрасывается, если тест-кейс равен null.</exception>
        /// <exception cref="ArgumentException">Выбрасывается, если входные данные или модель пустые.</exception>
        /// <exception cref="InvalidOperationException">Выбрасывается при ошибке десериализации или валидации.</exception>
        public async Task<(bool IsValid, string? Recommendation)> ValidateTestCaseAsync(TestCase testCase, string inputData, string llmModel)
        {
            if (testCase == null)
                throw new ArgumentNullException(nameof(testCase));
            ValidateInput(inputData, llmModel);

            bool isValid = true;
            string? recommendation = null;

            if (testCase.Steps == null || testCase.Steps.Count == 0)
            {
                isValid = false;
                recommendation = "Тест-кейс не содержит шагов. Добавьте как минимум один шаг с действием и ожидаемым результатом.";
            }
            else if (string.IsNullOrWhiteSpace(testCase.Precondition))
            {
                isValid = false;
                recommendation = "Отсутствует предусловие. Укажите предусловие для тест-кейса.";
            }
            else if (testCase.Steps.Any(s => string.IsNullOrWhiteSpace(s.Action) || string.IsNullOrWhiteSpace(s.ExpectedResult)))
            {
                isValid = false;
                recommendation = "Один или несколько шагов содержат пустые поля Action или ExpectedResult. Убедитесь, что все шаги заполнены корректно.";
            }
            else if (string.IsNullOrWhiteSpace(testCase.TestCode))
            {
                isValid = false;
                recommendation = "Код тест-кейса отсутствует. Убедитесь, что LLM генерирует валидный JUnit-код.";
            }

            if (isValid)
            {
                string testCaseJson = JsonSerializer.Serialize(testCase, JsonSerializerConfig.Default);
                string prompt = $@"Проверьте тест-кейс: {testCaseJson} на соответствие входным данным: {inputData}.
                Убедитесь, что:
                - Тест-кейс полностью соответствует входным данным.
                - Шаги теста покрывают ключевые сценарии.
                - Код теста (testCode) корректен и соответствует шагам.
                Верните JSON:
                {{
                    ""isValid"": true/false,
                    ""recommendation"": ""Рекомендация для улучшения или null""
                }}";
                string llmResponse = await _llmService.CallLlmAsync(prompt, llmModel);
                var validationResult = JsonSerializer.Deserialize<ValidationResult>(llmResponse, JsonSerializerConfig.Default)
                    ?? throw new InvalidOperationException("Не удалось десериализовать ответ LLM в результат валидации");
                isValid = validationResult.IsValid;
                recommendation = validationResult.Recommendation ?? recommendation;
            }

            _logger.LogInformation($"Валидация тест-кейса с ID {testCase.Id}: IsValid={isValid}, Recommendation={recommendation}");
            return (isValid, recommendation);
        }

        public string CreatePrompt(string inputData, TestCase? existingTestCase = null)
        {
            bool isUpdate = existingTestCase != null;
            string testCaseJson = isUpdate ? JsonSerializer.Serialize(existingTestCase) : string.Empty;
            
            string number = isUpdate ? existingTestCase!.Number : $"TC-{DateTime.UtcNow.Ticks}";
            string creationDate = isUpdate ? existingTestCase!.CreationDate.ToString("yyyy-MM-dd") : DateTime.UtcNow.ToString("yyyy-MM-dd");
            string author = isUpdate ? existingTestCase!.Author : "AI Generated";
            string status = isUpdate ? existingTestCase!.Status.ToString() : "Development";

            string promptStart = isUpdate
                ? $"На основе существующего тест-кейса: {testCaseJson} и новых изменений: {inputData} Обнови тест-кейс"
                : $"На основе входных данных: {inputData} Сгенерируй тест-кейс";

            return $@"{promptStart} в формате JSON:
            {{
                ""Number"": ""{number}"",
                ""CreationDate"": ""{creationDate}"",
                ""Name"": ""{(isUpdate ? "Обновленное название" : "Название тест-кейса")}"",
                ""Author"": ""{author}"",
                ""Precondition"": ""{(isUpdate ? "Обновленное предусловие" : "Описание предусловия")}"",
                ""Steps"": [
                    {{
                        ""StepNumber"": 1,
                        ""Action"": ""{(isUpdate ? "Обновленное действие" : "Описание действия")}"",
                        ""ExpectedResult"": ""{(isUpdate ? "Обновленный результат" : "Ожидаемый результат")}""
                    }}
                ],
                ""Postcondition"": ""{(isUpdate ? "Обновленное постусловие" : "Описание постусловия")}"",
                ""Status"": ""{status}""
            }} и {(isUpdate ? "обнови" : "сгенерируй")} автоматизированный тест-кейс на Java с использованием Selenium WebDriver в формате JUnit:
                @Test
                public void testName() {{
                    // {(isUpdate ? "Обновлённый код теста" : "Код теста")}
                }}
            Ответ верни в формате JSON: {{ ""testCase"": {{ ... }}, ""testCode"": ""..."" }}
            Убедитесь, что:
            - Ответ содержит ТОЛЬКО валидный JSON, без Markdown-форматирования (например, ```json или ```), тегов <think> или другого текста.
            - Поле testCase.steps содержит как минимум один шаг с непустыми полями action и expectedResult.
            - Поле testCode содержит полный Java-код с правильным экранированием кавычек (\"") и переносов строк (\\n).
            - Код теста завершен (включает все шаги и закрытие WebDriver).
            - Имена свойств в JSON используют camelCase (например, ""number"", ""creationDate"").
            - Поле creationDate имеет формат ""yyyy-MM-dd"" (например, ""2025-10-06"").";
        }
    }
}