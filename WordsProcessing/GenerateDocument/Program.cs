using System;

namespace GenerateDocument
{
    class Program
    {
        static void Main()
        {
            Console.Write("Choose the format you would like to export to (docx/html/rtf/txt/pdf): ");

            string input = Console.ReadLine();

            DocumentGenerator generator = new DocumentGenerator();

            if (!string.IsNullOrEmpty(input))
            {
                generator.SelectedExportFormat = input;
            }

            generator.Generate();

            Console.Write("Generating finished. You can find the document in the application's folder.");
            Console.Read();
        }
    }
}
