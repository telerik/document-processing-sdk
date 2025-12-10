# SigningApp

A .NET 8 console application that creates PDF documents with digital signatures using an Azure Function for the cryptographic signing operation.

## Purpose

This application showcases how to:
- Import existing PDF documents
- Create digital signature fields with visual representations
- Implement an external signer that delegates cryptographic operations to a remote service
- Configure signature properties (digest algorithm, timestamp server)
- Export signed PDF documents

## Dependencies

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Telerik Document Processing](https://docs.telerik.com/devtools/document-processing/) - Check the [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing)
- [Telerik Digital Signatures Documentation](https://docs.telerik.com/devtools/document-processing/libraries/radpdfprocessing/features/digital-signature)
- [External Digital Signing](https://docs.telerik.com/devtools/document-processing/libraries/radpdfprocessing/features/digital-signature/external-digital-signing)
