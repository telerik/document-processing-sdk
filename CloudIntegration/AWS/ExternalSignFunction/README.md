# External PDF Signing with AWS Lambda

A solution for digitally signing PDF documents using AWS Lambda with Telerik Document Processing, where the private key remains secure in AWS Lambda, separated from the document processing client application.

## Solution Overview

This solution demonstrates a secure architecture for PDF digital signatures where the private signing key remains in AWS Lambda, providing enhanced security by keeping sensitive keys in the cloud rather than on client machines.

### Architecture

```
???????????????????         ????????????????????????
?   SigningApp    ???????????  AWS Lambda          ?
?   (Client)      ? HTTPS   ?  (Signing Service)   ?
?                 ???????????                      ?
? - Telerik DPL   ?         ? - Private Key (.pfx) ?
? - Public Key    ?         ? - RSA Signing        ?
???????????????????         ????????????????????????
```

## Included Projects

- **ExternalSignAWSFunction** - AWS Lambda function that performs cryptographic signing
- **SigningApp** - Client application that creates and signs PDF documents

## Dependencies

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [AWS SDK for .NET](https://aws.amazon.com/sdk-for-net/)
- [Telerik Document Processing](https://docs.telerik.com/devtools/document-processing/) - Check the [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing)
- [AWS Lambda Documentation](https://docs.aws.amazon.com/lambda/)
- [Telerik Digital Signatures Documentation](https://docs.telerik.com/devtools/document-processing/libraries/radpdfprocessing/features/digital-signature)
