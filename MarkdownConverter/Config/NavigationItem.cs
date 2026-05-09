using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarkdownConverter.Config
{
    [DynamicallyAccessedMembers(
        memberTypes: DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties
    )]
    internal class NavigationItem
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("url")]
        public string Url { get; set; } = "";
    
        [JsonPropertyName("page")]
        public string Page { get; set; } = "";
    }
}