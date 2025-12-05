using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System;
using System.IO;
using Telerik.Windows.Documents.Extensibility;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.Export;

namespace CustomJpegImageConverter
{

    // SixLabors.ImageSharp 3.1.12
    // SixLabors.ImageSharp.Drawing 2.1.7

    internal class CustomJpegImageConverter : JpegImageConverterBase
    {
        public override bool TryConvertToJpegImageData(byte[] imageData, ImageQuality imageQuality, out byte[] jpegImageData)
        {
            jpegImageData = null;

            try
            {
                using (var imageStream = new MemoryStream(imageData))
                {
                    Image image = Image.Load(imageStream);
                    var imageFormat = image.Metadata.DecodedImageFormat;

                    // Handle transparency for PNG
                    if (imageFormat is PngFormat && image.PixelType.BitsPerPixel == 32)
                    {
                        var background = new Image<Rgba32>(image.Width, image.Height, Color.White);
                        background.Mutate(ctx => ctx.DrawImage(image, 1f));

                        image.Dispose(); // Dispose original
                        image = background; // Assign new image
                    }

                    var jpegEncoder = new JpegEncoder
                    {
                        Quality = (int)imageQuality
                    };

                    using (var ms = new MemoryStream())
                    {
                        image.Save(ms, jpegEncoder);
                        jpegImageData = ms.ToArray();
                    }

                    image.Dispose(); // Dispose final image
                }

                return true;
            }
            catch (SixLabors.ImageSharp.UnknownImageFormatException)
            {
                return false;
            }
            catch (SixLabors.ImageSharp.ImageProcessingException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}