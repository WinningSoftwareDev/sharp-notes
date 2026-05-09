namespace MarkdownConverter.MenuGenerator
{
    internal class MenuItem(string display, string url)
    {
        public string Display { get; set; } = display;
        public string Url { get; set; } = url;
    }
}