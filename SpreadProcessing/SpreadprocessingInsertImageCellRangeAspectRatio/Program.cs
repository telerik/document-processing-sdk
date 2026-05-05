// Auto-generated from: Inserting an Image in a Specified Worksheet Cell Range With SpreadProcessing While Preserving Aspect Ratio
// Source: spreadprocessing-insert-image-cell-range-aspect-ratio

using System;
using System.IO;
using Telerik.Windows.Documents.Media;
using Telerik.Windows.Documents.Spreadsheet.Model;
using Telerik.Windows.Documents.Spreadsheet.Model.Shapes;

namespace SpreadprocessingInsertImageCellRangeAspectRatio
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Workbook workbook = new Workbook();
            workbook.Worksheets.Add();

            // Define your target cell range
            int startRow = 0;
            int startColumn = 3;
            int endRow = 10;
            int endColumn = 6;

            Worksheet worksheet = workbook.Worksheets[0];

            // Calculate the total width and height of the cell range
            double rangeWidth = 0;
            for (int col = startColumn; col <= endColumn; col++)
            {
                rangeWidth += worksheet.Columns[col].GetWidth().Value.Value;
            }

            double rangeHeight = 0;
            for (int row = startRow; row <= endRow; row++)
            {
                rangeHeight += worksheet.Rows[row].GetHeight().Value.Value;
            }

            // Create the image
            ImageSource imageSource = new ImageSource(File.ReadAllBytes("image.jpeg"), "jpeg");

            FloatingImage image = new FloatingImage(worksheet, new CellIndex(startRow, startColumn), 0, 0);
            image.ImageSource = imageSource;

            // Calculate the scaling to fit within the range while maintaining aspect ratio
            double imageWidth = image.Width;
            double imageHeight = image.Height;

            double scaleX = rangeWidth / imageWidth;
            double scaleY = rangeHeight / imageHeight;
            double scale = Math.Min(scaleX, scaleY);

            // Use the dimension that hits the boundary first
            if (scaleX < scaleY)
            {
                // Width is the limiting factor
                image.SetWidth(true, rangeWidth);
            }
            else
            {
                // Height is the limiting factor
                image.SetHeight(true, rangeHeight);
            }

            // Add the image to the worksheet
            worksheet.Images.Add(image);

            Console.WriteLine("Image inserted successfully.");
        }
    }
}
