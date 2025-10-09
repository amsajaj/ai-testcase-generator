using System.Globalization;
using System.Text.RegularExpressions;

namespace APISegaAI.Service.Implementations
{
    /// для взаимодействия с большими языковыми моделями (LLM) через REST API. 
    /// предоставляет функциональность для отправки запросов к моделям (Qwen3-30B-A3B, Qwen3-32b, Gemma 3-27b) 
    /// и получения ответов для генерации тест-кейсов, проверки или создания тестовых данных. 
    public class LlmService : ILlmService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LlmService> _logger;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, string> _modelEndpoints;

        public LlmService(IHttpClientFactory clientFactory, ILogger<LlmService> logger, IConfiguration configuration)
        {
            _httpClient = clientFactory.CreateClient("LlmClient") ?? throw new ArgumentNullException(nameof(clientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            var endpointsSection = _configuration.GetSection("LlmSettings:ModelEndpoints");
            if (endpointsSection.Exists())
            {
                foreach (var endpoint in endpointsSection.GetChildren())
                {
                    _modelEndpoints[endpoint.Key] = endpoint.Value;
                }
            }
        }

        /// <summary>
        /// Асинхронно отправляет запрос к LLM с указанным промптом и моделью, возвращая текстовый ответ.
        /// </summary>
        /// <param name="prompt">Текст запроса (промпт) для LLM. Не может быть пустым.</param>
        /// <param name="model">Название модели LLM (например, "qwen3-32b-awq", "qwen3-30b-a3b", "gemma-3-27b"). Не может быть пустым.</param>
        /// <returns>Задача, возвращающая строку с ответом от LLM.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если промпт или модель пустые или null.</exception>
        /// <exception cref="InvalidOperationException">Выбрасывается при ошибке подключения к LLM, пустом ответе или ошибке парсинга ответа.</exception>
        /// <exception cref="Exception">Выбрасывается при неизвестных ошибках во время выполнения запроса.</exception>
        public async Task<string> CallLlmAsync(string prompt, string model)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Промт не может быть нулевым или пустым", nameof(prompt));

            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Модель LLM не может быть нулевой или пустой", nameof(model));

            var maxTokens = model switch
            {
                "gemma-3-27b-it-bnb-4bit" => Math.Min(16000, int.Parse(_configuration["LlmSettings:MaxTokens"] ?? "10000")),
                _ => Math.Min(32000, int.Parse(_configuration["LlmSettings:MaxTokens"] ?? "10000"))
            };

            var payload = new
            {
                model,
                messages = new[] { new { role = "user", content = prompt } },
                stream = false,
                max_tokens = maxTokens,
                temperature = double.Parse(_configuration["LlmSettings:Temperature"]?.Replace(",", ".") ?? "0.7",  CultureInfo.InvariantCulture),
                think_mode = false
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var endpoint = _modelEndpoints[model];
            _logger.LogInformation($"Отправка запроса LLM с моделью {model} на эндпоинт {endpoint}");

            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Получен ответ от LLM для модели {model}");

            var jsonDoc = JsonDocument.Parse(responseContent);
            var answer = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? throw new InvalidOperationException("Ответ LLM не содержит содержимого");

            answer = RemoveMarkdownAndThinkTags(answer ?? throw new ArgumentNullException(nameof(answer)));
            if (!IsValidJson(answer))
            {
                _logger.LogWarning("Получен некорректный JSON от LLM: {Response}", answer);
                // Повторный запрос с уточнением
                string retryPrompt = $"{prompt}\nПредыдущий ответ содержал некорректный JSON или дополнительные теги. Верните ТОЛЬКО валидный JSON с полным кодом теста, без тегов <think> или другого текста.";
                answer = await CallLlmAsync(retryPrompt, model);
                answer = RemoveMarkdownAndThinkTags(answer);
            }
            return answer ?? string.Empty;
        }

        private string RemoveMarkdownAndThinkTags(string input)
        {
            var result = Regex.Replace(input, @"<think>[\s\S]*?</think>", "");
            result = Regex.Replace(result, @"```json\s*([\s\S]*?)\s*```", "$1");
            result = Regex.Replace(result, @"```\s*([\s\S]*?)\s*```", "$1");
            return result.Trim();
        }

        private bool IsValidJson(string input)
        {
            try
            {
                JsonDocument.Parse(input);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}