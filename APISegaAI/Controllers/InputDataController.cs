namespace APISegaAI.Controllers
{
    [ApiController] 
    [Route("ap/input-data")] 
    public class InputDataController : BaseController<InputDataController>
    {
        private readonly IInputDataService _inputDataService; 

        public InputDataController(IInputDataService inputDataService, ILogger<InputDataController> logger)
            : base(logger) => _inputDataService = inputDataService ?? throw new ArgumentNullException(nameof(inputDataService));
        
        /// <summary>
        /// Сохраняет входные данные для генерации тест-кейсов (файл, текст или URL).
        /// </summary>
        /// <param name="file">Файл с данными (JSON, TXT, PDF, DOCX, необязательный).</param>
        /// <param name="textData">Текстовые данные (например, функциональные требования, необязательные).</param>
        /// <param name="url">URL для парсинга содержимого (необязательный).</param>
        /// <param name="type">
        ///     Тип данных (например, "Requirements", "Scenario", "URL"). 
        ///     что бы понять, какого рода данные обрабатываются 
        ///     (например, функциональные требования, пользовательский сценарий, содержимое веб-страницы).
        /// </param>
        /// <returns>
        /// Возвращает объект <see cref="InputData"/>, содержащий сохранённые входные данные.
        /// </returns>
        /// <response code="200">Успешно сохранены входные данные. Возвращает объект InputData.</response>
        /// <response code="400">Некорректный запрос (например, отсутствуют все источники данных или неверный тип).</response>
        /// <response code="413">Размер файла превышает допустимый лимит (10 МБ).</response>
        /// <example>
        /// POST /api/input-data?type=Requirements
        /// Content-Type: multipart/form-data
        /// [form-data]
        /// file: (upload file "requirements.txt" with content: "Система должна позволять регистрироваться с email")
        /// textData: "Пользователь вводит email и пароль"
        /// url: "https://example.com/register"
        /// type: "Requirements"
        /// 
        /// Response (200 OK):
        /// {
        ///   "id": "guid-123",
        ///   "content": "Система должна позволять регистрироваться с email\nПользователь вводит email и пароль\nURL: https://example.com/register\nPage Content: Register page content...",
        ///   "type": "Requirements",
        ///   "createdAt": "2025-09-29T07:44:00Z",
        ///   "testCaseId": null
        /// }
        /// </example>
        [HttpPost]
        [ProducesResponseType(typeof(InputData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        public async Task<IActionResult> SaveInputData(
            [FromForm] IFormFile? file,
            [FromForm] string? textData,
            [FromForm] string? url,
            [FromQuery]
            [Required(ErrorMessage = "Тип данных не может быть пустым")]
            [MinLength(1, ErrorMessage = "Тип данных не может быть пустым")]
            string type) =>
                await ExecuteAsync(async () =>
                {
                    var inputData = await _inputDataService.SaveInputDataAsync(file, textData, url, type);
                    return (inputData, $"Успешно сохранены входные данные с ID {inputData?.Id} и типом {type}");
                }, "SaveInputData");
        
        /// <summary>
        /// Получает входные данные по id.
        /// </summary>
        /// <param name="id">id входных данных.</param>
        /// <returns>
        /// Возвращает объект <see cref="InputData"/> или 404, если данные не найдены.
        /// </returns>
        /// <response code="200">Успешно возвращены входные данные.</response>
        /// <response code="400">ID пустой или некорректный.</response>
        /// <response code="404">Входные данные с указанным ID не найдены.</response>
        /// <example>
        /// GET /api/input-data/guid-123
        /// 
        /// Response (200 OK):
        /// {
        ///   "id": "guid-123",
        ///   "content": "Система должна позволять регистрироваться с email\nURL: https://example.com/register",
        ///   "type": "Requirements",
        ///   "createdAt": "2025-09-29T07:44:00Z",
        ///   "testCaseId": null
        /// }
        /// 
        /// Response (404 Not Found):
        /// {
        ///   "message": "Входные данные с ID guid-123 не найдены"
        /// }
        /// </example>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(InputData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInputDataById(
            [FromRoute]
            [Required(ErrorMessage = "ID не может быть пустым")]
            [MinLength(1, ErrorMessage = "ID не может быть пустым")]
            string id)
        {
            var validationResult = ValidateRequiredString(id, "id");
            if (validationResult != null) return validationResult;

            return await ExecuteAsync(async () => await _inputDataService.GetInputDataByIdAsync(id),
                "GetInputDataById",
                $"Успешно получены входные данные с ID {id}",
                $"Входные данные с ID {id} не найдены");
        }

        /// <summary>
        /// Получает входные данные, связанные с указанным тест-кейсом.
        /// </summary>
        /// <param name="testCaseId">id тест-кейса.</param>
        /// <returns>
        /// Возвращает объект <see cref="InputData"/> или 404, если данные не найдены.
        /// </returns>
        /// <response code="200">Успешно возвращены входные данные.</response>
        /// <response code="400">ID тест-кейса пустой или некорректный.</response>
        /// <response code="404">Входные данные для указанного тест-кейса не найдены.</response>
        /// <example>
        /// GET /api/input-data/by-testcase/guid-456
        /// 
        /// Response (200 OK):
        /// {
        ///   "id": "guid-123",
        ///   "content": "Система должна позволять регистрироваться с email\nURL: https://example.com/register",
        ///   "type": "Requirements",
        ///   "createdAt": "2025-09-29T07:44:00Z",
        ///   "testCaseId": "guid-456"
        /// }
        /// 
        /// Response (404 Not Found):
        /// {
        ///   "message": "Входные данные для тест-кейса с ID guid-456 не найдены"
        /// }
        /// </example>
        [HttpGet("by-testcase/{testCaseId}")]
        [ProducesResponseType(typeof(InputData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInputDataByTestCaseId(
            [FromRoute]
            [Required(ErrorMessage = "ID тест-кейса не может быть пустым")]
            [MinLength(1, ErrorMessage = "ID тест-кейса не может быть пустым")]
            string testCaseId)
        {
            var validationResult = ValidateRequiredString(testCaseId, "testCaseId");
            if (validationResult != null) return validationResult;

            return await ExecuteAsync(async () => await _inputDataService.GetInputDataByTestCaseIdAsync(testCaseId),
                "GetInputDataByTestCaseId",
                $"Успешно получены входные данные для тест-кейса с ID {testCaseId}",
                $"Входные данные для тест-кейса с ID {testCaseId} не найдены");
        }
    }
}