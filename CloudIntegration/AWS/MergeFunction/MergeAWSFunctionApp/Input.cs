namespace MergeAWSFunctionApp
{
    /// <summary>
    /// Represents the input parameters for the PDF merge Lambda function.
    /// </summary>
    public class Input
    {
        /// <summary>
        /// Gets or sets the array of PDF file names to be merged from the S3 bucket.
        /// </summary>
        /// <value>
        /// An array of file names (including extensions) that exist in the specified S3 bucket.
        /// Files will be merged in the order they appear in this array.
        /// </value>
        public required string[] FileNames { get; set; }

        /// <summary>
        /// Gets or sets the name of the AWS S3 bucket containing the source PDF files.
        /// </summary>
        /// <value>
        /// The S3 bucket name where both the source files are located and the merged result will be stored.
        /// </value>
        public required string BucketName { get; set; }
    }
}