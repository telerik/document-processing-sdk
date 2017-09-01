using System;

namespace ConvertDocuments
{
    class Program
    {
        static void Main()
        {
            Console.Write("Press Enter for converting a sample document or paste a path to a file you would like to convert: ");
            string input = Console.ReadLine();


            Console.Write("Choose output format (xlsx/csv/txt/pdf): ");
            string format = Console.ReadLine().ToLower();

            DocumentConverter converter = new DocumentConverter();
            if (string.IsNullOrEmpty(input))
            {
                converter.ConvertSampleDocument(format);
            }
            else
            {
                converter.ConvertCustomDocument(input, format);
            }

            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}
