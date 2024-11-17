using System.Collections.Generic;
using System.Threading.Tasks;
using MetaExchange.ConsoleApp.Models;

namespace MetaExchange.ConsoleApp.Services
{
    public interface IExecutionService
    {
        Task<ExecutionPlan> GetBestExecutionPlanAsync(List<Exchange> exchanges, string orderType, decimal amount);
    }
}
