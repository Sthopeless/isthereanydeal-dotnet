using System;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class HistoricalLow
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

    public class GameHistoricalLow
    {
        [JsonProperty("id")]
        public string Id { get; set; } = ""; // Game ID

        [JsonProperty("low")]
        public HistoricalLow Low { get; set; } = new HistoricalLow();
    }
}