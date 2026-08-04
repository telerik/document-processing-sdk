using System.Diagnostics;
using System.IO;
using Telerik.Documents.Flow.FormatProviders.Pdf;
using Telerik.Documents.Flow.Model;
using Telerik.Documents.Flow.Model.Shapes;
using Telerik.Documents.Flow.Model.Styles;

 

namespace CreateHeaderWithLeftCenterRightContent
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Required for .NET Standard when exporting images to PDF format
            //Requires installing Telerik.Documents.ImageUtils NuGet package
            //Telerik.Documents.ImageUtils.ImagePropertiesResolver defaultImagePropertiesResolver = new Telerik.Documents.ImageUtils.ImagePropertiesResolver();
            //Telerik.Documents.Extensibility.FixedExtensibilityManager.ImagePropertiesResolver = defaultImagePropertiesResolver;

            //.NET Standard
            //Telerik.Documents.Core.Fonts.FontWeight regularFontWeight = Telerik.Documents.Core.Fonts.FontWeights.Normal;
            //Telerik.Documents.Core.Fonts.FontWeight boldFontWeight = Telerik.Documents.Core.Fonts.FontWeights.Bold;
            //Telerik.Documents.Primitives.Size size = Telerik.Documents.Model.PaperTypeConverter.ToSize(Telerik.Documents.Model.PaperTypes.A4);
            //Telerik.Documents.Primitives.Padding pageMargins = new Telerik.Documents.Primitives.Padding(40, 40, 40, 40);
            //Telerik.Documents.Core.Fonts.FontStyle italicFontStyle = Telerik.Documents.Core.Fonts.FontStyles.Italic;

            //.NET (Target OS: Windows)
            System.Windows.FontWeight regularFontWeight = System.Windows.FontWeights.Normal;
            System.Windows.FontWeight boldFontWeight = System.Windows.FontWeights.Bold;
            System.Windows.Size size = Telerik.Documents.Model.PaperTypeConverter.ToSize(Telerik.Documents.Model.PaperTypes.A4);
            Telerik.Documents.Primitives.Padding pageMargins = new Telerik.Documents.Primitives.Padding(40, 40, 40, 40);
            System.Windows.FontStyle italicFontStyle = System.Windows.FontStyles.Italic;

            RadFlowDocument document = new RadFlowDocument();
            Telerik.Documents.Flow.Model.Section contentSection = document.Sections.AddSection();
            contentSection.PageMargins = pageMargins;
            contentSection.PageSize =  size;
            contentSection.Blocks.AddParagraph().Inlines.AddRun("Hello RadWordsProcessing!");

            Header header = document.Sections.First().Headers.Add();

            Table table = header.Blocks.AddTable();
            TableRow row = table.Rows.AddTableRow();

            TableCell cell = new TableCell(document);

            Run leftHeader = new Run(document);
            leftHeader.Text = "Left";
            leftHeader.FontWeight = boldFontWeight;
            leftHeader.FontSize = 16;
            cell.Blocks.AddParagraph().Inlines.Add(leftHeader);
            cell.PreferredWidth = new TableWidthUnit(size.Width / 3);
            row.Cells.Add(cell);

            cell = new TableCell(document);
            Paragraph p = cell.Blocks.AddParagraph();

            Run centerHeader = new Run(document);
            centerHeader.Text = "Center";
            centerHeader.FontWeight = regularFontWeight;
            centerHeader.FontStyle = italicFontStyle;
            centerHeader.FontSize = 18;
            p.Inlines.Add(centerHeader);

            p = cell.Blocks.AddParagraph();
            ImageInline imageInline = new ImageInline(document);
            imageInline.Image.Width = 50;
            imageInline.Image.Height = 50;
            byte[] data = File.ReadAllBytes("ProgressNinjas.png");
            imageInline.Image.ImageSource = new Telerik.Documents.Media.ImageSource(data, "png");
            p.Inlines.Add(imageInline);
            cell.PreferredWidth = new TableWidthUnit(size.Width / 3);
            row.Cells.Add(cell);

            cell = new TableCell(document);
            Run rightHeader = new Run(document);
            rightHeader.Text = "Right";
            rightHeader.FontWeight = boldFontWeight;
            rightHeader.FontStyle = italicFontStyle;
            rightHeader.FontSize = 20;
            cell.Blocks.AddParagraph().Inlines.Add(rightHeader);
            cell.PreferredWidth = new TableWidthUnit(size.Width / 3);
            row.Cells.Add(cell);

            Telerik.Documents.Flow.FormatProviders.Pdf.PdfFormatProvider provider = new PdfFormatProvider();

            string outputFilePath = "output.pdf";
            using (Stream output = File.OpenWrite(outputFilePath))
            {
                provider.Export(document, output, TimeSpan.FromSeconds(10));
            }

            Process.Start(new ProcessStartInfo() { FileName = outputFilePath, UseShellExecute = true });
        }
    }
}
