# MergeApp

A .NET console application that invokes an AWS Lambda function to merge PDF files and automatically downloads the result.

## Overview

This client application demonstrates how to interact with AWS Lambda functions from a .NET application. It sends a merge request to the Lambda function, waits for the response, downloads the merged PDF from S3, and opens it automatically.

## Features

- **Lambda Invocation**: Calls AWS Lambda functions programmatically
- **JSON Serialization**: Automatic input/output serialization
- **S3 Download**: Retrieves merged PDFs from Amazon S3
- **Auto-Open**: Automatically opens downloaded files
- **Error Handling**: Comprehensive error reporting
- **Configurable**: Easy configuration through constants

## Project Structure

```
MergeApp/
├── Program.cs       # Main application logic
├── MergeApp.csproj      # Project configuration
└── DownloadedFiles/     # Created at runtime for downloads
```

## Dependencies

### NuGet Packages

- **AWSSDK.Lambda**: AWS Lambda client for .NET
- **AWSSDK.S3**: Amazon S3 client for .NET

Both packages use latest versions (`*` wildcard in project file).

## Configuration

### Required Settings

Before running the application, update these constants in `Program.cs`:

```csharp
// Lambda function name (must match deployed function)
private const string LambdaFunctionName = "MergePdfAwsFunction";

// AWS region (must match Lambda and S3 region)
private static readonly Amazon.RegionEndpoint Region = Amazon.RegionEndpoint.EUNorth1;

// S3 bucket name (must match your bucket)
private const string bucketName = "telerik-dpl-demo-bucket";

// Local download directory
private const string DownloadDirectory = "DownloadedFiles";
```

### Input Files

Specify which PDFs to merge in the `Main` method:

```csharp
var input = new
{
    FileNames = new[] { "file1.pdf", "file2.pdf" },  // Update with your file names
    BucketName = bucketName
};
```

## Prerequisites

1. **.NET 8 SDK** or later
2. **AWS Account** with:
   - Lambda function deployed (see MergeAWSFunctionApp)
   - S3 bucket with PDF files
3. **AWS Credentials** configured via:
   - AWS CLI (`aws configure`)
   - Environment variables
   - AWS credentials file
   - IAM role (if running on EC2)

## Setup

### 1. Install .NET 8

Download from: https://dotnet.microsoft.com/download/dotnet/8.0

### 2. Configure AWS Credentials

#### Option A: AWS CLI
```bash
aws configure
```

Enter:
- AWS Access Key ID
- AWS Secret Access Key
- Default region: `eu-north-1`
- Output format: `json`

#### Option B: Environment Variables
```bash
# Windows PowerShell
$env:AWS_ACCESS_KEY_ID="your-access-key"
$env:AWS_SECRET_ACCESS_KEY="your-secret-key"
$env:AWS_REGION="eu-north-1"

# Linux/macOS
export AWS_ACCESS_KEY_ID="your-access-key"
export AWS_SECRET_ACCESS_KEY="your-secret-key"
export AWS_REGION="eu-north-1"
```

### 3. Prepare S3 Bucket

Upload test PDF files:
```bash
aws s3 cp file1.pdf s3://telerik-dpl-demo-bucket/
aws s3 cp file2.pdf s3://telerik-dpl-demo-bucket/
```

Verify upload:
```bash
aws s3 ls s3://telerik-dpl-demo-bucket/
```

### 4. Deploy Lambda Function

Ensure the MergeAWSFunctionApp is deployed:
```bash
cd ../MergeAWSFunctionApp
dotnet lambda deploy-function MergePdfAwsFunction
```

## Usage

### Build the Application

```bash
dotnet build
```

### Run the Application

```bash
dotnet run
```

### Expected Output

```
Invoking Lambda function...
Lambda invocation successful!
Merged file: Merged_a1b2c3d4-e5f6-7890-abcd-ef1234567890.pdf
Downloading file from S3...
Download complete: DownloadedFiles/Merged_a1b2c3d4-e5f6-7890-abcd-ef1234567890.pdf
Opening file...
```

