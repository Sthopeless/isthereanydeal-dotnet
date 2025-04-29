using System;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class WaitlistGame
    {
        [JsonProperty("id")]
        public string Id { get; set; } = ""; // Game ID

        [JsonProperty("slug")]
        public string Slug { get; set; } = "";

        [JsonProperty("title")]
        public string Title { get; set; } = "";

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("assets")]
        public AssetMap Assets { get; set; } = new AssetMap();

        [JsonProperty("mature")]
        public bool Mature { get; set; }

        [JsonProperty("added")]
        public DateTimeOffset? Added { get; set; }
    }
}