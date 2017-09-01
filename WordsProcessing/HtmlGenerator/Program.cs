using System;

namespace HtmlGenerator
{
    class Program
    {
        static void Main()
        {
            DocumentGenerator generator = new DocumentGenerator();
            generator.Generate();

            Console.Write("Generating finished. You can find the document in the application's folder.");
            Console.Read();
        }
    }
}
