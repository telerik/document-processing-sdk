# External PDF Signing with AWS Lambda

A complete solution for digitally signing PDF documents using AWS Lambda for secure external signing operations with Telerik Document Processing libraries.

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

## Projects

### 1. SigningApp

Client application that creates and signs PDF documents using external signing.

**Technology Stack:**
- .NET 8.0
- Telerik Document Processing
- AWS SDK for Lambda

**Key Features:**
- PDF document creation and manipulation
- Visual signature field generation
- AWS Lambda integration
- Timestamp server support

[?? Full Documentation](SigningApp/README.md)

### 2. ExternalSignAWSFunctionApp

AWS Lambda function that performs the actual signing operation with the private key.

**Technology Stack:**
- .NET 8.0
- AWS Lambda Core
- RSA Cryptography

**Key Features:**
- Secure private key storage
- RSA signature generation
- Multiple hash algorithm support
- JSON request/response handling

[?? Full Documentation](ExternalSignAWSFunctionApp/README.md)

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- AWS Account with appropriate permissions
- Valid X.509 certificate (both .crt and .pfx files)
- Telerik Document Processing license
- AWS CLI (optional, for deployment)

### Quick Start

1. **Deploy Lambda Function**
   ```bash
   cd ExternalSignAWSFunctionApp
   # Follow deployment instructions in ExternalSignAWSFunctionApp/README.md
   ```

2. **Configure Client Application**
   - Update Lambda function ARN in `SigningApp/Program.cs`
   - Place your certificate (.crt) in `SigningApp/Resources/`

3. **Run the Application**
   ```bash
   cd SigningApp
   dotnet run
   ```

## Solution Structure

```
ExternalSignAWSFunctionApp/
??? ExternalSignAWSFunctionApp/         # Lambda function project
?   ??? Function.cs                     # Lambda handler
?   ??? Resources/                      # Certificate storage
?   ?   ??? JohnDoe.pfx                # Private key (not in source control)
?   ??? ExternalSignAWSFunctionApp.csproj
?   ??? README.md
?
??? SigningApp/                         # Client application
?   ??? Program.cs                      # Main application
?   ??? LambdaFunctionSigner.cs        # External signer implementation
?   ??? Resources/                      # Client resources
?   ?   ??? JohnDoe.crt                # Public key certificate
?   ?   ??? SampleDocument.pdf         # Sample PDF
?   ??? SigningApp.csproj
?   ??? README.md
?
??? README.md                          # This file
```

## How It Works

### Signing Flow

1. **Client Prepares Document**
   - SigningApp creates or loads a PDF document
   - Creates signature field with visual representation
   - Prepares data to be signed

2. **Request Signing**
   - Client sends data (Base64-encoded) to Lambda
   - Includes digest algorithm selection (SHA256/384/512)

3. **Lambda Signs Data**
   - Lambda loads private key certificate
   - Extracts RSA private key
   - Signs data using selected hash algorithm
   - Returns Base64-encoded signature

4. **Client Completes Signature**
   - Receives signature from Lambda
   - Embeds signature in PDF document
   - Optionally adds timestamp
   - Saves signed document

### Security Flow

```
Client                   AWS Lambda              Certificate
  ?                          ?                       ?
  ?? Load Public Key ?????????????????????????????????
  ?                          ?                       ?
  ?? Send Data to Sign ???????                       ?
  ?                          ?                       ?
  ?                          ?? Load Private Key ?????
  ?                          ?                       ?
  ?                          ?? Sign Data with RSA ???
  ?                          ?                       ?
  ?? Receive Signature ???????                       ?
  ?                          ?                       ?
  ?? Embed in PDF                                    ?
  ?                                                   ?
  ?? Save Signed Document                            ?
```

## Security Considerations

### Certificate Management

? **DO:**
- Store private keys only in AWS Lambda
- Use AWS Secrets Manager for passwords
- Rotate certificates regularly
- Use strong certificate passwords
- Keep public keys separate from private keys

? **DON'T:**
- Commit .pfx files to source control
- Share private keys with clients
- Use weak certificate passwords
- Store passwords in code

### AWS Security

- Use IAM roles with minimal permissions
- Enable CloudWatch logging
- Implement request throttling
- Use VPC for sensitive deployments
- Enable AWS X-Ray for monitoring

### Application Security

- Validate all inputs
- Use HTTPS for all communication
- Implement proper error handling
- Add request authentication
- Consider API Gateway for additional security layer

## Configuration

### Environment Variables (Lambda)

