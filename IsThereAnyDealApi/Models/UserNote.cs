using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class UserNote
    {
        [JsonProperty("gid")]
        public string GameId { get; set; } = ""; // Game ID

        [JsonProperty("note")]
        public string Note { get; set; } = "";
    }
}