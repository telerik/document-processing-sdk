# External PDF Signing with Azure Functions

A solution for digitally signing PDF documents using Azure Functions with Telerik Document Processing, where the private key remains secure in an Azure Function, separated from the document processing client application.

## Included Projects

- **ExternalSignFunctionApp** - Azure Function that performs cryptographic signing
- **SigningApp** - Client application that creates and signs PDF documents

## Overview

The solution consists of two projects:

1. **SigningApp** - A console application that creates and signs PDF documents
2. **ExternalSignFunctionApp** - An Azure Function that performs the cryptographic signing operation

## Architecture

```
┌─────────────────┐                      ┌──────────────────────────┐
│   SigningApp    │                      │  ExternalSignFunctionApp │
│                 │   HTTP POST          │                          │
│  - Creates PDF  │ ────────────────────>│  - Signs data with       │
│  - Adds         │   (data to sign)     │    private key           │
│    signature    │                      │  - Returns signature     │
│    field        │ <────────────────────│                          │
│  - Uses         │   (signature bytes)  │                          │ 
│    external     │                      │                          │
│    signer       │                      │                          │
└─────────────────┘                      └──────────────────────────┘
```

## Key Features

- **External Signing**: Cryptographic operations are performed remotely, keeping private keys secure
- **Certificate Management**: Public certificate (.crt) in the client app, private key (.pfx) in the Azure Function
- **Configurable Digest Algorithms**: Supports SHA256, SHA384, and SHA512
- **Timestamp Support**: Includes timestamp server integration for long-term signature validity
- **Visual Signature**: Creates a visible signature field on the PDF document


## Dependencies

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools v4](https://docs.microsoft.com/azure/azure-functions/functions-run-local)
- [Telerik Document Processing](https://docs.telerik.com/devtools/document-processing/) - Check the [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing)
- [Azure Functions Documentation](https://docs.microsoft.com/azure/azure-functions/)
- [Telerik Digital Signatures Documentation](https://docs.telerik.com/devtools/document-processing/libraries/radpdfprocessing/features/digital-signature)
