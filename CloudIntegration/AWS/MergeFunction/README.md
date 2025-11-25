# AWS PDF Merge Solution

A serverless solution for merging PDF documents using AWS Lambda and Telerik Document Processing.

## Overview

The solution showcases a cloud-based PDF processing workflow:
1. A client application invokes an AWS Lambda function
2. The Lambda function retrieves PDF files from an S3 bucket
3. PDFs are merged using Telerik Document Processing
4. The merged PDF is saved back to S3
5. The client downloads and opens the result

## Included Projects

- **MergeAWSFunctionApp** - AWS Lambda function that merges PDFs from S3
- **MergeApp** - Console application that invokes the Lambda function

## Dependencies

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [AWS CLI](https://aws.amazon.com/cli/)
- [Telerik Document Processing](https://docs.telerik.com/devtools/document-processing/) - Check the [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing)
- [AWS Lambda .NET Documentation](https://docs.aws.amazon.com/lambda/latest/dg/lambda-csharp.html)

## License

This project is provided as a sample/demo. Please ensure you have appropriate licenses for:
- Telerik Document Processing Library. Check the [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing).
