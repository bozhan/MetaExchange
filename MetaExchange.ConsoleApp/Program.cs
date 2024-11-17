using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MetaExchange.ConsoleApp.Services;

namespace MetaExchange.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Set up Dependency Injection
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IExchangeRepository, ExchangeRepository>()
                // Register other services here in future steps
                .BuildServiceProvider();

            // Resolve the ExchangeRepository
            var exchangeRepository = serviceProvider.GetService<IExchangeRepository>();

            if (exchangeRepository == null)
            {
                Console.WriteLine("Failed to initialize Exchange Repository.");
                return;
            }

            // Specify the directory containing JSON files
            var directoryPath = "Exchanges"; // Ensure this directory exists and contains JSON files

            try
            {
                var exchanges = await exchangeRepository.GetAllExchangesAsync(directoryPath);
                Console.WriteLine($"Loaded {exchanges.Count} exchanges.");
                // Proceed with further processing in next steps
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading exchanges: {ex.Message}");
            }
        }
    }
}
