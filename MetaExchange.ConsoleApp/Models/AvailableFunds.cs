using System.Text.Json.Serialization;

namespace MetaExchange.ConsoleApp.Models
{
    public class AvailableFunds
    {
        [JsonPropertyName("Crypto")]
        public decimal Crypto { get; set; }

        [JsonPropertyName("Euro")]
        public decimal Euro { get; set; }
    }
}
