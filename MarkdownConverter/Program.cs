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
                Console.WriteLine("You have provided invalid arguments.");
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

            try
            {
                Directory.CreateDirectory(projectPath);
                Console.WriteLine($"Successfully initialised project at: {projectPath}");
                Directory.CreateDirectory(Path.Combine(projectPath, "posts"));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error creating project at: {projectPath} - {e.Message}");
            }
        }
    }
}