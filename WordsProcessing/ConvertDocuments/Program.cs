using System;

namespace ConvertDocuments
{
    class Program
    {
        static void Main()
        {
            Console.Write("Press Enter for converting a sample document or paste a path to a file you would like to convert: ");
            string input = Console.ReadLine();
            
            Console.Write("Choose output format (docx/html/rtf/txt/pdf): ");
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

            Console.WriteLine("Document converted. You can find the result in the application's folder.");
            Console.ReadKey();
        }
    }
}
