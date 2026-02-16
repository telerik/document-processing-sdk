# PerUserIsolatedStorage

This example demonstrates how to implement **per-user isolated document storage** using the Telerik Document Processing AI Agent Tools in a multi-user ASP.NET Core Web API.

## What This Example Demonstrates

In multi-user scenarios, each user must have their own isolated document repositories to prevent data leakage between users. This example shows:

- **Per-User Isolation**: Each authenticated user gets their own `WorkbookRepository` and `PdfRepository` instances
- **Thread-Safe Session Management**: Uses `ConcurrentDictionary` to safely manage user sessions across concurrent requests
- **Automatic Cleanup**: Background service removes stale sessions after 2 hours of inactivity
- **JWT Authentication**: Users are identified via JWT tokens, and their user ID determines which repository they access

### How Isolation Works

```
User A (token contains userId: "alice")
    └── Session A
        ├── WorkbookRepository A (Alice's spreadsheets)
        └── PdfRepository A (Alice's PDFs)

User B (token contains userId: "bob")  
    └── Session B
        ├── WorkbookRepository B (Bob's spreadsheets)
        └── PdfRepository B (Bob's PDFs)
```

Each user can only access their own documents. The AI tools operate on the user's specific repositories.

---

## Prerequisites

1. **.NET 10 SDK** installed
2. **Azure OpenAI** resource with a deployed model
3. **Telerik NuGet Feed** configured (for Document Processing packages)

---

## Configuration

### Step 1: Add Your Azure OpenAI Credentials

Open `appsettings.json` and update the `AzureOpenAI` section:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://YOUR-RESOURCE-NAME.openai.azure.com",
    "ApiKey": "YOUR_AZURE_OPENAI_API_KEY",
    "DeploymentName": "gpt-4o"
  }
}
```

**Where to find these values in Azure Portal:**
- **Endpoint**: Azure Portal → Your OpenAI resource → Overview → "Endpoint"
- **ApiKey**: Azure Portal → Your OpenAI resource → Keys and Endpoint → "KEY 1" or "KEY 2"
- **DeploymentName**: Azure Portal → Your OpenAI resource → Model deployments → The name of your deployed model

### Step 2: (Optional) JWT Configuration

The JWT settings in `appsettings.json` are pre-configured for testing. In production, use a secure key stored in a vault:

```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyForTestingPurposes12345!",
    "Issuer": "PerUserIsolatedStorage",
    "Audience": "PerUserIsolatedStorage"
  }
}
```

---

## Running the API

```powershell
cd "c:\your\location\PerUserIsolatedStorage"
dotnet run
```

The API will start and display the URL (typically `http://localhost:5000` or similar).

---

## Testing with PowerShell

### Complete Test Script

Copy and paste this entire script into PowerShell:

```powershell
# ============================================
# Configuration - Update the port if needed
# ============================================
$baseUrl = "http://localhost:5000"

# ============================================
# Step 1: Get a JWT Token
# ============================================
Write-Host "Getting JWT token..." -ForegroundColor Cyan
$tokenResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/token" -Method Post -Body '{"userId": "user123", "username": "TestUser"}' -ContentType "application/json"
$token = $tokenResponse.token
Write-Host "Token received. Expires at: $($tokenResponse.expiresAt)" -ForegroundColor Green

# ============================================
# Step 2: Set up headers for authenticated requests
# ============================================
$headers = @{ "Authorization" = "Bearer $token" }

# ============================================
# Step 3: Chat with the AI (using document tools)
# ============================================
Write-Host "`nSending chat message..." -ForegroundColor Cyan
$chatBody = @{ "message" = "Create a spreadsheet with columns: Name, Department, Salary. Add 3 sample employees." } | ConvertTo-Json

$chatResponse = Invoke-RestMethod -Uri "$baseUrl/api/documentchat/chat" -Method Post -Headers $headers -Body $chatBody -ContentType "application/json"
Write-Host "AI Response:" -ForegroundColor Green
Write-Host $chatResponse.message

# ============================================
# Step 4: List documents in user's repository
# ============================================
Write-Host "`nListing user documents..." -ForegroundColor Cyan
$documents = Invoke-RestMethod -Uri "$baseUrl/api/documentchat/documents" -Method Get -Headers $headers
Write-Host "Spreadsheets: $($documents.spreadsheets -join ', ')"
Write-Host "PDFs: $($documents.pdfs -join ', ')"

# ============================================
# Step 5: (Optional) Clear all documents
# ============================================
# Invoke-RestMethod -Uri "$baseUrl/api/documentchat/documents" -Method Delete -Headers $headers

# ============================================
# Step 6: (Optional) Logout and cleanup
# ============================================
# Invoke-RestMethod -Uri "$baseUrl/api/documentchat/logout" -Method Post -Headers $headers
```

### Individual Commands

**Get a token:**
```powershell
$token = (Invoke-RestMethod -Uri "http://localhost:5000/api/auth/token" -Method Post -Body '{"userId": "user123"}' -ContentType "application/json").token
```

**Chat with documents:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/documentchat/chat" -Method Post -Headers @{"Authorization"="Bearer $token"} -Body '{"message":"Hello, what can you do?"}' -ContentType "application/json"
```

**List documents:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/documentchat/documents" -Method Get -Headers @{"Authorization"="Bearer $token"}
```

**Clear documents:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/documentchat/documents" -Method Delete -Headers @{"Authorization"="Bearer $token"}
```

**Logout:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/documentchat/logout" -Method Post -Headers @{"Authorization"="Bearer $token"}
```

---

## Learn More

For more information about the Telerik Document Processing AI Agent Tools, see:
https://docs.telerik.com/devtools/document-processing/ai-tools/agent-tools/multi-user-scenario
