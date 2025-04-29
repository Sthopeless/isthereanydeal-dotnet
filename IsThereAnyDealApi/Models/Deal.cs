using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class Deal
    {
        [JsonProperty("shop")]
        public Shop Shop { get; set; } = new Shop();

        [JsonProperty("price")]
        public Price Price { get; set; } = new Price();

        [JsonProperty("regular")]
        public Price Regular { get; set; } = new Price();

        [JsonProperty("cut")]
        public int Cut { get; set; } // Percentage

        [JsonProperty("voucher")]
        public string? Voucher { get; set; }

        [JsonProperty("storeLow")]
        public Price? StoreLow { get; set; }

        // --- Properties specific to obj.deal (full deals list) ---
        [JsonProperty("historyLow", NullValueHandling = NullValueHandling.Ignore)]
        public Price? HistoryLow { get; set; }

        [JsonProperty("historyLow_1y", NullValueHandling = NullValueHandling.Ignore)]
        public Price? HistoryLow1Y { get; set; }

        [JsonProperty("historyLow_3m", NullValueHandling = NullValueHandling.Ignore)]
        public Price? HistoryLow3M { get; set; }
        // --- End specific properties ---

        [JsonProperty("flag")]
        public string? Flag { get; set; } // e.g., "H", "N", "S" or null

        [JsonProperty("drm")]
        public List<Drm> Drm { get; set; } = new List<Drm>();

        [JsonProperty("platforms")]
        public List<Platform> Platforms { get; set; } = new List<Platform>();

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("expiry")]
        public DateTimeOffset? Expiry { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; } = "";
    }

    public class Drm
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; } = "";
    }

    public class Platform
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; } = "";
    }
}