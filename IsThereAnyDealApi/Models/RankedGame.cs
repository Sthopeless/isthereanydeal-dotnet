using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class RankedGame
    {
        [JsonProperty("position")]
        public int Position { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; } = ""; // Game ID

        [JsonProperty("slug")]
        public string Slug { get; set; } = "";

        [JsonProperty("title")]
        public string Title { get; set; } = "";

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("mature")]
        public bool Mature { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; } // e.g., Waitlist count or Collection count
    }
}