# MergeAWSFunctionApp

An AWS Lambda function that merges multiple PDF documents into a single file using Telerik Document Processing Library.

## Overview

This serverless function retrieves PDF files from an AWS S3 bucket, merges them into a single PDF document, and saves the result back to the same bucket. It's designed to run in AWS Lambda with .NET 8 runtime.

## Features

- **Serverless Architecture**: No infrastructure management required
- **S3 Integration**: Seamless reading and writing to AWS S3
- **PDF Merging**: Uses Telerik Document Processing for high-quality merges
- **Automatic Naming**: Generates unique filenames for merged PDFs
- **Timeout Protection**: Configurable timeouts for import/export operations
- **Cold Start Optimization**: Ready-to-run compilation enabled

## Project Structure

```
MergeAWSFunctionApp/
├── Function.cs                 # Main Lambda handler
├── Input.cs                    # Input model for the function
└── MergeAWSFunctionApp.csproj  # Project configuration
```

## Dependencies

### NuGet Packages

- **Amazon.Lambda.Core**: Core Lambda functionality
- **Amazon.Lambda.Serialization.SystemTextJson**: JSON serialization
- **AWSSDK.S3**: Amazon S3 client
- **Telerik.Documents.Fixed**: PDF processing library

## Function Input

The Lambda function accepts a JSON payload with the following structure:

```json
{
  "FileNames": ["file1.pdf", "file2.pdf", "file3.pdf"],
  "BucketName": "your-bucket-name"
}
```

### Input Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `FileNames` | `string[]` | Yes | Array of PDF filenames to merge (in order) |
| `BucketName` | `string` | Yes | Name of the S3 bucket containing the files |

## Function Output

The function returns a string containing the filename of the merged PDF:

```
Merged_a1b2c3d4-e5f6-7890-abcd-ef1234567890.pdf
```

The merged file is automatically saved to the same S3 bucket specified in the input.

## Configuration

### AWS Region

The function is configured for **EU North 1 (Stockholm)**. To change the region, update the following in `Function.cs`:

```csharp
Amazon.RegionEndpoint AmazonRegion = Amazon.RegionEndpoint.EUNorth1;
```

Available regions: [AWS Regions Documentation](https://docs.aws.amazon.com/general/latest/gr/rande.html)

### Timeout Settings

PDF import and export operations have a 20-second timeout. Adjust if needed:

```csharp
// In MergePdfs method
RadFixedDocument documentToMerge = provider.Import(stream, TimeSpan.FromSeconds(20));

// In FunctionHandler method
provider.Export(merged, resultStream, TimeSpan.FromSeconds(20));
```

## Deployment

### Prerequisites

1. AWS CLI installed and configured
2. AWS Lambda Tools for .NET
   ```bash
   dotnet tool install -g Amazon.Lambda.Tools
   ```
3. Valid Telerik license

### Deploy Using AWS CLI

```bash
# Build and package
dotnet lambda package

# Deploy
dotnet lambda deploy-function MergePdfAwsFunction --region eu-north-1
```

### Deploy Using Visual Studio

1. Right-click on the project in Solution Explorer
2. Select **Publish to AWS Lambda...**
3. Configure:
   - **Function Name**: `MergePdfAwsFunction`
   - **Runtime**: `.NET 8 (Container)`
   - **Memory**: 512 MB (minimum recommended)
   - **Timeout**: 30 seconds (adjust based on your needs)
4. Click **Upload**

### Deploy Using AWS SAM

Create a `template.yaml`:

```yaml
AWSTemplateFormatVersion: '2010-09-09'
Transform: AWS::Serverless-2016-10-31

Resources:
  MergePdfFunction:
    Type: AWS::Serverless::Function
    Properties:
      FunctionName: MergePdfAwsFunction
      Handler: MergeAWSFunctionApp::MergeAWSFunctionApp.Function::FunctionHandler
      Runtime: dotnet8
      CodeUri: ./
      MemorySize: 512
      Timeout: 30
      Policies:
        - S3ReadPolicy:
            BucketName: your-bucket-name
        - S3WritePolicy:
            BucketName: your-bucket-name
```

Deploy:
```bash
sam build
sam deploy --guided
```

## IAM Permissions

The Lambda execution role requires the following permissions:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
  "Effect": "Allow",
      "Action": [
        "s3:GetObject",
        "s3:PutObject"
      ],
      "Resource": "arn:aws:s3:::your-bucket-name/*"
    },
    {
"Effect": "Allow",
      "Action": [
        "logs:CreateLogGroup",
        "logs:CreateLogStream",
        "logs:PutLogEvents"
      ],
      "Resource": "arn:aws:logs:*:*:*"
    }
  ]
}
```

## Testing

### Test in AWS Console

1. Navigate to AWS Lambda Console
2. Select your function
3. Click **Test**
4. Create a test event:
   ```json
   {
     "FileNames": ["test1.pdf", "test2.pdf"],
     "BucketName": "your-test-bucket"
   }
   ```
5. Click **Test** to execute

### Test Locally

Use AWS Lambda Test Tool (included with AWS Toolkit for Visual Studio):

```bash
# Install the tool
dotnet tool install -g Amazon.Lambda.TestTool-8.0

