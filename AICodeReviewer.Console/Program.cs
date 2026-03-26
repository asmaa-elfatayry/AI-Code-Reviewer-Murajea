using AICodeReviewer.Console.Configuration;
using AICodeReviewer.Console.Services;
using AICodeReviewer.Core.AI;
using AICodeReviewer.Core.AI.Providers;
using AICodeReviewer.Core.Models;
using AICodeReviewer.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AICodeReviewer.Console;

class Program
{
    static async Task Main(string[] args)
    {
        // الحصول على مجلد المشروع (للـ config)
        var projectDir = GetProjectDirectory();

        System.Console.WriteLine("🤖 AI Code Reviewer — Murajea");
        System.Console.WriteLine("================================");
        System.Console.WriteLine();
        System.Console.WriteLine($"📁 Tool Directory: {projectDir}");
        System.Console.WriteLine();

        // قراءة الإعدادات من مجلد الـ tool
        var config = LoadConfiguration(projectDir);

        // قراءة اللغة
        var language = config.Language ?? "en";
        System.Console.WriteLine($"🌐 Language: {(language == "ar" ? "العربية" : "English")}");
        System.Console.WriteLine();

        // إنشاء AI Provider مع اللغة
        IAIProvider aiReviewer = CreateAIProvider(config, language);

        System.Console.WriteLine($"📡 Using AI Provider: {aiReviewer.ProviderName}");

        // التحقق من توفر الـ AI Provider
        if (!aiReviewer.IsAvailable)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("⚠️ ERROR: AI Provider is not available!");
            System.Console.WriteLine();
            System.Console.WriteLine("📌 Please check your configuration:");
            System.Console.WriteLine();

            if (config.AI?.Provider?.ToLower() == "ollama")
            {
                System.Console.WriteLine("   For Ollama:");
                System.Console.WriteLine("   1. Make sure Ollama is installed: https://ollama.ai");
                System.Console.WriteLine("   2. Run: ollama serve");
                System.Console.WriteLine($"   3. Pull a model: ollama pull {config.AI?.Models?.Ollama ?? "codellama:7b-instruct"}");
                System.Console.WriteLine($"   4. Check URL: {config.AI?.Ollama?.BaseUrl ?? "http://localhost:11434"}");
            }
            else if (config.AI?.Provider?.ToLower() == "groq")
            {
                System.Console.WriteLine("   For Groq:");
                System.Console.WriteLine("   1. Get API key from: https://console.groq.com");
                System.Console.WriteLine("   2. Add your API key to appsettings.json under AI.Groq.ApiKey");
                System.Console.WriteLine("   3. Free tier: 30 requests/minute, 1,000 requests/day");
            }
            else if (config.AI?.Provider?.ToLower() == "deepseek")
            {
                System.Console.WriteLine("   For DeepSeek:");
                System.Console.WriteLine("   1. Get API key from: https://platform.deepseek.com");
                System.Console.WriteLine("   2. Add your API key to appsettings.json under AI.DeepSeek.ApiKey");
                System.Console.WriteLine("   3. Free tier: 500,000 tokens for new users");
            }

            System.Console.WriteLine();
            System.Console.WriteLine("❌ Review cancelled. No AI provider available.");
            return;
        }

        System.Console.WriteLine();

        // تحديد المسار (مجلد المستخدم)
        string targetPath;
        if (args.Length > 0)
        {
            targetPath = args[0];
        }
        else
        {
            // لو مفيش arguments، خد المجلد الحالي
            targetPath = Directory.GetCurrentDirectory();
            System.Console.WriteLine($"⚠️ No path specified, using current directory: {targetPath}");
            System.Console.WriteLine($"💡 Tip: ai-reviewer <path-to-code>");
        }

        if (!Directory.Exists(targetPath) && !File.Exists(targetPath))
        {
            System.Console.WriteLine($"❌ Path not found: {targetPath}");
            return;
        }

        System.Console.WriteLine($"📁 Target: {targetPath}");
        System.Console.WriteLine();

        // إنشاء الخدمات
        var codeReader = new CodeReader();
        var functionExtractor = new FunctionExtractor();
        var reportGenerator = new ReportGenerator();

        // الحصول على الملفات (استبعاد bin, obj)
        List<string> files;
        if (File.Exists(targetPath) && targetPath.EndsWith(".cs"))
        {
            files = new List<string> { targetPath };
        }
        else
        {
            files = await codeReader.GetAllCSharpFilesAsync(targetPath);
        }

        // فلترة الملفات لاستبعاد مجلدات bin, obj, .vs
        files = files.Where(f => !f.Contains("\\bin\\") &&
                                  !f.Contains("\\obj\\") &&
                                  !f.Contains("/bin/") &&
                                  !f.Contains("/obj/") &&
                                  !f.Contains("\\.vs\\") &&
                                  !f.Contains("\\node_modules\\")).ToList();

        if (files.Count == 0)
        {
            System.Console.WriteLine("⚠️ No C# files found to review.");
            System.Console.WriteLine("💡 Make sure your path contains .cs files and not in bin/obj folders.");
            return;
        }

        System.Console.WriteLine($"📄 Found {files.Count} C# files");
        System.Console.WriteLine();

        var allResults = new List<ReviewResult>();

        foreach (var file in files)
        {
            System.Console.WriteLine($"🔍 Reviewing: {Path.GetFileName(file)}...");

            var code = await codeReader.ReadFileAsync(file);
            var functions = functionExtractor.ExtractFunctions(code, file);

            System.Console.WriteLine($"   Found {functions.Count} functions");

            // مراجعة كل دالة
            for (int i = 0; i < functions.Count; i++)
            {
                System.Console.Write($"   [{i + 1}/{functions.Count}] {functions[i].FunctionName}... ");
                try
                {
                    var reviewed = await aiReviewer.ReviewFunctionAsync(functions[i]);
                    functions[i] = reviewed;
                    System.Console.WriteLine("✅");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"❌ Error: {ex.Message}");
                }
            }

            // توليد التقرير
            var result = new ReviewResult
            {
                FilePath = file,
                FunctionReviews = functions,
                Summary = await aiReviewer.GenerateSummaryAsync(functions)
            };

            allResults.Add(result);

            // عرض التقرير
            System.Console.WriteLine();
            System.Console.WriteLine(reportGenerator.GenerateMarkdown(result));
            System.Console.WriteLine(new string('-', 50));
        }

