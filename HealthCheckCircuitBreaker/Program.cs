using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;

namespace HealthCheckCircuitBreakerDemo
{
    class Program
    {
        private static HttpClient _httpClient = new HttpClient();
        private static AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        static void Main(string[] args)
        {
            _circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(5),
                    onBreak: (exception, timespan) => 
                        Console.WriteLine($"Circuit broken due to health check failure! Duration: {timespan}."),
                    onReset: () => 
                        Console.WriteLine("Circuit reset."),
                    onHalfOpen: () => 
                        Console.WriteLine("Circuit half-open.")
                );

            // Periodically check the health status
            var timer = new System.Timers.Timer(TimeSpan.FromSeconds(3).TotalMilliseconds);
            timer.Elapsed += async (sender, e) => await HealthCheckAsync();
            timer.Start();

            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();
        }

        private static async Task HealthCheckAsync()
        {
            try
            {
                Console.WriteLine("Running health check...");
                await _circuitBreakerPolicy.ExecuteAsync(() => CheckExternalServiceHealth());
                Console.WriteLine("Health check succeeded.");
            }
            catch (BrokenCircuitException)
            {
                Console.WriteLine("Circuit is broken. Skipping health check.");
            }
            catch (Exception)
            {
                Console.WriteLine("Health check failed.");
            }
        }

        private static async Task CheckExternalServiceHealth()
        {
            var response = await _httpClient.GetAsync("https://external-service.example.com/health");
            response.EnsureSuccessStatusCode();
        }
    }
}
