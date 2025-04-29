using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class Profile
    {
        [JsonProperty("accountId")]
        public string AccountId { get; set; } = "";

        [JsonProperty("accountName")]
        public string AccountName { get; set; } = "";
    }

    public class LinkProfileResponse
    {
        [JsonProperty("token")]
        public string Token { get; set; } = "";
    }
}