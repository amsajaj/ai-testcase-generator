using APISegaAI.Middleware.CustomException;

namespace APISegaAI.Middleware.Extensions
{
    public static class ExtensionsMiddleware
    {
        public static IApplicationBuilder UseErrorHandlerMiddleware(this IApplicationBuilder builder) =>
            builder.UseMiddleware<ErrorHandlerMiddleware>();
    }
}