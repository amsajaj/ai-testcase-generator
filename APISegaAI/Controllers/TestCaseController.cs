namespace APISegaAI.Controllers
{
    [ApiController] 
    [Route("api/test-cases")]
    public class TestCaseController : BaseController<TestCaseController> 
    {
        private readonly ITestCaseService _testCaseService;
        private readonly IHistoryService _historyService;

        public TestCaseController(ITestCaseService testCaseService, IHistoryService historyService, ILogger<TestCaseController> logger)
            : base(logger)
        {
            _testCaseService = testCaseService ?? throw new ArgumentNullException(nameof(testCaseService));
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        }

        /// <summary>
        /// Генерирует новый тест-кейс на основе входных данных с использованием LLM
        /// </summary>
        /// <param name="inputData">Входные данные (текст требований, сценариев или URL)</param>
        /// <param name="llmModel">Модель LLM (например, qwen3-32b-awq)</param>
        /// <returns>Сгенерированный тест-кейс, код теста и рекомендация (если есть)</returns>
        /// <example>
        /// POST api/test-cases/generate
        /// 
        /// Content-Type: multipart/form-data
        /// inputData: "Система должна регистрировать пользователя с email и паролем"
        /// llmModel: "qwen3-32b-awq"
        ///
        /// Ответ:
        /// ```json
        /// {
        ///   "TestCase": {
        ///     "id": "123e4567-e89b-12d3-a456-426614174000",
        ///     "number": "TC-001",
        ///     "creationDate": "2025-09-28",
        ///     "name": "Регистрация пользователя",
        ///     "author": "AI Generated",
        ///     "precondition": "Пользователь находится на странице регистрации",
        ///     "steps": [
        ///       {
        ///         "id": "456e7890-e89b-12d3-a456-426614174001",
        ///         "testCaseId": "123e4567-e89b-12d3-a456-426614174000",
        ///         "stepNumber": 1,
        ///         "action": "Ввести email: test@example.com",
        ///         "expectedResult": "Поле email заполнено корректно"
        ///       },
        ///       {
        ///         "id": "789e1234-e89b-12d3-a456-426614174002",
        ///         "testCaseId": "123e4567-e89b-12d3-a456-426614174000",
        ///         "stepNumber": 2,
        ///         "action": "Ввести пароль: Pass1234",
        ///         "expectedResult": "Поле пароля заполнено корректно"
        ///       }
        ///     ],
        ///     "postcondition": "Пользователь зарегистрирован",
        ///     "status": "Development",
        ///     "testCode": 
        ///     "@Test\npublic void testRegistration() {\n    
        ///         driver.get(\"https://example.com/register\");\n    
        ///         WebElement email = driver.findElement(By.id(\"email\"));\n    
        ///         WebElement password = driver.findElement(By.id(\"password\"));\n    
        ///         WebElement submitButton = driver.findElement(By.id(\"submit-btn\"));\n    
        ///         email.sendKeys(\"test@example.com\");\n    
        ///         password.sendKeys(\"Pass1234\");\n    
        ///         submitButton.click();\n    
        ///         WebElement successMessage = wait.until(ExpectedConditions.presenceOfElementLocated(By.className(\"success\")));\n    
        ///         Assertions.assertTrue(successMessage.getText().contains(\"Регистрация успешна\"));\n
        ///     }",
        ///     "history": [
        ///       {
        ///         "id": "901e2345-e89b-12d3-a456-426614174003",
        ///         "testCaseId": "123e4567-e89b-12d3-a456-426614174000",
        ///         "timestamp": "2025-09-28T20:11:00Z",
        ///         "action": "Generated",
        ///         "user": "System",
        ///         "details": "Generated with model Qwen3-32b-awq"
        ///       }
        ///     ]
        ///   },
        ///   "Recommendation": null
        /// }
        /// ```
        /// </example>
        /// <response code="200">Возвращает тест-кейс, код теста и рекомендацию</response>
        /// <response code="400">Некорректные входные данные или модель LLM</response>
        [HttpPost("generate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TestCaseResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenerateTestCase([FromBody] GenerateTestCaseRequest request) =>
            await ExecuteAsync(async () =>
            {
                var (testCase, recommendation) = await _testCaseService.GenerateTestCaseAsync(request.InputData, request.LlmModel);
                await _historyService.AddHistoryEntryAsync(testCase.Id, "Generated", "System", $"Generated with model {request.LlmModel}");
                return new TestCaseResponse { TestCase = testCase, Recommendation = recommendation };
            }, "GenerateTestCase", $"Test case сгенерированный с помощью модели {request.LlmModel}");
        
        /// <summary>
        /// Обновляет шаги тест-кейса
        /// </summary>
        /// <param name="id">id тест-кейса</param>
        /// <param name="updatedSteps">Список обновленных шагов</param>
        /// <example>
        /// PUT api/test-cases/123e4567-e89b-12d3-a456-426614174000/steps
        /// Content-Type: application/json
        /// [
        ///   {
        ///     "id": "new-guid-1",
        ///     "stepNumber": 1,
        ///     "action": "Ввести email: test@example.com",
        ///     "expectedResult": "Поле заполнено"
        ///   },
        ///   {
        ///     "id": "new-guid-2",
        ///     "stepNumber": 2,
        ///     "action": "Нажать кнопку регистрации",
        ///     "expectedResult": "Регистрация успешна"
        ///   }
        /// ]
        /// </example>
        /// <response code="204">Шаги успешно обновлены</response>
        /// <response code="404">Тест-кейс не найден</response>
        [HttpPut("{id}/steps")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTestCaseSteps(
            [FromRoute, Required(ErrorMessage = "ID тест-кейса не может быть пустым"), 
            MinLength(1, ErrorMessage = "ID тест-кейса не может быть пустым")] 
            string id,
            [FromBody, Required(ErrorMessage = "Список шагов не может быть пустым")] 
            List<TestStep> updatedSteps) =>
                await ExecuteAsync(async () =>
                {
                    await _testCaseService.UpdateTestCaseStepsAsync(id, updatedSteps);
                    await _historyService.AddHistoryEntryAsync(id, "StepsUpdated", "System", $"Updated {updatedSteps.Count} steps");
                    
                }, "UpdateTestCaseSteps", $"Updated {updatedSteps.Count} steps");

        /// <summary>
        /// Обновляет тест-кейс на основе новых входных данных
        /// </summary>
        /// <param name="id">id тест-кейса</param>
        /// <param name="changesInput">Новые входные данные для обновления</param>
        /// <param name="llmModel">Модель LLM (например, qwen3-32b-awq)</param>
        /// <returns>Обновленный тест-кейс, код теста и рекомендация (если есть)</returns>
        /// <example>
        /// POST api/test-cases/123e4567-e89b-12d3-a456-426614174000/update
        /// Content-Type: multipart/form-data
        /// changesInput: "Добавить проверку пароля минимум 8 символов"
        /// llmModel: "qwen3-32b-awq"
        /// 
        /// Ответ:
        /// ```json
        /// {
        ///   "testCase": {
        ///     "id": "123e4567-e89b-12d3-a456-426614174000",
        ///     "number": "TC-001",
        ///     "creationDate": "2025-09-28",
        ///     "name": "Регистрация пользователя с усиленной валидацией",
        ///     "author": "AI Generated",
        ///     "precondition": "Пользователь находится на странице регистрации",
        ///     "steps": [
        ///       {
        ///         "id": "456e7890-e89b-12d3-a456-426614174004",
        ///         "testCaseId": "123e4567-e89b-12d3-a456-426614174000",
        ///         "stepNumber": 1,
        ///         "action": "Ввести email: test@example.com",
        ///         "expectedResult": "Поле email заполнено корректно"
        ///       },
        ///       {
        ///         "id": "789e1234-e89b-12d3-a456-426614174005",
        ///         "testCaseId": "123e4567-e89b-12d3-a456-426614174000",
        ///         "stepNumber": 2,
        ///         "action": "Ввести пароль: Pass1234!@",
        ///         "expectedResult": "Поле пароля содержит минимум 8 символов, включая буквы, цифры и спецсимволы"
        ///       }
        ///     ],
        ///     "postcondition": "Пользователь зарегистрирован и получил письмо подтверждения",
        ///     "status": "Development",
        ///     "testCode": 
        ///     "@Test\npublic void testRegistrationForm() {\n    
        ///         driver.get(\"https://example.com/register\");\n    
        ///         WebElement email = driver.findElement(By.id(\"email\"));\n    
        ///         WebElement password = driver.findElement(By.id(\"password\"));\n    
        ///         WebElement submitButton = driver.findElement(By.id(\"submit-btn\"));\n    
        ///         email.sendKeys(\"test@example.com\");\n    
        ///         password.sendKeys(\"Pass1234!@\");\n    
        ///         submitButton.click();\n    
        ///         WebElement confirmation = wait.until(ExpectedConditions.presenceOfElementLocated(By.className(\"confirmation\")));\n    
        ///         Assertions.assertTrue(confirmation.getText().contains(\"Регистрация успешна\"));\n
        ///     }",
        ///     "history": [
        ///       {
        ///         "id": "901e2345-e89b-12d3-a456-426614174006",
        ///         "testCaseId": "123e4567-e89b-12d3-a456-426614174000",
        ///         "timestamp": "2025-09-28T20:15:00Z",
        ///         "action": "Updated",
        ///         "user": "System",
        ///         "details": "Updated with changes: Добавить проверку пароля минимум 8 символов"
        ///       }
        ///     ]
        ///   },
        ///   "recommendation": null
        /// }
        /// ```
        /// </example>
        /// <response code="200">Возвращает обновленный тест-кейс, код теста и рекомендацию</response>
        /// <response code="404">Тест-кейс не найден</response>
        [HttpPost("{id}/update")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TestCaseResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTestCaseWithChanges(
            [FromRoute, Required(ErrorMessage = "ID тест-кейса не может быть пустым"),
            MinLength(1, ErrorMessage = "ID тест-кейса не может быть пустым")]
            string id,
            [FromBody] UpdateTestCaseWithChangesRequest request) =>
            await ExecuteAsync(async () =>
            {
                var (testCase, recommendation) = await _testCaseService.UpdateTestCaseWithChangesAsync(id, request.ChangesInput, request.LlmModel);
                await _historyService.AddHistoryEntryAsync(id, "Updated", "System", $"Обновлено с учетом изменений: {request.ChangesInput}");
                return new TestCaseResponse { TestCase = testCase, Recommendation = recommendation };
            }, "UpdateTestCaseWithChanges", $"Test case обновлено с учетом изменений");
        
        /// <summary>
        /// Получает тест-кейс по id
        /// </summary>
        /// <param name="id">id тест-кейса</param>
        /// <returns>Тест-кейс</returns>
        /// <example>
        /// GET api/test-cases/123e4567-e89b-12d3-a456-426614174000
        /// 
        /// Ответ:
        /// ```json
        /// {
        ///   "id": "123e4567-e89b-12d3-a456-426614174000",
        ///   "number": "TC-001",
        ///   "creationDate": "2025-09-28",
        ///   "name": "Регистрация пользователя",
        ///   "author": "AI Generated",
        ///   "precondition": "Пользователь находится на странице регистрации",
        ///   "steps": [
        ///     {
        ///       "id": "456e7890-e89b-12d3-a456-426614174001",
        ///       "testCaseId": "123e4567-e89b-12d3-a456-426614174000",
        ///       "stepNumber": 1,
        ///       "action": "Ввести email: test@example.com",
        ///       "expectedResult": "Поле email заполнено корректно"
        ///     },
        ///     {
        ///       "id": "789e1234-e89b-12d3-a456-426614174002",
        ///       "testCaseId": "123e4567-e89b-12d3-a456-426614174000",
        ///       "stepNumber": 2,
        ///       "action": "Ввести пароль: Pass1234",
        ///       "expectedResult": "Поле пароля заполнено корректно"
        ///     }
        ///   ],
        ///   "postcondition": "Пользователь зарегистрирован",
        ///   "status": "Development",
        ///   "history": [
        ///     {
        ///       "id": "901e2345-e89b-12d3-a456-426614174003",
        ///       "testCaseId": "123e4567-e89b-12d3-a456-426614174000",
        ///       "timestamp": "2025-09-28T20:11:00Z",
        ///       "action": "Generated",
        ///       "user": "System",
        ///       "details": "Generated with model Qwen3-32b-awq"
        ///     }
        ///   ]
        /// }
        /// ```
        /// </example>
        /// <response code="200">Возвращает тест-кейс</response>
        /// <response code="404">Тест-кейс не найден</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TestCase))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTestCaseById(
            [FromRoute, Required(ErrorMessage = "ID тест-кейса не может быть пустым"), 
            MinLength(1, ErrorMessage = "ID тест-кейса не может быть пустым")] 
            string id) =>
                await ExecuteAsync(async () => await _testCaseService.GetTestCaseByIdAsync(id), 
                    "GetTestCaseById", 
                    $"Извлеченный тестовый пример с ID {id}", 
                    $"Test case с ID {id} не найден");
        
        /// <summary>
        /// Получает тест-кейс по номеру
        /// </summary>
        /// <param name="number">Номер тест-кейса (например, TC-001)</param>
        /// <returns>Тест-кейс</returns>
        /// <example>
        /// GET api/test-cases/by-number/TC-001
        /// 
        /// Ответ:
        /// ```json
        /// {
        ///   "id": "123e4567-e89b-12d3-a456-426614174000",
        ///   "number": "TC-001",
        ///   "creationDate": "2025-09-28",
        ///   "name": "Регистрация пользователя",
        ///   "author": "AI Generated",
        ///   "precondition": "Пользователь находится на странице регистрации",
        ///   "steps": [
        ///     {
        ///       "id": "456e7890-e89b-12d3-a456-426614174001",
        ///       "testCaseId": "123e4567-e89b-12d3-a456-426614174000",
        ///       "stepNumber": 1,
        ///       "action": "Ввести email: test@example.com",
        ///       "expectedResult": "Поле email заполнено корректно"
        ///     },
        ///     {
        ///       "id": "789e1234-e89b-12d3-a456-426614174002",
        ///       "testCaseId": "123e4567-e89b-12d3-a456-426614174000",
        ///       "stepNumber": 2,
        ///       "action": "Ввести пароль: Pass1234",
        ///       "expectedResult": "Поле пароля заполнено корректно"
        ///     }
        ///   ],
        ///   "postcondition": "Пользователь зарегистрирован",
        ///   "status": "Development",
        ///   "history": [
        ///     {
        ///       "id": "901e2345-e89b-12d3-a456-426614174003",
        ///       "testCaseId": "123e4567-e89b-12d3-a456-426614174000",
        ///       "timestamp": "2025-09-28T20:11:00Z",
        ///       "action": "Generated",
        ///       "user": "System",
        ///       "details": "Generated with model Qwen3-32b-awq"
        ///     }
        ///   ]
        /// }
        /// ```
        /// </example>
        /// <response code="200">Возвращает тест-кейс</response>
        /// <response code="404">Тест-кейс не найден</response>
        [HttpGet("by-number/{number}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TestCase))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTestCaseByNumber(
            [FromRoute, Required(ErrorMessage = "Номер тест-кейса не может быть пустым"), 
            MinLength(1, ErrorMessage = "Номер тест-кейса не может быть пустым")] 
            string number) =>
                await ExecuteAsync(async () => await _testCaseService.GetTestCaseByNumberAsync(number), 
                    "GetTestCaseByNumber", 
                    $"Извлеченный тестовый пример с номером {number}", 
                    $"Test case c номером {number} не найден");
        
        /// <summary>
        /// Получает список всех тест-кейсов с опциональной фильтрацией по статусу
        /// </summary>
        /// <param name="status">Статус тест-кейса (Development, Active, Archive)</param>
        /// <returns>Список тест-кейсов</returns>
        /// <example>
        /// GET api/test-cases?status=Active
        /// 
        /// Ответ:
        /// ```json
        /// [
        ///   {
        ///     "id": "123e4567-e89b-12d3-a456-426614174000",
        ///     "number": "TC-001",
        ///     "creationDate": "2025-09-28",
        ///     "name": "Регистрация пользователя",
        ///     "author": "AI Generated",
        ///     "precondition": "Пользователь находится на странице регистрации",
        ///     "steps": [
        ///       {
        ///         "id": "456e7890-e89b-12d3-a456-426614174001",
        ///         "testCaseId": "123e4567-e89b-12d3-a456-426614174000",
        ///         "stepNumber": 1,
        ///         "action": "Ввести email: test@example.com",
        ///         "expectedResult": "Поле email заполнено корректно"
        ///       },
        ///       {
        ///         "id": "789e1234-e89b-12d3-a456-426614174002",
        ///         "testCaseId": "123e4567-e89b-12d3-a456-426614174000",
        ///         "stepNumber": 2,
        ///         "action": "Ввести пароль: Pass1234",
        ///         "expectedResult": "Поле пароля заполнено корректно"
        ///       }
        ///     ],
        ///     "postcondition": "Пользователь зарегистрирован",
        ///     "status": "Development",
        ///     "history": [
        ///       {
        ///         "id": "901e2345-e89b-12d3-a456-426614174003",
        ///         "testCaseId": "123e4567-e89b-12d3-a456-426614174000",
        ///         "timestamp": "2025-09-28T20:11:00Z",
        ///         "action": "Generated",
        ///         "user": "System",
        ///         "details": "Generated with model Qwen3-32b-awq"
        ///       }
        ///     ]
        ///   },
        ///   {
        ///     "id": "987e6543-e89b-12d3-a456-426614174999",
        ///     "number": "TC-002",
        ///     "creationDate": "2025-09-28",
        ///     "name": "Авторизация пользователя",
        ///     "author": "AI Generated",
        ///     "precondition": "Пользователь зарегистрирован",
        ///     "steps": [
        ///       {
        ///         "id": "456e7890-e89b-12d3-a456-426614174010",
        ///         "testCaseId": "987e6543-e89b-12d3-a456-426614174999",
        ///         "stepNumber": 1,
        ///         "action": "Ввести email: test@example.com",
        ///         "expectedResult": "Поле email заполнено"
        ///       },
        ///       {
        ///         "id": "789e1234-e89b-12d3-a456-426614174011",
        ///         "testCaseId": "987e6543-e89b-12d3-a456-426614174999",
        ///         "stepNumber": 2,
        ///         "action": "Ввести пароль: Pass1234",
        ///         "expectedResult": "Авторизация успешна"
        ///       }
        ///     ],
        ///     "postcondition": "Пользователь авторизован",
        ///     "status": "Active",
        ///     "history": [
        ///       {
        ///         "id": "901e2345-e89b-12d3-a456-426614174012",
        ///         "testCaseId": "987e6543-e89b-12d3-a456-426614174999",
        ///         "timestamp": "2025-09-28T20:12:00Z",
        ///         "action": "Generated",
        ///         "user": "System",
        ///         "details": "Generated with model Qwen3-32b-awq"
        ///       }
        ///     ]
        ///   }
        /// ]
        /// ```
        /// </example>
        /// <response code="200">Возвращает список тест-кейсов</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<TestCase>))]
        public async Task<IActionResult> GetAllTestCases([FromQuery] TestCaseStatus? status = null) =>
            await ExecuteAsync(async () => await _testCaseService.GetAllTestCasesAsync(status), 
                "GetAllTestCases", 
                $"Извлечены все test cases");
        
        /// <summary>
        /// Удаляет тест-кейс
        /// </summary>
        /// <param name="id">id тест-кейса</param>
        /// <example>
        /// DELETE api/test-cases/123e4567-e89b-12d3-a456-426614174000
        /// </example>
        /// <response code="204">Тест-кейс успешно удалён</response>
        /// <response code="404">Тест-кейс не найден</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTestCase(
            [FromRoute, Required(ErrorMessage = "ID тест-кейса не может быть пустым"), 
            MinLength(1, ErrorMessage = "ID тест-кейса не может быть пустым")] 
            string id) =>
                await ExecuteAsync(async () =>
                {
                    var result = await _testCaseService.DeleteTestCaseAsync(id);
                    if(!result)
                        throw new InvalidOperationException("Test case not found");
                }, "DeleteTestCase", $"Deleted test case with ID {id}");
    }
}