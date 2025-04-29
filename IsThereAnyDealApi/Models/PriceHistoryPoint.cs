using System;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class PriceHistoryPoint
    {
        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("shop")]
        public Shop Shop { get; set; } = new Shop();

        [JsonProperty("deal")]
        public HistoryDealInfo? Deal { get; set; } // Nullable if price info wasn't available at that time
    }

    public class HistoryDealInfo
    {
        [JsonProperty("price")]
        public Price Price { get; set; } = new Price();

        [JsonProperty("regular")]
        public Price Regular { get; set; } = new Price();

        [JsonProperty("cut")]
        public double Cut { get; set; } // Note: Schema says number, example shows integer. Double is safer.
    }
}