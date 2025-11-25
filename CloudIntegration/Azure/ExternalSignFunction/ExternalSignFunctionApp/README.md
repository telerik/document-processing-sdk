# ExternalSignFunctionApp

An HTTP-triggered Azure Function that receives data to be signed, performs RSA signing with a private key certificate, and returns the signature bytes.

## Purpose

This Azure Function serves as a secure signing service that:
- Keeps private keys isolated from client applications
- Performs RSA digital signing operations
- Supports multiple digest algorithms (SHA256, SHA384, SHA512)
- Provides a simple HTTP-based API for signing operations

## Architecture

This is an **isolated worker process** Azure Function using:
- .NET 8.0
- Azure Functions v4
- HTTP trigger with anonymous authorization (for demo purposes)

## Key Components

### ExternalSign.cs

The main Azure Function that handles signing requests.


## Dependencies

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools v4](https://docs.microsoft.com/azure/azure-functions/functions-run-local)
- [Azure Functions Documentation](https://docs.microsoft.com/azure/azure-functions/)
- [RSA Cryptography Documentation](https://docs.microsoft.com/dotnet/api/system.security.cryptography.rsa)
