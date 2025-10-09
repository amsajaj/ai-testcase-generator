namespace APISegaAI.Controllers
{
    [ApiController]
    public abstract class BaseController<T> : ControllerBase where T : BaseController<T>
    {
        protected readonly ILogger<T> _logger;

        protected BaseController(ILogger<T> logger) =>
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        protected IActionResult HandleValidationErrors()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                _logger.LogWarning("Произошли ошибки проверки: {Errors}", string.Join("; ", errors));
                return BadRequest(new { Errors = errors });
            }
            return null!;
        }

        protected async Task<IActionResult> ExecuteAsync<TResponse>(
            Func<Task<TResponse>> action,
            string operationName,
            string successMessage = null!,
            string notFoundMessage = null!)
        {
            var validationResult = HandleValidationErrors();
            if (validationResult != null)
            {
                return validationResult;
            }

            var result = await action();
            if (result == null && notFoundMessage != null)
            {
                _logger.LogWarning($"{operationName} ошибка: {notFoundMessage}");
                return NotFound(notFoundMessage);
            }

            if (successMessage != null)
            {
                _logger.LogInformation($"{operationName} успех: {successMessage}");
            }

            return result switch
            {
                null => NoContent(),
                _ => Ok(result)
            };
        }

        protected async Task<IActionResult> ExecuteAsync(
            Func<Task> action,
            string operationName,
            string successMessage = null!)
        {
            var validationResult = HandleValidationErrors();
            if (validationResult != null)
            {
                return validationResult;
            }

            await action();
           
            if (successMessage != null)
            {
                _logger.LogInformation($"{operationName} успех: {successMessage}");
            }

            return NoContent();
        }

        protected IActionResult ReturnFileContent(byte[] fileContent, string contentType, string fileName)
        {
            if (fileContent == null || fileContent.Length == 0)
            {
                _logger.LogWarning($"Содержимое файла пусто для {fileName}");
                return NotFound($"Файл {fileName} не удалось сгенерировать.");
            }

            _logger.LogInformation($"Успешно сгенерированный файл {fileName}");
            return File(fileContent, contentType, fileName);
        }

        protected IActionResult ValidateRequiredString(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.LogWarning($"Обязательный параметр {parameterName} является пустым или null");
                return BadRequest(new { Error = $"{parameterName} не может быть пустым" });
            }
            return null!;
        }
    }
}