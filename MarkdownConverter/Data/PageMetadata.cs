namespace MarkdownConverter.Data
{
    internal record PageMetadata(
        PageHeader PageHeader,
        string FilePath, 
        string OutputUrl, 
        string HtmlContent
    );
}