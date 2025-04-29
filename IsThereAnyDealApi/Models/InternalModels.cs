using System.Collections.Generic;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class InternalPlayersStats
    {
        [JsonProperty("current")]
        public double Current { get; set; }

        [JsonProperty("day")]
        public double Day { get; set; }

        [JsonProperty("peak")]
        public double Peak { get; set; }
    }

    public class InternalHltbOverview
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("main")]
        public double? Main { get; set; }

        [JsonProperty("extra")]
        public double? Extra { get; set; }

        [JsonProperty("complete")]
        public double? Complete { get; set; }
    }

    public class InternalReviewsOverview
    {
        [JsonProperty("metauser")]
        public InternalReviewSource? Metauser { get; set; }

        [JsonProperty("opencritic")]
        public InternalReviewSource? Opencritic { get; set; }
    }

    public class InternalReviewSource
    {
        [JsonProperty("score")]
        public int? Score { get; set; }

        [JsonProperty("verdict")]
        public string? Verdict { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; } = "";
    }

    public class InternalCurrencyRate
    {
        [JsonProperty("from")]
        public string From { get; set; } = "";

        [JsonProperty("to")]
        public string To { get; set; } = "";

        [JsonProperty("rate")]
        public double Rate { get; set; }
    }

    public class InternalTwitchStream
    {
        [JsonProperty("user_name")]
        public string UserName { get; set; } = "";

        [JsonProperty("title")]
        public string Title { get; set; } = "";

        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; set; } = "";

        [JsonProperty("viewer_count")]
        public int ViewerCount { get; set; }

        [JsonProperty("game")]
        public string Game { get; set; } = "";
    }
}