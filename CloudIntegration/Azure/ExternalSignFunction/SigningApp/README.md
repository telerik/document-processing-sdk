# SigningApp

A .NET 8 console application that demonstrates PDF digital signing using Telerik Document Processing with an external signing service.

## Purpose

This application showcases how to:
- Import existing PDF documents
- Create digital signature fields with visual representations
- Implement an external signer that delegates cryptographic operations to a remote service
- Configure signature properties (digest algorithm, timestamp server)
- Export signed PDF documents

## Key Components

### Program.cs

The main entry point that orchestrates the PDF signing workflow:

1. **Document Import**: Loads a PDF document from `Resources/SampleDocument.pdf`
2. **Signature Visualization**: Creates a Form XObject with a circle and text to visually represent the signature
3. **Signature Configuration**: Sets up the signature field with:
   - Unique signature name
   - SHA512 digest algorithm
   - DigiCert timestamp server for long-term validity
4. **Signature Widget**: Adds a visual widget at position (200, 600) with size 100x100
5. **Document Export**: Saves the signed document as `ExternallySignedDocument.pdf`

### MyExternalSigner.cs

Custom implementation of `ExternalSignerBase` that:

- **Loads Certificate Chain**: Retrieves the public certificate (.crt) for signature verification
- **Delegates Signing**: Sends data to be signed to an Azure Function via HTTP POST
- **Handles Communication**: Manages the HTTP request/response with the signing service

### Resources

- **JohnDoe.crt**: Public X.509 certificate used for signature verification
- **SampleDocument.pdf**: Sample PDF document to be signed (input file)

## Configuration

### Azure Function URL

The external signing service URL is configured in `MyExternalSigner.cs`:

```csharp
string functionUrl = "http://localhost:7062/api/ExternalSign";
```

**For local development**: Use `http://localhost:7062/api/ExternalSign`  
**For production**: Replace with your deployed Azure Function URL (e.g., `https://your-function-app.azurewebsites.net/api/ExternalSign`)

### Signature Settings

Configured in `Program.cs`:

```csharp
signature.Settings.DigestAlgorithm = DigestAlgorithmType.Sha512;
signature.Settings.TimeStampServer = new TimeStampServer("http://timestamp.digicert.com", TimeSpan.FromSeconds(10));
```

**Digest Algorithms**: Choose from Sha256, Sha384, or Sha512  
**Timestamp Server**: Optional but recommended for long-term signature validity

## Dependencies

This project requires:

- **Telerik.Documents.Fixed**: Provides PDF document processing capabilities
- **.NET 8.0**: Target framework

## Running the Application

### Prerequisites

1. Ensure the ExternalSignFunctionApp is running (either locally or deployed to Azure)
2. Verify the function URL in `MyExternalSigner.cs` matches your running function
3. Ensure the certificate files exist in the Resources folder

### Steps

1. Open the solution in Visual Studio
2. Set SigningApp as the startup project
3. Press F5 or click "Start Debugging"
4. The application will:
   - Import the sample PDF
   - Add a digital signature
   - Save the signed document
   - Automatically open the signed PDF

### Output

- **File**: `ExternallySignedDocument.pdf` in the output directory
- **Content**: The original document with an added digital signature field
- **Signature**: Contains a cryptographic signature created by the external service

## How It Works

### Step-by-Step Process

1. **PDF Import**
   ```csharp
   RadFixedDocument document = ImportDocument(provider);
   ```
   Loads the PDF document using `PdfFormatProvider`

2. **Signature Field Creation**
   ```csharp
   SignatureField signatureField = new SignatureField(signatureName);
   ```
   Creates a new signature field with a unique name

3. **External Signer Setup**
   ```csharp
   ExternalSignerBase externalSigner = new MyExternalSigner();
   Signature signature = new Signature(externalSigner);
   ```
   Initializes the custom external signer

4. **Visual Representation**
   ```csharp
   Form form = new Form();
   FixedContentEditor formEditor = new FixedContentEditor(form.FormSource);
   formEditor.DrawCircle(new Point(50, 50), 20);
formEditor.DrawText(signatureName);
   ```
   Creates a visible signature appearance

5. **Widget Placement**
   ```csharp
   SignatureWidget widget = signatureField.Widgets.AddWidget();
   widget.Rect = new Rect(200, 600, 100, 100);
   page.Annotations.Add(widget);
   ```
   Positions the signature on the PDF page

6. **External Signing**
   - The signer sends data to the Azure Function
   - The function signs the data with the private key
   - The signature bytes are returned and embedded in the PDF

7. **Document Export**
   ```csharp
   provider.Export(document, stream, TimeSpan.FromSeconds(60));
   ```
   Saves the signed PDF with the embedded signature

## Customization

### Change Signature Position

Modify the widget rectangle in `Program.cs`:

```csharp
widget.Rect = new Rect(x, y, width, height);
```

### Change Signature Appearance

Customize the Form XObject drawing:

```csharp
formEditor.DrawCircle(new Point(50, 50), 20);
formEditor.DrawText("Your Text");
// Add images, shapes, or other content
```

### Change Digest Algorithm

Update the signature settings:

```csharp
signature.Settings.DigestAlgorithm = DigestAlgorithmType.Sha256; // or Sha384, Sha512
```

### Use Different Certificate

Replace `Resources/JohnDoe.crt` with your certificate and update the path in `MyExternalSigner.cs`:

```csharp
string publicKey = "Resources/YourCertificate.crt";
```

## Security Notes

🔒 **Certificate Security**:
- This application only contains the **public certificate** (.crt)
- The **private key** remains secure in the Azure Function
- This separation ensures private keys are never exposed to client applications

🌐 **Network Security**:
- For production, always use HTTPS URLs
- Implement proper authentication (API keys, Azure AD)
- Consider using Azure Private Endpoints for enhanced security

## Troubleshooting

### Common Issues

**Error: "Connection refused"**
- Ensure the Azure Function is running
- Verify the URL in `MyExternalSigner.cs` is correct
- Check firewall settings

**Error: "Certificate not found"**
- Verify `Resources/JohnDoe.crt` exists
- Ensure the file is set to "Copy to Output Directory"
- Check the file path is correct

**Error: "Could not load file or assembly"**
- Restore NuGet packages
- Verify Telerik.Documents.Fixed is installed
- Check the Telerik license is valid. Check the [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing).

**Signature validation fails**
- Ensure the .crt file matches the .pfx file in the Azure Function
- Verify the timestamp server is accessible
- Check the digest algorithm matches between client and server

## Further Reading

- [Telerik RadPdfProcessing Digital Signatures](https://docs.telerik.com/devtools/document-processing/libraries/radpdfprocessing/features/digital-signature)
- [Externally Sign a PDF Document](https://docs.telerik.com/devtools/document-processing/libraries/radpdfprocessing/features/digital-signature/external-digital-signing)
- [PDF Signature Widgets](https://docs.telerik.com/devtools/document-processing/libraries/radpdfprocessing/model/interactive-forms/form-fields/signaturefield)
- [Pricing and Licensing FAQ](https://www.telerik.com/faqs/pricing-and-licensing).
