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
    private string _language = "en";  // 👈 إضافة

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

    public void SetLanguage(string language)  // 👈 إضافة
    {
        _language = language;
    }

    public async Task<FunctionReview> ReviewFunctionAsync(FunctionReview function)
    {
        // 👈 اختيار الـ System Prompt حسب اللغة
        var systemPrompt = _language == "ar"
            ? @"أنت خبير متخصص في .NET و C# مع خبرة 10 سنوات.
مهمتك هي تحليل كود C# وتقديم ملاحظات بناءة ودقيقة.
يجب أن يكون الرد بصيغة JSON فقط بدون أي نص خارج JSON.

تعليمات مهمة:
1. قم بتحليل الكود بدقة
2. حدد المشاكل المحتملة (أمان، أداء، أفضل الممارسات)
3. قدم حلولاً عملية قابلة للتنفيذ
4. استخدم لغة عربية فصحى واضحة"
            : @"You are a senior .NET backend developer specializing in code review.
Your job is to analyze C# code and provide constructive feedback.
Always respond with valid JSON only.";

        // 👈 اختيار الـ User Prompt حسب اللغة
        var userPrompt = _language == "ar"
            ? $@"
قم بمراجعة دالة C# التالية:

اسم الدالة: {function.FunctionName}
رقم السطر: {function.LineNumber}

الكود:
```csharp
{function.Code}
قم بالرد بصيغة JSON بالهيكل التالي:
{{
""issues"": [
{{ ""severity"": ""Warning"", ""message"": ""وصف المشكلة"", ""codeSnippet"": ""السطر المرتبط"" }}
],
""suggestion"": ""اقتراح التحسين الشامل""
}}

درجة الخطورة: ""Info""، ""Warning""، أو ""Critical"".
قم بالرد باللغة العربية."
: $@"
Review the following C# function:

Function Name: {function.FunctionName}
Line Number: {function.LineNumber}

Code:

csharp
{function.Code}
Return JSON with this exact structure:
{{
""issues"": [
{{ ""severity"": ""Warning"", ""message"": ""issue description"", ""codeSnippet"": ""relevant code line"" }}
],
""suggestion"": ""overall improvement suggestion""
}}

Severity must be: ""Info"", ""Warning"", or ""Critical"".";

        var request = new
        {
            model = _model,
            messages = new[]
        {
new { role = "system", content = systemPrompt },
new { role = "user", content = userPrompt }
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
            function.Suggestion = _language == "ar"
            ? $"⚠️ خطأ في Groq: {ex.Message}"
            : $"⚠️ Groq Error: {ex.Message}";
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
            return (new List<Issue>(), _language == "ar" ? "تعذر تحليل استجابة الذكاء الاصطناعي" : "Unable to parse response");
        }
    }
}