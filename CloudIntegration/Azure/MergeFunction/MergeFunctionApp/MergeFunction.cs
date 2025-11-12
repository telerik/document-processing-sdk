using HttpMultipartParser;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Windows.Documents.Fixed.Model;

namespace FunctionApp1
{
    /// <summary>
    /// Azure Function that provides PDF merging functionality via HTTP endpoint.
    /// Accepts multiple PDF files via multipart form data and returns a single merged PDF document.
    /// </summary>
    public class MergeFunction
    {
        /// <summary>
        /// HTTP-triggered Azure Function that merges multiple PDF files into a single document.
        /// </summary>
        /// <param name="req">The HTTP request containing PDF files as multipart form data.</param>
        /// <returns>
        /// An <see cref="HttpResponseData"/> containing the merged PDF document with HTTP status 200 (OK),
        /// or an error response if the operation fails.
        /// </returns>
        /// <remarks>
        /// This function:
        /// <list type="bullet">
        /// <item><description>Accepts POST requests with PDF files in multipart/form-data format</description></item>
        /// <item><description>Uses Telerik Document Processing to merge the PDFs</description></item>
        /// <item><description>Returns the merged PDF with appropriate Content-Type headers</description></item>
        /// <item><description>Applies a 20-second timeout for PDF export operations</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// Example request using HttpClient:
        /// <code>
        /// using var content = new MultipartFormDataContent();
        /// content.Add(new ByteArrayContent(pdfBytes1), "files", "file1.pdf");
        /// content.Add(new ByteArrayContent(pdfBytes2), "files", "file2.pdf");
        /// var response = await httpClient.PostAsync(functionUrl, content);
        /// </code>
        /// </example>
        [Function("MergeFunction")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            Task<MultipartFormDataParser> parsedFormBody = MultipartFormDataParser.ParseAsync(req.Body);

            PdfFormatProvider provider = new PdfFormatProvider();
            RadFixedDocument result = MergePdfs(provider, parsedFormBody.Result.Files);

            using (MemoryStream outputStream = new MemoryStream())
            {
                provider.Export(result, outputStream, TimeSpan.FromSeconds(20));
                outputStream.Seek(0, SeekOrigin.Begin);

                HttpResponseData httpResponseData = req.CreateResponse(HttpStatusCode.OK);
                httpResponseData.Headers.Add("Content-Type", "application/pdf");
                await outputStream.CopyToAsync(httpResponseData.Body);

                return httpResponseData;
            }
        }

        /// <summary>
        /// Merges multiple PDF documents into a single <see cref="RadFixedDocument"/>.
        /// </summary>
        /// <param name="pdfFormatProvider">The <see cref="PdfFormatProvider"/> used to import PDF files.</param>
        /// <param name="files">A read-only collection of <see cref="FilePart"/> objects representing the PDF files to merge.</param>
        /// <returns>
        /// A <see cref="RadFixedDocument"/> containing all pages from the input PDF files in the order they were provided.
        /// </returns>
        /// <remarks>
        /// The method processes each file sequentially and merges them in the order provided.
        /// Each PDF import operation has a 20-second timeout to prevent long-running operations.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pdfFormatProvider"/> or <paramref name="files"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a file cannot be imported as a valid PDF document.</exception>
        static RadFixedDocument MergePdfs(PdfFormatProvider pdfFormatProvider, IReadOnlyList<FilePart> files)
        {
            RadFixedDocument mergedDocument = new RadFixedDocument();
            foreach (FilePart file in files)
            {
                RadFixedDocument documentToMerge = pdfFormatProvider.Import(file.Data, TimeSpan.FromSeconds(20));
                mergedDocument.Merge(documentToMerge);
            }

            return mergedDocument;
        }
    }
}
