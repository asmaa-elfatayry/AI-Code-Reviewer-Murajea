

namespace AICodeReviewer.Console.Configuration;

/// <summary>
/// Root configuration class for the application
/// </summary>
public class AppConfig
{
    public string? Language { get; set; }
    public AIConfig? AI { get; set; }
}

/// <summary>
/// AI provider configuration
/// </summary>
public class AIConfig
{
    /// <summary>
    /// Provider type: Ollama, Groq, DeepSeek, OpenAI
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Model names for each provider
    /// </summary>
    public ModelConfig? Models { get; set; }

    /// <summary>
    /// Ollama specific configuration
    /// </summary>
    public OllamaConfig? Ollama { get; set; }

    /// <summary>
    /// Groq specific configuration
    /// </summary>
    public GroqConfig? Groq { get; set; }

    /// <summary>
    /// DeepSeek specific configuration
    /// </summary>
    public DeepSeekConfig? DeepSeek { get; set; }
}

/// <summary>
/// Model names configuration
/// </summary>
public class ModelConfig
{
    public string? Ollama { get; set; }
    public string? Groq { get; set; }
    public string? DeepSeek { get; set; }
}

/// <summary>
/// Ollama local server configuration
/// </summary>
public class OllamaConfig
{
    /// <summary>
    /// Base URL for Ollama API (default: http://localhost:11434)
    /// </summary>
    public string? BaseUrl { get; set; }
}

/// <summary>
/// Groq Cloud configuration
/// </summary>
public class GroqConfig
{
    /// <summary>
    /// Groq API Key from console.groq.com
    /// </summary>
    public string? ApiKey { get; set; }
}

/// <summary>
/// DeepSeek API configuration
/// </summary>
public class DeepSeekConfig
{
    /// <summary>
    /// DeepSeek API Key from platform.deepseek.com
    /// </summary>
    public string? ApiKey { get; set; }
}