The merged PDF will:
1. Be downloaded to `DownloadedFiles/` directory
2. Open automatically in your default PDF viewer

## How It Works

### Application Flow

```
1. Create Input Parameters
   ├─ FileNames array
   └─ BucketName string

2. Serialize to JSON
   └─ System.Text.Json

3. Create Lambda Client
   └─ AmazonLambdaClient (with region)

4. Invoke Lambda Function
   ├─ Request-Response invocation
   └─ Wait for completion

5. Process Response
   ├─ Check status code (200 = success)
   ├─ Extract merged filename
   └─ Handle errors

6. Download from S3
   ├─ Create S3 client
   ├─ Download to local directory
   └─ Save file

7. Open File
   └─ Process.Start with default application
```

### Code Walkthrough

#### Lambda Invocation

```csharp
// Create and configure the invoke request
InvokeRequest invokeRequest = new()
{
    FunctionName = LambdaFunctionName,
    InvocationType = InvocationType.RequestResponse,  // Synchronous
    Payload = jsonInput  // Serialized JSON
};

// Invoke the function
InvokeResponse response = await lambdaClient.InvokeAsync(invokeRequest);
```

#### Response Handling

```csharp
if (response.StatusCode == 200)
{
  // Success: Extract filename
    string responsePayload = Encoding.UTF8.GetString(response.Payload.ToArray());
    string mergedFileName = responsePayload.Trim('"');
    
    // Download the file
    await DownloadFileFromS3(input.BucketName, mergedFileName);
}
else
{
    // Error: Display details
    Console.WriteLine($"Function error: {response.FunctionError}");
    string errorPayload = Encoding.UTF8.GetString(response.Payload.ToArray());
    Console.WriteLine($"Error details: {errorPayload}");
}
```

#### S3 Download

```csharp
// Create download request
GetObjectRequest request = new()
{
    BucketName = bucketName,
    Key = fileName
};

// Download and save
using GetObjectResponse s3Response = await s3Client.GetObjectAsync(request);
using Stream responseStream = s3Response.ResponseStream;
using FileStream fileStream = File.Create(localFilePath);
await responseStream.CopyToAsync(fileStream);
```

## IAM Permissions

### Required Permissions

The AWS credentials used by this application need:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
    "Action": [
        "lambda:InvokeFunction"
      ],
      "Resource": "arn:aws:lambda:eu-north-1:YOUR_ACCOUNT_ID:function:MergePdfAwsFunction"
  },
    {
 "Effect": "Allow",
      "Action": [
        "s3:GetObject"
      ],
      "Resource": "arn:aws:s3:::telerik-dpl-demo-bucket/*"
    }
  ]
}
```

### Create IAM Policy

```bash
aws iam create-policy \
  --policy-name MergeAppClientPolicy \
  --policy-document file://policy.json
```

### Attach to User

```bash
aws iam attach-user-policy \
  --user-name your-username \
  --policy-arn arn:aws:iam::YOUR_ACCOUNT_ID:policy/MergeAppClientPolicy
```

## Customization

### Change Download Location

```csharp
private const string DownloadDirectory = "C:\\MyDownloads\\PDFs";
```

### Add Progress Reporting

```csharp
Console.WriteLine("Invoking Lambda function...");
InvokeResponse response = await lambdaClient.InvokeAsync(invokeRequest);
Console.WriteLine($"Invocation complete. Status: {response.StatusCode}");
```

### Disable Auto-Open

Comment out the `Process.Start` line:

```csharp
// Process.Start(new ProcessStartInfo() 
// { 
//     FileName = localFilePath, 
//     UseShellExecute = true 
// });
```

### Multiple Merge Operations

```csharp
var mergeTasks = new[]
{
    new { FileNames = new[] { "file1.pdf", "file2.pdf" }, BucketName = bucketName },
    new { FileNames = new[] { "file3.pdf", "file4.pdf" }, BucketName = bucketName }
};

