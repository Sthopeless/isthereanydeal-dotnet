using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class AssetMap
    {
        [JsonProperty("banner145")]
        public string? Banner145 { get; set; }

        [JsonProperty("banner300")]
        public string? Banner300 { get; set; }

        [JsonProperty("banner400")]
        public string? Banner400 { get; set; }

        [JsonProperty("banner600")]
        public string? Banner600 { get; set; }

        [JsonProperty("boxart")]
        public string? Boxart { get; set; }
    }
}