        // 👈 👈 👈 المكان الجديد: مجلد Reports في مجلد المستخدم
        var userReportsDir = GetUserReportsDirectory(targetPath);
        if (!Directory.Exists(userReportsDir))
        {
            Directory.CreateDirectory(userReportsDir);
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // حفظ Markdown شامل
        var outputPath = Path.Combine(userReportsDir, $"review_{timestamp}.md");
        var fullReport = string.Join("\n\n", allResults.Select(r => reportGenerator.GenerateMarkdown(r)));
        await File.WriteAllTextAsync(outputPath, fullReport);
        System.Console.WriteLine($"📄 Markdown report saved to: {outputPath}");

        // حفظ PDF لكل ملف
        var pdfGenerator = new PdfReportGenerator();

        foreach (var result in allResults)
        {
            var fileName = Path.GetFileNameWithoutExtension(result.FilePath);
            var pdfPath = Path.Combine(userReportsDir, $"review_{timestamp}_{fileName}.pdf");
            var pdfBytes = pdfGenerator.GenerateReport(result);
            await File.WriteAllBytesAsync(pdfPath, pdfBytes);
            System.Console.WriteLine($"📄 PDF report saved to: {pdfPath}");
        }

        // حفظ HTML لكل ملف (اختياري)
        foreach (var result in allResults)
        {
            var fileName = Path.GetFileNameWithoutExtension(result.FilePath);
            var htmlPath = Path.Combine(userReportsDir, $"review_{timestamp}_{fileName}.html");
            var htmlReport = reportGenerator.GenerateHtml(result);
            await File.WriteAllTextAsync(htmlPath, htmlReport);
            System.Console.WriteLine($"📄 HTML report saved to: {htmlPath}");
        }

        System.Console.WriteLine();
        System.Console.WriteLine("✨ Review completed!");
    }

    // 👈 دالة جديدة: جلب مجلد Reports في مشروع المستخدم
    static string GetUserReportsDirectory(string targetPath)
    {
        // إذا كان targetPath ملف، خذ مجلده
        string baseDir;
        if (File.Exists(targetPath))
        {
            baseDir = Path.GetDirectoryName(targetPath) ?? Directory.GetCurrentDirectory();
        }
        else if (Directory.Exists(targetPath))
        {
            baseDir = targetPath;
        }
        else
        {
            baseDir = Directory.GetCurrentDirectory();
        }

        return Path.Combine(baseDir, "AIReviewReports");
    }

    static string GetProjectDirectory()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;

        // لو في bin\Debug\net8.0\ أو bin\Release\net8.0\ أو bin\ فقط
        if (baseDir.Contains("\\bin\\") || baseDir.Contains("/bin/"))
        {
            // ابحث عن مجلد bin في المسار
            var binIndex = baseDir.IndexOf("\\bin\\");
            if (binIndex == -1)
            {
                binIndex = baseDir.IndexOf("/bin/");
            }

            if (binIndex > 0)
            {
                // خد كل حاجة قبل bin
                return baseDir.Substring(0, binIndex);
            }
        }

        // لو bin مش موجود، ارجع المجلد الحالي
        return baseDir;
    }

    static AppConfig LoadConfiguration(string projectDir)
    {
        var configPath = Path.Combine(projectDir, "appsettings.json");

        System.Console.WriteLine($"🔍 Looking for config at: {configPath}");

        if (File.Exists(configPath))
        {
            try
            {
                var json = File.ReadAllText(configPath);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"⚠️ Error loading config: {ex.Message}");
                return new AppConfig();
            }
        }

        System.Console.WriteLine($"⚠️ appsettings.json not found at: {configPath}");
        System.Console.WriteLine("⚠️ Using default configuration");
        return new AppConfig();
    }

    static IAIProvider CreateAIProvider(AppConfig config, string language)
    {
        var provider = config.AI?.Provider?.ToLower() ?? "ollama";

        return provider switch
        {
            "ollama" => AIReviewerFactory.CreateProvider(
                AIReviewerFactory.ProviderType.Ollama,
                model: config.AI?.Models?.Ollama,
                baseUrl: config.AI?.Ollama?.BaseUrl,
                language: language),

            "groq" => AIReviewerFactory.CreateProvider(
                AIReviewerFactory.ProviderType.Groq,
                apiKey: config.AI?.Groq?.ApiKey,
                model: config.AI?.Models?.Groq,
                language: language),

            "deepseek" => AIReviewerFactory.CreateProvider(
                AIReviewerFactory.ProviderType.DeepSeek,
                apiKey: config.AI?.DeepSeek?.ApiKey,
                model: config.AI?.Models?.DeepSeek,
                language: language),

            _ => AIReviewerFactory.CreateProvider(AIReviewerFactory.ProviderType.Ollama, language: language)
        };
    }
}