using System.Text.Json.Serialization;
namespace Unison.ViewModels;
public class VoskModel
{
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("lang")]
        public string Lang { get; set; } = string.Empty;

        [JsonPropertyName("lang_text")]
        public string LangText { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("obsolete")]
        public string Obsolete { get; set; } = string.Empty;

        [JsonPropertyName("md5")]
        public string Md5 { get; set; } = string.Empty;

        [JsonPropertyName("size_text")]
        public string SizeText { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
}


