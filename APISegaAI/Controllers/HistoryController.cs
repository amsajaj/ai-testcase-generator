namespace APISegaAI.Controllers
{
    [ApiController] 
    [Route("api/history")]
    public class HistoryController : BaseController<HistoryController>
    {
        private readonly IHistoryService _historyService; 
        
        public HistoryController(IHistoryService historyService, ILogger<HistoryController> logger)
            : base(logger) => _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        
        /// <summary>
        /// Получает историю операций для указанного тест-кейса по его идентификатору.
        /// </summary>
        /// <param name="testCaseId">Уникальный идентификатор тест-кейса.</param>
        /// <returns>
        /// Возвращает список объектов <see cref="HistoryEntry"/>, представляющих историю операций для тест-кейса, или 404, если записи не найдены.
        /// </returns>
        /// <response code="200">Успешно возвращён список записей истории.</response>
        /// <response code="400">ID тест-кейса пустой или некорректный.</response>
        /// <response code="404">История для указанного тест-кейса не найдена.</response>
        /// <example>
        /// GET /api/history/by-testcase/guid-123
        /// 
        /// Response (200 OK):
        /// [
        ///   {
        ///     "id": "guid-456",
        ///     "testCaseId": "guid-123",
        ///     "timestamp": "2025-09-29T08:07:00Z",
        ///     "action": "Generated",
        ///     "user": "System",
        ///     "details": "Тест-кейс сгенерирован через LLM Qwen3-32b-awq"
        ///   },
        ///   {
        ///     "id": "guid-457",
        ///     "testCaseId": "guid-123",
        ///     "timestamp": "2025-09-29T08:10:00Z",
        ///     "action": "Edited",
        ///     "user": "User123",
        ///     "details": "Обновлены шаги тест-кейса"
        ///   }
        /// ]
        /// 
        /// Response (404 Not Found):
        /// {
        ///   "message": "История для тест-кейса с ID guid-123 не найдена"
        /// }
        /// </example>
        [HttpGet("by-testcase/{testCaseId}")]
        [ProducesResponseType(typeof(List<HistoryEntry>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHistoryByTestCaseId(
            [FromRoute]
            [Required(ErrorMessage = "ID тест-кейса не может быть пустым")]
            [MinLength(1, ErrorMessage = "ID тест-кейса не может быть пустым")]
            string testCaseId) =>
                await ExecuteAsync(async () =>
                {
                    var history = await _historyService.GetHistoryByTestCaseIdAsync(testCaseId);
                    if (history == null || !history.Any())
                    {
                        throw new InvalidOperationException($"История для тест-кейса с ID {testCaseId} не найдена");
                    }
                    return history;
                }, "GetHistoryByTestCaseId", 
                $"Успешно получена история для тест-кейса с ID {testCaseId}", 
                $"История для тест-кейса с ID {testCaseId} не найдена");
        
        /// <summary>
        /// Добавляет запись в историю операций для указанного тест-кейса.
        /// </summary>
        /// <param name="request">Объект запроса, содержащий ID тест-кейса, действие, пользователя и необязательные детали.</param>
        /// <returns>
        /// Возвращает 204 No Content при успешном добавлении записи в историю.
        /// </returns>
        /// <response code="204">Запись в историю успешно добавлена.</response>
        /// <response code="400">Некорректный запрос (например, пустой ID тест-кейса, действие или пользователь).</response>
        /// <response code="404">Тест-кейс с указанным ID не найден.</response>
        /// <example>
        /// POST /api/history
        /// Content-Type: application/json
        /// {
        ///   "testCaseId": "guid-123",
        ///   "action": "Generated",
        ///   "user": "System",
        ///   "details": "Тест-кейс сгенерирован через LLM Qwen3-32b-awq"
        /// }
        /// 
        /// Response (204 No Content):
        /// {}
        /// 
        /// Response (400 Bad Request):
        /// {
        ///   "errors": { "testCaseId": ["ID тест-кейса не может быть пустым"] }
        /// }
        /// 
        /// Response (404 Not Found):
        /// {
        ///   "message": "Тест-кейс с ID guid-123 не найден"
        /// }
        /// </example>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddHistoryEntry([FromBody] AddHistoryEntryRequest request) =>
            await ExecuteAsync(async () =>
            {
                await _historyService.AddHistoryEntryAsync(request.TestCaseId, request.Action, request.User, request.Details);
               
            }, "AddHistoryEntry", 
               $"Успешно добавлена запись в историю для тест-кейса с ID {request.TestCaseId}, действие: {request.Action}");
    }
}