using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ExternalSignFunctionApp
{
    /// <summary>
    /// Azure Function that provides external digital signing services for PDF documents.
    /// This function receives data to be signed, performs the cryptographic operation using a stored certificate,
    /// and returns the signature bytes.
    /// </summary>
    public class ExternalSign
    {
        /// <summary>
        /// HTTP-triggered Azure Function that signs data using a private key stored in a certificate.
        /// </summary>
        /// <param name="req">The HTTP request containing the data to be signed in the request body 
        /// and the digest algorithm as a query parameter.</param>
        /// <returns>An HTTP response containing the signed data as binary content or an error message.</returns>
        /// <remarks>
        /// The function accepts both GET and POST requests with anonymous authorization.
        /// The digest algorithm (e.g., "Sha256", "Sha384", "Sha512") should be provided as a query parameter.
        /// </remarks>
        [Function("ExternalSign")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            byte[] dataToSign;
            using (MemoryStream ms = new())
            {
                await req.Body.CopyToAsync(ms);
                dataToSign = ms.ToArray();
            }

            string? digestAlgorithm = req.Query["digestAlgorithm"];
            byte[]? result = SignData(dataToSign, digestAlgorithm);

            HttpResponseData httpResponseData;
            if (result != null)
            {
                httpResponseData = req.CreateResponse(HttpStatusCode.OK);
                httpResponseData.Headers.Add("Content-Type", "application/octet-stream");
                httpResponseData.Headers.Add("Content-Disposition", "attachment; filename=signed-data.bin");
                await httpResponseData.WriteBytesAsync(result);
            }
            else
            {
                httpResponseData = req.CreateResponse(HttpStatusCode.InternalServerError);
                httpResponseData.Headers.Add("Content-Type", "text/plain");
                await httpResponseData.WriteStringAsync("Signing operation failed");
            }

            return httpResponseData;
        }

        /// <summary>
        /// Signs the provided data using RSA encryption with the specified digest algorithm.
        /// </summary>
        /// <param name="dataToSign">The byte array containing the data to be signed.</param>
        /// <param name="digestAlgorithm">The hash algorithm to use (Sha256, Sha384, or Sha512). Defaults to Sha256 if not specified.</param>
        /// <returns>A byte array containing the cryptographic signature, or null if signing fails.</returns>
        /// <remarks>
        /// This method loads the private key from a PFX certificate file stored in the Resources folder.
        /// The certificate password is hardcoded for demonstration purposes only.
        /// In production, use Azure Key Vault or similar secure storage for certificates and passwords.
        /// </remarks>
        static byte[]? SignData(byte[] dataToSign, string? digestAlgorithm)
        {
            string certificateFilePassword = "johndoe";
            string certificateFilePath = "Resources/JohnDoe.pfx";

            using (Stream stream = File.OpenRead(certificateFilePath))
            {
                byte[] certRawData = new byte[stream.Length];
                stream.ReadExactly(certRawData, 0, (int)stream.Length);

                using (X509Certificate2 fullCert = new X509Certificate2(certRawData, certificateFilePassword))
                using (RSA? rsa = fullCert.GetRSAPrivateKey())
                {
                    HashAlgorithmName algorithmName = HashAlgorithmName.SHA256;
                    algorithmName = digestAlgorithm switch
                    {
                        "Sha384" => HashAlgorithmName.SHA384,
                        "Sha512" => HashAlgorithmName.SHA512,
                        _ => HashAlgorithmName.SHA256,
                    };

                    byte[]? bytes = rsa?.SignData(dataToSign, algorithmName, RSASignaturePadding.Pkcs1);
                    return bytes;
                }
            }
        }
    }
}
