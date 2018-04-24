using System;
using System.Diagnostics;

namespace GenerateDocument
{
    public class Program
    {
        private static readonly string ResultDirName = AppDomain.CurrentDomain.BaseDirectory + "Demo results/";

        public static void Main()
        {
            DocumentGenerator generator = new DocumentGenerator();
            generator.Export(ResultDirName);
        }
    }
}