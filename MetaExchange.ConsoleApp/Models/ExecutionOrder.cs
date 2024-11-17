namespace MetaExchange.ConsoleApp.Models
{
    public class ExecutionOrder
    {
        public string ExchangeId { get; set; }
        public string OrderId { get; set; }
        public string Type { get; set; } // "Buy" or "Sell"
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
    }
}
