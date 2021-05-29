using Newtonsoft.Json;

namespace FiveRP.Gamemode.Features.Inventories
{
    public class ItemsJson
    {
        [JsonProperty("hash")]
        public string ItemHash { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }
    }
}