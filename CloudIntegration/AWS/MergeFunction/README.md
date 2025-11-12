# AWS PDF Merge Solution

This solution demonstrates how to merge PDF documents using AWS Lambda and Telerik Document Processing Library (DPL). 
The solution consists of two projects that work together to provide a serverless PDF merging capability using AWS cloud infrastructure.

## Overview

The solution showcases a cloud-based PDF processing workflow:
1. A client application invokes an AWS Lambda function
2. The Lambda function retrieves PDF files from an S3 bucket
3. PDFs are merged using Telerik Document Processing
4. The merged PDF is saved back to S3
5. The client downloads and opens the result

## Projects

### MergeAWSFunctionApp
An AWS Lambda function that performs the actual PDF merging operation using Telerik Document Processing Library.

**Key Features:**
- Serverless PDF processing
- Retrieves source PDFs from AWS S3
- Merges multiple PDFs into a single document
- Stores the merged result back to S3
- Built for .NET 8

[View MergeAWSFunctionApp README](MergeAWSFunctionApp/README.md)

### MergeApp
A console application that invokes the Lambda function and handles the merged PDF file.

**Key Features:**
- Invokes AWS Lambda functions
- Handles Lambda responses
- Downloads merged PDFs from S3
- Automatically opens the result
- Built for .NET 8

[View MergeApp README](MergeApp/README.md)

## Prerequisites

- .NET 8 SDK or later
- AWS Account with appropriate permissions
- AWS CLI configured with credentials
- Visual Studio 2022 or later (recommended) or Visual Studio Code
- Telerik Document Processing license

## AWS Services Used

- **AWS Lambda**: Serverless compute for PDF processing
- **Amazon S3**: Object storage for PDF files
- **IAM**: Identity and access management for permissions

## Getting Started

### 1. Set Up AWS Resources

#### Create an S3 Bucket
```bash
aws s3 mb s3://your-bucket-name --region eu-north-1
```

#### Upload Sample PDF Files
```bash
aws s3 cp file1.pdf s3://your-bucket-name/
aws s3 cp file2.pdf s3://your-bucket-name/
```

### 2. Deploy the Lambda Function

#### Using AWS CLI
```bash
cd MergeAWSFunctionApp
dotnet lambda deploy-function MergePdfAwsFunction
```

#### Using Visual Studio
1. Right-click on the MergeAWSFunctionApp project
2. Select "Publish to AWS Lambda..."
3. Follow the deployment wizard

### 3. Configure IAM Permissions

Ensure your Lambda execution role has the following permissions:
- `s3:GetObject` - Read files from S3
- `s3:PutObject` - Write merged files to S3

Example IAM policy:
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
    }
  ]
}
```

### 4. Configure and Run the Client Application

1. Open `MergeApp/Program.cs`
2. Update the following constants:
   ```csharp
   private const string LambdaFunctionName = "MergePdfAwsFunction";
   private static readonly Amazon.RegionEndpoint Region = Amazon.RegionEndpoint.EUNorth1;
   private const string bucketName = "your-bucket-name";
   ```
3. Update the file names in the `Main` method:
   ```csharp
   FileNames = new[] { "file1.pdf", "file2.pdf" }
   ```
4. Run the application:
   ```bash
   cd MergeApp
   dotnet run
   ```

## Configuration

### Lambda Function Configuration

**Region**: EU North 1 (Stockholm) - `eu-north-1`  
**Runtime**: .NET 8  
**Timeout**: Adjust based on the size and number of PDFs (recommended: 30+ seconds)  
**Memory**: 512 MB or higher depending on PDF complexity

### Client Application Configuration

Update these constants in `Program.cs`:
- `LambdaFunctionName`: Name of your deployed Lambda function
- `Region`: AWS region matching your Lambda and S3 resources
- `bucketName`: Name of your S3 bucket
- `DownloadDirectory`: Local directory for downloaded files (default: "DownloadedFiles")

## Architecture

```
┌─────────────┐         ┌──────────────────┐         ┌─────────────┐
│             │ Invoke  │                  │ Get/Put │             │
│  MergeApp   ├────────>│  Lambda Function ├────────>│  S3 Bucket  │
│  (Client)   │         │  (MergeAWSFunctionApp)     │             │
│             │<────────│                  │         │             │
└─────────────┘ Response└──────────────────┘         └─────────────┘
       │                                                     │
       │              Download merged PDF                    │
       └─────────────────────────────────────────────────────┘
```

## Technology Stack

- **.NET 8**: Target framework
- **AWS SDK for .NET**: AWS service integration
  - `AWSSDK.Lambda`: Lambda function invocation
  - `AWSSDK.S3`: S3 object storage operations
- **Amazon.Lambda.Core**: Lambda runtime support
- **Telerik Document Processing**: PDF manipulation
  - `Telerik.Documents.Fixed`: PDF processing library

## Cost Considerations

This solution uses AWS services that may incur costs:
- **Lambda**: Charged per request and compute time
- **S3**: Charged for storage and data transfer
- **Data Transfer**: Charges may apply for data transfer between services

Consider using AWS Free Tier for development and testing.

## Troubleshooting

### Lambda Function Errors

Check CloudWatch Logs for detailed error messages:
```bash
aws logs tail /aws/lambda/MergePdfAwsFunction --follow
```

### Permission Issues

Ensure:
1. Lambda execution role has S3 permissions
2. Client AWS credentials have Lambda invoke permissions
3. S3 bucket policy allows access from Lambda

## Further Reading

- [AWS Lambda .NET Developer Guide](https://docs.aws.amazon.com/lambda/latest/dg/lambda-csharp.html)
- [Telerik Document Processing Documentation](https://docs.telerik.com/devtools/document-processing/)
- [AWS SDK for .NET Documentation](https://aws.amazon.com/sdk-for-net/)

## License

This project is provided as a sample/demo. Please ensure you have appropriate licenses for:
- Telerik Document Processing Library. Check the [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing).
