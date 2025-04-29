using System.Collections.Generic;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class WaitlistStats
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("price")]
        public PriceStats Price { get; set; } = new PriceStats();

        [JsonProperty("cut")]
        public CutStats Cut { get; set; } = new CutStats();
    }

    public class PriceStats
    {
        [JsonProperty("currency")]
        public string Currency { get; set; } = "";

        [JsonProperty("any")]
        public int Any { get; set; } // Count of users with no price limit

        [JsonProperty("average")]
        public double Average { get; set; }

        [JsonProperty("buckets")]
        public List<StatBucket> Buckets { get; set; } = new List<StatBucket>();
    }

    public class CutStats
    {
        [JsonProperty("average")]
        public double Average { get; set; }

        [JsonProperty("buckets")]
        public List<StatBucket> Buckets { get; set; } = new List<StatBucket>();
    }

    public class StatBucket
    {
        [JsonProperty("bucket")]
        public double Bucket { get; set; } // The threshold value (e.g., price or cut)

        [JsonProperty("count")]
        public int Count { get; set; } // Number of users in this bucket

        [JsonProperty("percentile")]
        public double Percentile { get; set; }
    }
}