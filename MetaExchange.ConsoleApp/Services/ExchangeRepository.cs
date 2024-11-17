using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MetaExchange.ConsoleApp.Models;

namespace MetaExchange.ConsoleApp.Services
{
    public class ExchangeRepository : IExchangeRepository
    {
        public async Task<List<Exchange>> GetAllExchangesAsync(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

            var exchangeFiles = Directory.GetFiles(directoryPath, "*.json");
            var exchanges = new List<Exchange>();

            foreach (var file in exchangeFiles)
            {
                var jsonContent = await File.ReadAllTextAsync(file);
                var exchange = JsonSerializer.Deserialize<Exchange>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (exchange != null)
                    exchanges.Add(exchange);
            }

            return exchanges;
        }
    }
}
