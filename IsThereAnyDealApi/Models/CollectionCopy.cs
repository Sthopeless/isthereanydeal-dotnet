using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class CollectionCopy
    {
        [JsonProperty("id")]
        public int Id { get; set; } // Copy ID

        [JsonProperty("game")]
        public CopyGameReference Game { get; set; } = new CopyGameReference();

        [JsonProperty("shop")]
        public Shop? Shop { get; set; }

        [JsonProperty("redeemed")]
        public bool Redeemed { get; set; }

        [JsonProperty("price")]
        public Price? Price { get; set; }

        [JsonProperty("note")]
        public string? Note { get; set; }

        [JsonProperty("tags")]
        public List<UserTag> Tags { get; set; } = new List<UserTag>();

        [JsonProperty("added")]
        public DateTimeOffset Added { get; set; }
    }

    // Nested object within obj.collection.copy
    public class CopyGameReference
    {
         [JsonProperty("id")]
         public string Id { get; set; } = ""; // Game ID
    }

    public class UserTag
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; } = "";
    }

    public class NewCollectionCopy
    {
        [JsonProperty("gameId")]
        public string GameId { get; set; } = "";

        [JsonProperty("redeemed")]
        public bool Redeemed { get; set; }

        [JsonProperty("shop")]
        public int? ShopId { get; set; } // Shop ID

        [JsonProperty("price")]
        public NewPrice? Price { get; set; }

        [JsonProperty("note")]
        public string? Note { get; set; }

        [JsonProperty("tags")]
        public List<string>? Tags { get; set; } // List of tag strings
    }

    public class UpdateCollectionCopy
    {
        [JsonProperty("id")]
        public int Id { get; set; } // Required: ID of the copy to update

        [JsonProperty("redeemed", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Redeemed { get; set; }

        [JsonProperty("shop", NullValueHandling = NullValueHandling.Ignore)]
        public int? ShopId { get; set; } // Use null to remove shop? Check API behavior.

        [JsonProperty("price", NullValueHandling = NullValueHandling.Ignore)]
        public NewPrice? Price { get; set; } // Use null to remove price? Check API behavior.

        [JsonProperty("note", NullValueHandling = NullValueHandling.Ignore)]
        public string? Note { get; set; } // Use null to remove note? Check API behavior.

        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public List<string>? Tags { get; set; } // Use null/empty list to remove tags? Check API behavior.
    }

     // Nested object for price in NewCollectionCopy / UpdateCollectionCopy
    public class NewPrice
    {
        [JsonProperty("amount")]
        public double Amount { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; } = "";
    }
}