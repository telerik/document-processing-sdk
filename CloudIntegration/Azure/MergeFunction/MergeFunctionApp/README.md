# PDF Merge Azure Function

This Azure Function provides a serverless HTTP endpoint for merging multiple PDF documents into a single PDF file using Telerik Document Processing.

## Overview

The **MergeFunction** is an HTTP-triggered Azure Function that accepts multiple PDF files via multipart form data and returns a single merged PDF document. It leverages the Telerik Document Processing library for reliable PDF manipulation.

## Features

- ? **HTTP-triggered serverless function** - Accessible via standard HTTP POST requests
- ? **Multiple PDF support** - Merge 2 or more PDF documents in a single request
- ? **Multipart form data** - Standard file upload format
- ? **Anonymous access** - No authentication required (configurable)
- ? **Automatic content-type handling** - Returns proper PDF MIME type
- ? **Timeout protection** - 20-second timeout for long-running operations

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools](https://docs.microsoft.com/azure/azure-functions/functions-run-local) (v4)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/) with Azure Functions extension
- Valid Telerik Document Processing license (for production use)

## Project Structure

```
MergeFunctionApp/
??? MergeFunction.cs          # Main Azure Function implementation
??? Program.cs   # Function app host configuration
??? host.json   # Function app host settings
??? local.settings.json       # Local development settings
??? MergeFunctionApp.csproj   # Project file with dependencies
```

## Dependencies

- **Microsoft.Azure.Functions.Worker** - Azure Functions worker runtime
- **Microsoft.Azure.Functions.Worker.Extensions.Http** - HTTP trigger support
- **Telerik.Documents.Fixed** - PDF document processing
- **HttpMultipartParser** - Multipart form data parsing
- **Microsoft.ApplicationInsights.WorkerService** - Application monitoring

## Getting Started

### Local Development

1. **Clone the repository** and navigate to the project directory:
   ```bash
   cd MergeFunctionApp
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Configure local settings** (if needed):
   Create or edit `local.settings.json`:
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "UseDevelopmentStorage=true",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
     }
   }
   ```

4. **Run the function locally**:
   ```bash
   func start
   ```
   or
   ```bash
   dotnet run
   ```

5. The function will be available at: `http://localhost:7071/api/MergeFunction` (port may vary)

### Testing the Function

Use the included `MergeApp` console application or any HTTP client:

#### Using the MergeApp Client

```bash
cd ../MergeApp
dotnet run
```

#### Using cURL

```bash
curl -X POST http://localhost:7071/api/MergeFunction \
  -F "files=@file1.pdf" \
  -F "files=@file2.pdf" \
  -o merged_output.pdf
```

#### Using PowerShell

```powershell
$uri = "http://localhost:7071/api/MergeFunction"
$form = @{
    files = Get-Item -Path "file1.pdf"
    files = Get-Item -Path "file2.pdf"
}
Invoke-RestMethod -Uri $uri -Method Post -Form $form -OutFile "merged_output.pdf"
```

#### Using C# HttpClient

```csharp
using var content = new MultipartFormDataContent();

foreach (string filePath in pdfFilePaths)
{
    byte[] fileBytes = File.ReadAllBytes(filePath);
    var fileContent = new ByteArrayContent(fileBytes);
    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
    content.Add(fileContent, "files", Path.GetFileName(filePath));
}

var response = await httpClient.PostAsync(functionUrl, content);
response.EnsureSuccessStatusCode();

byte[] mergedPdf = await response.Content.ReadAsByteArrayAsync();
await File.WriteAllBytesAsync("merged.pdf", mergedPdf);
```

## API Reference

### Endpoint

```
POST /api/MergeFunction
```

### Request

- **Content-Type**: `multipart/form-data`
- **Body**: One or more PDF files with the form field name `files`

### Response

- **Status Code**: `200 OK` (on success)
- **Content-Type**: `application/pdf`
- **Body**: Binary PDF data of the merged document

### Error Handling

The function may return error responses in the following scenarios:
- Invalid PDF files
- Corrupted or password-protected PDFs
- Timeout during processing (>20 seconds)
- Memory limitations for very large files

## Deployment

### Deploy to Azure

1. **Create Azure Function App** (if not already created):
 ```bash
   az functionapp create \
     --resource-group <resource-group> \
     --consumption-plan-location <location> \
     --runtime dotnet-isolated \
     --functions-version 4 \
     --name <function-app-name> \
     --storage-account <storage-account>
   ```

2. **Deploy the function**:
   ```bash
   func azure functionapp publish <function-app-name>
   ```
   or use Visual Studio's publish feature

3. **Access the deployed function**:
   ```
   https://<function-app-name>.azurewebsites.net/api/MergeFunction
   ```

### Configuration

After deployment, configure the following in Azure Portal:

- **Application Settings**: Add any required configuration
- **Authentication**: Enable if needed (currently set to Anonymous)
- **CORS**: Configure allowed origins if calling from web applications
- **Monitoring**: Application Insights is pre-configured for telemetry

## Performance Considerations

- **Timeout**: Default timeout is 20 seconds for PDF processing operations
- **Memory**: Large PDF files may require increased memory allocation
- **Cold Start**: First request may take longer due to function initialization
- **Concurrency**: Multiple requests are handled in parallel by Azure Functions

## Limitations

- Maximum file size depends on Azure Functions plan limits
- Processing time limited by timeout settings
- Memory constraints apply based on hosting plan
- Telerik Document Processing license required for production use

## Troubleshooting

### Common Issues

**Issue**: Function returns 500 Internal Server Error
- **Solution**: Check Application Insights logs for detailed error messages

**Issue**: Timeout during merge operation
- **Solution**: Reduce file sizes or increase timeout in code

**Issue**: Missing dependencies
- **Solution**: Run `dotnet restore` and ensure all NuGet packages are installed

**Issue**: Telerik licensing error
- **Solution**: Ensure valid license is configured or use trial version

## Related Projects

- **MergeApp**: Console application for testing the merge function locally

## License

This project uses Telerik Document Processing, which requires a valid license for production use.

## Support

For issues related to:
- **Azure Functions**: [Azure Functions Documentation](https://docs.microsoft.com/azure/azure-functions/)
- **Telerik Document Processing**: [Telerik Documentation](https://docs.telerik.com/devtools/document-processing/)

## Additional Resources

- [Telerik Document Processing Overview](https://docs.telerik.com/devtools/document-processing/introduction)
- [Azure Functions Best Practices](https://docs.microsoft.com/azure/azure-functions/functions-best-practices)
- [PDF Processing with RadFixedDocument](https://docs.telerik.com/devtools/document-processing/libraries/radpdfprocessing/model/radfixeddocument)
