using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Documents.Fixed.FormatProviders.Ocr;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Windows.Documents.Fixed.Model;
using Telerik.Windows.Documents.Ocr;
using Telerik.Windows.Documents.TesseractOcr;

namespace TesseractOcrProviderDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Requirement for Images in .NET Standard - https://docs.telerik.com/devtools/document-processing/libraries/radpdfprocessing/cross-platform/images
            //FixedExtensibilityManager.ImagePropertiesResolver = new ImagePropertiesResolver();

            TesseractOcrProvider tesseractOcrProvider = new TesseractOcrProvider(".");
            tesseractOcrProvider.LanguageCodes = new List<string>() { "eng" };
            //tesseractOcrProvider.CorrectVerticalPosition = false; //Available only for .NET Standard
            tesseractOcrProvider.DataPath = AppDomain.CurrentDomain.BaseDirectory + @"..\..\";
            tesseractOcrProvider.ParseLevel = OcrParseLevel.Word;

            string imagePath = AppDomain.CurrentDomain.BaseDirectory + @"..\..\images\image.png";

            string imageText = tesseractOcrProvider.GetAllTextFromImage(File.ReadAllBytes(imagePath));
            Dictionary<Rectangle, string> imageTextAndTextDimentions = tesseractOcrProvider.GetTextFromImage(File.ReadAllBytes(imagePath));

            OcrFormatProvider OcProvider = new OcrFormatProvider(tesseractOcrProvider);

            RadFixedDocument document = new RadFixedDocument();

            RadFixedPage page = new RadFixedPage();
            page = OcProvider.Import(new FileStream(imagePath, FileMode.Open), null);
            document.Pages.Add(page);

            string outputPath = "output.pdf";
            File.Delete(outputPath);
            PdfFormatProvider pdfFormatProvider = new PdfFormatProvider();
            using (Stream output = File.OpenWrite(outputPath))
            {
                pdfFormatProvider.Export(document, output, TimeSpan.FromSeconds(10));
            }

            var psi = new ProcessStartInfo()
            {
                FileName = outputPath,
                UseShellExecute = true
            };
            Process.Start(psi);

        }
    }
}
