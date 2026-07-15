using CompareCrsdAndVets.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CompareCrsdAndVets.Common
{
    internal class DataPerser
    {
        public List<TraceVectorData> ReadMassDataBin(string filePath, int rowCount, List<TraceColumnData> columns)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("ファイルパスが空です。", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("指定されたファイルが見つかりません。", filePath);
            }

            if (rowCount <= 0 || columns == null || columns.Count == 0)
            {
                return new List<TraceVectorData>();
            }

            List<TraceVectorData> vectors = columns.Select(col => new TraceVectorData
            {
                Name = col.Signal,
                Unit = col.Unit
            }).ToList();

            long requiredBytes = (long)rowCount * columns.Count * sizeof(double);
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Length < requiredBytes)
            {
                throw new InvalidDataException($"MassData.bin のデータサイズが不足しています。期待値:{requiredBytes} byte / 実際:{fileInfo.Length} byte");
            }

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {
                    for (int colIndex = 0; colIndex < columns.Count; colIndex++)
                    {
                        vectors[colIndex].TypedData.Add(br.ReadDouble());
                    }
                }
            }

            return vectors;
        }
    }
}
