
namespace AICodeReviewer.Core.AI.Providers;

public static class AIReviewerFactory
{
    public enum ProviderType
    {
        Ollama,
        Groq,
        DeepSeek,
    }

    public static IAIProvider CreateProvider(
          ProviderType provider,
          string? apiKey = null,
          string? model = null,
          string? baseUrl = null,
          string? language = "en")  // 👈 إضافة language
    {
        IAIProvider aiProvider = provider switch
        {
            ProviderType.Ollama => new OllamaProvider(
                model ?? "codellama:7b-instruct",
                baseUrl ?? "http://localhost:11434"),

            ProviderType.Groq => new GroqProvider(
                apiKey ?? throw new ArgumentException("API key required for Groq"),
                model ?? "llama-3.3-70b-versatile"),

            ProviderType.DeepSeek => new DeepSeekProvider(
                apiKey ?? throw new ArgumentException("API key required for DeepSeek"),
                model ?? "deepseek-chat"),

            _ => throw new NotSupportedException($"Provider {provider} not supported")
        };

        // 👈 تعيين اللغة
        aiProvider.SetLanguage(language);

        return aiProvider;
    }
}