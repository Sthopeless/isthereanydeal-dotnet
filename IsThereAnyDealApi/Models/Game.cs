using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class Game
    {
        [JsonProperty("id")]
        public string Id { get; set; } = ""; // UUID format

        [JsonProperty("slug")]
        public string Slug { get; set; } = "";

        [JsonProperty("title")]
        public string Title { get; set; } = "";

        [JsonProperty("type")]
        public string? Type { get; set; } // "game", "dlc", "package", or null

        [JsonProperty("mature")]
        public bool Mature { get; set; }

        [JsonProperty("assets")]
        public AssetMap Assets { get; set; } = new AssetMap();
    }
}