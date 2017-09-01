using System;

namespace GenerateDocuments
{
    class Program
    {
        static void Main()
        {
            Console.Write("Choose the format you would like to export to (xlsx/csv/txt/pdf): ");

            string input = Console.ReadLine();

            DocumentGenerator generator = new DocumentGenerator();
            if (!string.IsNullOrEmpty(input))
            {
                generator.SelectedExportFormat = input;
            }

            generator.Generate();

            Console.Write("Done.");
            Console.Read();
        }
    }
}
