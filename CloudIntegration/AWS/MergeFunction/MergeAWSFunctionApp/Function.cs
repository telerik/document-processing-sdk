using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Windows.Documents.Fixed.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MergeAWSFunctionApp;

/// <summary>
/// AWS Lambda function that merges multiple PDF files from an S3 bucket into a single PDF document.
/// </summary>
/// <remarks>
/// This function uses Telerik Document Processing Library to merge PDF documents stored in AWS S3.
/// The merged document is saved back to the same S3 bucket with a unique generated filename.
/// </remarks>
public class Function
{
    /// <summary>
    /// The AWS region endpoint where the S3 bucket is located.
    /// </summary>
    Amazon.RegionEndpoint AmazonRegion = Amazon.RegionEndpoint.EUNorth1;

    /// <summary>
    /// Lambda function handler that processes PDF merge requests.
    /// </summary>
    /// <param name="input">The input object containing file names and bucket information for the merge operation.</param>
    /// <param name="context">The Lambda context that provides methods for logging and describing the Lambda environment.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the filename of the merged PDF in the S3 bucket.</returns>
    /// <exception cref="Amazon.S3.AmazonS3Exception">Thrown when there are issues accessing the S3 bucket or files.</exception>
    /// <remarks>
    /// The function performs the following steps:
    /// 1. Downloads all specified PDF files from the S3 bucket
    /// 2. Merges them into a single PDF document using Telerik Document Processing
    /// 3. Uploads the merged PDF back to the same S3 bucket with a unique filename
    /// 4. Returns the generated filename for the merged PDF
    /// </remarks>
    public async Task<string> FunctionHandler(Input input, ILambdaContext context)
    {
        AmazonS3Config config = new AmazonS3Config()
        {
            RegionEndpoint = AmazonRegion
        };

        // Get source files from Amazon S3.
        using AmazonS3Client client = new AmazonS3Client(config);
        List<Task<GetObjectResponse>> getResponses = new List<Task<GetObjectResponse>>();

        PdfFormatProvider provider = new PdfFormatProvider();
        RadFixedDocument merged = await MergePdfs(provider, input, client);

        using MemoryStream resultStream = new MemoryStream();
        provider.Export(merged, resultStream, TimeSpan.FromSeconds(20));

        // Save the resulting file to Amazon S3
        var pdfFileName = $"Merged_{Guid.NewGuid()}.pdf";
        PutObjectResponse response = await client.PutObjectAsync(new PutObjectRequest()
        {
            BucketName = input.BucketName,
            Key = pdfFileName,
            InputStream = resultStream,
            ContentType = "application/pdf"
        });

        return pdfFileName;
    }

    /// <summary>
    /// Merges multiple PDF files from S3 into a single RadFixedDocument.
    /// </summary>
    /// <param name="provider">The PDF format provider used for importing PDF documents.</param>
    /// <param name="input">The input object containing the file names and bucket information.</param>
    /// <param name="client">The Amazon S3 client used to retrieve files from the bucket.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the merged RadFixedDocument.</returns>
    /// <exception cref="Amazon.S3.AmazonS3Exception">Thrown when a file cannot be retrieved from the S3 bucket.</exception>
    /// <remarks>
    /// Files are merged in the order they appear in the input.FileNames array.
    /// Each PDF is imported with a 20-second timeout limit.
    /// </remarks>
    private async Task<RadFixedDocument> MergePdfs(PdfFormatProvider provider, Input input, AmazonS3Client client)
    {
        RadFixedDocument mergedDocument = new RadFixedDocument();

        foreach ((string value, int index) fileName in input.FileNames.Select((value, index) => (value, index)))
        {
            GetObjectRequest getRequest = new GetObjectRequest()
            {
                BucketName = input.BucketName,
                Key = fileName.value,
            };

            GetObjectResponse file = await client.GetObjectAsync(getRequest);
            using (MemoryStream stream = new MemoryStream())
            {
                await file.ResponseStream.CopyToAsync(stream);

                RadFixedDocument documentToMerge = provider.Import(stream, TimeSpan.FromSeconds(20));
                mergedDocument.Merge(documentToMerge);
            }
        }

        return mergedDocument;
    }
}

/// <summary>
/// Represents a casing transformation result with both lower and upper case versions.
/// </summary>
/// <param name="Lower">The lowercase version of the text.</param>
/// <param name="Upper">The uppercase version of the text.</param>
/// <remarks>
/// This record appears to be unused in the current implementation and may be a remnant from template code.
/// </remarks>
public record Casing(string Lower, string Upper);