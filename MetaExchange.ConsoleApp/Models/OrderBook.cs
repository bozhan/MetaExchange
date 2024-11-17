using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MetaExchange.ConsoleApp.Models
{
    public class OrderBook
    {
        [JsonPropertyName("Bids")]
        public List<OrderWrapper> Bids { get; set; }

        [JsonPropertyName("Asks")]
        public List<OrderWrapper> Asks { get; set; }
    }
}