# Run
dotnet lambda-test-tool-8.0
```

### Unit Testing

Create test files in your S3 bucket:
```bash
aws s3 cp test1.pdf s3://your-bucket/
aws s3 cp test2.pdf s3://your-bucket/
```

## How It Works

1. **Input Processing**: Lambda receives JSON input with file names and bucket name
2. **S3 Connection**: Establishes connection to specified AWS region
3. **File Retrieval**: Downloads each PDF file from S3 sequentially
4. **Merge Operation**: 
   - Creates empty `RadFixedDocument`
   - Imports each PDF using `PdfFormatProvider`
   - Merges pages into the main document using `Merge()` method
5. **Export**: Exports merged document to memory stream
6. **Upload**: Saves result to S3 with unique GUID-based filename
7. **Response**: Returns the generated filename

## Troubleshooting

### Issue: Timeout Errors

**Solution**: Increase Lambda timeout
```bash
aws lambda update-function-configuration \
  --function-name MergePdfAwsFunction \
  --timeout 60
```

### Issue: Out of Memory

**Solution**: Increase memory allocation
```bash
aws lambda update-function-configuration \
  --function-name MergePdfAwsFunction \
  --memory-size 1024
```

### Issue: S3 Access Denied

**Solution**: Verify IAM role permissions
```bash
aws iam get-role-policy \
  --role-name your-lambda-role \
  --policy-name your-policy-name
```

### Issue: File Not Found in S3

**Solution**: Verify files exist
```bash
aws s3 ls s3://your-bucket-name/
```

### Issue: Telerik License Errors

**Solution**: Ensure license is properly configured
- Check license file location
- Verify license supports server-side usage
- Review Telerik licensing documentation

## Monitoring and Logging

### CloudWatch Logs

View logs:
```bash
aws logs tail /aws/lambda/MergePdfAwsFunction --follow
```

### Metrics

Monitor in CloudWatch:
- **Invocations**: Number of function calls
- **Duration**: Execution time
- **Errors**: Failed invocations
- **Throttles**: Rate-limited requests

### X-Ray Tracing

Enable for detailed performance analysis:
```bash
aws lambda update-function-configuration \
  --function-name MergePdfAwsFunction \
  --tracing-config Mode=Active
```

## Related Resources

- [AWS Lambda .NET Documentation](https://docs.aws.amazon.com/lambda/latest/dg/lambda-csharp.html)
- [Telerik PDF Processing](https://docs.telerik.com/devtools/document-processing/libraries/radpdfprocessing/overview)
- [AWS S3 .NET SDK](https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/s3-apis-intro.html)
- [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing).

## License

Requires valid Telerik Document Processing license for production use. Check the [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing).
