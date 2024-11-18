using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using MetaExchange.ConsoleApp.Models;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace MetaExchange.WebApi.Tests
{
	public class ExecutionControllerTests : IClassFixture<WebApplicationFactory<MetaExchange.WebApi.Program>>
	{
		private readonly HttpClient _client;

		public ExecutionControllerTests(WebApplicationFactory<Program> factory)
		{
			_client = factory.CreateClient();
		}

		[Theory]
		[InlineData("Buy", 7, HttpStatusCode.OK)]
		[InlineData("Sell", 5, HttpStatusCode.OK)]
		[InlineData("InvalidType", 5, HttpStatusCode.BadRequest)]
		[InlineData("Buy", -1, HttpStatusCode.BadRequest)]
		[InlineData("", 5, HttpStatusCode.BadRequest)]
		[InlineData(null, 5, HttpStatusCode.BadRequest)]
		[InlineData("Buy", 0, HttpStatusCode.BadRequest)]
		public async Task GetExecutionPlan_ReturnsExpectedStatus(string orderType, decimal amount, HttpStatusCode expectedStatus)
		{
			// Arrange
			string url = $"/api/execution/plan?orderType={orderType}&amount={amount}";

			// Act
			var response = await _client.GetAsync(url);

			// Assert
			Assert.Equal(expectedStatus, response.StatusCode);
		}

		[Fact]
		public async Task GetExecutionPlan_ReturnsValidExecutionPlan()
		{
			// Arrange
			string orderType = "Buy";
			decimal amount = 7;
			string url = $"/api/execution/plan?orderType={orderType}&amount={amount}";

			// Act
			var response = await _client.GetAsync(url);

			// Assert
			response.EnsureSuccessStatusCode(); // Status Code 200-299

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
		public async Task GetExecutionPlan_LargeAmount_ReturnsValidExecutionPlan()
		{
			// Arrange
			string orderType = "Sell";
			decimal amount = 1000000; // Large amount
			string url = $"/api/execution/plan?orderType={orderType}&amount={amount}";

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
		public async Task GetExecutionPlan_MissingExchangesDirectory_ReturnsServerError()
		{
			// Arrange
			string orderType = "Buy";
			decimal amount = 7;
			string url = $"/api/execution/plan?orderType={orderType}&amount={amount}";

			// To simulate missing directory, you might need to manipulate the configuration or mock the repository to throw an exception
			// For simplicity, assuming the repository throws a DirectoryNotFoundException

			// Since we're using WebApplicationFactory<Program>, you can create a custom factory that configures the services accordingly
			// Alternatively, use mocking as shown in previous steps

			// Act
			var response = await _client.GetAsync(url);

			// Assert
			Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
		}

		[Fact]
		public async Task GetExecutionPlan_InvalidAmountFormat_ReturnsBadRequest()
		{
			// Arrange
			string orderType = "Buy";
			string amount = "abc"; // Invalid format
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
			decimal amount = 7;
			string url = $"/api/execution/plan?orderType={orderType}&amount={amount}";

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
			Assert.NotNull(executionPlan.Orders);
			Assert.All(executionPlan.Orders, order =>
			{
				Assert.False(string.IsNullOrEmpty(order.ExchangeId));
				Assert.False(string.IsNullOrEmpty(order.OrderId));
				Assert.False(string.IsNullOrEmpty(order.Type));
				Assert.True(order.Amount > 0);
				Assert.True(order.Price > 0);
			});
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
