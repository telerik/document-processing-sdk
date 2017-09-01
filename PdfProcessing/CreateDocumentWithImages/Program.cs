using System;
using System.Diagnostics;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.Export;

namespace CreateDocumentWithImages
{
    public class Program
    {
        private static readonly string ResultDirName = AppDomain.CurrentDomain.BaseDirectory + "Demo results/";

        static void Main()
        {
            Console.Write("Choose a value for image quality (1 - High, 2 - Medium, 3 - Low): ");
            string inputQuality = Console.ReadLine();

            ImageQuality imageQuality;

            switch (inputQuality)
            {
                case "1":
                    imageQuality = ImageQuality.High;
                    break;
                case "2":
                    imageQuality = ImageQuality.Medium;
                    break;
                case "3":
                    imageQuality = ImageQuality.Low;
                    break;
                default: imageQuality = ImageQuality.High;
                    break;
            }

            DocumentGenerator generator = new DocumentGenerator(imageQuality);
            generator.SaveFile(ResultDirName);

            Console.WriteLine("The document is saved.");
            Process.Start(ResultDirName);
            Console.Read();
        }
    }
}
