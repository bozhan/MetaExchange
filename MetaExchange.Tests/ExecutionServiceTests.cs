using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using MetaExchange.ConsoleApp.Services;
using MetaExchange.ConsoleApp.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace MetaExchange.Tests
{
	public class ExecutionServiceTests
	{
		private readonly IExecutionService _executionService;

		public ExecutionServiceTests()
		{
			_executionService = new ExecutionService();
		}

		[Fact]
		public async Task GetBestExecutionPlanAsync_BuyOrder_ReturnsCorrectPlan()
		{
			// Arrange
			var exchanges = new List<Exchange>
			{
				new Exchange
				{
					Id = "exchange-01",
					AvailableFunds = new AvailableFunds { Crypto = 10, Euro = 100000 },
					OrderBook = new OrderBook
					{
						Asks = new List<OrderWrapper>
						{
							new OrderWrapper { Order = new Order { Id = "ask-1", Type = "Sell", Amount = 5, Price = 50000 } },
							new OrderWrapper { Order = new Order { Id = "ask-2", Type = "Sell", Amount = 5, Price = 51000 } }
						},
						Bids = new List<OrderWrapper>() // Not needed for Buy
                    }
				}
			};

			string orderType = "Buy";
			double amount = 7;

			// Act
			var plan = await _executionService.GetBestExecutionPlanAsync(exchanges, orderType, amount);

			// Assert
			Assert.NotNull(plan);
			Assert.Equal(2, plan.Orders.Count);
			Assert.Equal(7, plan.Orders.Sum(o => o.Amount));
			Assert.Equal(5, plan.Orders[0].Amount);
			Assert.Equal(50000, plan.Orders[0].Price);
			Assert.Equal(2, plan.Orders[1].Amount);
			Assert.Equal(51000, plan.Orders[1].Price);
			Assert.Equal(5 * 50000 + 2 * 51000, plan.TotalCostOrRevenue);
		}

		[Fact]
		public async Task GetBestExecutionPlanAsync_SellOrder_ReturnsCorrectPlan()
		{
			// Arrange
			var exchanges = new List<Exchange>
			{
				new Exchange
				{
					Id = "exchange-01",
					AvailableFunds = new AvailableFunds { Crypto = 10, Euro = 100000 },
					OrderBook = new OrderBook
					{
						Bids = new List<OrderWrapper>
						{
							new OrderWrapper { Order = new Order { Id = "bid-1", Type = "Buy", Amount = 5, Price = 60000 } },
							new OrderWrapper { Order = new Order { Id = "bid-2", Type = "Buy", Amount = 5, Price = 59000 } }
						},
						Asks = new List<OrderWrapper>() // Not needed for Sell
                    }
				}
			};

			string orderType = "Sell";
			double amount = 7;

			// Act
			var plan = await _executionService.GetBestExecutionPlanAsync(exchanges, orderType, amount);

			// Assert
			Assert.NotNull(plan);
			Assert.Equal(2, plan.Orders.Count);
			Assert.Equal(7, plan.Orders.Sum(o => o.Amount));
			Assert.Equal(5, plan.Orders[0].Amount);
			Assert.Equal(60000, plan.Orders[0].Price);
			Assert.Equal(2, plan.Orders[1].Amount);
			Assert.Equal(59000, plan.Orders[1].Price);
			Assert.Equal(5 * 60000 + 2 * 59000, plan.TotalCostOrRevenue);
		}

		[Fact]
		public async Task GetBestExecutionPlanAsync_BuyOrder_InsufficientFunds_ReturnsPartialPlan()
		{
			// Arrange
			var exchanges = new List<Exchange>
			{
				new Exchange
				{
					Id = "exchange-01",
					AvailableFunds = new AvailableFunds { Crypto = 10, Euro = 300000 }, // Limited EUR
                    OrderBook = new OrderBook
					{
						Asks = new List<OrderWrapper>
						{
							new OrderWrapper { Order = new Order { Id = "ask-1", Type = "Sell", Amount = 5, Price = 60000 } },
							new OrderWrapper { Order = new Order { Id = "ask-2", Type = "Sell", Amount = 5, Price = 61000 } }
						},
						Bids = new List<OrderWrapper>() // Not needed for Buy
                    }
				}
			};

			string orderType = "Buy";
			double amount = 7;

			// Act
			var plan = await _executionService.GetBestExecutionPlanAsync(exchanges, orderType, amount);

			// Assert
			Assert.NotNull(plan);
			// Calculate how much can be bought with 300,000 EUR
			// 5 BTC * 60,000 = 300,000 EUR
			Assert.Single(plan.Orders);
			Assert.Equal(5, plan.Orders[0].Amount);
			Assert.Equal(60000, plan.Orders[0].Price);
			Assert.Equal(5 * 60000, plan.TotalCostOrRevenue);
		}

		[Fact]
		public async Task GetBestExecutionPlanAsync_SellOrder_InsufficientBtc_ReturnsPartialPlan()
		{
			// Arrange
			var exchanges = new List<Exchange>
			{
				new Exchange
				{
					Id = "exchange-01",
					AvailableFunds = new AvailableFunds { Crypto = 4, Euro = 100000 }, // Limited BTC
                    OrderBook = new OrderBook
					{
						Bids = new List<OrderWrapper>
						{
							new OrderWrapper { Order = new Order { Id = "bid-1", Type = "Buy", Amount = 3, Price = 70000 } },
							new OrderWrapper { Order = new Order { Id = "bid-2", Type = "Buy", Amount = 3, Price = 69000 } }
						},
						Asks = new List<OrderWrapper>() // Not needed for Sell
                    }
				}
			};

			string orderType = "Sell";
			double amount = 7;

			// Act
			var plan = await _executionService.GetBestExecutionPlanAsync(exchanges, orderType, amount);

			// Assert
			Assert.NotNull(plan);
			// Only 4 BTC available to sell
			Assert.Equal(2, plan.Orders.Count);
			Assert.Equal(3, plan.Orders[0].Amount);
			Assert.Equal(70000, plan.Orders[0].Price);
			Assert.Equal(1, plan.Orders[1].Amount);
			Assert.Equal(69000, plan.Orders[1].Price);
			Assert.Equal(3 * 70000 + 1 * 69000, plan.TotalCostOrRevenue);
		}

		// Additional tests can be added here to cover more edge cases
	}
}
