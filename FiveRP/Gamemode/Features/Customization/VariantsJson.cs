using Newtonsoft.Json;

namespace FiveRP.Gamemode.Features.Customization
{
    public class VariantsJson
    {
        [JsonProperty("skin")]
        public string Skin { get; set; }

        [JsonProperty("variants")]
        public string Variants { get; set; }
    }
}