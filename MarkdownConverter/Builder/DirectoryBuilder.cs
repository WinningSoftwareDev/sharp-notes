namespace MarkdownConverter.Builder
{
    internal static class DirectoryBuilder
    {
        public static void CreateOutputDirectory(string projectPath)
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
    }
}