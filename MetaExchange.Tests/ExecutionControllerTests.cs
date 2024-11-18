using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MetaExchange.ConsoleApp.Models;
using MetaExchange.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Xunit;

namespace MetaExchange.WebApi.Tests
{
	public class ExecutionControllerTests : IClassFixture<CustomWebApplicationFactory>
	{
		private readonly HttpClient _client;
		private readonly CustomWebApplicationFactory _factory;

		public ExecutionControllerTests(CustomWebApplicationFactory factory)
		{
			_factory = factory;

			// Ensure the environment is set to Development for detailed errors
			_client = _factory.WithWebHostBuilder(builder =>
			{
				builder.UseEnvironment("Development");
			}).CreateClient();
		}

		[Theory]
		[InlineData("Buy", 0, HttpStatusCode.BadRequest)]
		[InlineData("Buy", 7, HttpStatusCode.OK)]
		[InlineData("InvalidType", 5, HttpStatusCode.BadRequest)]
		[InlineData("Buy", -1, HttpStatusCode.BadRequest)]
		[InlineData("Sell", 5, HttpStatusCode.OK)]
		[InlineData("", 5, HttpStatusCode.BadRequest)]
		[InlineData(null, 5, HttpStatusCode.BadRequest)]
		public async Task GetExecutionPlan_ReturnsExpectedStatus(string orderType, decimal amount, HttpStatusCode expectedStatus)
		{
			// Arrange
			string url = $"/api/execution/plan?orderType={orderType}&amount={amount}";

			if (expectedStatus == HttpStatusCode.OK)
			{
				var mockExchanges = new List<Exchange>
				{
					new Exchange { Id = "exchange-01", AvailableFunds = new AvailableFunds(), OrderBook = new OrderBook() },
					new Exchange { Id = "exchange-02", AvailableFunds = new AvailableFunds(), OrderBook = new OrderBook() }
				};

				var mockExecutionPlan = new ExecutionPlan
				{
					Orders = new List<ExecutionOrder>
					{
						new ExecutionOrder { ExchangeId = "exchange-01", OrderId = "order-1", Type = "Buy", Amount = 5m, Price = 50000m },
						new ExecutionOrder { ExchangeId = "exchange-02", OrderId = "order-2", Type = "Buy", Amount = 2m, Price = 51000m }
					},
					TotalCostOrRevenue = 5m * 50000m + 2m * 51000m
				};

				// Setup mocks
				_factory.MockExchangeRepository
						.Setup(repo => repo.GetAllExchangesAsync(It.IsAny<string>()))
						.ReturnsAsync(mockExchanges);

				_factory.MockExecutionService
						.Setup(service => service.GetBestExecutionPlanAsync(mockExchanges, orderType, amount))
						.ReturnsAsync(mockExecutionPlan);
			}

			// Act
			var response = await _client.GetAsync(url);

			// Assert
			Assert.Equal(expectedStatus, response.StatusCode);
		}

		[Fact]
		public async Task GetExecutionPlan_InvalidAmountFormat_ReturnsBadRequest()
		{
			// Arrange
			string orderType = "Buy";
			string amount = "abc"; // Non-numeric amount
			string url = $"/api/execution/plan?orderType={orderType}&amount={amount}";

			// Act
			var response = await _client.GetAsync(url);

			// Assert
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		}

		[Fact]
		public async Task GetExecutionPlan_UnsupportedHttpMethod_ReturnsMethodNotAllowed()
		{
			// Arrange
			string url = "/api/execution/plan?orderType=Buy&amount=7";

			// Act
			var response = await _client.PostAsync(url, null); // Using POST instead of GET

			// Assert
			Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
		}

