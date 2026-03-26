

namespace AICodeReviewer.Core.Models;

public class ReviewResult
{
    public string FilePath { get; set; } = string.Empty;
    public DateTime ReviewDate { get; set; } = DateTime.Now;
    public List<FunctionReview> FunctionReviews { get; set; } = new();
    public string Summary { get; set; } = string.Empty;

    public int TotalIssues => FunctionReviews.Sum(f => f.Issues.Count);
    public int CriticalIssues => FunctionReviews.Sum(f => f.Issues.Count(i => i.Severity == IssueSeverity.Critical));
    public int Warnings => FunctionReviews.Sum(f => f.Issues.Count(i => i.Severity == IssueSeverity.Warning));
}
