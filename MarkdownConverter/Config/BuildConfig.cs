using System.Text.Json.Serialization;

namespace MarkdownConverter.Config
{
    internal class BuildConfig
    {
        [JsonPropertyName("navigation")]
        public List<NavigationItem> Navigation { get; init; } = [];
    }
}