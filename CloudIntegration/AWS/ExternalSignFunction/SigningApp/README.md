# SigningApp

A .NET 8 console application that demonstrates how to digitally sign PDF documents using AWS Lambda for external signing operations and Telerik Document Processing libraries.

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

## Project Structure

```
SigningApp/
??? Program.cs                  # Main application entry point
??? LambdaFunctionSigner.cs    # External signer implementation
??? Resources/
?   ??? JohnDoe.crt            # Public key certificate
?   ??? SampleDocument.pdf     # Sample PDF to sign
??? SigningApp.csproj          # Project configuration
```

## Configuration

### AWS Lambda Function

Update the Lambda function details in `Program.cs`:

```csharp
ExternalSignerBase externalSigner = new LambdaFunctionSigner(
    "arn:aws:lambda:YOUR-REGION:YOUR-ACCOUNT:function:YOUR-FUNCTION-NAME", 
    "YOUR-REGION"
);
```

### Certificate

Place your public certificate (.crt file) in the `Resources` folder and update the path in `LambdaFunctionSigner.cs` if needed:

```csharp
string publicKey = "Resources/JohnDoe.crt";
```

### Signature Settings

Customize signature settings in `Program.cs`:

```csharp
signature.Settings.DigestAlgorithm = DigestAlgorithmType.Sha512;
signature.Settings.TimeStampServer = new TimeStampServer(
    "http://timestamp.digicert.com", 
    TimeSpan.FromSeconds(10)
);
```

## How It Works

1. **Import PDF**: Loads a PDF document from the Resources folder
2. **Create Signature Field**: Creates a visual signature field with custom appearance
3. **Configure External Signer**: Sets up the LambdaFunctionSigner to handle signing
4. **Sign Document**: Sends the data to AWS Lambda for signing with the private key
5. **Export PDF**: Saves the signed document and opens it automatically

## Usage

1. Configure your AWS Lambda function ARN and region
2. Place your certificate and sample PDF in the Resources folder
3. Run the application:
   ```
   dotnet run
   ```
4. The signed PDF will be created as `ExternallySignedDocument.pdf` and opened automatically

## Dependencies

- **AWSSDK.Lambda** (3.7.413.3): AWS SDK for Lambda integration
- **Telerik.Documents.Fixed** (2025.4.1104): PDF document processing

## Key Components

### LambdaFunctionSigner

Custom implementation of `ExternalSignerBase` that:
- Communicates with AWS Lambda function via SDK
- Sends data to be signed with digest algorithm selection
- Receives and processes the signature response
- Provides the certificate chain for validation

### Program.cs

Main application flow that:
- Imports the PDF document
- Creates signature field with visual representation
- Configures signing settings
- Exports the signed document

## Security Considerations

- Private keys are stored only in AWS Lambda, never on client machines
- Certificates should be properly secured in AWS
- Use HTTPS for all communication
- Implement proper AWS IAM permissions
- Consider using AWS Secrets Manager for certificate passwords

## Troubleshooting

### Common Issues

1. **Lambda Invocation Failed**: Check AWS credentials and permissions
2. **Certificate Not Found**: Verify the certificate path in Resources folder
3. **Invalid Signature**: Ensure the public and private certificates match
4. **Timeout**: Increase timeout values for Lambda invocation or timestamp server

## Related Projects

- **ExternalSignAWSFunctionApp**: The AWS Lambda function that performs the actual signing

## License

Requires a valid Telerik Document Processing license. See Telerik licensing documentation for details.

## Additional Resources

- [Telerik Document Processing Documentation](https://docs.telerik.com/devtools/document-processing/)
- [AWS Lambda Documentation](https://docs.aws.amazon.com/lambda/)
- [Digital Signatures in PDF](https://www.adobe.com/devnet-docs/acrobatetk/tools/DigSig/)
