using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.S3;
using Amazon.S3.Model;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

/// <summary>
/// Console application that invokes an AWS Lambda function to merge PDF files and downloads the result.
/// </summary>
/// <remarks>
/// This application demonstrates how to:
/// 1. Invoke an AWS Lambda function with JSON input
/// 2. Handle the Lambda response
/// 3. Download the resulting merged PDF from S3
/// 4. Automatically open the downloaded file
/// </remarks>

/// <summary>
/// The name of the AWS Lambda function to invoke for PDF merging.
/// </summary>
const string LambdaFunctionName = "MergePdfAwsFunction"; // Update with your Lambda function name

/// <summary>
/// The AWS region where the Lambda function and S3 bucket are located.
/// </summary>
Amazon.RegionEndpoint region = Amazon.RegionEndpoint.EUNorth1; // Update as needed

/// <summary>
/// The local directory where downloaded files will be saved.
/// </summary>
const string DownloadDirectory = "DownloadedFiles";

/// <summary>
/// The name of the S3 bucket containing the PDF files to merge.
/// </summary>
const string bucketName = "telerik-dpl-demo-bucket";

// Create the input parameters for the Lambda function
var input = new
{
    FileNames = new[] { "file1.pdf", "file2.pdf" },
    BucketName = bucketName
};

// Serialize the input to JSON
string jsonInput = JsonSerializer.Serialize(input);

// Create Lambda client
using AmazonLambdaClient lambdaClient = new(region);

// Create invoke request
InvokeRequest invokeRequest = new()
{
    FunctionName = LambdaFunctionName,
    InvocationType = InvocationType.RequestResponse,
    Payload = jsonInput
};

// Invoke the Lambda function
InvokeResponse response = await lambdaClient.InvokeAsync(invokeRequest);

// Check the response
if (response.StatusCode == 200)
{
    string responsePayload = Encoding.UTF8.GetString(response.Payload.ToArray());
    string mergedFileName = responsePayload.Trim('"'); // Remove quotes from JSON string

    // Download the merged file from S3
    await DownloadFileFromS3(input.BucketName, mergedFileName);
}
else
{
    if (response.FunctionError != null)
    {
        Console.WriteLine($"Function error: {response.FunctionError}");
        string errorPayload = Encoding.UTF8.GetString(response.Payload.ToArray());
        Console.WriteLine($"Error details: {errorPayload}");
    }
}

/// <summary>
/// Downloads a file from AWS S3 bucket to the local file system and opens it.
/// </summary>
/// <param name="bucketName">The name of the S3 bucket containing the file.</param>
/// <param name="fileName">The key (filename) of the file in the S3 bucket.</param>
/// <returns>A task representing the asynchronous download operation.</returns>
/// <exception cref="Amazon.S3.AmazonS3Exception">Thrown when the file cannot be downloaded from S3.</exception>
/// <remarks>
/// This method:
/// 1. Creates the download directory if it doesn't exist
/// 2. Downloads the file from S3 to the local directory
/// 3. Automatically opens the downloaded file using the default system application
/// </remarks>
async Task DownloadFileFromS3(string bucketName, string fileName)
{
    // Create download directory if it doesn't exist
    if (!Directory.Exists(DownloadDirectory))
    {
        Directory.CreateDirectory(DownloadDirectory);
    }

    string localFilePath = Path.Combine(DownloadDirectory, fileName);

    // Create S3 client
    using AmazonS3Client s3Client = new(region);

    // Create download request
    GetObjectRequest request = new()
    {
        BucketName = bucketName,
        Key = fileName
    };

    // Download the file
    using GetObjectResponse s3Response = await s3Client.GetObjectAsync(request);
    using Stream responseStream = s3Response.ResponseStream;
    using FileStream fileStream = File.Create(localFilePath);

    await responseStream.CopyToAsync(fileStream);

    Process.Start(new ProcessStartInfo() { FileName = localFilePath, UseShellExecute = true });
}
