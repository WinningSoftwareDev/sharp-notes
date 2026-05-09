using Markdig;
using MarkdownConverter.MenuGenerator;
using System.Text.Json;
using MarkdownConverter.Config;

namespace MarkdownConverter
{
    internal record PageMetadata(
        PageHeader PageHeader,
        string FilePath, 
        string OutputUrl, 
        string HtmlContent
    );
    
    internal class PageHeader {
        public string Title { get; init; } = "";
        public DateTime Date { get; init; }
        public List<string> Tags { get; init; } = [];
    }

    internal static class Program
    {
        private static string? _projectName = null;
        private static string? _sourceDirectory = null;
        private static string? _targetDirectory = null;
        private static string? _navigation = null;
        
        private const string NameKey = "--name";
        private const string SourceDirectoryKey = "--src";
        private const string TargetDirectoryKey = "--to";
        
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
                if (arg.StartsWith(NameKey))
                {
                    _projectName = arg[$"{NameKey}=".Length..];
                }

                if (arg.StartsWith(SourceDirectoryKey))
                {
                    _sourceDirectory = arg[$"{SourceDirectoryKey}=".Length..];
                }

                if (arg.StartsWith(TargetDirectoryKey))
                {
                    _targetDirectory = arg[$"{TargetDirectoryKey}=".Length..];
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
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseYamlFrontMatter()
                .Build();
            _navigation = NavigationBuilder.BuildNavigation(_sourceDirectory!);
            
            try
            {
                CreateOutputDirectory(projectPath);
                List<PageMetadata> allPages = [];
                var files = Directory.GetFiles(_sourceDirectory!, "*.md", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var fileContent = File.ReadAllText(file);
                    var (header, markdownBody) = ParseWithMetadata(fileContent);
            
                    var relPath = Path.GetRelativePath(_sourceDirectory!, file);
                    var url = "/" + Path.ChangeExtension(relPath, ".html").Replace("\\", "/");

                    allPages.Add(new PageMetadata(
                        header, 
                        file, 
                        url, 
                        Markdown.ToHtml(markdownBody, pipeline)
                    ));
                }
                
                var layoutHtml = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "assets", "layout.html"));

                foreach (var page in allPages)
                {
                    var finalHtml = layoutHtml
                        .Replace("{{Title}}", page.PageHeader.Title)
                        .Replace("{{Navigation}}", _navigation)
                        .Replace("{{Content}}", page.HtmlContent);

                    var outputPath = Path.Combine(projectPath, page.OutputUrl.TrimStart('/'));
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                    File.WriteAllText(outputPath, finalHtml);
            
                    Console.WriteLine($"[Build] {page.PageHeader.Title} -> {page.OutputUrl}");
                }
                
                // foreach (var file in files)
                // {
                //     ProcessMarkdownFile(file, projectPath, pipeline);
                // }
                
                Console.WriteLine($"Successfully created a new static site at: {projectPath}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error creating project at: {projectPath} - {e.Message}");
            }
        }

        private static void CreateOutputDirectory(string projectPath)
        {
            if (Directory.Exists(projectPath))
            {
                Console.WriteLine($"Output directory already exists. Cleaning up existing files: {projectPath}");
                Directory.Delete(projectPath, true);
            }
                
            Directory.CreateDirectory(projectPath);
            CreateAssetsDirectory(projectPath);
        }

        private static void CreateAssetsDirectory(string projectPath)
        {
            Directory.CreateDirectory(Path.Combine(projectPath, "themes"));
            File.Copy(Path.Combine(
                    AppContext.BaseDirectory, "assets", "themes", "default.css"), 
                Path.Combine(projectPath, "themes", "default.css")
            );
        }

        private static void ProcessMarkdownFile(string file, string projectPath, MarkdownPipeline pipeline)
        {
            var outputPath = Path.Combine(
                projectPath, 
                Path.ChangeExtension(
                    Path.GetRelativePath(_sourceDirectory!, file), 
                    ".html"
                )
            );
            var outputDirectory = Path.GetDirectoryName(outputPath);
                    
            if (outputDirectory != null) 
            {
                Directory.CreateDirectory(outputDirectory);
            }
            
            var markdown = File.ReadAllText(file);
            var htmlText = File.ReadAllText(
                Path.Combine(AppContext.BaseDirectory, "assets", "layout.html")
            );
            var html = htmlText
                .Replace("{{Title}}", _projectName)
                .Replace("{{Navigation}}", _navigation)
                .Replace("{{Content}}", Markdown.ToHtml(markdown, pipeline));
                    
            File.WriteAllText(outputPath, html);
                    
            Console.WriteLine($"[Build] {file} -> {outputPath}");
        }

        private static (PageHeader Header, string Body) ParseWithMetadata(string content)
        {
            if (!content.StartsWith("---"))
            {
                return (new PageHeader { Title = "Untitled" }, content);
            }
            
            var parts = content.Split("---", StringSplitOptions.RemoveEmptyEntries);
            var yaml = parts[0];
            var body = parts.Length > 1 ? parts[1] : "";

            // Here you would use YamlDotNet to turn 'yaml' string into 'PageHeader'
            // For a quick test, let's just manually grab the title
            var titleLine = yaml.Split('\n').FirstOrDefault(l => l.StartsWith("title:"));
            var title = titleLine?.Split(':')[1].Trim() ?? "Untitled";

            return (new PageHeader { Title = title, Date = DateTime.Now, Tags = [] }, body);
        }
    }
}