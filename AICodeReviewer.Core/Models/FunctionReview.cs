

namespace AICodeReviewer.Core.Models;

public class FunctionReview
{
    public string FunctionName { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string Code { get; set; } = string.Empty;
    public List<Issue> Issues { get; set; } = new();
    public string Suggestion { get; set; } = string.Empty;
}

public class Issue
{
    public IssueSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? CodeSnippet { get; set; }
}
