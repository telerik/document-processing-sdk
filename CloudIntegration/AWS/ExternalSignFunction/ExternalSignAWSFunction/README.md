# ExternalSignAWSFunction

An AWS Lambda function that receives data to be signed, performs RSA signing with a private key certificate, and returns the signature bytes.

## Overview

This Lambda function serves as a secure signing service that keeps private keys in the AWS cloud environment. It is designed to work with PDF signing applications that use Telerik Document Processing or similar libraries for external signing scenarios.

## Features

- **Secure Key Storage**: Private keys remain in AWS Lambda, never exposed to clients
- **RSA Signing**: Supports RSA signature generation with PKCS#1 padding
- **Multiple Hash Algorithms**: Supports SHA256, SHA384, and SHA512
- **Synchronous Invocation**: RequestResponse invocation for immediate results
- **JSON Serialization**: Uses System.Text.Json for efficient serialization

## Prerequisites

- AWS Account with appropriate permissions
- .NET 8.0 runtime
- Valid X.509 certificate with private key (.pfx format)
- AWS Lambda deployment tools (AWS CLI, SAM CLI, or Visual Studio AWS Toolkit)

## Dependencies

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Amazon.Lambda.Core](https://www.nuget.org/packages/Amazon.Lambda.Core/)

- ## Related Projects

- **SigningApp**: Client application that invokes this Lambda function for PDF signing

- - [AWS Lambda .NET Documentation](https://docs.aws.amazon.com/lambda/latest/dg/lambda-csharp.html)
- [RSA Cryptography Documentation](https://docs.microsoft.com/dotnet/api/system.security.cryptography.rsa)

## License

This project is provided as a sample/demo. Please ensure you have appropriate licenses for:
- Telerik Document Processing Library. Check the [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing).