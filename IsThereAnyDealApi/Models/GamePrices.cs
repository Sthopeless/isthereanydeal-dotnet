using System.Collections.Generic;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class GamePrices
    {
        [JsonProperty("id")]
        public string Id { get; set; } = ""; // Game ID (UUID)

        [JsonProperty("historyLow")]
        public PriceHistoryLowSummary HistoryLow { get; set; } = new PriceHistoryLowSummary();

        [JsonProperty("deals")]
        public List<Deal> Deals { get; set; } = new List<Deal>(); // Uses the full Deal model from obj.deal
    }

    public class PriceHistoryLowSummary
    {
        [JsonProperty("all")]
        public Price? All { get; set; }

        [JsonProperty("y1")]
        public Price? Y1 { get; set; } // Last 1 year

        [JsonProperty("m3")]
        public Price? M3 { get; set; } // Last 3 months
    }
}