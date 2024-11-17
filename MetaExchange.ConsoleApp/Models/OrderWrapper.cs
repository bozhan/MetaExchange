using System.Text.Json.Serialization;

namespace MetaExchange.ConsoleApp.Models
{
    public class OrderWrapper
    {
        [JsonPropertyName("Order")]
        public Order Order { get; set; }
    }
}
