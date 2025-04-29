using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class GameSubscriptions
    {
        [JsonProperty("id")]
        public string Id { get; set; } = ""; // Game ID

        [JsonProperty("subs")]
        public List<SubscriptionInfo> Subs { get; set; } = new List<SubscriptionInfo>();
    }

    public class SubscriptionInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; } // Subscription ID

        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("leaving")]
        public DateTimeOffset? Leaving { get; set; }
    }
}