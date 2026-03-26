using AICodeReviewer.Core.Models;


namespace AICodeReviewer.Core.interfaces;

public interface IFunctionExtractor
{
    List<FunctionReview> ExtractFunctions(string code, string filePath);
}
