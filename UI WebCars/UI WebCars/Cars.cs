using System.Text.Json.Serialization;

namespace UI_WebCars
{
    public class Cars
    {
        [JsonPropertyName("id")]
        public int id { get; set; }

        [JsonPropertyName("firm")]
        public string Firm { get; set; }
        [JsonPropertyName("model")]
        public string Model { get; set; }
        [JsonPropertyName("year")]
        public int Year { get; set; }
        [JsonPropertyName("power")]
        public int Power { get; set; }
        [JsonPropertyName("color")]
        public string Color { get; set; }
        [JsonPropertyName("price")]
        public int Price { get; set; }
    }
}
