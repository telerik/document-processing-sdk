# MergeAWSFunctionApp

An AWS Lambda function that retrieves PDF files from S3, merges them into a single document, and saves the result back to S3.

## Overview

This serverless function retrieves PDF files from an AWS S3 bucket, merges them into a single PDF document, and saves the result back to the same bucket. It's designed to run in AWS Lambda with .NET 8 runtime.

## Dependencies

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Amazon.Lambda.Core](https://www.nuget.org/packages/Amazon.Lambda.Core/)
- [AWSSDK.S3](https://www.nuget.org/packages/AWSSDK.S3/)
- [Telerik Document Processing](https://docs.telerik.com/devtools/document-processing/) - Check the [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing)
- [AWS Lambda .NET Documentation](https://docs.aws.amazon.com/lambda/latest/dg/lambda-csharp.html)
- [Telerik RadPdfProcessing](https://docs.telerik.com/devtools/document-processing/libraries/radpdfprocessing/overview)
