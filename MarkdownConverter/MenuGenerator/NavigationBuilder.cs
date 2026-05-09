namespace MarkdownConverter.MenuGenerator
{
    internal static class NavigationBuilder
    {
        public static string BuildNavigation()
        {
            var items = new List<string>()
            {
                BuildMenuItem("Home", "/"),
                BuildMenuItem("About", "/about"),
            };

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