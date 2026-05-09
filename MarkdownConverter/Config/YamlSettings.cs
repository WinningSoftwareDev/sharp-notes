using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MarkdownConverter.Config
{
    internal static class YamlSettings
    {
        public static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }
}