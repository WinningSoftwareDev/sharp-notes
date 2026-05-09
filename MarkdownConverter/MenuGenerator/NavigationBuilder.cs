using MarkdownConverter.Config;
using System.Text.Json;

namespace MarkdownConverter.MenuGenerator
{
    internal static class NavigationBuilder
    {
        public static string BuildNavigation(string sourceDirectory)
        {
            var text = File.ReadAllText(Path.Combine(sourceDirectory!, "build.json"));
            var json = JsonSerializer.Deserialize<BuildConfig>(text);
            List<string> items = [];

            if (json is not null)
            {
                foreach (var navItem in json.Navigation)
                {
                    items.Add(BuildMenuItem(navItem.Title, navItem.Url));
                }
            }

            return $"<ul>{string.Join("", items)}</ul>";
        }

        private static string BuildMenuItem(string display, string url)
        {
            var item = new MenuItem(display, url);
            var finalUrl = item.Url.Contains(".html") || item.Url == "/" 
                ? item.Url 
                : item.Url + ".html";

            return $"<li><a href='{finalUrl}'>{item.Display}</a></li>";
        }
    }
}