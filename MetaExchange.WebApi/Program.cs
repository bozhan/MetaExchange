using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MetaExchange.ConsoleApp.Services;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System;

namespace MetaExchange.WebApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			// Create the builder
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllers();

			// Add services from the console app
			builder.Services.AddSingleton<IExchangeRepository, ExchangeRepository>();
			builder.Services.AddSingleton<IExecutionService, ExecutionService>();

			// Configure logging
			builder.Services.AddLogging();

			// Add Swagger services
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			// Bind configuration
			builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

			// Build the app
			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI();
			}
			else
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			// Global Exception Handling
			app.UseExceptionHandler(a => a.Run(async context =>
			{
				var feature = context.Features.Get<IExceptionHandlerPathFeature>();
				var exception = feature?.Error;

				var result = JsonSerializer.Serialize(new { error = "An unexpected error occurred." });
				context.Response.ContentType = "application/json";
				context.Response.StatusCode = 500;
				await context.Response.WriteAsync(result);
			}));

			app.UseRouting();

			app.UseAuthorization();

			app.MapControllers();

			app.Run();
		}
	}
}

// Expose the Program class for testing
public partial class Program { }
