using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using MetaExchange.ConsoleApp.Services;
using MetaExchange.WebApi.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace MetaExchange.WebApi.Tests
{
	public class CustomWebApplicationFactory : WebApplicationFactory<Program>
	{
		public Mock<IExecutionService> MockExecutionService { get; } = new Mock<IExecutionService>();
		public Mock<IExchangeRepository> MockExchangeRepository { get; } = new Mock<IExchangeRepository>();
		public Mock<ILogger<ExecutionController>> MockLogger { get; } = new Mock<ILogger<ExecutionController>>();

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.ConfigureServices(services =>
			{
				// Remove existing service registrations
				services.RemoveAll<IExecutionService>();
				services.RemoveAll<IExchangeRepository>();
				services.RemoveAll<ILogger<ExecutionController>>();

				// Register mocked services
				services.AddSingleton(MockExecutionService.Object);
				services.AddSingleton(MockExchangeRepository.Object);
				services.AddSingleton(MockLogger.Object);
			});
		}
	}
}