# ExternalSignFunctionApp

An Azure Functions application that provides external digital signing services for PDF documents. This function receives data to be signed, performs cryptographic operations using a stored private key, and returns the signature bytes.

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

#### Run Method

HTTP-triggered function that:
1. Reads the data to be signed from the request body
2. Extracts the digest algorithm from query parameters
3. Calls `SignData()` to perform the cryptographic operation
4. Returns the signature bytes or an error message

**Endpoint**: `POST /api/ExternalSign?digestAlgorithm={algorithm}`

**Request**:
- Method: POST (or GET for testing)
- Body: Raw binary data to be signed
- Query Parameter: `digestAlgorithm` (optional, defaults to SHA256)

**Response**:
- Success (200): Binary signature data
- Error (500): Error message text

#### SignData Method

Performs the actual signing operation:
1. Loads the certificate with private key from `Resources/JohnDoe.pfx`
2. Extracts the RSA private key
3. Selects the appropriate hash algorithm based on the parameter
4. Signs the data using RSA with PKCS#1 padding
5. Returns the signature bytes

### Program.cs

Configures the Azure Functions host with:
- Functions web application configuration
- Application Insights telemetry for monitoring
- Dependency injection setup

### Resources

- **JohnDoe.pfx**: X.509 certificate containing the private key for signing
  - Password: `johndoe` (hardcoded for demo purposes)

## Configuration

### Certificate Configuration

Located in `ExternalSign.cs`:

```csharp
string certificateFilePath = "Resources/JohnDoe.pfx";
string certificateFilePassword = "johndoe";
```

⚠️ **Security Warning**: This configuration is for demonstration only. In production:
- Store certificates in **Azure Key Vault**
- Use **Managed Identity** to access Key Vault
- Never hardcode passwords in source code

### Supported Digest Algorithms

The function supports three hash algorithms:

| Query Parameter Value | Hash Algorithm | Use Case |
|-----------------------|----------------|----------|
| `Sha256` | SHA-256 | Default, good balance of security and performance |
| `Sha384` | SHA-384 | Higher security for sensitive documents |
| `Sha512` | SHA-512 | Maximum security, larger signature size |

If no algorithm is specified, SHA-256 is used by default.

## Local Development

### Prerequisites

1. .NET 8.0 SDK
2. Azure Functions Core Tools v4
3. Visual Studio 2022 or VS Code with Azure Functions extension

### Running Locally

#### Using Azure Functions Core Tools

```bash
cd ExternalSignFunctionApp
func start
```

The function will start on `http://localhost:7062` (default)

#### Using Visual Studio

1. Set `ExternalSignFunctionApp` as the startup project
2. Press F5 or click "Start Debugging"
3. The function will start and display the endpoint URL

### Testing the Function

#### Using cURL

```bash
# Prepare test data
echo "Hello World" > test.txt

# Send signing request
curl -X POST http://localhost:7062/api/ExternalSign?digestAlgorithm=Sha256 \
  --data-binary @test.txt \
  -H "Content-Type: application/octet-stream" \
  -o signature.bin
```

#### Using PowerShell

```powershell
$bytes = [System.Text.Encoding]::UTF8.GetBytes("Hello World")
$response = Invoke-RestMethod -Uri "http://localhost:7062/api/ExternalSign?digestAlgorithm=Sha256" `
  -Method Post `
  -Body $bytes `
  -ContentType "application/octet-stream"
```

## Deployment to Azure

### Using Visual Studio

1. Right-click the `ExternalSignFunctionApp` project
2. Select "Publish"
3. Choose "Azure" as the target
4. Select or create an Azure Function App
5. Click "Publish"

### Using Azure CLI

```bash
# Create a resource group
az group create --name MyResourceGroup --location eastus

# Create a storage account
az storage account create --name mystorageaccount --resource-group MyResourceGroup

# Create a function app
az functionapp create --resource-group MyResourceGroup \
  --consumption-plan-location eastus \
  --runtime dotnet-isolated \
  --runtime-version 8 \
  --functions-version 4 \
  --name MyExternalSignFunction \
  --storage-account mystorageaccount

