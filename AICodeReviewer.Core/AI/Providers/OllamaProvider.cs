using AICodeReviewer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AICodeReviewer.Core.AI.Providers;

public class OllamaProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly string _baseUrl;
    private string _language = "en";  // 👈 إضافة

    public string ProviderName => "Ollama";
    public bool IsAvailable => CheckAvailability().Result;

    public OllamaProvider(string model = "codellama:7b-instruct", string baseUrl = "http://localhost:11434")
    {
        _httpClient = new HttpClient();
        _model = model;
        _baseUrl = baseUrl;
    }

    public void SetLanguage(string language)  // 👈 إضافة
    {
        _language = language;
    }

    private async Task<bool> CheckAvailability()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<FunctionReview> ReviewFunctionAsync(FunctionReview function)
    {
        // 👈 اختيار الـ Prompt حسب اللغة
        var prompt = _language == "ar"
            ? $@"أنت خبير متخصص في .NET و C# مع خبرة 10 سنوات.
قم بمراجعة دالة C# التالية وتقديم ملاحظاتك بصيغة JSON.

اسم الدالة: {function.FunctionName}
رقم السطر: {function.LineNumber}

الكود:
```csharp
{function.Code}
قم بالرد بصيغة JSON فقط بالهيكل التالي:
{{
""issues"": [
{{ ""severity"": ""Warning"", ""message"": ""وصف المشكلة"", ""codeSnippet"": ""السطر المرتبط"" }}
],
""suggestion"": ""اقتراح التحسين الشامل""
}}

درجة الخطورة: ""Info""، ""Warning""، أو ""Critical"".
قم بالرد باللغة العربية."
: $@"You are a senior .NET backend developer. Review this C# function and provide feedback in JSON format.

Function Name: {function.FunctionName}
Line: {function.LineNumber}

Code:

csharp
{function.Code}
Return ONLY valid JSON with this structure:
{{
""issues"": [
{{ ""severity"": ""Warning"", ""message"": ""issue description"", ""codeSnippet"": ""relevant code"" }}
],
""suggestion"": ""improvement suggestion""
}}

Severity: Info, Warning, or Critical";

        try
        {
            var request = new
            {
                model = _model,
                prompt = prompt,
                stream = false,
                options = new
                {
                    temperature = 0.3,
                    num_predict = 800
                }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseJson);
            var ollamaResponse = doc.RootElement.GetProperty("response").GetString() ?? "";

            var (issues, suggestion) = ParseResponse(ollamaResponse);

            function.Issues = issues;
            function.Suggestion = suggestion;
        }
        catch (Exception ex)
        {
            function.Suggestion = _language == "ar"
            ? $"⚠️ خطأ في Ollama: {ex.Message}\nتأكد من تشغيل Ollama (ollama serve)"
            : $"⚠️ Ollama Error: {ex.Message}\nMake sure Ollama is running (ollama serve)";
            function.Issues = new List<Issue>();
        }

        return function;
    }

    public async Task<string> GenerateSummaryAsync(List<FunctionReview> reviews)
    {
        var totalIssues = reviews.Sum(r => r.Issues.Count);
        var critical = reviews.Sum(r => r.Issues.Count(i => i.Severity == IssueSeverity.Critical));
        var warnings = reviews.Sum(r => r.Issues.Count(i => i.Severity == IssueSeverity.Warning));

        if (_language == "ar")
        {
            var result = $@"
📊 ملخص المراجعة
━━━━━━━━━━━━━━━━━━━━━━━━━━
📝 عدد الدوال التي تمت مراجعتها: {reviews.Count}
🐛 إجمالي المشاكل: {totalIssues}
🔴 خطيرة: {critical}
⚠️ تحذيرات: {warnings}
";

            if (critical > 0)
            {
                result += "\n⚠️ يرجى إصلاح المشاكل الخطيرة قبل المتابعة!";
            }
            else if (totalIssues == 0)
            {
                result += "\n✨ ممتاز! لم يتم العثور على أي مشاكل.";
            }

            return await Task.FromResult(result);
        }
        else
        {
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
            return (new List<Issue>(), _language == "ar" ? "تعذر تحليل استجابة النموذج" : "Unable to parse model response");
        }
    }
}