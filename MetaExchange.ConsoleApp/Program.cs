using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MetaExchange.ConsoleApp.Services;
using MetaExchange.ConsoleApp.Models;
using Microsoft.Extensions.Logging;

namespace MetaExchange.ConsoleApp
{
	class Program
	{
		static async Task Main(string[] args)
		{
			// Set up Dependency Injection and Logging
			var serviceProvider = new ServiceCollection()
				.AddSingleton<IExchangeRepository, ExchangeRepository>()
				.AddSingleton<IExecutionService, ExecutionService>()
				.AddLogging(configure => configure.AddConsole())
				.BuildServiceProvider();

			// Resolve services
			var exchangeRepository = serviceProvider.GetService<IExchangeRepository>();
			var executionService = serviceProvider.GetService<IExecutionService>();
			var logger = serviceProvider.GetService<ILogger<Program>>();

			if (exchangeRepository == null || executionService == null || logger == null)
			{
				Console.WriteLine("Failed to initialize services.");
				return;
			}

			// Specify the directory containing JSON files
			var directoryPath = "Exchanges"; // Ensure this directory exists and contains JSON files

			try
			{
				var exchanges = await exchangeRepository.GetAllExchangesAsync(directoryPath);
				logger.LogInformation($"Loaded {exchanges.Count} exchanges.");

				// Prompt user for input
				Console.WriteLine("Enter order type (Buy/Sell):");
				var orderType = Console.ReadLine()?.Trim();

				Console.WriteLine("Enter amount of BTC to transact:");
				var amountInput = Console.ReadLine()?.Trim();

				if (!double.TryParse(amountInput, out double amount) || amount <= 0)
				{
					Console.WriteLine("Invalid amount entered.");
					return;
				}

				// Get the best execution plan
				var executionPlan = await executionService.GetBestExecutionPlanAsync(exchanges, orderType, amount);

				// Display the execution plan
				DisplayExecutionPlan(executionPlan, orderType, amount);
			}
			catch (Exception ex)
			{
				logger.LogError($"An error occurred: {ex.Message}");
			}
		}

		static void DisplayExecutionPlan(ExecutionPlan plan, string orderType, double requestedAmount)
		{
			if (plan.Orders.Count == 0)
			{
				Console.WriteLine("No execution orders could be generated based on the available order books and balances.");
				return;
			}

			Console.WriteLine($"\nBest Execution Plan for {orderType}ing {requestedAmount} BTC:");
			Console.WriteLine("-------------------------------------------------------");

			foreach (var order in plan.Orders)
			{
				Console.WriteLine($"Exchange: {order.ExchangeId}");
				Console.WriteLine($"  Order ID: {order.OrderId}");
				Console.WriteLine($"  Type: {order.Type}");
				Console.WriteLine($"  Amount: {order.Amount} BTC");
				Console.WriteLine($"  Price: €{order.Price} per BTC");
				Console.WriteLine($"  Total: €{order.Amount * order.Price}");
				Console.WriteLine();
			}

			if (orderType.Equals("Buy", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine($"Total EUR Spent: €{plan.TotalCostOrRevenue:F2}");
			}
			else if (orderType.Equals("Sell", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine($"Total EUR Earned: €{plan.TotalCostOrRevenue:F2}");
			}
		}
	}
}
