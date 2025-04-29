using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class GameStoreLow
    {
        [JsonProperty("id")]
        public string Id { get; set; } = ""; // Game ID

        [JsonProperty("lows")]
        public List<StoreLowRecord> Lows { get; set; } = new List<StoreLowRecord>();
    }

    public class StoreLowRecord
    {
        [JsonProperty("shop")]
        public Shop Shop { get; set; } = new Shop();

        [JsonProperty("price")]
        public Price Price { get; set; } = new Price();

        [JsonProperty("regular")]
        public Price Regular { get; set; } = new Price();

        [JsonProperty("cut")]
        public int Cut { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
    }
}