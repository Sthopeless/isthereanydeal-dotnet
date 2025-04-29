using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class UserInfo
    {
        [JsonProperty("username")]
        public string? Username { get; set; }
    }
}