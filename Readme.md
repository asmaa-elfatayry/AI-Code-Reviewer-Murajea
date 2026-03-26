# 🤖 AI Code Reviewer — Murajea

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-blue?style=flat-square&logo=dotnet)
![License](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)
![Version](https://img.shields.io/badge/Version-1.0.3-green?style=flat-square)
[![NuGet](https://img.shields.io/badge/NuGet-AICodeReviewer.Murajea-blue?style=flat-square)](https://www.nuget.org/packages/AICodeReviewer.Murajea)

**AI-powered code review tool for .NET developers**

</div>

---

## 📖 About

**Murajea** (مراجع – Arabic for "reviewer") is a powerful .NET Global Tool that uses Artificial Intelligence to automatically review your C# code and provide professional, actionable feedback. It helps you write better code by identifying issues, suggesting improvements, and following .NET best practices.

---

## ✨ Features

| Feature | Description |
|---|---|
| 🤖 **AI-Powered Review** | Instant feedback on your C# code using advanced AI models |
| 🎯 **Multiple AI Providers** | Choose from Ollama (local/free), Groq (fast cloud), or DeepSeek (powerful cloud) |
| 🌍 **Bilingual Support** | Full support for Arabic (ar) and English (en) |
| 📊 **Multiple Report Formats** | Export reviews as Markdown, HTML, or professional PDF |
| 📄 **Professional PDF Reports** | Beautiful reports with quality scores and color-coded issues |
| 📁 **Flexible Input** | Review single files, folders, or entire projects |
| 🚀 **Easy to Use** | Simple command-line interface |بق

---

## 🚀 Installation

### Install as a .NET Global Tool

```bash
dotnet tool install --global AICodeReviewer.Murajea
```

### Update to Latest Version

```bash
dotnet tool update --global AICodeReviewer.Murajea
```

### Uninstall

```bash
dotnet tool uninstall --global AICodeReviewer.Murajea
```

---

## 📖 Usage

### Basic Commands

```bash
# Review an entire project folder
ai-reviewer D:\MyProject\src

# Review a single C# file
ai-reviewer D:\MyProject\Services\UserService.cs

# Review the current directory
ai-reviewer .
```


## 📁 Where to Place `appsettings.json`

The tool looks for `appsettings.json` in the **current working directory** (where you run the `ai-reviewer` command).

### ✅ Correct Placement

If your project structure is:
C:\Users\YourName\source\repos\MyProject
├── Controllers
├── Models
├── Services
├── Program.cs
└── appsettings.json ← Place it here

text

Run the tool from the same folder:

```bash
cd C:\Users\YourName\source\repos\MyProject
ai-reviewer .
```

❌ Common Mistake
If you run the tool from a parent folder, it won't find the config file:

```bash
cd C:\Users\YourName\source\repos
ai-reviewer MyProject          // ❌ This will look for appsettings.json in repos folder
```

💡 Tip
To check where the tool is looking for the config file, look at the first line after running:


🔍 Looking for config at: C:\Users\...\appsettings.json
If the path is wrong, simply navigate to the correct folder and try again.

Note: You can also specify the full path to your project:

```bash
ai-reviewer C:\Users\YourName\source\repos\MyProject
```

The tool will still look for appsettings.json in your current directory, not in the target project folder.


### Example Output

```
🤖 AI Code Reviewer — Murajea
================================

📁 Tool Directory: C:\Users\...\.dotnet\tools\aicodereviewer.murajea
🌐 Language: العربية

📡 Using AI Provider: Groq
📁 Target: D:\MyProject\src

📄 Found 5 C# files

🔍 Reviewing: UserService.cs...
   Found 3 functions
   [1/3] CreateUser... ✅
   [2/3] GetUserById... ✅
   [3/3] UpdateUserEmailAsync... ✅

📄 Markdown report saved to: D:\MyProject\MurajeaReports\review_20260326_163822.md
📄 PDF report saved to: D:\MyProject\MurajeaReports\review_20260326_163822_UserService.pdf
📄 HTML report saved to: D:\MyProject\MurajeaReports\review_20260326_163822_UserService.html

✨ Review completed!
```

### Reports Location

Reports are saved in the `MurajeaReports/` folder inside your project directory:

```
D:\MyProject\MurajeaReports/
├── review_20260326_163822.md                    # Combined Markdown report
├── review_20260326_163822_UserService.pdf       # PDF per file
├── review_20260326_163822_UserService.html      # HTML per file
└── review_20260326_163822_OrderService.pdf
```

---

## ⚙️ Configuration

Create an `appsettings.json` file in the directory where you run the tool:

```json
{
  "Murajea": {
    "Language": "en",
    "AI": {
      "Provider": "groq",
      "Models": {
        "Ollama": "codellama:7b-instruct",
        "Groq": "llama-3.3-70b-versatile",
        "DeepSeek": "deepseek-chat"
      },
      "Ollama": {
        "BaseUrl": "http://localhost:11434"
      },
      "Groq": {
        "ApiKey": "YOUR_GROQ_API_KEY"
      },
      "DeepSeek": {
        "ApiKey": "YOUR_DEEPSEEK_API_KEY"
      }
    }
  },
  "OtherAppSettings": {
    "some": "value"
  }
}
```

### Configuration Options

| Setting | Description | Default |
|---|---|---|
| `Language` | Interface language (`ar` or `en`) | `en` |
| `AI.Provider` | AI provider (`ollama`, `groq`, `deepseek`) | `ollama` |
| `AI.Models.*` | Model name for each provider | Provider-specific |
| `AI.Ollama.BaseUrl` | Ollama API endpoint | `http://localhost:11434` |
| `AI.Groq.ApiKey` | Your Groq API key | Required for Groq |
| `AI.DeepSeek.ApiKey` | Your DeepSeek API key | Required for DeepSeek |

---

## 🤖 AI Providers Setup

### 🔹 Ollama (Local – Free & Private)

1. Install Ollama from [ollama.ai](https://ollama.ai)

2. Pull a model (recommended for code review):

```bash
ollama pull codellama:7b-instruct
```

3. Start the Ollama server:

```bash
ollama serve
```

4. Set the provider in `appsettings.json`:

```json
"AI": { "Provider": "ollama" }
```

### 🔹 Groq (Cloud – Fast & Free Tier)

1. Get your API key from [console.groq.com](https://console.groq.com)

2. Set the provider and API key in `appsettings.json`:

```json
"AI": {
  "Provider": "groq",
  "Groq": { "ApiKey": "your-api-key-here" }
}
```

> **Free tier limits:** 30 requests/minute, 1,000 requests/day

### 🔹 DeepSeek (Cloud – Powerful & Free Tier)

1. Get your API key from [platform.deepseek.com](https://platform.deepseek.com)

2. Set the provider and API key in `appsettings.json`:

```json
"AI": {
  "Provider": "deepseek",
  "DeepSeek": { "ApiKey": "your-api-key-here" }
}
```

> **Free tier:** 500,000 tokens for new users

---

## 📊 Understanding the Report

### Executive Summary

| Metric | Description |
|---|---|
| **Total Issues** | Overall number of issues found |
| **Critical** | High-priority issues that need immediate attention |
| **Warnings** | Potential problems that should be addressed |
| **Functions** | Number of functions reviewed |

### Code Quality Score

A score from **0–100** based on:

- Each issue reduces score by 5 points
- Critical issues reduce score by an additional 10 points

### Issue Categories

| Severity | Description |
|---|---|
| 🔴 **Critical** | Security vulnerabilities, potential crashes, data loss |
| ⚠️ **Warning** | Performance issues, code smells, best practice violations |
| ℹ️ **Info** | Suggestions for improvement, minor optimizations |

---

## 🛠️ Requirements

- .NET 8.0 SDK or later

### For Ollama Provider

- Ollama installed
- At least 8GB RAM (16GB recommended)

### For Cloud Providers

- Internet connection
- API key from the respective provider

---

## 📁 Project Structure

```
AICodeReviewer/
├── AICodeReviewer.Console/           # Main application
│   ├── Program.cs                    # Entry point
│   ├── Services/                     # Report generators
│   └── Configuration/                # Configuration classes
│
├── AICodeReviewer.Core/              # Core logic
│   ├── AI/                           # AI providers
│   │   ├── IAIProvider.cs
│   │   ├── AIReviewerFactory.cs
│   │   └── Providers/
│   │       ├── OllamaProvider.cs
│   │       ├── GroqProvider.cs
│   │       └── DeepSeekProvider.cs
│   ├── Models/                       # Data models
│   ├── Interfaces/                   # Interfaces
│   └── Services/                     # Core services
│
├── README.md
├── LICENSE
└── AICodeReviewer.sln
```

---

## 🤝 Contributing

Contributions are welcome!

1. **Fork** the repository
2. **Create** a feature branch:

```bash
git checkout -b feature/amazing-feature
```

3. **Commit** your changes:

```bash
git commit -m 'Add some amazing feature'
```

4. **Push** to your branch:

```bash
git push origin feature/amazing-feature
```

5. **Open a Pull Request**

---

## 📝 License

This project is licensed under the **MIT License** – see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- [Ollama](https://ollama.ai) – for local LLM inference
- [Groq](https://groq.com) – for fast cloud inference
- [DeepSeek](https://deepseek.com) – for powerful code models
- [QuestPDF](https://www.questpdf.com) – for PDF generation
- [Microsoft.CodeAnalysis](https://github.com/dotnet/roslyn) – for C# code analysis

---

## 📧 Contact & Support

- **GitHub Issues:** [Report a bug or request a feature](https://github.com/asmaa-elfatayry/AI-Code-Reviewer-Murajea/issues)
- **Email:** asmaa.elfatayry@gmail.com

---


Made with curiosity. Driven by care. Given freely ✨

