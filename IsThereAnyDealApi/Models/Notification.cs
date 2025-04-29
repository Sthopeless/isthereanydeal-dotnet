using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class Notification
    {
        [JsonProperty("id")]
        public string Id { get; set; } = ""; // Notification ID (UUID)

        [JsonProperty("type")]
        public string Type { get; set; } = ""; // e.g., "waitlist"

        [JsonProperty("title")]
        public string Title { get; set; } = "";

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("read")]
        public DateTimeOffset? Read { get; set; }
    }

    public class WaitlistNotificationDetail
    {
        [JsonProperty("id")]
        public string Id { get; set; } = ""; // Notification ID (UUID)

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("read")]
        public DateTimeOffset? Read { get; set; }

        [JsonProperty("games")]
        public List<WaitlistNotificationGame> Games { get; set; } = new List<WaitlistNotificationGame>();
    }

    public class WaitlistNotificationGame
    {
        [JsonProperty("id")]
        public string Id { get; set; } = ""; // Game ID

        [JsonProperty("slug")]
        public string Slug { get; set; } = "";

        [JsonProperty("title")]
        public string Title { get; set; } = "";

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("mature")]
        public bool Mature { get; set; }

        [JsonProperty("historyLow")]
        public Price? HistoryLow { get; set; } // Nullable price

        [JsonProperty("lastPrice")]
        public Price? LastPrice { get; set; } // Nullable price

        [JsonProperty("deals")]
        public List<NotificationDeal> Deals { get; set; } = new List<NotificationDeal>();
    }

    public class NotificationDeal
    {
        [JsonProperty("shop")]
        public Shop Shop { get; set; } = new Shop();

        [JsonProperty("price")]
        public Price Price { get; set; } = new Price();

        [JsonProperty("regular")]
        public Price Regular { get; set; } = new Price();

        [JsonProperty("cut")]
        public int Cut { get; set; }

        [JsonProperty("voucher")]
        public string? Voucher { get; set; }

        [JsonProperty("storeLow")]
        public Price? StoreLow { get; set; } // Difference from main Deal model

        [JsonProperty("flag")]
        public string? Flag { get; set; }

        [JsonProperty("drm")]
        public List<Drm> Drm { get; set; } = new List<Drm>();

        [JsonProperty("platforms")]
        public List<Platform> Platforms { get; set; } = new List<Platform>();

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("expiry")]
        public DateTimeOffset? Expiry { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; } = "";
    }
}