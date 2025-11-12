# ExternalSignAWSFunctionApp

An AWS Lambda function that performs digital signature operations for PDF documents. This function receives data to be signed, signs it using an RSA private key from a certificate, and returns the signature.

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

## Project Structure

```
ExternalSignAWSFunctionApp/
??? Function.cs                        # Lambda function handler
??? Resources/
?   ??? JohnDoe.pfx                   # Private key certificate (not in source control)
??? ExternalSignAWSFunctionApp.csproj # Project configuration
??? README.md                         # This file
```

## Configuration

### Certificate Setup

1. Place your certificate file (.pfx format) in the `Resources` folder
2. Update the certificate details in `Function.cs`:

```csharp
string certificateFilePassword = "your-password";
string certificateFilePath = "Resources/YourCertificate.pfx";
```

**Important**: Never commit certificate passwords or private keys to source control. Use AWS Secrets Manager or environment variables for production deployments.

### Lambda Configuration

Recommended Lambda settings:
- **Runtime**: .NET 8
- **Memory**: 512 MB (minimum)
- **Timeout**: 30 seconds
- **Handler**: `ExternalSignAWSFunctionApp::ExternalSignAWSFunctionApp.Function::FunctionHandler`

## Input Format

The function expects JSON input with the following structure:

```json
{
  "DataToSign": "base64-encoded-data",
  "DigestAlgorithm": "Sha512"
}
```

### Input Properties

- **DataToSign** (required): Base64-encoded byte array of data to be signed
- **DigestAlgorithm** (optional): Hash algorithm to use
  - Valid values: `"Sha256"`, `"Sha384"`, `"Sha512"`
  - Default: `"Sha256"`

## Output Format

The function returns a Base64-encoded string representing the digital signature:

```json
"base64-encoded-signature"
```

## How It Works

1. **Receive Request**: Lambda receives JSON with data to sign and algorithm
2. **Decode Data**: Base64 string is decoded to byte array
3. **Load Certificate**: Private key certificate is loaded from Resources folder
4. **Extract RSA Key**: RSA private key is extracted from certificate
5. **Select Algorithm**: Hash algorithm is selected based on input
6. **Sign Data**: RSA signature is generated using PKCS#1 padding
7. **Encode Result**: Signature bytes are Base64-encoded
8. **Return Response**: Encoded signature is returned as JSON string

## Dependencies

- **Amazon.Lambda.Core** (2.5.0): Core Lambda runtime
- **Amazon.Lambda.Serialization.SystemTextJson** (2.4.4): JSON serialization

## Deployment

### Using AWS CLI

```bash
# Build the project
dotnet build --configuration Release

# Publish the project
dotnet publish -c Release

# Create deployment package
cd bin/Release/net8.0/publish
zip -r deployment.zip .

# Deploy to Lambda
aws lambda update-function-code \
  --function-name ExternalSignPdfAWSFunction \
  --zip-file fileb://deployment.zip
```

### Using AWS SAM

```bash
# Build
sam build

# Deploy
sam deploy --guided
```

### Using Visual Studio AWS Toolkit

1. Right-click the project
2. Select "Publish to AWS Lambda"
3. Follow the deployment wizard

## Security Best Practices

### Certificate Management

- **Never commit** .pfx files or passwords to source control
- Use AWS Secrets Manager to store certificates and passwords
- Rotate certificates regularly
- Use strong passwords for certificate protection

### IAM Permissions

Create minimal IAM role for Lambda execution:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "logs:CreateLogGroup",
        "logs:CreateLogStream",
        "logs:PutLogEvents"
      ],
      "Resource": "arn:aws:logs:*:*:*"
    }
  ]
}
```

### Network Security

- Deploy Lambda in VPC if accessing private resources
- Use VPC endpoints for AWS service communication
- Implement proper security groups and NACLs

### Access Control

- Use IAM policies to restrict who can invoke the function
- Consider using API Gateway with authentication
- Implement request throttling and rate limiting
- Use AWS WAF for additional protection

## Testing

### Local Testing

Use AWS Lambda Mock Test Tool or create a test project:

```csharp
var function = new Function();
var input = new Function.Input 
{ 
    DataToSign = Convert.ToBase64String(testData),
    DigestAlgorithm = "Sha512"
};
var result = await function.FunctionHandler(input, mockContext);
```

### AWS Console Testing

Create a test event in Lambda console:

```json
{
  "DataToSign": "SGVsbG8gV29ybGQh",
  "DigestAlgorithm": "Sha256"
}
```

## Monitoring

### CloudWatch Logs

Lambda automatically logs to CloudWatch. Monitor for:
- Invocation errors
- Certificate loading failures
- Timeout issues
- Memory usage

### CloudWatch Metrics

Key metrics to monitor:
- **Invocations**: Total number of function calls
- **Duration**: Execution time
- **Errors**: Failed invocations
- **Throttles**: Rate-limited requests

### X-Ray Tracing

Enable AWS X-Ray for detailed performance analysis:

```csharp
// Add to project
// <PackageReference Include="Amazon.Lambda.XRayTracing" Version="1.1.0" />
```

## Error Handling

The function throws `InvalidOperationException` in these cases:
- Certificate does not contain RSA private key
- Signing operation fails
- Certificate file not found

Errors are logged to CloudWatch and returned as Lambda error responses.

## Performance Considerations

- **Cold Starts**: First invocation after deployment or idle time will be slower
- **Memory**: More memory = faster CPU, consider 512MB minimum
- **Certificate Loading**: Loaded on each invocation; consider caching for high throughput
- **Provisioned Concurrency**: Use for consistent performance

## Troubleshooting

### Common Issues

1. **Certificate Not Found**
   - Verify file is included in deployment package
   - Check `CopyToOutputDirectory` in .csproj

2. **Invalid Certificate Password**
   - Verify password is correct
   - Check for encoding issues

3. **RSA Key Not Found**
   - Ensure certificate contains private key
   - Verify .pfx format (not .cer or .crt)

4. **Timeout Errors**
   - Increase Lambda timeout setting
   - Check certificate file size

## Cost Considerations

Lambda pricing is based on:
- Number of requests
- Duration of execution
- Memory allocated

Typical costs for this function:
- Request: $0.20 per 1M requests
- Compute: Depends on memory and duration
- Free tier: 1M requests and 400,000 GB-seconds per month

## Related Projects

- **SigningApp**: Client application that invokes this Lambda function for PDF signing

## Additional Resources

- [AWS Lambda .NET Documentation](https://docs.aws.amazon.com/lambda/latest/dg/lambda-csharp.html)
- [RSA Cryptography in .NET](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsa)
- [X.509 Certificates](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates)
- [AWS Lambda Best Practices](https://docs.aws.amazon.com/lambda/latest/dg/best-practices.html)
