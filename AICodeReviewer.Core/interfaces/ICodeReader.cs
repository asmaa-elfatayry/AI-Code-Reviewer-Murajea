
namespace AICodeReviewer.Core.interfaces;

public interface ICodeReader
{
    Task<string> ReadFileAsync(string filePath);
    Task<List<string>> GetAllCSharpFilesAsync(string projectPath);
}
