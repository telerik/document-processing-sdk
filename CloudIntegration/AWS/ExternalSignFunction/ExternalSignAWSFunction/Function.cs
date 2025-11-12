using Amazon.Lambda.Core;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ExternalSignAWSFunctionApp;

/// <summary>
/// AWS Lambda function that performs digital signature operations for PDF documents.
/// </summary>
public class Function
{
    /// <summary>
    /// Handler for Lambda function invocations.
    /// This method receives data to be signed, signs it using a private key certificate,
    /// and returns the Base64-encoded signature.
    /// </summary>
    /// <param name="input">The input containing the data to sign and digest algorithm.</param>
    /// <param name="context">The Lambda execution context.</param>
    /// <returns>A Base64-encoded string representing the digital signature.</returns>
    /// <exception cref="InvalidOperationException">Thrown when signing operation fails.</exception>
    public async Task<string> FunctionHandler(Input input, ILambdaContext context)
    {
        // Decode the Base64-encoded data back to bytes
        byte[] dataToSign = Convert.FromBase64String(input.DataToSign);

        byte[]? result = SignData(dataToSign, input.DigestAlgorithm);

        if (result != null)
        {
            return Convert.ToBase64String(result);
        }
        else
        {
            throw new InvalidOperationException("Signing operation failed");
        }
    }
    
    /// <summary>
    /// Signs the provided data using RSA private key from a certificate.
    /// </summary>
    /// <param name="dataToSign">The byte array containing the data to be signed.</param>
    /// <param name="digestAlgorithm">The hash algorithm to use (Sha256, Sha384, or Sha512).</param>
    /// <returns>The signed data as a byte array.</returns>
    /// <exception cref="InvalidOperationException">Thrown when certificate does not contain an RSA private key.</exception>
    static byte[]? SignData(byte[] dataToSign, string? digestAlgorithm)
    {
        string certificateFilePassword = "johndoe";
        string certificateFilePath = "Resources/JohnDoe.pfx";

        using (Stream stream = File.OpenRead(certificateFilePath))
        {
            byte[] certRawData = new byte[stream.Length];
            stream.ReadExactly(certRawData, 0, (int)stream.Length);

            using (X509Certificate2 fullCert = new X509Certificate2(certRawData, certificateFilePassword))
            {
                using (RSA? rsa = fullCert.GetRSAPrivateKey())
                {
                    if (rsa == null)
                    {
                        throw new InvalidOperationException("Certificate does not contain an RSA private key");
                    }

                    // Map the digest algorithm string to HashAlgorithmName
                    HashAlgorithmName algorithmName = HashAlgorithmName.SHA256;
                    algorithmName = digestAlgorithm switch
                    {
                        "Sha384" => HashAlgorithmName.SHA384,
                        "Sha512" => HashAlgorithmName.SHA512,
                        _ => HashAlgorithmName.SHA256,
                    };

                    byte[]? bytes = rsa.SignData(dataToSign, algorithmName, RSASignaturePadding.Pkcs1);
                    return bytes;
                }
            }
        }
    }

    /// <summary>
    /// Input model for the Lambda function containing data to sign and algorithm selection.
    /// </summary>
    public class Input
    {
        /// <summary>
        /// Gets or sets the Base64-encoded data to be signed.
        /// </summary>
        public string DataToSign { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the digest algorithm to use for signing (e.g., "Sha256", "Sha384", "Sha512").
        /// </summary>
        public string? DigestAlgorithm { get; set; }
    }
}