using Newtonsoft.Json;

namespace FiveRP.Gamemode.Features.Weapons
{
    public class WeaponsJson
    {
        [JsonProperty("hash")]
        public string WeaponHash { get; set; }

        [JsonProperty("ammo")]
        public int Ammo { get; set; }

        [JsonProperty("flags")]
        public string Flags { get; set; }
    }
}