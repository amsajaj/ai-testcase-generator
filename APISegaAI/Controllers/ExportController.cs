namespace APISegaAI.Controllers
{
    [ApiController] 
    [Route("api/export")]
    public class ExportController : BaseController<ExportController>
    {
        private readonly IExportService _exportService;
        public ExportController(IExportService exportService, ILogger<ExportController> logger)
            : base(logger) => _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        

        /// <summary>
        /// Экспортирует тест-кейс в формате Excel.
        /// </summary>
        /// <param name="testCaseId">Уникальный идентификатор тест-кейса.</param>
        /// <returns>
        /// Файл Excel с данными тест-кейса (включая шаги).
        /// </returns>
        /// <response code="200">Успешно сгенерирован файл Excel.</response>
        /// <response code="400">ID тест-кейса пустой или некорректный.</response>
        /// <response code="404">Тест-кейс с указанным ID не найден.</response>
        /// <example>
        /// GET /api/export/testcase/guid-123/excel
        /// 
        /// Response (200 OK):
        /// Content-Type: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
        /// Content-Disposition: attachment; filename="TestCase_guid-123.xlsx"
        /// [Binary data of Excel file]
        /// 
        /// Response (404 Not Found):
        /// {
        ///   "message": "Тест-кейс с ID guid-123 не найден"
        /// }
        /// </example>
        [HttpGet("testcase/{testCaseId}/excel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportToExcel(
            [FromRoute]
            [Required(ErrorMessage = "ID тест-кейса не может быть пустым")]
            [MinLength(1, ErrorMessage = "ID тест-кейса не может быть пустым")]
            string testCaseId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Некорректный ID тест-кейса для экспорта в Excel: {TestCaseId}", testCaseId);
                return BadRequest(ModelState);
            }

            var fileContent = await _exportService.ExportToExcelAsync(testCaseId);
            _logger.LogInformation("Успешно экспортирован тест-кейс с ID {TestCaseId} в Excel", testCaseId);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"TestCase_{testCaseId}.xlsx");
        }

        /// <summary>
        /// Экспортирует тестовые данные (datapool) в формате CSV.
        /// </summary>
        /// <param name="dataPoolId">Уникальный идентификатор datapool.</param>
        /// <returns>
        /// Файл CSV с тестовыми данными.
        /// </returns>
        /// <response code="200">Успешно сгенерирован файл CSV.</response>
        /// <response code="400">ID datapool пустой или некорректный.</response>
        /// <response code="404">Datapool с указанным ID не найден.</response>
        /// <response code="400">Datapool пустой, нет данных для экспорта.</response>
        /// <example>
        /// GET /api/export/datapool/guid-456/csv
        /// 
        /// Response (200 OK):
        /// Content-Type: text/csv
        /// Content-Disposition: attachment; filename="DataPool_guid-456.csv"
        /// [Binary data of CSV file]
        /// Example content:
        /// "email","password"
        /// "test@example.com","testpass"
        /// "invalid@","wrongpass"
        /// 
        /// Response (404 Not Found):
        /// {
        ///   "message": "Datapool с ID guid-456 не найден"
        /// }
        /// </example>
        [HttpGet("datapool/{dataPoolId}/csv")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportToCsv(
            [FromRoute]
            [Required(ErrorMessage = "ID datapool не может быть пустым")]
            [MinLength(1, ErrorMessage = "ID datapool не может быть пустым")]
            string dataPoolId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Некорректный ID datapool для экспорта в CSV: {DataPoolId}", dataPoolId);
                return BadRequest(ModelState);
            }

            var fileContent = await _exportService.ExportToCsvAsync(dataPoolId);
            _logger.LogInformation("Успешно экспортирован datapool с ID {DataPoolId} в CSV", dataPoolId);
            return File(fileContent, "text/csv", $"DataPool_{dataPoolId}.csv");
        }

        /// <summary>
        /// Экспортирует тест-кейс в Zephyr Scale через REST API.
        /// </summary>
        /// <param name="testCaseId">Уникальный идентификатор тест-кейса.</param>
        /// <returns>
        /// Подтверждение успешного экспорта.
        /// </returns>
        /// <response code="200">Тест-кейс успешно экспортирован в Zephyr Scale.</response>
        /// <response code="400">ID тест-кейса пустой или некорректный.</response>
        /// <response code="404">Тест-кейс с указанным ID не найден.</response>
        /// <response code="500">Ошибка при экспорте в Zephyr Scale (например, сбой API).</response>
        /// <example>
        /// POST /api/export/testcase/guid-123/zephyr
        /// 
        /// Response (200 OK):
        /// {
        ///   "message": "Тест-кейс успешно экспортирован в Zephyr Scale"
        /// }
        /// 
        /// Response (404 Not Found):
        /// {
        ///   "message": "Тест-кейс с ID guid-123 не найден"
        /// }
        /// </example>
        [HttpPost("testcase/{testCaseId}/zephyr")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportToZephyr(
            [FromRoute]
            [Required(ErrorMessage = "ID тест-кейса не может быть пустым")]
            [MinLength(1, ErrorMessage = "ID тест-кейса не может быть пустым")]
            string testCaseId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Некорректный ID тест-кейса для экспорта в Zephyr: {TestCaseId}", testCaseId);
                return BadRequest(ModelState);
            }

            await _exportService.ExportToZephyrAsync(testCaseId);
            _logger.LogInformation("Успешно экспортирован тест-кейс с ID {TestCaseId} в Zephyr Scale", testCaseId);
            return Ok(new { message = "Тест-кейс успешно экспортирован в Zephyr Scale" });
        }

        /// <summary>
        /// Экспортирует код тест-кейса в формате файла (например, .java).
        /// </summary>
        /// <param name="testCaseId">Уникальный идентификатор тест-кейса.</param>
        /// <returns>
        /// Файл с кодом теста (например, JUnit/Selenium).
        /// </returns>
        /// <response code="200">Успешно сгенерирован файл с кодом теста.</response>
        /// <response code="400">ID тест-кейса пустой или некорректный.</response>
        /// <response code="404">Тест-кейс с указанным ID не найден.</response>
        /// <response code="400">Тест-кейс не содержит кода теста.</response>
        /// <example>
        /// GET /api/export/testcase/guid-123/code
        /// 
        /// Response (200 OK):
        /// Content-Type: text/plain
        /// Content-Disposition: attachment; filename="TestCase_guid-123.java"
        /// [Binary data of Java file]
        /// Example content:
        /// @Test
        /// public void testLoginForm() {
        ///     driver.get("https://example.com/login");
        ///     // ... (Selenium code)
        /// }
        /// 
        /// Response (404 Not Found):
        /// {
        ///   "message": "Тест-кейс с ID guid-123 не найден"
        /// }
        /// </example>
        [HttpGet("testcase/{testCaseId}/code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportTestCode(
            [FromRoute]
            [Required(ErrorMessage = "ID тест-кейса не может быть пустым")]
            [MinLength(1, ErrorMessage = "ID тест-кейса не может быть пустым")]
            string testCaseId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Некорректный ID тест-кейса для экспорта кода: {TestCaseId}", testCaseId);
                return BadRequest(ModelState);
            }

            var fileContent = await _exportService.ExportTestCodeAsync(testCaseId);
            _logger.LogInformation("Успешно экспортирован код тест-кейса с ID {TestCaseId}", testCaseId);
            return File(fileContent, "text/plain", $"TestCase_{testCaseId}.java");
        }
    }
}