using CheckTestProcedures.Project;
using System;
using System.IO;
using System.Text;

namespace CheckTestProcedures
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.GetEncoding(932);

            CommandLineParser parser = new CommandLineParser();
            if (!parser.TryParse(args, out var options, out var errorMessage))
            {
                WriteUsage(errorMessage);
                return 1;
            }

            try
            {
                CheckTestProceduresRunner runner = new CheckTestProceduresRunner();
                return runner.Run(options);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 3;
            }
        }

        private static void WriteUsage(string errorMessage)
        {
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                Console.Error.WriteLine(errorMessage);
            }

            Console.Error.WriteLine("CheckTestProcedures.exe <inputPath> [-o <outputFilePath>] [-f <formatFilePath>]");
        }
    }
}
