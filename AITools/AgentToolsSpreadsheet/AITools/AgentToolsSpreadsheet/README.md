# AgentToolsSpreadsheet

## Configuration

This application requires Azure OpenAI credentials to run. **Do not commit actual API keys to the repository.**

### Setting up Environment Variables

You have two options:

#### Option 1: User Secrets (Recommended for Development)
```bash
dotnet user-secrets set "AZUREOPENAI_KEY" "your-actual-key"
dotnet user-secrets set "AZUREEMBEDDINGOPENAI_KEY" "your-actual-embedding-key"
```

#### Option 2: Environment Variables
Set the following environment variables in your system or in Visual Studio:
- `AZUREOPENAI_KEY` - Your Azure OpenAI API key
- `AZUREOPENAI_ENDPOINT` - Your Azure OpenAI endpoint
- `AZUREEMBEDDINGOPENAI_KEY` - Your Azure Embedding OpenAI API key
- `AZUREEMBEDDINGOPENAI_ENDPOINT` - Your Azure Embedding OpenAI endpoint
- `AZUREEMBEDDINGOPENAI_DEPLOYMENT` - Deployment name
- `AZUREEMBEDDINGOPENAI_APIVERSION` - API version

#### Option 3: Update launchSettings.json locally
You can update `Properties/launchSettings.json` with your actual keys for local development, but **do not commit these changes**.
