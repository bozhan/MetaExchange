using System.Collections.Generic;
using System.Threading.Tasks;
using MetaExchange.ConsoleApp.Models;

namespace MetaExchange.ConsoleApp.Services
{
    public interface IExchangeRepository
    {
        Task<List<Exchange>> GetAllExchangesAsync(string directoryPath);
    }
}
