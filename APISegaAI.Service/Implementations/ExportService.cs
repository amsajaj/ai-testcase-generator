namespace APISegaAI.Service.Implementations
{
    /// для экспорта тест-кейсов и тестовых данных в различные форматы. 
    /// Поддерживает экспорт тест-кейсов в Excel, тестовых данных в CSV и интеграцию с Zephyr Scale через REST API. 
    public class ExportService : IExportService
    {
        private readonly IUnitOfWork _unitOfWork; 
        private readonly HttpClient _httpClient; 
        private readonly IConfiguration _configuration;
        
        public ExportService(IUnitOfWork unitOfWork, IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpClient = clientFactory.CreateClient("ZephyrClient");
        }

        // Экспортирует тестовые данные (datapool) в CSV-файл.
        public async Task<byte[]> ExportToCsvAsync(string dataPoolId)
        {
            if (string.IsNullOrEmpty(dataPoolId))
                throw new ArgumentException("ID datapool не может быть пустым или null", nameof(dataPoolId));

            var dataPool = await _unitOfWork.DataPools.GetByIdAsync(dataPoolId);
            if (dataPool == null)
                throw new InvalidOperationException($"Datapool с ID {dataPoolId} не найден");

            var firstItem = dataPool.Items.FirstOrDefault();
            if (firstItem == null)
                throw new InvalidOperationException("Datapool пустой, нет данных для экспорта");

            var jsonDoc = JsonDocument.Parse(firstItem.Data);
            var headers = jsonDoc.RootElement.EnumerateObject().Select(p => p.Name).ToList();

            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);

            await writer.WriteLineAsync(string.Join(",", headers.Select(h => $"\"{h.Replace("\"", "\"\"")}\"")));

            foreach (var item in dataPool.Items)
            {
                var json = JsonDocument.Parse(item.Data);
                var values = headers.Select(h =>
                {
                    var value = json.RootElement.GetProperty(h).ToString();
                    return $"\"{value.Replace("\"", "\"\"")}\"";
                });
                await writer.WriteLineAsync(string.Join(",", values));
            }

            await writer.FlushAsync();
            return memoryStream.ToArray();
        }

        // Экспортирует тест-кейс в Excel-файл.
        public async Task<byte[]> ExportToExcelAsync(string testCaseId)
        {
            if (string.IsNullOrEmpty(testCaseId))
                throw new ArgumentException("ID тест-кейса не может быть пустым или null", nameof(testCaseId));

            var testCase = await _unitOfWork.TestCases.GetByIdAsync(testCaseId);
            if (testCase == null)
                throw new InvalidOperationException($"Тест-кейс с ID {testCaseId} не найден");

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("TestCase");

            worksheet.Cell(1, 1).Value = "Number";
            worksheet.Cell(1, 2).Value = "Creation Date";
            worksheet.Cell(1, 3).Value = "Name";
            worksheet.Cell(1, 4).Value = "Author";
            worksheet.Cell(1, 5).Value = "Precondition";
            worksheet.Cell(1, 6).Value = "Postcondition";
            worksheet.Cell(1, 7).Value = "Status";
            worksheet.Cell(1, 8).Value = "Step Number";
            worksheet.Cell(1, 9).Value = "Action";
            worksheet.Cell(1, 10).Value = "Expected Result";

            worksheet.Cell(2, 1).Value = testCase.Number;
            worksheet.Cell(2, 2).Value = testCase.CreationDate.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cell(2, 3).Value = testCase.Name;
            worksheet.Cell(2, 4).Value = testCase.Author;
            worksheet.Cell(2, 5).Value = testCase.Precondition;
            worksheet.Cell(2, 6).Value = testCase.Postcondition;
            worksheet.Cell(2, 7).Value = testCase.Status.ToString();

            // Шаги
            int row = 2;
            foreach (var step in testCase.Steps)
            {
                worksheet.Cell(row, 8).Value = step.StepNumber;
                worksheet.Cell(row, 9).Value = step.Action;
                worksheet.Cell(row, 10).Value = step.ExpectedResult;
                row++;
            }

            worksheet.Column(1).Width = 25;  // Number
            worksheet.Column(2).Width = 10;  // Creation Date
            worksheet.Column(3).Width = 35;  // Name
            worksheet.Column(4).Width = 10;  // Author
            worksheet.Column(5).Width = 35;  // Precondition
            worksheet.Column(6).Width = 35;  // Postcondition
            worksheet.Column(7).Width = 12;  // Status
            worksheet.Column(8).Width = 12;  // Step Number
            worksheet.Column(9).Width = 40;  // Action
            worksheet.Column(10).Width = 40; // Expected Result
            //worksheet.Columns().AdjustToContents(); прооблема из за linux
                        
            using var memoryStrem = new MemoryStream();
            workbook.SaveAs(memoryStrem);

            return memoryStrem.ToArray();
        }

        /// <summary>
        /// Экспортирует тест-кейс в Zephyr Scale через REST API.
        /// </summary>
        /// <param name="testCaseId">Идентификатор тест-кейса.</param>
        /// <returns>Задача, представляющая асинхронную операцию экспорта.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если <paramref name="testCaseId"/> пустой или null.</exception>
        /// <exception cref="InvalidOperationException">Выбрасывается, если тест-кейс не найден или произошла ошибка при экспорте.</exception>
        public async Task ExportToZephyrAsync(string testCaseId)
        {
            if (string.IsNullOrEmpty(testCaseId))
                throw new ArgumentException("ID тест-кейса не может быть пустым или null", nameof(testCaseId));

            var testCase = await _unitOfWork.TestCases.GetByIdAsync(testCaseId);
            if (testCase == null)
                throw new InvalidOperationException($"Тест-кейс с ID {testCaseId} не найден");

            // Формирование payload для Zephyr Scale API
            var payload = new
            {
                projectKey = _configuration["ZephyrSettings:ProjectKey"] ?? "TEST",
                name = testCase.Name,
                precondition = testCase.Precondition,
                objective = $"Тест-кейс {testCase.Number} для проверки функционала",
                status = testCase.Status.ToString(),
                steps = testCase.Steps.Select(s => new
                {
                    description = s.Action,
                    expectedResult = s.ExpectedResult
                }).ToList()
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            try
            {
                var response = await _httpClient.PostAsync("v2/testcases", content);
                response.EnsureSuccessStatusCode();

                await _unitOfWork.HistoryEntries.AddAsync(new HistoryEntry
                {
                    TestCaseId = testCaseId,
                    Action = "Exported",
                    User = "System",
                    Details = "Exported to Zephyr Scale"
                });
                await _unitOfWork.SaveChangesAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Ошибка при экспорте в Zephyr Scale: {ex.Message}", ex);
            }
        }

        public async Task<byte[]> ExportTestCodeAsync(string testCaseId)
        {
            if (string.IsNullOrEmpty(testCaseId))
                throw new ArgumentException("ID тест-кейса не может быть пустым или null", nameof(testCaseId));

            var testCase = await _unitOfWork.TestCases.GetByIdAsync(testCaseId);

            if (testCase == null)
                throw new InvalidOperationException($"Тест-кейс с ID {testCaseId} не найден");

            if (string.IsNullOrEmpty(testCase.TestCode))
                throw new InvalidOperationException($"Тест-кейс с ID {testCaseId} не содержит кода теста");

            return Encoding.UTF8.GetBytes(testCase.TestCode);
        }
    }
}