		[Fact]
		public async Task GetExecutionPlan_ResponseContainsRequiredFields()
		{
			// Arrange
			string orderType = "Buy";
			decimal amount = 7m;
			string url = $"/api/execution/plan?orderType={orderType}&amount={amount}";

			// Mock data
			var mockExchanges = new List<Exchange>
			{
				new Exchange { Id = "exchange-01", AvailableFunds = new AvailableFunds { Euro = 100000m, Crypto = 50m }, OrderBook = new OrderBook() },
				new Exchange { Id = "exchange-02", AvailableFunds = new AvailableFunds { Euro = 150000m, Crypto = 75m }, OrderBook = new OrderBook() }
			};

			var mockExecutionPlan = new ExecutionPlan
			{
				Orders = new List<ExecutionOrder>
				{
					new ExecutionOrder { ExchangeId = "exchange-01", OrderId = "order-1", Type = "Buy", Amount = 5m, Price = 50000m },
					new ExecutionOrder { ExchangeId = "exchange-02", OrderId = "order-2", Type = "Buy", Amount = 2m, Price = 51000m }
				},
				TotalCostOrRevenue = 5m * 50000m + 2m * 51000m
			};

			// Setup mocks
			_factory.MockExchangeRepository
					.Setup(repo => repo.GetAllExchangesAsync(It.IsAny<string>()))
					.ReturnsAsync(mockExchanges);

			_factory.MockExecutionService
					.Setup(service => service.GetBestExecutionPlanAsync(mockExchanges, orderType, amount))
					.ReturnsAsync(mockExecutionPlan);

			// Act
			var response = await _client.GetAsync(url);

			// Assert
			response.EnsureSuccessStatusCode();

			var content = await response.Content.ReadAsStringAsync();
			var executionPlan = JsonSerializer.Deserialize<ExecutionPlan>(content, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});

			Assert.NotNull(executionPlan);
			Assert.NotEmpty(executionPlan.Orders);
			Assert.Equal(amount, executionPlan.Orders.Sum(o => o.Amount));
			Assert.True(executionPlan.TotalCostOrRevenue > 0);
		}

		[Fact]
		public async Task GetExecutionPlan_ConcurrentRequests_ReturnsOk()
		{
			// Arrange
			var urls = new[]
			{
				"/api/execution/plan?orderType=Buy&amount=10",
				"/api/execution/plan?orderType=Sell&amount=5",
				"/api/execution/plan?orderType=Buy&amount=20",
				"/api/execution/plan?orderType=Sell&amount=15"
			};

			var mockExchanges = new List<Exchange>
			{
				new Exchange { Id = "exchange-01", AvailableFunds = new AvailableFunds { Euro = 200000m, Crypto = 100m }, OrderBook = new OrderBook() },
				new Exchange { Id = "exchange-02", AvailableFunds = new AvailableFunds { Euro = 250000m, Crypto = 150m }, OrderBook = new OrderBook() }
			};

			var mockExecutionPlan = new ExecutionPlan
			{
				Orders = new List<ExecutionOrder>
				{
					new ExecutionOrder { ExchangeId = "exchange-01", OrderId = "order-1", Type = "Buy", Amount = 5m, Price = 50000m },
					new ExecutionOrder { ExchangeId = "exchange-02", OrderId = "order-2", Type = "Sell", Amount = 5m, Price = 51000m }
				},
				TotalCostOrRevenue = 5m * 50000m + 5m * 51000m
			};

			// Setup mocks
			_factory.MockExchangeRepository
					.Setup(repo => repo.GetAllExchangesAsync(It.IsAny<string>()))
					.ReturnsAsync(mockExchanges);

			_factory.MockExecutionService
					.Setup(service => service.GetBestExecutionPlanAsync(mockExchanges, It.IsAny<string>(), It.IsAny<decimal>()))
					.ReturnsAsync(mockExecutionPlan);

			// Act
			var tasks = urls.Select(url => _client.GetAsync(url));
			var responses = await Task.WhenAll(tasks);

			// Assert
			foreach (var response in responses)
			{
				Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			}
		}
	}
}
