using Polly;
using Serilog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsolePolly
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.ColoredConsole()
                .CreateLogger();

            var httpClient = new HttpClient();
            var response = await Policy
                .HandleResult<HttpResponseMessage>(message => !message.IsSuccessStatusCode)
                
                .WaitAndRetryAsync(50, i => TimeSpan.FromSeconds(2), (result, timeSpan, retryCount, context) =>
                {
                    logger.Warning($"Zurich API is not working. Request failed with {result.Result.StatusCode}. Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
                })
                .ExecuteAsync(() => httpClient.GetAsync("https://demo5823023.mockable.io/"));

            if (response.IsSuccessStatusCode)
                logger.Information("API is working, response was successful! Finishing application...");
            else
                logger.Error($"Response failed. Status code {response.StatusCode}");
        }
    }
}
