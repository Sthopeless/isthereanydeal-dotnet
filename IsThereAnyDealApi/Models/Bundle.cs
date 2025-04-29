using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class Bundle
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; } = "";

        [JsonProperty("page")]
        public BundlePage Page { get; set; } = new BundlePage();

        [JsonProperty("url")]
        public string Url { get; set; } = "";

        [JsonProperty("details")]
        public string? Details { get; set; } // Optional URL

        [JsonProperty("isMature")]
        public bool IsMature { get; set; }

        [JsonProperty("publish")]
        public DateTimeOffset Publish { get; set; }

        [JsonProperty("expiry")]
        public DateTimeOffset? Expiry { get; set; }

        [JsonProperty("counts")]
        public BundleCounts Counts { get; set; } = new BundleCounts();

        [JsonProperty("tiers")]
        public List<BundleTier> Tiers { get; set; } = new List<BundleTier>();
    }

    public class BundlePage
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("shopId")]
        public int? ShopId { get; set; }
    }

    public class BundleCounts
    {
        [JsonProperty("games")]
        public int Games { get; set; }

        [JsonProperty("media")]
        public int Media { get; set; }
    }

    public class BundleTier
    {
        [JsonProperty("price")]
        public Price? Price { get; set; }

        [JsonProperty("games")]
        public List<Game> Games { get; set; } = new List<Game>(); // Uses the basic Game model
    }
}