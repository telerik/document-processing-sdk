# MergeApp - PDF Merge Function Test Client

A .NET 8.0 console application that demonstrates how to interact with the Azure Function PDF Merge service. This client provides a simple example of sending multiple PDF files to the merge endpoint and handling the response.

## Overview

**MergeApp** is a test client application designed to work with the **MergeFunctionApp** Azure Function. It demonstrates the complete workflow of:
- Loading PDF files from disk
- Packaging files as multipart form data
- Sending HTTP requests to the Azure Function
- Receiving and saving the merged PDF result
- Automatically opening the result in the default PDF viewer

## Features

- ✅ **Simple console interface** - Easy to run and test
- ✅ **Multipart form data** - Demonstrates proper file upload format
- ✅ **Error handling** - Validates HTTP response status
- ✅ **Auto-open result** - Launches merged PDF in default viewer
- ✅ **Example PDFs included** - Ready-to-run with sample files

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Running instance of **MergeFunctionApp** (local or deployed)
- Sample PDF files in the `Resources` directory

## Project Structure

```
MergeApp/
├── Program.cs     # Main application logic
├── Resources/
│   ├── file1.pdf          # Sample PDF file 1
│   └── file2.pdf          # Sample PDF file 2
├── MergeApp.csproj # Project file
└── README.md   # This file
```

## Getting Started

### 1. Start the Azure Function

First, ensure the MergeFunctionApp is running:

```bash
cd ../MergeFunctionApp
func start
# or
dotnet run
```

Note the port number (default: 7071, but may vary based on configuration).

### 2. Update the Function URL

Edit `Program.cs` and update the `functionUrl` variable if needed:

```csharp
string functionUrl = "http://localhost:7071/api/MergeFunction"; // Update port if different
```

For deployed Azure Functions, use:
```csharp
string functionUrl = "https://<your-function-app>.azurewebsites.net/api/MergeFunction";
```

### 3. Run the Application

```bash
cd MergeApp
dotnet run
```

### 4. View the Result

The application will:
1. Send the PDF files to the merge function
2. Save the merged result as `merged_output.pdf`
3. Automatically open the merged PDF in your default viewer
4. Display success message with the output file path

## Configuration

### Changing PDF Files to Merge

Modify the `pdfFilePaths` array in `Program.cs`:

```csharp
string[] pdfFilePaths = { 
    "Resources/file1.pdf", 
    "Resources/file2.pdf",
    "Resources/file3.pdf"  // Add more files
};
```

### Using Your Own PDFs

1. Place your PDF files in the `Resources` directory
2. Update the `pdfFilePaths` array with your file paths
3. Run the application

### Adjusting Timeout

For large files, you may need to increase the HTTP client timeout:

```csharp
httpClient.Timeout = TimeSpan.FromMinutes(5); // Increase as needed
```

## Code Example

The core functionality demonstrates a standard HTTP multipart upload:

```csharp
using MultipartFormDataContent content = new();

foreach (string filePath in pdfFilePaths)
{
    byte[] fileBytes = File.ReadAllBytes(filePath);
    ByteArrayContent fileContent = new(fileBytes);
 fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
    content.Add(fileContent, "files", Path.GetFileName(filePath));
}

HttpResponseMessage response = await httpClient.PostAsync(functionUrl, content);
response.EnsureSuccessStatusCode();

byte[] result = await response.Content.ReadAsByteArrayAsync();
await File.WriteAllBytesAsync("merged_output.pdf", result);
```

## Expected Output

When successful, you'll see:

```
Successfully merged 2 PDF files.
Output saved to: C:\Path\To\MergeApp\merged_output.pdf
```

The merged PDF will open automatically in your default PDF viewer.

## Error Handling

### Common Issues

**Issue**: Connection refused or timeout
- **Solution**: Ensure MergeFunctionApp is running and the port number is correct

**Issue**: 500 Internal Server Error
- **Solution**: Check Azure Function logs for detailed error messages
- Verify PDF files are valid and not corrupted

**Issue**: File not found error
- **Solution**: Ensure PDF files exist in the Resources directory
- Check file paths are correct

**Issue**: Access denied when opening PDF
- **Solution**: Close the PDF if already open, or change the output filename

### Debugging

To debug the application:

1. Set breakpoints in Visual Studio or VS Code
2. Press F5 to start debugging
3. Inspect the `response` variable for HTTP status codes
4. Check the `result` byte array for PDF data

## Testing with Different Scenarios

### Test with 2 PDFs (Default)
```csharp
string[] pdfFilePaths = { "Resources/file1.pdf", "Resources/file2.pdf" };
```

### Test with Multiple PDFs
```csharp
string[] pdfFilePaths = { 
    "Resources/file1.pdf", 
    "Resources/file2.pdf",
    "Resources/file3.pdf",
    "Resources/file4.pdf"
};
```

### Test with Single PDF
```csharp
string[] pdfFilePaths = { "Resources/file1.pdf" };
```

## Integration with Other Projects

This client can be adapted for:
- **Web applications** - Convert to a web API client
- **Desktop applications** - Integrate into WinForms/WPF apps
- **Batch processing** - Process multiple sets of PDFs
- **Cloud workflows** - Trigger from Azure Logic Apps or Power Automate

## Performance Considerations

- **File Size**: Large PDFs may take longer to upload and process
- **Network Speed**: Upload time depends on bandwidth to Azure Function
- **Function Timeout**: Default is 20 seconds; adjust for large files
- **Memory**: Entire PDF is loaded into memory; consider file size limits

## Extending the Application

### Add Progress Reporting

```csharp
Console.WriteLine("Loading PDF files...");
Console.WriteLine("Uploading to merge function...");
Console.WriteLine("Processing merge...");
Console.WriteLine("Downloading result...");
```

### Add Error Logging

```csharp
try
{
    // Merge logic
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"HTTP Error: {ex.Message}");
    File.WriteAllText("error.log", ex.ToString());
}
```

### Support Command-Line Arguments

```csharp
if (args.Length > 0)
{
    pdfFilePaths = args;
}
```

## Related Projects

- **MergeFunctionApp**: The Azure Function that performs the PDF merging

## Dependencies

This project uses only built-in .NET libraries:
- `System.Net.Http` - HTTP client functionality
- `System.Diagnostics` - Process launching for PDF viewer
- `System.IO` - File operations

No external NuGet packages required.

## Building and Publishing

### Build
```bash
dotnet build
```

### Publish (Self-Contained)
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

### Publish (Framework-Dependent)
```bash
dotnet publish -c Release
```

## License

This test client is provided as part of the Telerik Document Processing samples. Check the [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing).

## Support

For issues or questions:
- Check the [MergeFunctionApp README](../MergeFunctionApp/README.md) for Azure Function setup
- Review [Azure Functions documentation](https://docs.microsoft.com/azure/azure-functions/)
- Consult [Telerik Document Processing documentation](https://docs.telerik.com/devtools/document-processing/)

## Additional Resources

- [HttpClient Best Practices](https://docs.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [Multipart Form Data in .NET](https://docs.microsoft.com/aspnet/web-api/overview/advanced/sending-html-form-data-part-2)
- [Azure Functions HTTP Triggers](https://docs.microsoft.com/azure/azure-functions/functions-bindings-http-webhook-trigger)
