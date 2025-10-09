namespace APISegaAI.Service.Implementations
{
    /// для обработки и хранения входных данных, используемых для генерации тест-кейсов. 
    /// Поддерживает работу с файлами, текстовыми данными и URL, включая парсинг веб-страниц. 
    public class InputDataService : IInputDataService
    {
        private readonly IUnitOfWork _unitOfWork; 
        private readonly HttpClient _httpClient; // Для парсинга URL если потребуется парсинг URL с использованием специфичных HTTP-запросов (например, с авторизацией, с сертификатами и Bearer-токеном)

        public InputDataService(IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
	        _httpClient = httpClientFactory.CreateClient() ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        // Получает входные данные по id
        public async Task<InputData?> GetInputDataByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("ID не может быть пустым или null", nameof(id));

            return await _unitOfWork.InputData.GetByIdAsync(id);
        }

        // Получает входные данные, связанные с указанным тест-кейсом.
        public async Task<InputData?> GetInputDataByTestCaseIdAsync(string testCaseId)
        {
            if (string.IsNullOrEmpty(testCaseId))
                throw new ArgumentException("ID тест-кейса не может быть пустым или null", nameof(testCaseId));

            return await _unitOfWork.InputData.GetByTestCaseIdAsync(testCaseId);
        }

        /// <summary>
        /// Сохраняет входные данные (файл, текст или URL) в бд.
        /// Если передан URL, выполняется его парсинг для извлечения содержимого страницы.
        /// Сохранённые данные могут использоваться для:
        /// 1) Генерации тест-кейсов через LLM на основе контента страницы.
        /// 2) Повторного использования данных без необходимости повторного парсинга.
        /// 3) Аудита и истории операций, связывая данные с тест-кейсами через <see cref="InputData.TestCaseId"/>.
        /// </summary>
        /// <param name="file">Файл с входными данными (например, txt). Может быть null.</param>
        /// <param name="textData">Текстовые данные (например, функциональные требования). Может быть null.</param>
        /// <param name="url">URL-адрес для парсинга или сохранения как метаданные. Может быть null.</param>
        /// <param name="type">Тип входных данных (например, "Requirements", "Scenario", "URL").</param>
        /// <returns>Сохранённый объект <see cref="InputData"/>.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если <paramref name="type"/> пустой или все источники данных (файл, текст, URL) пусты.</exception>
        /// <exception cref="InvalidOperationException">Выбрасывается, если не удалось получить содержимое из источников данных.</exception>
        public async Task<InputData> SaveInputDataAsync(IFormFile? file, string? textData, string? url, string type) // нужен пример url страницы 
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("Тип данных не может быть пустым или null", nameof(type));

            if (file == null && string.IsNullOrEmpty(textData) && string.IsNullOrEmpty(url))
                throw new ArgumentException("Необходимо предоставить хотя бы один источник данных: файл, текст или URL");

            string content = string.Empty;

            // обработка файла
            if (file != null)
            {
                if (file.Length > 10 * 1024 * 1024) // Ограничение на 10 МБ
                    throw new ArgumentException("Размер файла превышает допустимый лимит (10 МБ)");

                using var stream = new StreamReader(file.OpenReadStream());
                content += await stream.ReadToEndAsync();
            }

            // обработка текстовых данных
            if (!string.IsNullOrEmpty(textData))
            {
                content += string.IsNullOrEmpty(content) ? textData : "\n" + textData;
            }

            // обработка URL
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    // сохранение URL как метаданных
                    content += string.IsNullOrEmpty(content) ? $"URL: {url}" : "\nURL: " + url;

                    var web = new HtmlWeb();
                    var doc = await web.LoadFromWebAsync(url);
                    var pageContent = doc.DocumentNode.SelectSingleNode("//body")?.InnerText.Trim();
                    if (!string.IsNullOrEmpty(pageContent))
                    {
                        content += "\nPage Content:\n" + pageContent;
                    }
                }
                catch (Exception ex)
                {
                    // если парсинг URL не удался, сохраняем только сам URL
                    content += $"\nFailed to parse URL: {ex.Message}";
                }
            }

            if (string.IsNullOrEmpty(content))
                throw new InvalidOperationException("Не удалось получить содержимое из предоставленных данных");

            var inputData = new InputData
            {
                Content = content,
                Type = type,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.InputData.AddAsync(inputData);
            await _unitOfWork.SaveChangesAsync();

            return inputData;
        }
    }
}