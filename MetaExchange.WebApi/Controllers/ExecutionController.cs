using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MetaExchange.ConsoleApp.Services;
using MetaExchange.ConsoleApp.Models;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.Configuration;
using System;

namespace MetaExchange.WebApi.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ExecutionController : ControllerBase
	{
		private readonly IExecutionService _executionService;
		private readonly IExchangeRepository _exchangeRepository;
		private readonly ILogger<ExecutionController> _logger;
		private readonly string _exchangesDirectory;

		public ExecutionController(
			IExecutionService executionService,
			IExchangeRepository exchangeRepository,
			ILogger<ExecutionController> logger,
			IConfiguration configuration)
		{
			_executionService = executionService;
			_exchangeRepository = exchangeRepository;
			_logger = logger;

			// Get the exchanges directory from configuration
			_exchangesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuration["ExchangesDirectory"] ?? "Exchanges");
		}

		/// <summary>
		/// Gets the best execution plan for a given order type and amount.
		/// </summary>
		/// <param name="orderType">The type of order: Buy or Sell.</param>
		/// <param name="amount">The amount of BTC to transact.</param>
		/// <returns>An execution plan with the best possible orders.</returns>
		[HttpGet("plan")]
		public async Task<IActionResult> GetExecutionPlan([FromQuery] string orderType, [FromQuery] decimal amount)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(orderType) || amount <= 0)
				{
					return BadRequest("Invalid order type or amount.");
				}

				if (!Directory.Exists(_exchangesDirectory))
				{
					_logger.LogError($"Exchanges directory not found at path: {_exchangesDirectory}");
					return StatusCode(500, "Exchanges directory not found.");
				}

				var exchanges = await _exchangeRepository.GetAllExchangesAsync(_exchangesDirectory);
				var executionPlan = await _executionService.GetBestExecutionPlanAsync(exchanges, orderType, amount);

				return Ok(executionPlan);
			}
			catch (Exception ex)
			{
				_logger.LogError($"An error occurred: {ex.Message}");
				_logger.LogError($"Stack Trace: {ex.StackTrace}");
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}
	}
}
