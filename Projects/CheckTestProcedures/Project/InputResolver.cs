using CheckTestProcedures.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CheckTestProcedures.Project
{
    internal sealed class InputResolver
    {
        public InputResolution Resolve(string inputPath)
        {
            if (string.IsNullOrWhiteSpace(inputPath))
            {
                throw new ArgumentException("inputPath が未指定です。", nameof(inputPath));
            }

            if (File.Exists(inputPath))
            {
                return ResolveFile(inputPath);
            }

            if (Directory.Exists(inputPath))
            {
                return ResolveDirectory(inputPath);
            }

            throw new FileNotFoundException(string.Format("入力パスが存在しません: {0}", inputPath), inputPath);
        }

        public string ResolveOutputBaseName(string inputPath, string outputFilePath, InputResolution resolution)
        {
            if (!string.IsNullOrWhiteSpace(outputFilePath))
            {
                return Path.GetFileNameWithoutExtension(outputFilePath);
            }

            if (resolution != null && !string.IsNullOrWhiteSpace(resolution.OutputBaseName))
            {
                return resolution.OutputBaseName;
            }

            return Path.GetFileNameWithoutExtension(inputPath);
        }

        private static InputResolution ResolveFile(string inputPath)
        {
            string extension = Path.GetExtension(inputPath).ToLowerInvariant();
            if (extension == ".tsv")
            {
                return new InputResolution
                {
                    Kind = InputKind.Tsv,
                    InputPath = inputPath,
                    ResolvedRootPath = Path.GetDirectoryName(inputPath),
                    OutputBaseName = Path.GetFileNameWithoutExtension(inputPath),
                    SourceFiles = new List<string> { inputPath }
                };
            }

            if (extension == ".zip" || extension == ".7z")
            {
                return new InputResolution
                {
                    Kind = InputKind.ZipArchive,
                    InputPath = inputPath,
                    ResolvedRootPath = Path.GetDirectoryName(inputPath),
                    OutputBaseName = Path.GetFileNameWithoutExtension(inputPath),
                    SourceFiles = new List<string> { inputPath }
                };
            }

            return new InputResolution
            {
                Kind = InputKind.Unknown,
                InputPath = inputPath,
                ResolvedRootPath = Path.GetDirectoryName(inputPath),
                OutputBaseName = Path.GetFileNameWithoutExtension(inputPath),
                SourceFiles = new List<string> { inputPath }
            };
        }

        private static InputResolution ResolveDirectory(string inputPath)
        {
            string[] archives = Directory.GetFiles(inputPath, "*.zip")
                .Concat(Directory.GetFiles(inputPath, "*.7z"))
                .ToArray();

            if (archives.Length > 0)
            {
                return new InputResolution
                {
                    Kind = InputKind.FolderWithZip,
                    InputPath = inputPath,
                    ResolvedRootPath = inputPath,
                    OutputBaseName = new DirectoryInfo(inputPath).Name,
                    SourceFiles = archives.ToList()
                };
            }

            string dataFolder = Path.Combine(inputPath, "DATA");
            if (Directory.Exists(dataFolder))
            {
                return new InputResolution
                {
                    Kind = InputKind.CustBackupFolder,
                    InputPath = inputPath,
                    ResolvedRootPath = inputPath,
                    OutputBaseName = new DirectoryInfo(inputPath).Name,
                    SourceFiles = new List<string> { inputPath }
                };
            }

            return new InputResolution
            {
                Kind = InputKind.Unknown,
                InputPath = inputPath,
                ResolvedRootPath = inputPath,
                OutputBaseName = new DirectoryInfo(inputPath).Name,
                SourceFiles = new List<string> { inputPath }
            };
        }
    }
}
