# Configuration Guide

This document provides detailed information about configuring the Azure PDF Merge solution.

## Table of Contents

- [MergeFunctionApp Configuration](#mergefunctionapp-configuration)
- [MergeApp Configuration](#mergeapp-configuration)
- [Environment-Specific Settings](#environment-specific-settings)
- [Performance Tuning](#performance-tuning)
- [Security Configuration](#security-configuration)

## MergeFunctionApp Configuration

### host.json

The `host.json` file configures global settings for the Azure Functions host.

```json
{
    "version": "2.0",
    "logging": {
        "applicationInsights": {
            "samplingSettings": {
                "isEnabled": true,
                "excludedTypes": "Request"
      },
    "enableLiveMetricsFilters": true
        }
    }
}
```

#### Key Settings

| Setting                          | Value     | Description                                      |
|----------------------------------|-----------|--------------------------------------------------|
| `version`                        | `2.0`     | Azure Functions runtime version                  |
| `samplingSettings.isEnabled`     | `true`    | Enable telemetry sampling to reduce costs        |
| `samplingSettings.excludedTypes` | `Request` | Don't sample HTTP request telemetry              |
| `enableLiveMetricsFilters`       | `true`    | Enable real-time metrics in Application Insights |

#### Optional Configurations

**Increase Function Timeout (Premium/Dedicated plans)**
```json
{
    "functionTimeout": "00:10:00"  // 10 minutes
}
```

**Configure Logging Levels**
```json
{
    "logging": {
        "logLevel": {
          "default": "Information",
 "Function.MergeFunction": "Debug",
   "Host": "Warning"
        }
    }
}
```

**Enable CORS for Local Development**
```json
{
 "extensions": {
"http": {
    "routePrefix": "api",
        "cors": {
         "allowedOrigins": ["http://localhost:3000", "http://localhost:8080"]
       }
        }
    }
}
```

### local.settings.json

Local development settings (not deployed to Azure).

```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "",
      "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
    }
}
```

#### Required Settings

| Setting                    | Value                      | Description                        |
|----------------------------|----------------------------|------------------------------------|
| `FUNCTIONS_WORKER_RUNTIME` | `dotnet-isolated`          | Use .NET isolated worker process   |
| `AzureWebJobsStorage`      | Connection string or empty | Storage account for function state |

#### Optional Settings

**Application Insights**
```json
{
    "Values": {
        "APPLICATIONINSIGHTS_CONNECTION_STRING": "InstrumentationKey=xxx;IngestionEndpoint=https://xxx"
    }
}
```

**Custom Settings**
```json
{
    "Values": {
        "MaxFileSizeMB": "100",
        "MergeTimeoutSeconds": "20",
        "EnableDetailedLogging": "true"
    }
}
```

**Telerik License**
```json
{
    "Values": {
        "TELERIK_LICENSE_KEY": "your-license-key-here"
    }
}
```

### Azure Application Settings

When deployed to Azure, configure these in the Azure Portal under **Configuration** → **Application Settings**:

```bash
# Using Azure CLI
az functionapp config appsettings set \
  --name func-pdf-merge \
  --resource-group rg-pdf-merge \
  --settings \
    "APPLICATIONINSIGHTS_CONNECTION_STRING=xxx" \
    "TELERIK_LICENSE_KEY=xxx" \
    "MaxFileSizeMB=100"
```

### Function Authorization Levels

Configure in `MergeFunction.cs`:

```csharp
// Anonymous - No authentication required
[HttpTrigger(AuthorizationLevel.Anonymous, "post")]

// Function - Requires function key
[HttpTrigger(AuthorizationLevel.Function, "post")]

// Admin - Requires admin key
[HttpTrigger(AuthorizationLevel.Admin, "post")]
```

**Recommended**: Use `Function` level for production and manage keys securely.

## MergeApp Configuration

### Function URL

Update the target URL in `Program.cs`:

```csharp
// Local development
string functionUrl = "http://localhost:7071/api/MergeFunction";

// Azure deployment
string functionUrl = "https://func-pdf-merge.azurewebsites.net/api/MergeFunction";

// With function key
string functionUrl = "https://func-pdf-merge.azurewebsites.net/api/MergeFunction?code=xxx";
```

### PDF File Paths

Configure which PDFs to merge:

```csharp
// Relative paths from application directory
string[] pdfFilePaths = { 
    "Resources/file1.pdf", 
    "Resources/file2.pdf" 
};

// Absolute paths
string[] pdfFilePaths = { 
    @"C:\Documents\file1.pdf",
    @"C:\Documents\file2.pdf"
};

// User input
Console.Write("Enter PDF paths (comma-separated): ");
string[] pdfFilePaths = Console.ReadLine()?.Split(',') ?? Array.Empty<string>();
```

### HTTP Client Timeout

Adjust timeout for large files:

```csharp
using HttpClient httpClient = new HttpClient();
httpClient.Timeout = TimeSpan.FromMinutes(5); // 5 minute timeout
```

### Output Configuration

```csharp
// Change output filename
string outputPath = "merged_output.pdf";

// Add timestamp to filename
string outputPath = $"merged_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

// Save to specific directory
string outputPath = Path.Combine(@"C:\Output", "merged.pdf");
```

## Environment-Specific Settings

### Development Environment

**local.settings.json**
```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
        "EnableDetailedLogging": "true"
    }
}
```

**host.json**
```json
{
    "logging": {
        "logLevel": {
            "default": "Debug"
        }
    }
}
```

### Staging Environment

**Azure Application Settings**
```bash
az functionapp config appsettings set \
  --name func-pdf-merge-staging \
  --resource-group rg-pdf-merge-staging \
  --settings \
    "Environment=Staging" \
    "EnableDetailedLogging=true"
```

### Production Environment

**Azure Application Settings**
```bash
az functionapp config appsettings set \
  --name func-pdf-merge-prod \
  --resource-group rg-pdf-merge-prod \
  --settings \
    "Environment=Production" \
    "EnableDetailedLogging=false" \
    "TELERIK_LICENSE_KEY=xxx"
```

**Enable Authentication**
```bash
az functionapp auth update \
  --name func-pdf-merge-prod \
  --resource-group rg-pdf-merge-prod \
  --enabled true \
  --action LoginWithAzureActiveDirectory
```

## Performance Tuning

### Timeout Configuration

**In MergeFunction.cs**
```csharp
// Increase PDF processing timeout
provider.Export(result, outputStream, TimeSpan.FromSeconds(60)); // 60 seconds
provider.Import(file.Data, TimeSpan.FromSeconds(60));
```

**In host.json** (Premium/Dedicated plans only)
```json
{
    "functionTimeout": "00:10:00"  // 10 minutes max
}
```

### Memory Optimization

**Use streaming for large files**
```csharp
// Instead of loading entire file into memory
byte[] fileBytes = File.ReadAllBytes(filePath);

// Use streams
using var fileStream = File.OpenRead(filePath);
```

### Concurrency Settings

**host.json**
```json
{
    "extensions": {
    "http": {
            "maxConcurrentRequests": 100,
            "maxOutstandingRequests": 200
        }
    }
}
```

### Application Insights Sampling

**Reduce costs with sampling**
```json
{
    "logging": {
        "applicationInsights": {
        "samplingSettings": {
    "isEnabled": true,
    "maxTelemetryItemsPerSecond": 5,
             "excludedTypes": "Request;Exception"  // Don't sample critical data
            }
    }
    }
}
```

## Security Configuration

### Enable HTTPS Only

**Azure Portal** or **Azure CLI**
```bash
az functionapp update \
  --name func-pdf-merge \
  --resource-group rg-pdf-merge \
  --set httpsOnly=true
```

### Configure CORS

**host.json** (local development)
```json
{
    "extensions": {
        "http": {
            "cors": {
        "allowedOrigins": [
             "https://yourdomain.com",
            "https://app.yourdomain.com"
  ],
      "supportCredentials": true
          }
        }
    }
}
```

**Azure Portal**
- Navigate to **CORS** settings
- Add allowed origins
- Save changes

### API Key Management

**Get function keys**
```bash
az functionapp keys list \
  --name func-pdf-merge \
  --resource-group rg-pdf-merge
```

**Rotate keys regularly**
```bash
az functionapp keys set \
  --name func-pdf-merge \
  --resource-group rg-pdf-merge \
  --key-name default \
  --key-value new-key-value
```

### Use Managed Identity

**Enable system-assigned identity**
```bash
az functionapp identity assign \
  --name func-pdf-merge \
  --resource-group rg-pdf-merge
```

**Access Azure Key Vault**
```csharp
// In Program.cs
builder.ConfigureAppConfiguration((context, config) =>
{
    var keyVaultEndpoint = Environment.GetEnvironmentVariable("KEY_VAULT_ENDPOINT");
    config.AddAzureKeyVault(new Uri(keyVaultEndpoint), new DefaultAzureCredential());
});
```

### Input Validation

**Add to MergeFunction.cs**
```csharp
// Validate file types
foreach (var file in files)
{
    if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
    {
        return req.CreateResponse(HttpStatusCode.BadRequest);
 }
}

// Validate file sizes
const long maxFileSizeBytes = 100 * 1024 * 1024; // 100 MB
if (file.Data.Length > maxFileSizeBytes)
{
    return req.CreateResponse(HttpStatusCode.RequestEntityTooLarge);
}
```

## Monitoring Configuration

### Application Insights

**Configure connection string**
```json
{
    "Values": {
  "APPLICATIONINSIGHTS_CONNECTION_STRING": "InstrumentationKey=xxx;IngestionEndpoint=https://xxx.in.applicationinsights.azure.com/;LiveEndpoint=https://xxx.livediagnostics.monitor.azure.com/"
    }
}
```

**Custom telemetry**
```csharp
private readonly TelemetryClient _telemetryClient;

public MergeFunction(TelemetryClient telemetryClient)
{
    _telemetryClient = telemetryClient;
}

public async Task<HttpResponseData> Run(HttpRequestData req)
{
    _telemetryClient.TrackEvent("MergeStarted", new Dictionary<string, string>
    {
     ["FileCount"] = files.Count.ToString()
    });
}
```

### Health Checks

**Add health endpoint**
```csharp
[Function("Health")]
public HttpResponseData HealthCheck([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
{
    var response = req.CreateResponse(HttpStatusCode.OK);
    response.WriteString("Healthy");
    return response;
}
```

## Troubleshooting

### Enable Verbose Logging

**host.json**
```json
{
    "logging": {
   "logLevel": {
        "default": "Trace",
        "Host": "Trace",
        "Function": "Trace"
  }
    }
}
```

### Debug Local Settings

```bash
# View current settings
func settings list

# Add setting
func settings add MySetting MyValue

# Remove setting
func settings remove MySetting
```

### View Configuration in Azure

```bash
# List all app settings
az functionapp config appsettings list \
  --name func-pdf-merge \
  --resource-group rg-pdf-merge
```

## Best Practices

1. **Never commit secrets** - Use Azure Key Vault or environment variables
2. **Use managed identities** - Avoid storing credentials
3. **Enable Application Insights** - Monitor performance and errors
4. **Set appropriate timeouts** - Based on expected file sizes
5. **Configure CORS properly** - Only allow necessary origins
6. **Use function-level auth** - Protect production endpoints
7. **Implement rate limiting** - Prevent abuse
8. **Regular key rotation** - Enhance security
9. **Monitor costs** - Use sampling to control telemetry costs
10. **Test configuration changes** - Always test in staging first

## Additional Resources

- [Azure Functions Configuration Reference](https://docs.microsoft.com/azure/azure-functions/functions-host-json)
- [Application Settings in Azure Functions](https://docs.microsoft.com/azure/azure-functions/functions-app-settings)
- [Application Insights Configuration](https://docs.microsoft.com/azure/azure-monitor/app/configuration-with-applicationinsights-config)
- [Azure Functions Security](https://docs.microsoft.com/azure/azure-functions/security-concepts)
