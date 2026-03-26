using AICodeReviewer.Core.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AICodeReviewer.Console.Services;

public class PdfReportGenerator
{
    static PdfReportGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateReport(ReviewResult result)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Segoe UI"));

                // Header with Gradient Effect
                page.Header()
                    .ShowOnce()
                    .Column(col =>
                    {
                        col.Item().PaddingBottom(10).Background(Colors.Blue.Darken4)
                            .Padding(20)
                            .Column(header =>
                            {
                                header.Item().Text("🤖 AI Code Review — Murajea")
                                    .FontSize(28)
                                    .Bold()
                                    .FontColor(Colors.White)
                                    .AlignCenter();

                                header.Item().PaddingTop(5).Text("Professional Code Analysis Report")
                                    .FontSize(14)
                                    .FontColor(Colors.Grey.Lighten2)
                                    .AlignCenter();
                            });

                        col.Item().PaddingTop(15).Row(row =>
                        {
                            row.RelativeItem().Text($"📄 File: {System.IO.Path.GetFileName(result.FilePath)}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken2);

                            row.RelativeItem().Text($"📅 Date: {result.ReviewDate:yyyy-MM-dd HH:mm:ss}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken2)
                                .AlignRight();
                        });

                        col.Item().PaddingTop(5).LineHorizontal(0.5f);
                    });

                // Content
                page.Content()
                    .PaddingVertical(20)
                    .Column(col =>
                    {
                        // Summary Section with Cards
                        col.Item().PaddingBottom(20).Column(summaryCol =>
                        {
                            summaryCol.Item().Text("📊 EXECUTIVE SUMMARY")
                                .FontSize(18)
                                .Bold()
                                .FontColor(Colors.Blue.Darken4);

                            summaryCol.Item().PaddingTop(10).Row(row =>
                            {
                                // Total Issues Card
                                row.RelativeItem().Background(Colors.Grey.Lighten3)
                                    .Padding(15)
                                    .CornerRadius(8)
                                    .Column(card =>
                                    {
                                        card.Item().Text("TOTAL ISSUES")
                                            .FontSize(10)
                                            .FontColor(Colors.Grey.Darken2)
                                            .AlignCenter();

                                        card.Item().Text(result.TotalIssues.ToString())
                                            .FontSize(32)
                                            .Bold()
                                            .FontColor(Colors.Blue.Darken4)
                                            .AlignCenter();
                                    });

                                // Critical Issues Card
                                row.RelativeItem().Background(Colors.Red.Lighten4)
                                    .Padding(15)
                                    .CornerRadius(8)
                                    .Column(card =>
                                    {
                                        card.Item().Text("CRITICAL")
                                            .FontSize(10)
                                            .FontColor(Colors.Red.Darken2)
                                            .AlignCenter();

                                        card.Item().Text(result.CriticalIssues.ToString())
                                            .FontSize(32)
                                            .Bold()
                                            .FontColor(Colors.Red.Darken4)
                                            .AlignCenter();
                                    });

                                // Warnings Card
                                row.RelativeItem().Background(Colors.Orange.Lighten4)
                                    .Padding(15)
                                    .CornerRadius(8)
                                    .Column(card =>
                                    {
                                        card.Item().Text("WARNINGS")
                                            .FontSize(10)
                                            .FontColor(Colors.Orange.Darken2)
                                            .AlignCenter();

                                        card.Item().Text(result.Warnings.ToString())
                                            .FontSize(32)
                                            .Bold()
                                            .FontColor(Colors.Orange.Darken4)
                                            .AlignCenter();
                                    });

                                // Functions Card
                                row.RelativeItem().Background(Colors.Green.Lighten4)
                                    .Padding(15)
                                    .CornerRadius(8)
                                    .Column(card =>
                                    {
                                        card.Item().Text("FUNCTIONS")
                                            .FontSize(10)
                                            .FontColor(Colors.Green.Darken2)
                                            .AlignCenter();

                                        card.Item().Text(result.FunctionReviews.Count.ToString())
                                            .FontSize(32)
                                            .Bold()
                                            .FontColor(Colors.Green.Darken4)
                                            .AlignCenter();
                                    });
                            });

                            // Quality Score
                            var score = CalculateQualityScore(result);
                            summaryCol.Item().PaddingTop(15).Background(Colors.Blue.Lighten5)
                                .Padding(15)
                                .CornerRadius(8)
                                .Row(row =>
                                {
                                    row.RelativeItem().Text("CODE QUALITY SCORE:")
                                        .FontSize(14)
                                        .Bold()
                                        .FontColor(Colors.Blue.Darken4);

                                    row.RelativeItem().Text($"{score}%")
                                        .FontSize(24)
                                        .Bold()
                                        .FontColor(GetScoreColor(score))
                                        .AlignRight();
                                });
                        });

                        // Functions Section
                        col.Item().PaddingTop(20).Text("🔍 DETAILED ANALYSIS")
                            .FontSize(18)
                            .Bold()
                            .FontColor(Colors.Blue.Darken4);

                        col.Item().PaddingTop(5).LineHorizontal(0.5f);

                        foreach (var function in result.FunctionReviews)
                        {
                            col.Item().PaddingTop(15).Column(funcCol =>
                            {
                                // Function Header
                                funcCol.Item().Background(Colors.Grey.Lighten4)
                                    .Padding(12)
                                    .CornerRadius(8)
                                    .Row(row =>
                                    {
                                        row.RelativeItem().Text($"📌 {function.FunctionName}")
                                            .FontSize(14)
                                            .Bold()
                                            .FontColor(Colors.Blue.Darken3);

                                        row.RelativeItem().Text($"Line {function.LineNumber}")
                                            .FontSize(11)
                                            .FontColor(Colors.Grey.Darken2)
                                            .AlignRight();
                                    });

                                // Code Block
                                funcCol.Item().PaddingTop(10).Background(Colors.Grey.Lighten5)
                                    .Padding(12)
                                    .CornerRadius(5)
                                    .Border(0.5f, Colors.Grey.Lighten2)
                                    .Text(code =>
                                    {
                                        code.Span("📝 Code:")
                                            .FontSize(10)
                                            .Bold()
                                            .FontColor(Colors.Grey.Darken2);

                                        code.Span("\n");  // 👈 بديل Line()
                                        code.Span("\n");  // 👈 بديل Line()


                                        code.Span(function.Code)
                                            .FontFamily("Consolas")
                                            .FontSize(9)
                                            .FontColor(Colors.Black);
                                    });

                                // Issues Section
                                if (function.Issues.Any())
                                {
                                    funcCol.Item().PaddingTop(10).Text("⚠️ Issues Found:")
                                        .FontSize(11)
                                        .Bold()
                                        .FontColor(Colors.Red.Darken2);

                                    foreach (var issue in function.Issues)
                                    {
                                        var (bgColor, icon, textColor) = GetIssueStyle(issue.Severity);

                                        funcCol.Item().PaddingTop(5).PaddingLeft(10).Background(bgColor)
                                            .Padding(10)
                                            .CornerRadius(5)
                                            .Row(row =>
                                            {
                                                row.ConstantColumn(30).Text(icon)
                                                    .FontSize(14);

                                                row.RelativeItem().Text($"{issue.Severity}: {issue.Message}")
                                                    .FontSize(10)
                                                    .FontColor(textColor);
                                            });
                                    }
                                }

                                // Suggestion Section
                                if (!string.IsNullOrEmpty(function.Suggestion))
                                {
                                    funcCol.Item().PaddingTop(10).Background(Colors.Green.Lighten5)
                                        .Padding(12)
                                        .CornerRadius(5)
                                        .Row(row =>
                                        {
                                            row.ConstantColumn(30).Text("💡")
                                                .FontSize(14);

                                            row.RelativeItem().Text($"Suggestion: {function.Suggestion}")
                                                .FontSize(10)
                                                .FontColor(Colors.Green.Darken4);
                                        });
                                }

                                funcCol.Item().PaddingTop(10).LineHorizontal(0.5f);
                            });
                        }
                    });

                // Footer
                page.Footer()
                    .PaddingTop(10)
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("🤖 AI Code Reviewer — Murajea | ")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Medium);
                        text.Span("Professional Code Analysis")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Medium);
                        text.Span("  |  ");
                        text.CurrentPageNumber()
                            .FontSize(8);
                        text.Span(" / ");
                        text.TotalPages()
                            .FontSize(8);
                    });
            });
        });

        return document.GeneratePdf();
    }

    private int CalculateQualityScore(ReviewResult result)
    {
        var totalIssues = result.TotalIssues;
        var totalFunctions = result.FunctionReviews.Count;

        if (totalFunctions == 0) return 100;

        // كل issue تنقص 5 نقاط
        var deduction = totalIssues * 5;
        var score = Math.Max(0, 100 - deduction);

        // Critical issues تنقص 10 نقاط إضافية
        score -= result.CriticalIssues * 10;

        return Math.Max(0, Math.Min(100, score));
    }

    private (string bgColor, string icon, string textColor) GetIssueStyle(IssueSeverity severity)
    {
        return severity switch
        {
            IssueSeverity.Critical => (Colors.Red.Lighten4, "🔴", Colors.Red.Darken4),
            IssueSeverity.Warning => (Colors.Orange.Lighten4, "⚠️", Colors.Orange.Darken4),
            _ => (Colors.Blue.Lighten4, "ℹ️", Colors.Blue.Darken4)
        };
    }

    private string GetScoreColor(int score)
    {
        return score switch
        {
            >= 80 => Colors.Green.Darken2,
            >= 60 => Colors.Orange.Darken2,
            _ => Colors.Red.Darken2
        };
    }
}