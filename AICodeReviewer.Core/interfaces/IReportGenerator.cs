using AICodeReviewer.Core.Models;


namespace AICodeReviewer.Core.interfaces;

public interface IReportGenerator
{
    /// <summary>
    /// Generate report in Markdown format
    /// </summary>
    string GenerateMarkdown(ReviewResult result);

    /// <summary>
    /// Generate report in HTML format
    /// </summary>
    string GenerateHtml(ReviewResult result);

    /// <summary>
    /// Generate report in JSON format
    /// </summary>
    string GenerateJson(ReviewResult result);
}
