using System;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class WaitlistSyncEntry
    {
        [JsonProperty("shop")]
        public int ShopId { get; set; }

        [JsonProperty("id")]
        public string GameIdentifier { get; set; } = ""; // Shop-specific or other stable ID

        [JsonProperty("title")]
        public string Title { get; set; } = "";
    }

    public class CollectionSyncEntry
    {
        [JsonProperty("shop")]
        public int ShopId { get; set; }

        [JsonProperty("id")]
        public string GameIdentifier { get; set; } = ""; // Shop-specific or other stable ID

        [JsonProperty("title")]
        public string Title { get; set; } = "";

        [JsonProperty("playtime")]
        public double? Playtime { get; set; } // In minutes

        [JsonProperty("lastPlayed")]
        public DateTimeOffset? LastPlayed { get; set; }
    }

    public class SyncResult
    {
        [JsonProperty("total")]
        public int Total { get; set; } // Parsed items

        [JsonProperty("added")]
        public int Added { get; set; }

        [JsonProperty("removed")]
        public int Removed { get; set; }
    }
}