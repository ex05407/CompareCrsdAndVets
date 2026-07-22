using CheckTestProcedures.Class;
using System;
using System.IO;

namespace CheckTestProcedures.Project
{
    internal sealed class CommandLineParser
    {
        public bool TryParse(string[] args, out CommandLineOptions options, out string errorMessage)
        {
            options = new CommandLineOptions();
            errorMessage = string.Empty;

            if (args == null || args.Length == 0)
            {
                errorMessage = "inputPath が指定されていません。";
                return false;
            }

            options.InputPath = args[0];
            if (string.IsNullOrWhiteSpace(options.InputPath))
            {
                errorMessage = "inputPath が指定されていません。";
                return false;
            }

            for (int i = 1; i < args.Length; i++)
            {
                string current = args[i];
                if (string.Equals(current, "-o", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        errorMessage = "-o の値が指定されていません。";
                        return false;
                    }

                    options.OutputFilePath = args[++i];
                    continue;
                }

                if (string.Equals(current, "-f", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        errorMessage = "-f の値が指定されていません。";
                        return false;
                    }

                    options.FormatFilePath = args[++i];
                    continue;
                }

                errorMessage = string.Format("不明な引数です: {0}", current);
                return false;
            }

            if (!File.Exists(options.InputPath) && !Directory.Exists(options.InputPath))
            {
                errorMessage = string.Format("入力パスが存在しません: {0}", options.InputPath);
                return false;
            }

            return true;
        }
    }
}
