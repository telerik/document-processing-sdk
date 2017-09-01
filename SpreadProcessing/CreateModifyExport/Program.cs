using System;

namespace CreateModifyExport
{
    class Program
    {
        static void Main()
        {
            Console.Write("Choose destination for export or press Enter for the default one: ");
            string input = Console.ReadLine();

            ReportGenerator generator = new ReportGenerator();

            if (!string.IsNullOrEmpty(input))
            {
                generator.ExportDirectory = input;
            }

            generator.ExportReports();
            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}
