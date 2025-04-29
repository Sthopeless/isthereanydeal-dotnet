using System.Collections.Generic;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class DealsListResult
    {
        [JsonProperty("nextOffset")]
        public int NextOffset { get; set; }

        [JsonProperty("hasMore")]
        public bool HasMore { get; set; }

        [JsonProperty("list")]
        public List<DealListItem> List { get; set; } = new List<DealListItem>();
    }

    public class DealListItem
    {
        [JsonProperty("id")]
        public string Id { get; set; } = ""; // Game ID

        [JsonProperty("slug")]
        public string Slug { get; set; } = "";

        [JsonProperty("title")]
        public string Title { get; set; } = "";

        [JsonProperty("type")]
        public string? Type { get; set; } // "game", "dlc", etc. or null

        [JsonProperty("mature")]
        public bool Mature { get; set; }

        [JsonProperty("assets")]
        public AssetMap Assets { get; set; } = new AssetMap();

        [JsonProperty("deal")]
        public Deal Deal { get; set; } = new Deal(); // Uses the full Deal model
    }
}