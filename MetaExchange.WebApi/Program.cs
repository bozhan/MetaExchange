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

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRouting();

app.UseEndpoints(endpoints =>
{
	endpoints.MapControllers();
});

app.Run();