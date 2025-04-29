using System;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class Shop
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Name { get; set; } = "";

        // Properties specific to resp.service.shops item
        [JsonProperty("deals", NullValueHandling = NullValueHandling.Ignore)]
        public int? Deals { get; set; }

        [JsonProperty("games", NullValueHandling = NullValueHandling.Ignore)]
        public int? Games { get; set; }

        [JsonProperty("update", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Update { get; set; }
    }
}