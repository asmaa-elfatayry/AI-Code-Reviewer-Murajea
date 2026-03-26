using AICodeReviewer.Core.interfaces;


namespace AICodeReviewer.Core.Services;

public class CodeReader : ICodeReader
{
    public async Task<string> ReadFileAsync(string filePath)
    {
        return await File.ReadAllTextAsync(filePath);
    }

    public Task<List<string>> GetAllCSharpFilesAsync(string projectPath)
    {
        var files = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("/bin/") && !f.Contains("/obj/") && !f.Contains("/node_modules/"))
            .ToList();

        return Task.FromResult(files);
    }
}
