// See https://aka.ms/new-console-template for more information

using Polly;
using Polly.CircuitBreaker;



class Program
{
    private static Random _random = new Random();
    private static AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
    
    static void Main(string[] args)
    {
        _circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 2,
                durationOfBreak: TimeSpan.FromSeconds(5),
                onBreak: (exception, timespan) =>
                    Console.WriteLine($"Circuit broken! Duration: {timespan}."),
                onReset: () =>
                    Console.WriteLine("Circuit reset."),
                onHalfOpen: () =>
                    Console.WriteLine("Circuit half-open.")
            );

        // Simulate some requests
        for (int i = 0; i < 10; i++)
        {
            MakeRequestAsync().Wait();
            Task.Delay(1000).Wait(); // Delay to easily observe output
        }
    }

    private static async Task MakeRequestAsync()
    {
        try
        {
            Console.WriteLine("Making a request...");
            await _circuitBreakerPolicy.ExecuteAsync(() => SimulateExternalService());
            Console.WriteLine("Request succeeded.");
        }
        catch (BrokenCircuitException)
        {
            Console.WriteLine("Circuit is currently broken. Skipping request.");
        }
        catch (Exception)
        {
            Console.WriteLine("Request failed.");
        }
    }

    private static async Task SimulateExternalService()
    {
        // Introduce some artificial delay
        await Task.Delay(500);

        // Randomly fail to simulate an unstable service
        if (_random.Next(0, 2) == 0)
        {
            throw new Exception("Simulated service failure.");
        }
    }
}