using Markdig;

namespace MarkdownConverter
{
    internal static class Program
    {
        private static string? _projectName = null;
        private static string? _sourceDirectory = null;
        private static string? _targetDirectory = null;
        
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
            
            try
            {
                CreateOutputDirectory(projectPath);
            
                foreach (
                    var file in 
                    Directory.GetFiles(_sourceDirectory!, "*.md", SearchOption.AllDirectories)
                )
                {
                    ProcessMarkdownFile(file, projectPath, pipeline);
                }
                
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
                Console.WriteLine($"Project directory already exists. Cleaning: {projectPath}");
                Directory.Delete(projectPath, true);
            }
                
            Directory.CreateDirectory(projectPath);
            Directory.CreateDirectory(Path.Combine(projectPath, "themes"));
            File.Copy(Path.Combine(
                AppContext.BaseDirectory, "assets", "themes", "default.css"), 
                Path.Combine(projectPath, "themes", "default.css")
            );
        }

        private static string BuildNavigation()
        {
            var items = new List<string>();
            var html = "";
            
            items.Add(BuildMenuItem("Home", "/"));
            items.Add(BuildMenuItem("About", "/about"));

            foreach (var item in items)
            {
                html = html + item;
            }

            return $"<ul>{html}</ul>";
        }

        private static string BuildMenuItem(string display, string url)
        {
            var item = new MenuItem(display, url);
            var finalUrl = item.Url.Contains(".html") || item.Url == "/" ? item.Url : item.Url + ".html";

            return $"<li><a href='{finalUrl}'>{item.Display}</a></li>";
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
                .Replace("{{Navigation}}", BuildNavigation())
                .Replace("{{Content}}", Markdown.ToHtml(markdown, pipeline));
                    
            File.WriteAllText(outputPath, html);
                    
            Console.WriteLine($"[Build] {file} -> {outputPath}");
        }
    }

    internal class MenuItem(string display, string url)
    {
        public string Display { get; set; } = display;
        public string Url { get; set; } = url;
    }
}