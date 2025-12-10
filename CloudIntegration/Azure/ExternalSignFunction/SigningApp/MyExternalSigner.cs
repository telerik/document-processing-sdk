using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using Telerik.Documents.Fixed.Model.DigitalSignatures;

/// <summary>
/// Custom implementation of an external signer that delegates the signing operation to an Azure Function.
/// This signer loads the certificate chain locally but performs the actual cryptographic signing remotely.
/// </summary>
public class MyExternalSigner : ExternalSignerBase
{
    /// <summary>
    /// Retrieves the X.509 certificate chain used for verification of the digital signature.
    /// </summary>
    /// <returns>An array of X509Certificate2 objects representing the certificate chain.</returns>
    /// <remarks>
    /// This method loads the public certificate (.crt file) from the Resources folder.
    /// The certificate is used for signature verification but does not contain the private key.
    /// </remarks>
    protected override X509Certificate2[] GetCertificateChain()
    {
        string publicKey = "Resources/JohnDoe.crt";

        return [new X509Certificate2(publicKey)];
    }

    /// <summary>
    /// Signs the provided data using an external Azure Function endpoint.
    /// </summary>
    /// <param name="dataToSign">The byte array containing the data to be signed.</param>
    /// <param name="settings">The signature settings containing the digest algorithm and other configuration.</param>
    /// <returns>A byte array containing the cryptographic signature.</returns>
    /// <remarks>
    /// This method sends the data to an Azure Function via HTTP POST request.
    /// The Azure Function performs the actual signing operation using the private key.
    /// The digest algorithm from the settings is passed as a query parameter.
    /// </remarks>
    protected override byte[] SignData(byte[] dataToSign, SignatureSettings settings)
    {
        string digestAlgorithm = settings.DigestAlgorithm.ToString();
        string functionUrl = "http://localhost:7062/api/ExternalSign";
        string url = $"{functionUrl}?digestAlgorithm={Uri.EscapeDataString(digestAlgorithm)}";

        using ByteArrayContent content = new(dataToSign);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        HttpResponseMessage response = PostAsync(url, content).Result;
        response.EnsureSuccessStatusCode();

        return response.Content.ReadAsByteArrayAsync().Result;
    }

    /// <summary>
    /// Asynchronously posts data to the specified URL.
    /// </summary>
    /// <param name="url">The URL of the Azure Function endpoint.</param>
    /// <param name="content">The content to be sent in the POST request.</param>
    /// <returns>A task that represents the asynchronous operation, containing the HTTP response message.</returns>
    private static async Task<HttpResponseMessage> PostAsync(string url, ByteArrayContent content)
    {
        HttpClient httpClient = new HttpClient();

        return await httpClient.PostAsync(url, content);
    }
}