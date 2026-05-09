namespace MarkdownConverter.Data
{
    internal class PageHeader {
        public string Title { get; init; } = "";
        public string Type { get; init; } = "post";
        public bool MenuItem { get; init; } = false;
        public int? MenuIndex { get; init; } = null;
        public DateTime? Date { get; init; } = null;
        public List<string> Tags { get; init; } = [];
    }
}