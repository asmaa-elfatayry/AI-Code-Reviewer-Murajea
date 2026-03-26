using AICodeReviewer.Core.Models;


namespace AICodeReviewer.Core.AI;

public interface IAIProvider
{
    /// <summary>
    /// Review a single function and return issues and suggestions
    /// </summary>
    Task<FunctionReview> ReviewFunctionAsync(FunctionReview function);

    /// <summary>
    /// Generate a summary for multiple functions
    /// </summary>
    Task<string> GenerateSummaryAsync(List<FunctionReview> reviews);


    void SetLanguage(string language);

    /// <summary>
    /// Name of the provider (for logging/debugging)
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Check if provider is available/configured
    /// </summary>
    bool IsAvailable { get; }
}
