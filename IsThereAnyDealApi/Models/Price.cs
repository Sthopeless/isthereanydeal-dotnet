using Newtonsoft.Json;

namespace IsThereAnyDeal.Api.Models
{
    public class Price
    {
        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("amountInt")]
        public int AmountInt { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; } = ""; // e.g., "USD", "EUR"
    }
}