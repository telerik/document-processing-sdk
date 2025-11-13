# Azure PDF Merge Solution

A complete serverless solution for merging PDF documents using Azure Functions and Telerik Document Processing. This solution demonstrates cloud-native PDF processing with a test client for easy validation.

## 🎯 Overview

This solution consists of two projects that work together to provide PDF merging capabilities:

1. **MergeFunctionApp** - Azure Function that performs the PDF merging
2. **MergeApp** - Console test client that demonstrates how to use the function

The solution showcases best practices for:
- Serverless PDF processing with Azure Functions
- HTTP multipart file uploads
- Telerik Document Processing integration
- .NET 8.0 isolated worker model
- Cloud-native application architecture

## 📦 Solution Structure

```
.
├── MergeFunctionApp/          # Azure Function application
│   ├── MergeFunction.cs       # HTTP-triggered function implementation
│   ├── Program.cs     # Function host configuration
│   ├── host.json              # Function app settings
│   ├── local.settings.json    # Local development settings
│   ├── MergeFunctionApp.csproj
│   └── README.md   # Detailed function documentation
│
├── MergeApp/           # Test client application
│   ├── Program.cs             # Client implementation
│   ├── Resources/             # Sample PDF files
│   │   ├── file1.pdf
│   │ └── file2.pdf
│   ├── MergeApp.csproj
│   └── README.md     # Client documentation
│
├── CONFIGURATION.md           # Configuration guide
└── README.md      # This file
```

## ✨ Features

### MergeFunctionApp
- ✅ HTTP-triggered Azure Function (isolated worker model)
- ✅ Multipart form data file upload support
- ✅ PDF merging using Telerik Document Processing
- ✅ Anonymous authentication (configurable)
- ✅ Application Insights integration
- ✅ Timeout protection (20 seconds default)
- ✅ Proper error handling and logging

### MergeApp
- ✅ Simple console-based test client
- ✅ Demonstrates proper HTTP multipart upload
- ✅ Includes sample PDF files
- ✅ Auto-opens merged result
- ✅ Comprehensive error handling

## 🚀 Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools](https://docs.microsoft.com/azure/azure-functions/functions-run-local) v4
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- Valid Telerik Document Processing license (for production)

### 1. Clone and Restore

```bash
# Navigate to solution directory
cd CloudIntegration/Azure/MergeFunction

# Restore all dependencies
dotnet restore
```

### 2. Run the Azure Function

```bash
cd MergeFunctionApp
func start
# or
dotnet run
```

The function will start at `http://localhost:7071/api/MergeFunction` (port may vary).

### 3. Test with the Client

Open a new terminal:

```bash
cd MergeApp
dotnet run
```

The client will:
1. Send sample PDFs to the function
2. Save the merged result as `merged_output.pdf`
3. Open the merged PDF in your default viewer

## 📖 Detailed Documentation

Each project includes comprehensive documentation:

- **[MergeFunctionApp README](MergeFunctionApp/README.md)** - Complete Azure Function guide
  - Local development setup
  - API reference
  - Deployment instructions
  - Configuration options
  - Troubleshooting guide

- **[MergeApp README](MergeApp/README.md)** - Test client documentation
  - Usage instructions
  - Configuration options
- Code examples
  - Error handling

- **[CONFIGURATION.md](CONFIGURATION.md)** - Configuration guide
  - host.json settings
  - local.settings.json
  - Azure application settings
  - Performance tuning
  - Security configuration

## 🔧 Technology Stack

### Backend (MergeFunctionApp)
- **.NET 8.0** - Latest LTS framework
- **Azure Functions v4** - Serverless compute platform
- **Telerik Document Processing** - Enterprise PDF manipulation
- **HttpMultipartParser** - Efficient file upload parsing
- **Application Insights** - Monitoring and telemetry

### Client (MergeApp)
- **.NET 8.0** - Console application
- **HttpClient** - HTTP communication
- Built-in .NET libraries only

## 🌐 API Reference

### Endpoint
```
POST /api/MergeFunction
```

### Request
- **Content-Type**: `multipart/form-data`
- **Body**: Multiple PDF files with field name `files`

### Response
- **Status**: `200 OK`
- **Content-Type**: `application/pdf`
- **Body**: Merged PDF binary data

### Example cURL
```bash
curl -X POST http://localhost:7071/api/MergeFunction \
  -F "files=@file1.pdf" \
  -F "files=@file2.pdf" \
  -o merged.pdf
```

### Example PowerShell
```powershell
$uri = "http://localhost:7071/api/MergeFunction"
$form = @{
    files = Get-Item "file1.pdf", "file2.pdf"
}
Invoke-RestMethod -Uri $uri -Method Post -Form $form -OutFile "merged.pdf"
```

### Example C#
```csharp
using var content = new MultipartFormDataContent();
content.Add(new ByteArrayContent(pdf1Bytes), "files", "file1.pdf");
content.Add(new ByteArrayContent(pdf2Bytes), "files", "file2.pdf");

var response = await httpClient.PostAsync(functionUrl, content);
var mergedPdf = await response.Content.ReadAsByteArrayAsync();
```

## 🚢 Deployment

### Local Development
1. Run `func start` in MergeFunctionApp directory
2. Function available at `http://localhost:7071`

