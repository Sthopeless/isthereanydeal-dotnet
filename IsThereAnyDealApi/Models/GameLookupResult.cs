using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class GameLookupResult
    {
        [JsonProperty("found")]
        public bool Found { get; set; }

        [JsonProperty("game")]
        public Game? Game { get; set; } // Nullable if not found
    }
}