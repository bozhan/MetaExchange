using System;
using System.Text.Json.Serialization;

namespace MetaExchange.ConsoleApp.Models
{
    public class Order
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonPropertyName("Time")]
        public DateTime Time { get; set; }

        [JsonPropertyName("Type")]
        public string? Type { get; set; } // "Buy" or "Sell"

        [JsonPropertyName("Kind")]
        public string? Kind { get; set; } // "Limit"

        [JsonPropertyName("Amount")]
        public double Amount { get; set; }

        [JsonPropertyName("Price")]
        public double Price { get; set; }
    }
}
