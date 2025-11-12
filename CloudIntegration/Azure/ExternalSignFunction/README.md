# External PDF Signing with Azure Functions

This solution demonstrates how to digitally sign PDF documents using **Telerik Document Processing** with an external signing service implemented as an **Azure Function**. This architecture separates the document processing from the cryptographic signing operation, enabling secure, centralized key management.

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

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or later
- Azure Functions Core Tools (for local debugging)
- Valid X.509 certificate with private key (.pfx) and public certificate (.crt)

## Getting Started

### 1. Certificate Setup

The solution uses sample certificates located in the `Resources` folders:

- **SigningApp/Resources/JohnDoe.crt** - Public certificate for verification
- **ExternalSignFunctionApp/Resources/JohnDoe.pfx** - Certificate with private key for signing

For production use, replace these with your own certificates.

### 2. Running the Azure Function

1. Navigate to the `ExternalSignFunctionApp` folder
2. Run the function locally:
   ```bash
   func start
   ```
   Or run it from Visual Studio by setting `ExternalSignFunctionApp` as the startup project.

3. Note the local URL (default: `http://localhost:7062/api/ExternalSign`)

### 3. Running the Signing Application

1. Ensure the Azure Function is running
2. Set `SigningApp` as the startup project
3. Run the application
4. The signed PDF will be generated as `ExternallySignedDocument.pdf` and automatically opened

## Project Structure

### SigningApp

```
SigningApp/
├── Program.cs              # Main application logic
├── MyExternalSigner.cs     # External signer implementation
├── Resources/
│   ├── JohnDoe.crt         # Public certificate
│   └── SampleDocument.pdf  # Input PDF document
└── SigningApp.csproj       # Project file
```

### ExternalSignFunctionApp

```
ExternalSignFunctionApp/
├── Program.cs						 # Azure Functions host configuration
├── ExternalSign.cs					 # HTTP-triggered function for signing
├── Resources/
│   └── JohnDoe.pfx					 # Certificate with private key
├── host.json						 # Function host configuration
├── local.settings.json              # Local development settings
└── ExternalSignFunctionApp.csproj   # Project file
```

## Configuration

### SigningApp Configuration

In `MyExternalSigner.cs`, update the function URL if needed:

```csharp
string functionUrl = "http://localhost:7062/api/ExternalSign";
```

For production, change this to your deployed Azure Function URL.

### Azure Function Configuration

In `ExternalSign.cs`, the certificate is loaded from:

```csharp
string certificateFilePath = "Resources/JohnDoe.pfx";
string certificateFilePassword = "johndoe";
```

**Security Note**: In production, use Azure Key Vault to store certificates and retrieve them securely.

## Security Considerations

⚠️ **Important**: This is a demonstration project. For production use:

1. **Certificate Storage**: Store certificates in Azure Key Vault, not in the file system
2. **Authentication**: Use proper authentication (Azure AD, API keys) instead of anonymous access
3. **HTTPS**: Always use HTTPS for production deployments
4. **Secrets Management**: Use Azure App Configuration or Key Vault for sensitive configuration
5. **Access Control**: Implement proper authorization and access control policies

## NuGet Packages

### SigningApp
- **Telerik.Documents.Fixed** - PDF document processing library

### ExternalSignFunctionApp
- **Microsoft.Azure.Functions.Worker** - Azure Functions runtime
- **Microsoft.Azure.Functions.Worker.Extensions.Http** - HTTP trigger support
- **Microsoft.ApplicationInsights.WorkerService** - Application monitoring

## How It Works

1. **Document Creation**: SigningApp creates or imports a PDF document
2. **Signature Field**: A signature field with visual appearance is added to the document
3. **External Signing**: The `MyExternalSigner` class:
   - Provides the certificate chain for verification
   - Sends data to be signed to the Azure Function
   - Receives the signature bytes back
4. **Cryptographic Operation**: The Azure Function:
   - Receives the data to sign via HTTP POST
   - Loads the private key from the certificate
   - Performs RSA signing with the specified digest algorithm
   - Returns the signature bytes
5. **Document Export**: The signed PDF is saved and displayed

## Supported Digest Algorithms

- SHA256 (default)
- SHA384
- SHA512

The digest algorithm is configured in `Program.cs`:

```csharp
signature.Settings.DigestAlgorithm = DigestAlgorithmType.Sha512;
```

## Troubleshooting

### Common Issues

**Problem**: Connection refused or timeout errors  
**Solution**: Ensure the Azure Function is running before starting SigningApp

**Problem**: Certificate errors  
**Solution**: Verify certificate files exist in the Resources folders and the password is correct

**Problem**: Signature validation fails  
**Solution**: Ensure the .crt and .pfx files are from the same certificate

## Additional Resources

- [Telerik Document Processing Documentation](https://docs.telerik.com/devtools/document-processing/introduction)
- [Azure Functions Documentation](https://docs.microsoft.com/azure/azure-functions/)
- [PDF Digital Signatures](https://docs.telerik.com/devtools/document-processing/libraries/radpdfprocessing/features/digital-signature)
- [Externally Sign a PDF Document](https://docs.telerik.com/devtools/document-processing/libraries/radpdfprocessing/features/digital-signature/external-digital-signing)

## License

This is a sample project for demonstration purposes. Refer to your Telerik Document Processing license for usage terms. Check the [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing).
