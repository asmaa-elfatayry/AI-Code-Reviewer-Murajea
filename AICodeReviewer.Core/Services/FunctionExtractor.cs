using AICodeReviewer.Core.interfaces;
using AICodeReviewer.Core.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace AICodeReviewer.Core.Services;

public class FunctionExtractor : IFunctionExtractor
{
    public List<FunctionReview> ExtractFunctions(string code, string filePath)
    {
        var functions = new List<FunctionReview>();

        try
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetCompilationUnitRoot();

            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var method in methods)
            {
                var lineSpan = method.GetLocation().GetLineSpan();
                var function = new FunctionReview
                {
                    FunctionName = method.Identifier.Text,
                    LineNumber = lineSpan.StartLinePosition.Line + 1,
                    Code = method.ToString(),
                    Issues = new List<Issue>()
                };

                functions.Add(function);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing {filePath}: {ex.Message}");
        }

        return functions;
    }
}
