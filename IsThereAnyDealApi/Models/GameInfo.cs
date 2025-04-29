using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class GameInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "";

        [JsonProperty("slug")]
        public string Slug { get; set; } = "";

        [JsonProperty("title")]
        public string Title { get; set; } = "";

        [JsonProperty("type")]
        public string? Type { get; set; } // "game", "dlc", "package", or null

        [JsonProperty("mature")]
        public bool Mature { get; set; }

        [JsonProperty("assets")]
        public AssetMap Assets { get; set; } = new AssetMap();

        [JsonProperty("earlyAccess")]
        public bool EarlyAccess { get; set; }

        [JsonProperty("achievements")]
        public bool Achievements { get; set; }

        [JsonProperty("tradingCards")]
        public bool TradingCards { get; set; }

        [JsonProperty("appid")]
        public int? Appid { get; set; } // Nullable int for Steam AppID

        [JsonProperty("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        [JsonProperty("releaseDate")]
        public string? ReleaseDate { get; set; }

        [JsonProperty("developers")]
        public List<Company> Developers { get; set; } = new List<Company>();

        [JsonProperty("publishers")]
        public List<Company> Publishers { get; set; } = new List<Company>();

        [JsonProperty("reviews")]
        public List<ReviewInfo> Reviews { get; set; } = new List<ReviewInfo>();

        [JsonProperty("stats")]
        public GameStats Stats { get; set; } = new GameStats();

        [JsonProperty("players")]
        public PlayerStats? Players { get; set; } // Nullable object

        [JsonProperty("urls")]
        public GameUrls Urls { get; set; } = new GameUrls();
    }

    public class Company
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; } = "";
    }

    public class ReviewInfo
    {
        [JsonProperty("score")]
        public double? Score { get; set; }
        [JsonProperty("source")]
        public string Source { get; set; } = "";
        [JsonProperty("count")]
        public int? Count { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; } = "";
    }

    public class GameStats
    {
        [JsonProperty("rank")]
        public int? Rank { get; set; }
        [JsonProperty("waitlisted")]
        public int? Waitlisted { get; set; }
        [JsonProperty("collected")]
        public int? Collected { get; set; }
    }

    public class PlayerStats
    {
        [JsonProperty("recent")]
        public int Recent { get; set; }
        [JsonProperty("day")]
        public int Day { get; set; }
        [JsonProperty("week")]
        public int Week { get; set; }
        [JsonProperty("peak")]
        public int Peak { get; set; }
    }

    public class GameUrls
    {
        [JsonProperty("game")]
        public string Game { get; set; } = "";
    }
}