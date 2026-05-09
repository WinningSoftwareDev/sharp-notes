using Markdig;
using MarkdownConverter.Builder;
using MarkdownConverter.Config;
using MarkdownConverter.Data;
using MarkdownConverter.MenuGenerator;

namespace MarkdownConverter
{
    internal static class Program
    {
        private static string? _projectName = null;
        private static string? _sourceDirectory = null;
        private static string? _targetDirectory = null;
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseYamlFrontMatter()
            .Build();
        
        private static void Main(string[] args)
        {
            ParseArguments(args);

            if (!ValidateArguments())
            {
                Console.WriteLine("Error: you have provided invalid arguments.");
                return;
            }
            
            GenerateSite();
        }

        private static void ParseArguments(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.StartsWith(GeneratorArguments.NameKey))
                {
                    _projectName = arg[$"{GeneratorArguments.NameKey}=".Length..];
                }

                if (arg.StartsWith(GeneratorArguments.SourceDirectoryKey))
                {
                    _sourceDirectory = arg[$"{GeneratorArguments.SourceDirectoryKey}=".Length..];
                }

                if (arg.StartsWith(GeneratorArguments.TargetDirectoryKey))
                {
                    _targetDirectory = arg[$"{GeneratorArguments.TargetDirectoryKey}=".Length..];
                }
            }
        }

        private static bool ValidateArguments()
        {
            return 
                !(
                    string.IsNullOrEmpty(_projectName) 
                    || string.IsNullOrEmpty(_sourceDirectory) 
                    || string.IsNullOrEmpty(_targetDirectory) 
                    || int.TryParse(_projectName, out _)
                    || !Directory.Exists(Path.GetFullPath(_sourceDirectory))
                );
        }

        private static void GenerateSite()
        {
            var projectPath = Path.Combine(_targetDirectory!, _projectName!);
            
            try
            {
                DirectoryBuilder.CreateOutputDirectory(projectPath);
                
                var allPages = Directory
                    .GetFiles(_sourceDirectory!, "*.md", SearchOption.AllDirectories)
                    .Select(GetPageMetadata)
                    .ToList();
                var layoutHtml = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "assets", "layout.html"));

                foreach (var page in allPages)
                {
                    GeneratePage(projectPath, page, layoutHtml);
                }
                
                Console.WriteLine($"Successfully created a new static site at: {projectPath}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error creating project at: {projectPath} - {e.Message}");
            }
        }

        private static void GeneratePage(string projectPath, PageMetadata page, string layoutHtml)
        {
            var html = layoutHtml
                .Replace("{{Title}}", page.PageHeader.Title)
                .Replace("{{Navigation}}", NavigationBuilder.BuildNavigation(_sourceDirectory!))
                .Replace("{{Content}}", page.HtmlContent.Replace("{{Generator:ListPosts}}", "Test!"));
            var outputPath = Path.Combine(projectPath, page.OutputUrl.TrimStart('/'));
                    
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            File.WriteAllText(outputPath, html);
            Console.WriteLine($"[Build] {page.PageHeader.Title} -> {page.OutputUrl}");
        }

        private static PageMetadata GetPageMetadata(string file)
        {
            var fileContent = File.ReadAllText(file);
            var (header, markdownBody) = ParseWithMetadata(fileContent);
            
            var relPath = Path.GetRelativePath(_sourceDirectory!, file);
            var url = "/" + Path.ChangeExtension(relPath, ".html").Replace("\\", "/");

            return new PageMetadata(
                header,
                file,
                url,
                Markdown.ToHtml(markdownBody, Pipeline)
            );
        }
        
        private static (PageHeader Header, string Body) ParseWithMetadata(string rawContent)
        {
            if (!rawContent.StartsWith("---"))
            {
                return (new PageHeader { Title = "Untitled", Date = DateTime.Now }, rawContent);
            }

            var frontMatterEndIndex = rawContent.IndexOf("---", 3, StringComparison.Ordinal);
            
            if (frontMatterEndIndex == -1)
            {
                return (new PageHeader { Title = "Untitled", Date = DateTime.Now }, rawContent);
            }

            try
            {
                var yaml = rawContent[3..frontMatterEndIndex].Trim();
                var body = rawContent[(frontMatterEndIndex + 3)..].Trim();
                var header = YamlSettings.Deserializer.Deserialize<PageHeader>(yaml);
                
                return (header, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Warning] Failed to parse YAML: {ex.Message}");
                return (new PageHeader { Title = "Untitled" }, rawContent);
            }
        }
    }
}