foreach (var mergeTask in mergeTasks)
{
    // Invoke Lambda and download...
}
```

## Error Handling

### Common Errors

#### 1. Credentials Not Found

```
Error: Unable to get IAM security credentials
```

**Solution**: Configure AWS credentials (see Setup section)

#### 2. Lambda Function Not Found

```
Error: Function not found: MergePdfAwsFunction
```

**Solution**: Verify Lambda function name and deployment

#### 3. Access Denied

```
Error: User is not authorized to perform: lambda:InvokeFunction
```

**Solution**: Add required IAM permissions

#### 4. Region Mismatch

```
Error: Function not found
```

**Solution**: Ensure Region matches Lambda deployment region

#### 5. File Not Found in S3

```
Error: The specified key does not exist
```

**Solution**: Verify files exist in S3 bucket

### Enhanced Error Handling

Add try-catch for better error reporting:

```csharp
try
{
    InvokeResponse response = await lambdaClient.InvokeAsync(invokeRequest);
    // Process response...
}
catch (AmazonLambdaException ex)
{
    Console.WriteLine($"Lambda error: {ex.Message}");
    Console.WriteLine($"Error code: {ex.ErrorCode}");
}
catch (AmazonS3Exception ex)
{
    Console.WriteLine($"S3 error: {ex.Message}");
    Console.WriteLine($"Status code: {ex.StatusCode}");
}
```

## Troubleshooting

### Debug Lambda Invocation

Add logging:

```csharp
Console.WriteLine($"Invoking: {LambdaFunctionName}");
Console.WriteLine($"Region: {Region.SystemName}");
Console.WriteLine($"Input: {jsonInput}");
```

### Verify S3 Access

Test S3 connection:

```bash
aws s3 ls s3://telerik-dpl-demo-bucket/ --region eu-north-1
```

### Check Lambda Logs

View CloudWatch logs:

```bash
aws logs tail /aws/lambda/MergePdfAwsFunction --follow
```

### Test Lambda Directly

Invoke from CLI:

```bash
aws lambda invoke \
  --function-name MergePdfAwsFunction \
  --payload '{"FileNames":["file1.pdf","file2.pdf"],"BucketName":"telerik-dpl-demo-bucket"}' \
  response.json
```

## Performance Considerations

### Synchronous vs Asynchronous

Current implementation uses **RequestResponse** (synchronous):
- Waits for Lambda to complete
- Returns result immediately
- Simpler error handling

For long-running operations, consider **Event** (asynchronous):

```csharp
InvocationType = InvocationType.Event  // Asynchronous
```

### Parallel Operations

For multiple merges:

```csharp
var tasks = mergeTasks.Select(async task =>
{
    // Invoke Lambda and download
}).ToList();

await Task.WhenAll(tasks);
```

### Connection Reuse

Lambda and S3 clients are disposable. For multiple operations, reuse clients:

```csharp
using var lambdaClient = new AmazonLambdaClient(Region);
using var s3Client = new AmazonS3Client(Region);

// Multiple operations...
```

## Security Best Practices

1. **Never Hardcode Credentials**: Use AWS credential providers
2. **Use IAM Roles**: When running on AWS infrastructure
3. **Least Privilege**: Grant minimal required permissions
4. **Secure Storage**: Don't commit credentials to source control
5. **Rotate Credentials**: Regularly update access keys

## Alternative Approaches

### Using AWS SDK Async Patterns

Already implemented with `async/await`

### Using AWS Lambda .NET SDK

For advanced scenarios:

```csharp
var client = new AmazonLambdaClient();
var request = new InvokeRequest
{
 FunctionName = "MergePdfAwsFunction",
    InvocationType = InvocationType.DryRun  // Validate without executing
};
```

### Using AWS Step Functions

For complex workflows with multiple steps

## Related Resources

- [AWS Lambda .NET Client](https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/Lambda/NLambdaClient.html)
- [AWS S3 .NET Client](https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/S3/NS3.html)
- [AWS SDK for .NET Developer Guide](https://docs.aws.amazon.com/sdk-for-net/latest/developer-guide/)

## License

This is a sample application demonstrating AWS Lambda integration. Ensure compliance with:
- AWS service terms
- Telerik licensing (for the Lambda function). Check the [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing).
- Your organization's policies
