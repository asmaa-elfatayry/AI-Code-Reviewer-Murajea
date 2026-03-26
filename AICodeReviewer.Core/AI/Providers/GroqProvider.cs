

using AICodeReviewer.Core.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AICodeReviewer.Core.AI.Providers;

public class GroqProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public string ProviderName => "Groq";
    public bool IsAvailable => !string.IsNullOrEmpty(_apiKey);

    public GroqProvider(string apiKey, string model = "llama-3.3-70b-versatile")
    {
        _httpClient = new HttpClient();
        _apiKey = apiKey;
        _model = model;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<FunctionReview> ReviewFunctionAsync(FunctionReview function)
    {
        var request = new
        {
            model = _model,
            messages = new[]
            {
                new {
                    role = "system",
                    content = @"You are a senior .NET backend developer specializing in code review.
Your job is to analyze C# code and provide constructive feedback.
Always respond with valid JSON only."
                },
                new {
                    role = "user",
                    content = $@"
Review the following C# function:

Function Name: {function.FunctionName}
Line Number: {function.LineNumber}

Code:
```csharp
{function.Code}
Return JSON with this exact structure:
{{
""issues"": [
{{ ""severity"": ""Warning"", ""message"": ""issue description"", ""codeSnippet"": ""relevant code line"" }}
],
""suggestion"": ""overall improvement suggestion""
}}

Severity must be: ""Info"", ""Warning"", or ""Critical""."
}
},
            temperature = 0.3,
            max_tokens = 800
        };

        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
            "https://api.groq.com/openai/v1/chat/completions",
            content);

            var responseJson = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseJson);
            var result = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";

            var (issues, suggestion) = ParseResponse(result);

            function.Issues = issues;
            function.Suggestion = suggestion;
        }
        catch (Exception ex)
        {
            function.Suggestion = $"⚠️ Groq Error: {ex.Message}";
            function.Issues = new List<Issue>();
        }

        return function;
    }

    public async Task<string> GenerateSummaryAsync(List<FunctionReview> reviews)
    {
        var totalIssues = reviews.Sum(r => r.Issues.Count);
        var critical = reviews.Sum(r => r.Issues.Count(i => i.Severity == IssueSeverity.Critical));
        var warnings = reviews.Sum(r => r.Issues.Count(i => i.Severity == IssueSeverity.Warning));

        var result = $@"
📊 Review Summary
━━━━━━━━━━━━━━━━━━━━━━━━━━
📝 Functions reviewed: {reviews.Count}
🐛 Total issues: {totalIssues}
🔴 Critical: {critical}
⚠️ Warnings: {warnings}
";

        if (critical > 0)
        {
            result += "\n⚠️ Please fix critical issues before merging!";
        }
        else if (totalIssues == 0)
        {
            result += "\n✨ Excellent! No issues found.";
        }

        return await Task.FromResult(result);
    }

    private (List<Issue> issues, string suggestion) ParseResponse(string response)
    {
        try
        {
            var json = response.Trim();
            var start = json.IndexOf('{');
            var end = json.LastIndexOf('}');

            if (start >= 0 && end > start)
            {
                json = json.Substring(start, end - start + 1);
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var issues = new List<Issue>();

            if (root.TryGetProperty("issues", out var issuesArray))
            {
                foreach (var item in issuesArray.EnumerateArray())
                {
                    var severityStr = item.GetProperty("severity").GetString();
                    var severity = severityStr?.ToLower() switch
                    {
                        "critical" => IssueSeverity.Critical,
                        "warning" => IssueSeverity.Warning,
                        _ => IssueSeverity.Info
                    };

                    issues.Add(new Issue
                    {
                        Severity = severity,
                        Message = item.GetProperty("message").GetString() ?? "",
                        CodeSnippet = item.TryGetProperty("codeSnippet", out var snippet)
                    ? snippet.GetString()
                    : null
                    });
                }
            }

            var suggestion = root.TryGetProperty("suggestion", out var suggestionProp)
            ? suggestionProp.GetString() ?? ""
            : "";

            return (issues, suggestion);
        }
        catch
        {
            return (new List<Issue>(), "Unable to parse response");
        }
    }
}
