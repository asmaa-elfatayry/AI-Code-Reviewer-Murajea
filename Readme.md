# 🤖 AI Code Reviewer — Murajea (Powered by AI)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](http://makeapullrequest.com)

**AI Code Reviewer** is a powerful .NET tool that uses Artificial Intelligence to automatically review your C# code and provide professional, actionable feedback. Whether you're working on a small project or a large enterprise application, Murajea helps you write better code by identifying issues, suggesting improvements, and following .NET best practices.

---

## ✨ Features

- 🔍 **Automated Code Review** – Get instant, AI-powered feedback on your C# code
- 📊 **Multiple Report Formats** – Export reviews as Markdown, HTML, or professional PDF
- 🎯 **Multiple AI Providers** – Choose the best option for your needs:
  - **Ollama** – Local, free, and private (runs on your machine)
  - **Groq** – Cloud-based, extremely fast, with a generous free tier
  - **DeepSeek** – Powerful cloud model with excellent code analysis
- 📁 **Flexible Input** – Review single files, entire folders, or complete projects
- 📄 **Professional PDF Reports** – Beautifully formatted reports with code quality scores, issue categorization, and detailed suggestions
- 🌍 **Arabic & English Support** – Reports and interface fully support Arabic text
- 🚀 **Easy to Use** – Simple command-line interface or right-click integration with VS Code

---

## 🚀 Installation

### Option 1: Install as a .NET Global Tool (Recommended)

```bash
dotnet tool install --global AICodeReviewer
```

After installation, you can run the tool from anywhere using:

```bash
ai-reviewer <path-to-code>
```

### Option 2: Build from Source

```bash
# Clone the repository
git clone https://github.com/asmaa-elfatayry/AI-Code-Reviewer-Murajea
cd AICodeReviewer

# Build the project
dotnet build

# Run the tool
dotnet run --project AICodeReviewer.Console -- <path-to-code>
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

### Example Output

```
🤖 AI Code Reviewer — Murajea
================================

📡 Using AI Provider: Groq
📁 Target: D:\MyProject\src

📄 Found 5 C# files

🔍 Reviewing: UserService.cs...
   Found 3 functions
   [1/3] CreateUser... ✅
   [2/3] GetUserById... ✅
   [3/3] UpdateUserEmailAsync... ✅

📄 Markdown report saved to: Reports/review_20260326_023748.md
📄 PDF report saved to: Reports/review_20260326_023748_UserService.pdf
📄 HTML report saved to: Reports/review_20260326_023748_UserService.html

✨ Review completed!
```

### Reports Location

All reports are saved in the `Reports/` folder inside the directory where you run the tool:

```
Reports/
├── review_20260326_023748.md                    # Combined Markdown report
├── review_20260326_023748_UserService.pdf       # PDF per file
├── review_20260326_023748_UserService.html      # HTML per file
└── review_20260326_023748_OrderService.pdf
```

---

## ⚙️ Configuration

Create an `appsettings.json` file in the same directory where you run the tool:

```json
{
  "AI": {
    "Provider": "ollama",
    "Models": {
      "Ollama": "codellama:7b-instruct",
      "Groq": "llama-3.3-70b-versatile",
      "DeepSeek": "deepseek-chat"
    },
    "Ollama": {
      "BaseUrl": "http://localhost:11434"
    },
    "Groq": {
      "ApiKey": "YOUR_GROQ_API_KEY_HERE"
    },
    "DeepSeek": {
      "ApiKey": "YOUR_DEEPSEEK_API_KEY_HERE"
    }
  }
}
```

### Configuration Options

| Setting | Description | Default |
|---|---|---|
| `AI.Provider` | AI provider to use (`ollama`, `groq`, `deepseek`) | `ollama` |
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

### Executive Summary Cards

| Card | Description |
|---|---|
| **Total Issues** | Overall number of issues found |
| **Critical** | High-priority issues that need immediate attention |
| **Warnings** | Potential problems that should be addressed |
| **Functions** | Number of functions reviewed |

### Code Quality Score

A score from **0–100** based on:

- Number of issues (each issue reduces score by 5 points)
- Critical issues (each critical reduces score by an additional 10 points)

### Issue Categories

| Level | Description |
|---|---|
| 🔴 **Critical** | Security vulnerabilities, potential crashes, data loss |
| ⚠️ **Warning** | Performance issues, code smells, best practice violations |
| ℹ️ **Info** | Suggestions for improvement, minor optimizations |

### Suggestions

Each function includes actionable suggestions for improvement with code examples.

---

## 🛠️ Requirements

### For Running the Tool

- .NET 8.0 SDK or later

### For Ollama Provider

- Ollama installed
- At least 8GB RAM (16GB recommended for larger models)

### For Cloud Providers

- Internet connection
- API key from the respective provider

---

## 📁 Project Structure

```
AICodeReviewer/
├── AICodeReviewer.Console/           # Main application
│   ├── Program.cs                    # Entry point
│   ├── appsettings.json              # Configuration
│   ├── Services/                     # Report generators
│   │   └── PdfReportGenerator.cs
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
├── samples/                          # Example projects (for testing)
├── README.md                         # This file
├── LICENSE                           # MIT License
└── AICodeReviewer.sln                # Solution file
```

---

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

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

### Development Setup

```bash
# Clone your fork
git clone https://github.com/asmaa-elfatayry/AI-Code-Reviewer-Murajea
cd AICodeReviewer

# Build and test
dotnet build
dotnet test

# Run with sample
dotnet run --project AICodeReviewer.Console -- samples/SampleProject
```

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
- **Email:** your.email@example.com

---

## ⭐ Show Your Support

If you find this tool useful, please give it a **star ⭐** on GitHub!  
It helps others discover the project and motivates continued development.

---

Made with curiosity to help others ✨