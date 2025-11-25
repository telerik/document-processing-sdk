# MergeApp

A .NET console application that sends merge requests to the AWS Lambda function, retrieves merged PDFs from S3, and opens the results.

## Overview

This client application demonstrates how to interact with AWS Lambda functions from a .NET application. It sends a merge request to the Lambda function, waits for the response, downloads the merged PDF from S3, and opens it automatically.

## Dependencies

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [AWS SDK for .NET](https://aws.amazon.com/sdk-for-net/) (AWSSDK.Lambda, AWSSDK.S3)
- [AWS Lambda .NET Client Documentation](https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/Lambda/NLambdaClient.html)
- [AWS S3 .NET Client Documentation](https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/S3/NS3.html)
