using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class CollectionGroup
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; } = "";

        [JsonProperty("public")]
        public bool Public { get; set; }
    }

    public class NewCollectionGroup
    {
        [JsonProperty("title")]
        public string Title { get; set; } = "";

        [JsonProperty("public")]
        public bool Public { get; set; }
    }

    public class UpdateCollectionGroup
    {
        [JsonProperty("id")]
        public int Id { get; set; } // Required: ID of the group to update

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string? Title { get; set; }

        [JsonProperty("public", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Public { get; set; }

        // Note: Position is nullable in the schema, allowing it to be potentially omitted
        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public int? Position { get; set; }
    }
}