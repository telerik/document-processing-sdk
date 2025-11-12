# SigningApp

A .NET 8 console application that creates PDF documents with digital signatures using an AWS Lambda function for the cryptographic signing operation.

## Overview

This application shows how to implement a secure PDF signing workflow where the actual signing operation is performed by an AWS Lambda function, keeping the private key secure in the cloud rather than on the client machine.

## Features

- **External Signing**: Delegates the signing operation to AWS Lambda
- **PDF Processing**: Uses Telerik Document Processing to create and manipulate PDF documents
- **Digital Signatures**: Adds visible signature fields with custom appearance
- **Timestamp Support**: Includes timestamp server integration for signature validation
- **Security**: Private keys are stored securely in AWS Lambda, not on client machines

## Prerequisites

- .NET 8.0 SDK or later
- AWS Account with Lambda function deployed (see ExternalSignAWSFunctionApp)
- Valid X.509 certificate (public key in .crt format)
- Telerik Document Processing license

## Dependencies

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [AWSSDK.Lambda](https://www.nuget.org/packages/AWSSDK.Lambda/)
- [Telerik Document Processing](https://docs.telerik.com/devtools/document-processing/) - Check the [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing)
- [Telerik Digital Signatures Documentation](https://docs.telerik.com/devtools/document-processing/libraries/radpdfprocessing/features/digital-signature)
- [External Digital Signing](https://docs.telerik.com/devtools/document-processing/libraries/radpdfprocessing/features/digital-signature/external-digital-signing)
- [AWS Lambda SDK Documentation](https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/Lambda/NLambdaClient.html)
