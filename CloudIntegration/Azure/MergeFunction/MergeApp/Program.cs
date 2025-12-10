/// <summary>
/// Console application that demonstrates how to use the Azure Function PDF Merge service.
/// This client application sends multiple PDF files to the MergeFunction endpoint and
/// saves the merged result to disk.
/// </summary>
/// <remarks>
/// The application performs the following steps:
/// <list type="number">
/// <item><description>Loads PDF files from the Resources directory</description></item>
/// <item><description>Packages them as multipart form data</description></item>
/// <item><description>Sends an HTTP POST request to the Azure Function</description></item>
/// <item><description>Saves the merged PDF response to disk</description></item>
/// <item><description>Opens the merged PDF in the default PDF viewer</description></item>
/// </list>
/// </remarks>

using System.Diagnostics;
using System.Net.Http.Headers;

// Configure the Azure Function endpoint URL
// Update this URL to match your deployed function or local development environment
string functionUrl = "http://localhost:7019/api/MergeFunction";

// Specify PDF files to merge from the Resources directory
string[] pdfFilePaths = { "Resources/file1.pdf", "Resources/file2.pdf" };

// Create multipart form data content for file upload
using MultipartFormDataContent content = new();

// Add each PDF file to the form data
foreach (string filePath in pdfFilePaths)
{
    byte[] fileBytes = File.ReadAllBytes(filePath);
    ByteArrayContent fileContent = new(fileBytes);
    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
    content.Add(fileContent, "files", Path.GetFileName(filePath));
}

// Send the merge request to the Azure Function
HttpResponseMessage response = PostAsync(functionUrl, content).Result;
response.EnsureSuccessStatusCode();

// Read the merged PDF from the response
byte[] result = response.Content.ReadAsByteArrayAsync().Result;

// Save the merged PDF to disk
string outputPath = "merged_output.pdf";
if (File.Exists(outputPath))
{
    File.Delete(outputPath);
}
File.WriteAllBytes(outputPath, result);

// Open the merged PDF in the default viewer
Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });

Console.WriteLine($"Successfully merged {pdfFilePaths.Length} PDF files.");
Console.WriteLine($"Output saved to: {Path.GetFullPath(outputPath)}");

/// <summary>
/// Sends an HTTP POST request with multipart form data to the specified URL.
/// </summary>
/// <param name="url">The target URL for the HTTP request.</param>
/// <param name="content">The multipart form data content containing PDF files to merge.</param>
/// <returns>
/// A <see cref="Task{HttpResponseMessage}"/> representing the asynchronous operation,
/// containing the HTTP response from the server.
/// </returns>
static async Task<HttpResponseMessage> PostAsync(string url, MultipartFormDataContent content)
{
    using HttpClient httpClient = new HttpClient();
    httpClient.Timeout = TimeSpan.FromMinutes(2); // Allow sufficient time for large files

    return await httpClient.PostAsync(url, content);
}