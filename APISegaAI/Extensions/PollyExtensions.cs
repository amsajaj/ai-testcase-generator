using Polly;
using Polly.Extensions.Http;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging; 

namespace APISegaAI.Extensions
{
    public static class PollyExtensions
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TaskCanceledException>()
                .Or<HttpRequestException>(ex => ex.Message.Contains("SSL connection could not be established"))
                .OrResult(response => response.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * 10),
                    onRetry: (response, timespan, retryAttempt, context) =>
                    {
                        Console.WriteLine($"Попытка повторного запроса #{retryAttempt} после задержки {timespan.TotalMilliseconds}ms. Ошибка: {response.Exception?.Message ?? response.Result?.ReasonPhrase}");
                    });
        }
    }
}