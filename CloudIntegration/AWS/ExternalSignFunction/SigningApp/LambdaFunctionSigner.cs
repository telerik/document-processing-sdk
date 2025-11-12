using Amazon.Lambda;
using Amazon.Lambda.Model;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Telerik.Documents.Fixed.Model.DigitalSignatures;

/// <summary>
/// External signer that calls AWS Lambda function to sign PDF documents.
/// This implementation uses the AWS Lambda SDK to invoke a Lambda function synchronously,
/// passing data to be signed and receiving the signature in return.
/// </summary>
public class LambdaFunctionSigner : ExternalSignerBase
{
    private readonly string functionName;
    private readonly AmazonLambdaClient lambdaClient;

    /// <summary>
    /// Creates a new instance of LambdaFunctionSigner.
    /// </summary>
    /// <param name="functionName">The AWS Lambda function name or ARN (e.g., "ExternalSignPdfAWSFunction" or "arn:aws:lambda:region:account:function:name")</param>
    /// <param name="region">The AWS region where the Lambda function is deployed (e.g., "us-east-1", "eu-north-1")</param>
    public LambdaFunctionSigner(string functionName, string region)
    {
        this.functionName = functionName;
        this.lambdaClient = new AmazonLambdaClient(Amazon.RegionEndpoint.GetBySystemName(region));
    }

    /// <summary>
    /// Gets the certificate chain used for signing.
    /// </summary>
    /// <returns>An array of X509Certificate2 objects representing the certificate chain.</returns>
    /// <remarks>
    /// The public certificate is loaded from the Resources folder and should match
    /// the private key certificate stored in the AWS Lambda function.
    /// </remarks>
    protected override X509Certificate2[] GetCertificateChain()
    {
        string publicKey = "Resources/JohnDoe.crt";
        return [new X509Certificate2(publicKey)];
    }

    /// <summary>
    /// Signs the provided data by invoking the AWS Lambda function.
    /// </summary>
    /// <param name="dataToSign">The byte array containing the data to be signed.</param>
    /// <param name="settings">The signature settings containing digest algorithm and other configuration.</param>
    /// <returns>A byte array containing the digital signature.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the Lambda invocation fails or returns an error.</exception>
    /// <remarks>
    /// This method:
    /// 1. Serializes the data and settings to JSON
    /// 2. Invokes the Lambda function synchronously
    /// 3. Validates the response
    /// 4. Returns the decoded signature bytes
    /// </remarks>
    protected override byte[] SignData(byte[] dataToSign, SignatureSettings settings)
    {
        var requestData = new
        {
            DataToSign = Convert.ToBase64String(dataToSign),
            DigestAlgorithm = settings.DigestAlgorithm.ToString()
        };

        string jsonRequest = JsonSerializer.Serialize(requestData, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        // Call the Lambda Function
        var invokeRequest = new InvokeRequest
        {
            FunctionName = this.functionName,
            InvocationType = InvocationType.RequestResponse, // synchronous
            Payload = jsonRequest
        };

        // Parse the response matching the Lambda's SignResponse record
        InvokeResponse invokeResponse = lambdaClient.InvokeAsync(invokeRequest).Result;

        // Check for errors
        if (invokeResponse.StatusCode != 200)
        {
            throw new InvalidOperationException($"Lambda invocation failed with status code: {invokeResponse.StatusCode}");
        }

        if (!string.IsNullOrEmpty(invokeResponse.FunctionError))
        {
            string errorMessage = Encoding.UTF8.GetString(invokeResponse.Payload.ToArray());
            throw new InvalidOperationException($"Lambda function error: {invokeResponse.FunctionError}. Details: {errorMessage}");
        }

        // Parse the response
        string jsonResponse = Encoding.UTF8.GetString(invokeResponse.Payload.ToArray());
        string? base64Signature = JsonSerializer.Deserialize<string>(jsonResponse);

        if (string.IsNullOrEmpty(base64Signature))
        {
            throw new InvalidOperationException("Invalid response from Lambda function");
        }

        return Convert.FromBase64String(base64Signature);
    }
}
