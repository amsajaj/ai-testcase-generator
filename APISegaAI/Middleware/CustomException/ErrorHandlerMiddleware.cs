namespace APISegaAI.Middleware.CustomException
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next; 
        private readonly ILogger _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            string errorMessage;
            int statusCode;

            switch (exception)
            {
                case AppException argEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    errorMessage = argEx.Message;
                    _logger.LogWarning(argEx, "Некорректный запрос: {Message}", argEx.Message);
                    break;

                case InvalidOperationException invOpEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    errorMessage = invOpEx.Message;
                    _logger.LogWarning(invOpEx, "Ошибка операции: {Message}", invOpEx.Message);
                    break;

                case KeyNotFoundException keyNotFoundEx:
                    statusCode = StatusCodes.Status404NotFound;
                    errorMessage = keyNotFoundEx.Message;
                    _logger.LogWarning(keyNotFoundEx, "Ресурс не найден: {Message}", keyNotFoundEx.Message);
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    errorMessage = "Внутренняя ошибка сервера";
                    _logger.LogError(exception, "Необработанная ошибка: {Message}", exception.Message);
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new { error = errorMessage };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}