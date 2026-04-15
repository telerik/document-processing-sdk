# Agent Tools in PdfViewer

A WPF application that integrates AI-powered PDF analysis using Azure OpenAI and Telerik UI controls.

## Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or later
- Azure OpenAI account with API access

### Configuration

Before running the project for the first time, you need to configure your Azure OpenAI credentials:

1. Open `Properties/launchSettings.json`
2. Replace the placeholder values with your actual Azure OpenAI credentials:

```json
{
  "profiles": {
    "AgentToolsInPdfViewerWPF": {
      "commandName": "Project",
      "environmentVariables": {
        "AZUREOPENAI_KEY": "your-key-here",
        "AZUREOPENAI_ENDPOINT": "https://your-endpoint-here.openai.azure.com/",
        "AZUREOPENAI_MODEL": "gpt-4.1-mini"
      }
    }
  }
}
```