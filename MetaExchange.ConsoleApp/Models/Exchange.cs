using System.Text.Json.Serialization;

namespace MetaExchange.ConsoleApp.Models
{
    public class Exchange
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonPropertyName("AvailableFunds")]
        public AvailableFunds AvailableFunds { get; set; }

        [JsonPropertyName("OrderBook")]
        public OrderBook OrderBook { get; set; }
    }
}
