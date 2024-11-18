using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MetaExchange.ConsoleApp.Services;
using Microsoft.Extensions.Configuration;
using System.IO;

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
	// In production, you might want to serve Swagger UI at a different endpoint or disable it
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();