Consider using environment variables for configuration:

```
CERTIFICATE_PATH=Resources/certificate.pfx
CERTIFICATE_PASSWORD_SECRET=arn:aws:secretsmanager:...
```

### Application Settings (Client)

Update in `SigningApp/Program.cs`:

```csharp
// Lambda configuration
string functionArn = "arn:aws:lambda:region:account:function:name";
string region = "us-east-1";

// Signature settings
signature.Settings.DigestAlgorithm = DigestAlgorithmType.Sha512;
signature.Settings.TimeStampServer = new TimeStampServer("http://timestamp.digicert.com", TimeSpan.FromSeconds(10));
```

## Testing

### Unit Testing Lambda Function

```csharp
[Fact]
public async Task TestSignData()
{
    var function = new Function();
    var input = new Function.Input
    {
        DataToSign = Convert.ToBase64String(testData),
        DigestAlgorithm = "Sha512"
    };
    
    var result = await function.FunctionHandler(input, mockContext);
    Assert.NotNull(result);
}
```

### Integration Testing

1. Deploy Lambda function to AWS
2. Update client with Lambda ARN
3. Run SigningApp
4. Verify signed PDF is created and valid

### Signature Validation

Validate the signed PDF using:
- Adobe Acrobat Reader
- PDF validation tools
- Telerik Document Processing validation APIs

## Performance

### Lambda Cold Start

- First invocation: ~2-5 seconds
- Subsequent invocations: <100ms
- Consider provisioned concurrency for consistent performance

### Optimization Tips

- Use appropriate memory allocation (512MB recommended)
- Minimize certificate file size
- Enable Lambda caching
- Use connection pooling for database scenarios

## Troubleshooting

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Lambda invocation failed | Invalid credentials or permissions | Check AWS credentials and IAM policies |
| Certificate not found | Missing file or incorrect path | Verify certificate exists in Resources folder |
| Invalid signature | Certificate mismatch | Ensure public and private keys match |
| Timeout | Lambda or timestamp server delay | Increase timeout values |
| Memory error | Insufficient Lambda memory | Increase memory allocation to 512MB+ |

### Debug Mode

Enable detailed logging:

```csharp
// In Lambda function
context.Logger.LogLine($"Signing data with {digestAlgorithm}");
```

## Cost Estimation

### Telerik License

- Requires valid Telerik Document Processing license
- See [Telerik Pricing](https://www.telerik.com/purchase)

### AWS Lambda Costs

AWS Lambda pricing is based on:
- **Request charges**: Per million requests
- **Compute charges**: Based on memory allocation and execution duration (GB-seconds)

Actual costs will vary depending on:
- Number of signing operations per month
- Lambda memory configuration (e.g., 512MB, 1024MB)
- Average execution time per signature
- AWS region

For detailed pricing information, refer to the [AWS Lambda Pricing](https://aws.amazon.com/lambda/pricing/) page.

**Cost optimization tips:**
- Use appropriate memory allocation to balance performance and cost
- Optimize cold start times
- Monitor actual usage with CloudWatch
- Consider AWS Free Tier for development and testing

## Monitoring and Logging

### CloudWatch Integration

Monitor key metrics:
- Invocation count
- Error rate
- Duration
- Throttles
- Concurrent executions

### Custom Metrics

```csharp
// Log custom metrics
context.Logger.LogLine($"Signature generated in {duration}ms");
```

## Deployment

### CI/CD Pipeline

Example GitHub Actions workflow:

```yaml
name: Deploy Lambda

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x
      
      - name: Configure AWS Credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: us-east-1
      
      - name: Install Amazon.Lambda.Tools
        run: dotnet tool install -g Amazon.Lambda.Tools
      
      - name: Deploy to Lambda
        working-directory: ./ExternalSignAWSFunction
        run: dotnet lambda deploy-function ExternalSignAWSFunction
```

## License

This solution requires:
- Telerik Document Processing license
- AWS account (charges apply)

## Support and Resources

### Documentation

- [Telerik Document Processing Docs](https://docs.telerik.com/devtools/document-processing/)
- [AWS Lambda .NET Docs](https://docs.aws.amazon.com/lambda/latest/dg/lambda-csharp.html)
- [Digital Signatures in PDF](https://www.adobe.com/devnet-docs/acrobatetk/tools/DigSig/)

### Community

- [Telerik Forums](https://www.telerik.com/forums)
- [AWS Lambda Community](https://forums.aws.amazon.com/forum.jspa?forumID=186)
