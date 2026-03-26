using AICodeReviewer.Core.interfaces;
using AICodeReviewer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AICodeReviewer.Core.Services;


public class ReportGenerator : IReportGenerator
{
    public string GenerateMarkdown(ReviewResult result)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"# AI Code Review Report");
        sb.AppendLine($"**File:** {result.FilePath}");
        sb.AppendLine($"**Date:** {result.ReviewDate}");
        sb.AppendLine();
        sb.AppendLine($"## Summary");
        sb.AppendLine($"- Total Issues: {result.TotalIssues}");
        sb.AppendLine($"- Critical: {result.CriticalIssues}");
        sb.AppendLine($"- Warnings: {result.Warnings}");
        sb.AppendLine();

        foreach (var function in result.FunctionReviews)
        {
            sb.AppendLine($"## 🔹 {function.FunctionName} (Line {function.LineNumber})");
            sb.AppendLine();

            foreach (var issue in function.Issues)
            {
                var emoji = issue.Severity == IssueSeverity.Critical ? "🔴" :
                           (issue.Severity == IssueSeverity.Warning ? "⚠️" : "ℹ️");
                sb.AppendLine($"- {emoji} **{issue.Severity}:** {issue.Message}");
            }

            if (!string.IsNullOrEmpty(function.Suggestion))
            {
                sb.AppendLine($"**💡 Suggestion:** {function.Suggestion}");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    public string GenerateHtml(ReviewResult result)
    {
        // Simple HTML template
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>AI Code Review - {Path.GetFileName(result.FilePath)}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .critical {{ color: red; }}
        .warning {{ color: orange; }}
        .info {{ color: blue; }}
        .function {{ background: #f5f5f5; padding: 10px; margin: 10px 0; }}
    </style>
</head>
<body>
    <h1>🤖 AI Code Review Report</h1>
    <p><strong>File:</strong> {result.FilePath}</p>
    <p><strong>Date:</strong> {result.ReviewDate}</p>
    
    <h2>Summary</h2>
    <ul>
        <li>Total Issues: {result.TotalIssues}</li>
        <li>Critical: {result.CriticalIssues}</li>
        <li>Warnings: {result.Warnings}</li>
    </ul>
    
    <h2>Function Reviews</h2>
    {string.Join("", result.FunctionReviews.Select(f => $@"
    <div class='function'>
        <h3>{f.FunctionName} (Line {f.LineNumber})</h3>
        {string.Join("", f.Issues.Select(i => $@"
        <p class='{i.Severity.ToString().ToLower()}'><strong>{i.Severity}:</strong> {i.Message}</p>
        "))}
        {(!string.IsNullOrEmpty(f.Suggestion) ? $"<p><strong>💡 Suggestion:</strong> {f.Suggestion}</p>" : "")}
    </div>
    "))}
</body>
</html>";

        return html;
    }

    public string GenerateJson(ReviewResult result)
    {
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }
}
