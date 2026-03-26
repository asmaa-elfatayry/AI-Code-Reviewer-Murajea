using AICodeReviewer.Core.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


namespace AICodeReviewer.Core.AI.Providers;

public class DeepSeekProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public string ProviderName => "DeepSeek";
    public bool IsAvailable => !string.IsNullOrEmpty(_apiKey);

    public DeepSeekProvider(string apiKey, string model = "deepseek-chat")
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
                content = @"You are a senior .NET backend developer. Respond with valid JSON only."
            },
            new {
                role = "user",
                content = $@"
Review this C# function:

Function: {function.FunctionName}
Code:
```csharp
{function.Code}
Return JSON: {{ ""issues"": [{{ ""severity"": ""Warning"", ""message"": ""..."" }}], ""suggestion"": ""..."" }}"
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
            "https://api.deepseek.com/v1/chat/completions",
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
            function.Suggestion = $"⚠️ DeepSeek Error: {ex.Message}";
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
        // نفس الـ ParseResponse من OllamaProvider
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
