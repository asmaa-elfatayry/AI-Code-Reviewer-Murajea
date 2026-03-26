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
        // الحصول على مجلد المشروع أولاً
        var projectDir = GetProjectDirectory();

        System.Console.WriteLine("🤖 AI Code Reviewer — Murajea");
        System.Console.WriteLine("================================");
        System.Console.WriteLine();
        System.Console.WriteLine($"📁 Project Directory: {projectDir}");
        System.Console.WriteLine();

        // قراءة الإعدادات
        var config = LoadConfiguration(projectDir);

        // إنشاء AI Provider
        IAIProvider aiReviewer = CreateAIProvider(config);

        System.Console.WriteLine($"📡 Using AI Provider: {aiReviewer.ProviderName}");
        if (!aiReviewer.IsAvailable)
        {
            System.Console.WriteLine($"⚠️ Warning: {aiReviewer.ProviderName} may not be available. Check your configuration.");
        }
        System.Console.WriteLine();

        // تحديد المسار
        string path;
        if (args.Length > 0)
        {
            path = args[0];
        }
        else
        {
            // لو مفيش arguments، خد مجلد الـ Samples
            var samplesPath = Path.Combine(projectDir, "..", "Samples", "SampleProject");

            // جرب مسار بديل لو الأول مش موجود
            if (!Directory.Exists(samplesPath))
            {
                samplesPath = Path.Combine(projectDir, "..", "..", "Samples", "SampleProject");
            }

            if (Directory.Exists(samplesPath))
            {
                path = samplesPath;
                System.Console.WriteLine($"🔍 No path specified, using default sample: {path}");
            }
            else
            {
                path = Directory.GetCurrentDirectory();
                System.Console.WriteLine($"⚠️ No path specified and sample not found, using current directory: {path}");
                System.Console.WriteLine($"💡 Tip: Run with: dotnet run --project AICodeReviewer.Console -- Samples/SampleProject");
            }
        }

        if (!Directory.Exists(path) && !File.Exists(path))
        {
            System.Console.WriteLine($"❌ Path not found: {path}");
            return;
        }

        System.Console.WriteLine($"📁 Target: {path}");
        System.Console.WriteLine();

        // إنشاء الخدمات
        var codeReader = new CodeReader();
        var functionExtractor = new FunctionExtractor();
        var reportGenerator = new ReportGenerator();

        // الحصول على الملفات
        List<string> files;
        if (File.Exists(path) && path.EndsWith(".cs"))
        {
            files = new List<string> { path };
        }
        else
        {
            files = await codeReader.GetAllCSharpFilesAsync(path);
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
                var reviewed = await aiReviewer.ReviewFunctionAsync(functions[i]);
                functions[i] = reviewed;
                System.Console.WriteLine("✅");
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

        // إنشاء مجلد Reports
        var reportsDir = Path.Combine(projectDir, "Reports");
        if (!Directory.Exists(reportsDir))
        {
            Directory.CreateDirectory(reportsDir);
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // حفظ Markdown شامل
        var outputPath = Path.Combine(reportsDir, $"review_{timestamp}.md");
        var fullReport = string.Join("\n\n", allResults.Select(r => reportGenerator.GenerateMarkdown(r)));
        await File.WriteAllTextAsync(outputPath, fullReport);
        System.Console.WriteLine($"📄 Markdown report saved to: {outputPath}");

        // حفظ PDF لكل ملف
        var pdfGenerator = new PdfReportGenerator();

        foreach (var result in allResults)
        {
            var fileName = Path.GetFileNameWithoutExtension(result.FilePath);
            var pdfPath = Path.Combine(reportsDir, $"review_{timestamp}_{fileName}.pdf");
            var pdfBytes = pdfGenerator.GenerateReport(result);
            await File.WriteAllBytesAsync(pdfPath, pdfBytes);
            System.Console.WriteLine($"📄 PDF report saved to: {pdfPath}");
        }

        // حفظ HTML لكل ملف (اختياري)
        foreach (var result in allResults)
        {
            var fileName = Path.GetFileNameWithoutExtension(result.FilePath);
            var htmlPath = Path.Combine(reportsDir, $"review_{timestamp}_{fileName}.html");
            var htmlReport = reportGenerator.GenerateHtml(result);
            await File.WriteAllTextAsync(htmlPath, htmlReport);
            System.Console.WriteLine($"📄 HTML report saved to: {htmlPath}");
        }

        System.Console.WriteLine();
        System.Console.WriteLine("✨ Review completed!");
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

    static IAIProvider CreateAIProvider(AppConfig config)
    {
        var provider = config.AI?.Provider?.ToLower() ?? "ollama";

        return provider switch
        {
            "ollama" => AIReviewerFactory.CreateProvider(
                AIReviewerFactory.ProviderType.Ollama,
                model: config.AI?.Models?.Ollama,
                baseUrl: config.AI?.Ollama?.BaseUrl),

            "groq" => AIReviewerFactory.CreateProvider(
                AIReviewerFactory.ProviderType.Groq,
                apiKey: config.AI?.Groq?.ApiKey,
                model: config.AI?.Models?.Groq),

            "deepseek" => AIReviewerFactory.CreateProvider(
                AIReviewerFactory.ProviderType.DeepSeek,
                apiKey: config.AI?.DeepSeek?.ApiKey,
                model: config.AI?.Models?.DeepSeek),

            _ => AIReviewerFactory.CreateProvider(AIReviewerFactory.ProviderType.Ollama)
        };
    }
}