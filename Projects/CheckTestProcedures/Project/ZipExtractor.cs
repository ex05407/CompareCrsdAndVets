using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace CheckTestProcedures.Project
{
    internal sealed class ZipExtractor
    {
        public string ExtractWorkingFolder(string archivePath, out IReadOnlyList<string> extractedFiles)
        {
            if (string.IsNullOrWhiteSpace(archivePath))
            {
                throw new ArgumentException("archivePath が未指定です。", nameof(archivePath));
            }

            if (!File.Exists(archivePath))
            {
                throw new FileNotFoundException(string.Format("圧縮ファイルが存在しません: {0}", archivePath), archivePath);
            }

            if (Path.GetExtension(archivePath).Equals(".7z", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("7z 形式の展開は現在の実装では未対応です。");
            }

            string tempFolder = Path.Combine(Path.GetTempPath(), "CheckTestProcedures_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempFolder);
            //ZipFile.ExtractToDirectory(archivePath, tempFolder);
            extractedFiles = new List<string> { tempFolder };
            return tempFolder;
        }
    }
}
