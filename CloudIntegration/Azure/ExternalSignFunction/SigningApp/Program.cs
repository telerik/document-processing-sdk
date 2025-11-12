using System.Diagnostics;
using Telerik.Documents.Fixed.Model.DigitalSignatures;
using Telerik.Documents.Primitives;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Windows.Documents.Fixed.Model;
using Telerik.Windows.Documents.Fixed.Model.Annotations;
using Telerik.Windows.Documents.Fixed.Model.DigitalSignatures;
using Telerik.Windows.Documents.Fixed.Model.Editing;
using Telerik.Windows.Documents.Fixed.Model.InteractiveForms;
using Telerik.Windows.Documents.Fixed.Model.Objects;
using Telerik.Windows.Documents.Fixed.Model.Resources;

PdfFormatProvider provider = new PdfFormatProvider();
RadFixedDocument document = ImportDocument(provider);

// The name of the signature must be unique.  
string signatureName = "SampleSignature";

// This is the Form XObject element that represents the contents of the signature field.  
Form form = new Form();
form.FormSource = new FormSource();
form.FormSource.Size = new Size(120, 120);

// We will use the editor to fill the Form XObject.  
FixedContentEditor formEditor = new FixedContentEditor(form.FormSource);
formEditor.DrawCircle(new Point(50, 50), 20);
formEditor.DrawText(signatureName);

// The Signature object is added to a signature field, so we can add a visualization to it.  
SignatureField signatureField = new SignatureField(signatureName);
ExternalSignerBase externalSigner = new MyExternalSigner();
Signature signature = new Signature(externalSigner);
signature.Settings.DigestAlgorithm = DigestAlgorithmType.Sha512;
signature.Settings.TimeStampServer = new TimeStampServer("http://timestamp.digicert.com", TimeSpan.FromSeconds(10));
signatureField.Signature = signature;

// The widget contains the Form XObject and defines the appearance of the signature field.  
SignatureWidget widget = signatureField.Widgets.AddWidget();
widget.Rect = new Rect(200, 600, 100, 100);
widget.Border = new AnnotationBorder(100, AnnotationBorderStyle.Solid, null);
widget.Content.NormalContentSource = form.FormSource;

// The Widget class inherits from Annotation. And, as any other annotation, must be added to the respective collection of the page.  
RadFixedPage page = document.Pages.AddPage();
page.Annotations.Add(widget);
document.AcroForm.FormFields.Add(signatureField);

// Add Signature flags 
document.AcroForm.SignatureFlags = SignatureFlags.AppendOnly;

ExportDocument(provider, document);

RadFixedDocument ImportDocument(PdfFormatProvider provider)
{
    using (FileStream stream = File.OpenRead("Resources/SampleDocument.pdf"))
    {
        return provider.Import(stream, TimeSpan.FromSeconds(20));
    }
}

void ExportDocument(PdfFormatProvider provider, RadFixedDocument document)
{
    string outputPath = "ExternallySignedDocument.pdf";
    File.Delete(outputPath);

    using (FileStream stream = File.Create(outputPath))
    {
        provider.Export(document, stream, TimeSpan.FromSeconds(60));
    }

    Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
}