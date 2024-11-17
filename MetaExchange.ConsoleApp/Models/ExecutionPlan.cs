using System.Collections.Generic;

namespace MetaExchange.ConsoleApp.Models
{
    public class ExecutionPlan
    {
        public List<ExecutionOrder> Orders { get; set; } = new List<ExecutionOrder>();
        public decimal TotalCostOrRevenue { get; set; }
    }
}
