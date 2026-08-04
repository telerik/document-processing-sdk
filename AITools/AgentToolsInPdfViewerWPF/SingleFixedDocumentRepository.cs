using System;
using System.Collections.Generic;
using System.IO;
using Telerik.Documents.AI.Tools.Core;
using Telerik.Documents.AI.Tools.Fixed.Core;
using Telerik.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Documents.Fixed.Model;
using Telerik.Documents.Fixed.Model.Editing;
using Telerik.Documents.Model;
using DocumentInfo = Telerik.Documents.AI.Tools.Core.DocumentInfo;

namespace AgentToolsInPdfViewerWPF
{
    /// <summary>
    /// Repository for single-document scenarios.
    /// Wraps one RadFixedDocument in memory. CreateDocument replaces the inner document.
    /// </summary>
    public class SingleFixedDocumentRepository : IFixedDocumentRepository
    {
        private const string DefaultDocumentId = "_current_";

        private readonly PdfFormatProvider pdfFormatProvider = new PdfFormatProvider();
        private RadFixedDocument document;
        private DocumentInfo documentInfo;

        public SingleFixedDocumentRepository(RadFixedDocument document, string documentName = null)
        {
            this.document = document ?? throw new ArgumentNullException(nameof(document));
            this.documentInfo = new DocumentInfo
            {
                Id = DefaultDocumentId,
                Name = documentName ?? "Current Document",
                Format = DocumentFormat.PDF
            };
        }

        public DocumentType DocumentType => DocumentType.FixedDocument;

        public bool SupportsCreation => true;

        public bool SupportsMultipleDocuments => false;

        public RadFixedDocument GetDocument(string documentId = null)
        {
            return this.document;
        }

        public IEnumerable<Telerik.Documents.AI.Tools.Core.DocumentInfo> ListDocuments()
        {
            return new[] { this.documentInfo };
        }

        public string CreateDocument(string documentId, string[] args)
        {
            RadFixedDocument newDocument = new RadFixedDocument();

            string paperTypeString = args != null && args.Length > 0 ? args[0] : "A4"; 
            if (!Enum.TryParse<PaperTypes>(paperTypeString, true, out PaperTypes paperType))
            {
                paperType = PaperTypes.A4;
            }
            using (RadFixedDocumentEditor editor = new RadFixedDocumentEditor(newDocument))
            {
                editor.SectionProperties.PageSize = PaperTypeConverter.ToSize(paperType);
            }

            this.document = newDocument;
            this.documentInfo = new DocumentInfo
            {
                Id = DefaultDocumentId,
                Name = documentId ?? "New Document",
                Format = DocumentFormat.PDF
            };

            return DefaultDocumentId;
        }

        public void Export(string documentId, DocumentFormat format, Stream destinationStream)
        {
            if (format != DocumentFormat.PDF)
            {
                throw new NotSupportedException(
                    $"Format {format} is not supported. Only PDF format is supported.");
            }

            this.pdfFormatProvider.Export(this.document, destinationStream, TimeSpan.FromSeconds(10));
        }

        public object GetDocumentAsObject(string documentId = null)
        {
            return this.GetDocument(documentId);
        }

        public string Import(Stream data, DocumentFormat format, string documentName = null)
        {
            throw new NotSupportedException(
                "SingleFixedDocumentRepository does not support importing documents.");
        }

        public void MergeAndExport(string[] sourceFileIds, Stream stream, DocumentFormat exportFormat)
        {
            throw new NotSupportedException(
                "SingleFixedDocumentRepository does not support merging documents.");
        }

        public bool RemoveDocument(string documentId)
        {
            throw new NotSupportedException(
                "SingleFixedDocumentRepository does not support removing documents.");
        }

        public void Clear()
        {
            throw new NotSupportedException(
                "SingleFixedDocumentRepository does not support clearing documents.");
        }
    }
}