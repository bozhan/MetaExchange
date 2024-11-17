using System.Text.Json.Serialization;

namespace MetaExchange.ConsoleApp.Models
{
    public class AvailableFunds
    {
        [JsonPropertyName("Crypto")]
        public double Crypto { get; set; }

        [JsonPropertyName("Euro")]
        public double Euro { get; set; }
    }
}
