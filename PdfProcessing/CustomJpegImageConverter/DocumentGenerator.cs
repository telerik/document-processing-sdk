﻿using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Threading;
using Telerik.Documents.Primitives;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.Export;
using Telerik.Windows.Documents.Fixed.Model;
using Telerik.Windows.Documents.Fixed.Model.ColorSpaces;
using Telerik.Windows.Documents.Fixed.Model.Editing;
using Telerik.Windows.Documents.Fixed.Model.Editing.Flow;
using Telerik.Windows.Documents.Fixed.Model.Resources;

namespace CustomJpegImageConverter
{
    internal class DocumentGenerator
    {
        public static readonly Size PageSize = new Size(Telerik.Windows.Documents.Media.Unit.MmToDip(210), Telerik.Windows.Documents.Media.Unit.MmToDip(297));
        public static readonly Thickness Margins = new Thickness(Telerik.Windows.Documents.Media.Unit.MmToDip(10));
        public static readonly Size RemainingPageSize = new Size(PageSize.Width - Margins.Left - Margins.Right, PageSize.Height - Margins.Top - Margins.Bottom);

        private readonly ImageSource imageSource;
        private readonly ImageQuality imageQuality;
        private readonly RadFixedDocument document;

        public DocumentGenerator(ImageSource imageSource, ImageQuality imageQuality)
        {
            this.imageSource = imageSource;
            this.imageQuality = imageQuality;
            this.document = new RadFixedDocument();

            this.AddPageWithImage();
        }

        private void AddPageWithImage()
        {
            RadFixedPage page = this.document.Pages.AddPage();
            page.Size = PageSize;
            FixedContentEditor editor = new FixedContentEditor(page);
            editor.GraphicProperties.StrokeThickness = 0;
            editor.GraphicProperties.IsStroked = false;
            editor.GraphicProperties.FillColor = new RgbColor(200, 200, 200);
            editor.DrawRectangle(new Rect(0, 0, PageSize.Width, PageSize.Height));
            Thickness thickness = new Thickness(100);
            editor.Position.Translate(thickness.Left, thickness.Top);

            Block block = new Block();
            block.HorizontalAlignment = HorizontalAlignment.Center;
            block.TextProperties.FontSize = 22;
            block.InsertText("Image converted from PNG to JPG");
            block.InsertLineBreak();
            block.InsertText(string.Format("when ImageQuality set to {0}", this.imageQuality));
            Size blockSize = block.Measure(RemainingPageSize, CancellationToken.None);
            editor.DrawBlock(block, RemainingPageSize);

            editor.Position.Translate(thickness.Left, blockSize.Height + thickness.Top + 20);

            Block imageBlock = new Block();
            imageBlock.HorizontalAlignment = HorizontalAlignment.Center;
            imageBlock.InsertImage(this.imageSource);
            editor.DrawBlock(imageBlock, RemainingPageSize);
        }

        internal void SaveFileAndPreview()
        {
            string resultFileName = string.Format("ExportedDocument_ImageQuality-{0}.pdf", this.imageQuality);
            if (File.Exists(resultFileName))
            {
                File.Delete(resultFileName);
            }

            PdfFormatProvider provider = new PdfFormatProvider();
            using (Stream stream = File.OpenWrite(resultFileName))
            {
                provider.ExportSettings.ImageQuality = this.imageQuality;
                provider.Export(this.document, stream, TimeSpan.FromSeconds(15));
            }

            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = resultFileName,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}