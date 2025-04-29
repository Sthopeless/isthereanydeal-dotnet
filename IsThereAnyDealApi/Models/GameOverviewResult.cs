using System.Collections.Generic;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class GameOverviewResult
    {
        [JsonProperty("prices")]
        public List<GameOverviewPrice> Prices { get; set; } = new List<GameOverviewPrice>();

        [JsonProperty("bundles")]
        public List<Bundle> Bundles { get; set; } = new List<Bundle>();
    }

    public class GameOverviewPrice
    {
        [JsonProperty("id")]
        public string Id { get; set; } = ""; // Game ID

        [JsonProperty("current")]
        public Deal? Current { get; set; } // Uses Deal model, but note some fields might be null compared to full Deal

        [JsonProperty("lowest")]
        public HistoricalLow? Lowest { get; set; } // Uses HistoricalLow model

        [JsonProperty("bundled")]
        public int Bundled { get; set; } // Count

        [JsonProperty("urls")]
        public GameUrls Urls { get; set; } = new GameUrls();
    }
}