### Deploy to Azure
```bash
# Login to Azure
az login

# Create resource group (if needed)
az group create --name rg-pdf-merge --location eastus

# Create storage account
az storage account create \
  --name stpdfmerge \
  --resource-group rg-pdf-merge \
  --location eastus

# Create function app
az functionapp create \
  --resource-group rg-pdf-merge \
  --consumption-plan-location eastus \
  --runtime dotnet-isolated \
  --functions-version 4 \
  --name func-pdf-merge \
  --storage-account stpdfmerge

# Deploy the function
cd MergeFunctionApp
func azure functionapp publish func-pdf-merge
```

### Post-Deployment Configuration

In Azure Portal, configure:
- **Authentication** - Enable if needed
- **CORS** - Add allowed origins
- **Application Settings** - Add environment variables
- **Monitoring** - Configure Application Insights alerts

## ⚡ Performance Considerations

| Aspect | Default | Recommendation |
|--------|---------|----------------|
| Timeout | 20 seconds | Adjust based on file sizes |
| Max File Size | Function plan limit | Use Premium plan for larger files |
| Concurrency | Auto-scaled | Monitor and adjust based on load |
| Memory | 1536 MB (Consumption) | Upgrade to Premium for large PDFs |
| Cold Start | ~2-5 seconds | Use Premium plan for instant response |

## 🔒 Security Best Practices

### Authentication
```csharp
// Update to Function or System level for production
[Function("MergeFunction")]
[HttpTrigger(AuthorizationLevel.Function, "post")]
public async Task<HttpResponseData> Run(...)
```

### Input Validation
- Validate file types (PDF only)
- Limit file sizes
- Sanitize file names
- Implement rate limiting

### Secrets Management
- Store sensitive data in Azure Key Vault
- Use managed identities
- Never commit secrets to source control

## 🧪 Testing

### Unit Tests
```bash
# Add test project
dotnet new xunit -n MergeFunctionApp.Tests
dotnet add reference ../MergeFunctionApp
```

### Integration Tests
- Use Azure Functions local emulator
- Test with various PDF sizes and formats
- Validate error scenarios

### Load Testing
- Use Azure Load Testing service
- Simulate concurrent requests
- Monitor function performance metrics

## 📊 Monitoring

### Application Insights Queries

**Function Execution Time**
```kusto
requests
| where name == "MergeFunction"
| summarize avg(duration), max(duration) by bin(timestamp, 5m)
```

**Error Rate**
```kusto
requests
| where name == "MergeFunction"
| summarize errorRate = countif(success == false) * 100.0 / count() by bin(timestamp, 1h)
```

**Request Volume**
```kusto
requests
| where name == "MergeFunction"
| summarize count() by bin(timestamp, 1h)
| render timechart
```

## 🛠️ Troubleshooting

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Connection refused | Function not running | Start function with `func start` |
| 500 error | Invalid PDF | Check Application Insights logs |
| Timeout | Large files | Increase timeout or optimize |
| License error | Telerik license missing | Configure valid license |
| Cold start delay | Consumption plan | Consider Premium plan |

### Debug Mode

Enable verbose logging in `host.json`:
```json
{
  "logging": {
    "logLevel": {
      "default": "Debug"
    }
  }
}
```

## 📚 Additional Resources

### Documentation
- [MergeFunctionApp Detailed Guide](MergeFunctionApp/README.md)
- [MergeApp Usage Guide](MergeApp/README.md)
- [Configuration Guide](CONFIGURATION.md)
- [Azure Functions Documentation](https://docs.microsoft.com/azure/azure-functions/)
- [Telerik Document Processing](https://docs.telerik.com/devtools/document-processing/)

### Learning Resources
- [Azure Functions Best Practices](https://docs.microsoft.com/azure/azure-functions/functions-best-practices)
- [Serverless Architecture Patterns](https://docs.microsoft.com/azure/architecture/serverless/)
- [PDF Processing with .NET](https://docs.telerik.com/devtools/document-processing/libraries/radpdfprocessing/overview)
- [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing)

## 🤝 Contributing

This solution is part of the Telerik Document Processing SDK samples. For contributions:

1. Follow the existing code style
2. Add XML documentation comments
3. Update relevant README files
4. Test thoroughly before submitting

## 📄 License

This solution uses Telerik Document Processing, which requires a valid license for production use. The sample code is provided as-is for demonstration purposes. Check the [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing).

## 💡 Use Cases

This solution can be adapted for:
- **Document Management Systems** - Merge uploaded documents
- **Report Generation** - Combine multiple reports
- **Invoice Processing** - Consolidate billing documents
- **Legal Documents** - Merge contracts and agreements
- **Educational Materials** - Combine course materials
- **Healthcare Records** - Merge patient documents

## 🔄 Future Enhancements

Potential improvements:
- Add support for other document formats (Word, Excel)
- Implement document splitting functionality
- Add watermarking and annotations
- Support for password-protected PDFs
- Batch processing with queue triggers
- Document preview generation
- Webhook notifications on completion

## 📞 Support

For issues or questions:
- **Azure Functions**: [Stack Overflow](https://stackoverflow.com/questions/tagged/azure-functions)
- **Telerik DPL**: [Feedback Portal](https://feedback.telerik.com/document-processing)
- **General .NET**: [.NET Community](https://dotnet.microsoft.com/platform/community)

## 🎓 Learning Path

1. **Start Here**: Run MergeApp to see the solution in action
2. **Explore**: Review MergeFunction.cs to understand the implementation
3. **Customize**: Modify timeout, add validation, or enhance error handling
4. **Deploy**: Publish to Azure and configure for production
5. **Monitor**: Use Application Insights to track usage and performance
6. **Extend**: Add new features based on your requirements

---

**Built with ❤️ using .NET 8.0, Azure Functions, and Telerik Document Processing**