# Deploy the function
func azure functionapp publish MyExternalSignFunction
```

### Post-Deployment Configuration

After deployment:

1. **Upload Certificate to Azure**:
   - Use Azure Key Vault to store the certificate
   - Grant the Function App access to Key Vault using Managed Identity

2. **Update Code to Use Key Vault**:
   ```csharp
   // Example using Azure.Security.KeyVault.Certificates
   var client = new CertificateClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
   var certificate = await client.GetCertificateAsync(certificateName);
   ```

3. **Configure Authentication**:
   - Change `AuthorizationLevel.Anonymous` to `AuthorizationLevel.Function`
   - Or implement Azure AD authentication

4. **Update Client URL**:
   - Update the URL in `SigningApp\MyExternalSigner.cs` to point to your deployed function

## Security Best Practices

### For Production Deployment

🔐 **Certificate Management**:
- ✅ Store certificates in Azure Key Vault
- ✅ Use Managed Identity for access
- ❌ Never store certificates in source control or file system

🔒 **Authentication & Authorization**:
- ✅ Use Function-level or Azure AD authentication
- ✅ Implement proper authorization checks
- ❌ Don't use anonymous access in production

🌐 **Network Security**:
- ✅ Use HTTPS only
- ✅ Consider Azure Private Endpoints
- ✅ Implement IP restrictions if needed

📝 **Logging & Monitoring**:
- ✅ Use Application Insights for monitoring
- ✅ Log signing operations (without sensitive data)
- ✅ Set up alerts for failures

### Recommended Architecture for Production

```
┌──────────────┐         ┌─────────────────┐         ┌──────────────┐
│ Client App   │────────>│ Azure Function  │────────>│ Key Vault    │
│ (SigningApp) │  HTTPS  │ (with Managed   │  Access │ (Certificate)│
└──────────────┘         │   Identity)     │  Cert   └──────────────┘
                         └─────────────────┘
                                 │
                                 ▼
                        ┌─────────────────┐
                        │ App Insights    │
                        │ (Monitoring)    │
                        └─────────────────┘
```

## Application Insights Integration

The function includes Application Insights for monitoring:

### Key Metrics to Monitor

- **Request Count**: Number of signing operations
- **Response Time**: Performance of signing operations
- **Failure Rate**: Failed signing attempts
- **Dependencies**: Certificate loading operations

### Viewing Telemetry

1. Navigate to your Function App in Azure Portal
2. Click "Application Insights"
3. View metrics, logs, and performance data

## Dependencies

### NuGet Packages

```xml
<PackageReference Include="Microsoft.Azure.Functions.Worker" Version="*" />
<PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Http" Version="*" />
<PackageReference Include="Microsoft.Azure.Functions.Worker.Sdk" Version="*" />
<PackageReference Include="Microsoft.ApplicationInsights.WorkerService" Version="*" />
```

### Framework References

```xml
<FrameworkReference Include="Microsoft.AspNetCore.App" />
```

## Configuration Files

### host.json

Configures the Functions host runtime. Default settings are suitable for most scenarios.

### local.settings.json

Local development settings (not deployed to Azure):

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

## API Reference

### POST /api/ExternalSign

Signs binary data using RSA encryption.

**Parameters**:
- `digestAlgorithm` (query, optional): Hash algorithm to use ("Sha256", "Sha384", "Sha512")

**Request Body**:
- Binary data to be signed

**Response**:
- **200 OK**: Binary signature data
  - Content-Type: `application/octet-stream`
  - Content-Disposition: `attachment; filename=signed-data.bin`
- **500 Internal Server Error**: Error message
  - Content-Type: `text/plain`

**Example Request**:
```http
POST /api/ExternalSign?digestAlgorithm=Sha512 HTTP/1.1
Host: localhost:7062
Content-Type: application/octet-stream
Content-Length: 1024

[binary data]
```

**Example Response**:
```http
HTTP/1.1 200 OK
Content-Type: application/octet-stream
Content-Disposition: attachment; filename=signed-data.bin
Content-Length: 256

[signature bytes]
```

## Troubleshooting

### Common Issues

**Problem**: Certificate not found  
**Solution**: Ensure `Resources/JohnDoe.pfx` exists and is set to "Copy to Output Directory"

**Problem**: Certificate password error  
**Solution**: Verify the password in the code matches your certificate

**Problem**: Function fails to start  
**Solution**: Check Azure Functions Core Tools version (requires v4)

**Problem**: Signature validation fails  
**Solution**: Ensure the digest algorithm matches between client and server

### Debug Logging

Enable detailed logging in `host.json`:

```json
{
  "logging": {
    "logLevel": {
      "default": "Information",
    "Function": "Debug"
    }
  }
}
```

## Performance Considerations

- **Certificate Loading**: The certificate is loaded on each request. Consider caching for production.
- **Timeouts**: Default timeout is 5 minutes. Adjust in `host.json` if needed.
- **Concurrency**: Function can handle multiple concurrent requests.
- **Cold Start**: First request after idle period may be slower (Azure Functions cold start).

## Further Reading

- [Azure Functions Documentation](https://docs.microsoft.com/azure/azure-functions/)
- [Isolated Worker Process](https://docs.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide)
- [Azure Key Vault Integration](https://docs.microsoft.com/azure/key-vault/general/overview)
- [X.509 Certificates in .NET](https://docs.microsoft.com/dotnet/api/system.security.cryptography.x509certificates.x509certificate2)
- [RSA Cryptography](https://docs.microsoft.com/dotnet/api/system.security.cryptography.rsa)
