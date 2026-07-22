using CheckTestProcedures.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CheckTestProcedures.Project
{
    internal sealed class TsvWriter
    {
        private static readonly Encoding Sjis = Encoding.GetEncoding(932);

        public void WriteAll(string filePath, IReadOnlyList<ExtractedResource> resources)
        {
            List<string[]> rows = new List<string[]>();
            rows.Add(new[] { "テストプロシージャ名", "大項目", "中項目", "小項目", "項目名", "出力内容" });

            foreach (ExtractedResource resource in resources ?? Array.Empty<ExtractedResource>())
            {
                foreach (ResourceRecord record in resource.Records)
                {
                    rows.Add(new[]
                    {
                        record.ProcedureName ?? resource.ProcedureName,
                        record.Category ?? string.Empty,
                        record.SubCategory ?? string.Empty,
                        record.Group ?? string.Empty,
                        record.Name ?? string.Empty,
                        record.Value ?? string.Empty
                    });
                }
            }

            WriteRows(filePath, rows);
        }

        public void WriteSelected(string filePath, IReadOnlyList<ExtractedResource> resources, ConfigSelectedSettings settings)
        {
            List<string[]> rows = new List<string[]>();
            List<ConfigSelectedItem> items = (settings?.Items ?? new List<ConfigSelectedItem>()).OrderBy(x => x.Index).ToList();
            List<string> groupRow = new List<string>();
            List<string> fieldRow = new List<string>();

            foreach (ConfigSelectedItem item in items)
            {
                groupRow.Add(GetGroupName(settings, item));
                fieldRow.Add(item.FieldName ?? string.Empty);
            }

            rows.Add(groupRow.ToArray());
            rows.Add(fieldRow.ToArray());

            foreach (ExtractedResource resource in resources ?? Array.Empty<ExtractedResource>())
            {
                Dictionary<string, string> map = resource.Records
                    .Where(x => !string.IsNullOrWhiteSpace(x.FieldName))
                    .GroupBy(x => x.FieldName)
                    .ToDictionary(x => x.Key, x => x.First().Value ?? string.Empty);

                List<string> values = new List<string> { resource.ProcedureName ?? string.Empty };
                foreach (ConfigSelectedItem item in items)
                {
                    string value;
                    map.TryGetValue(item.FieldName ?? string.Empty, out value);
                    values.Add(value ?? string.Empty);
                }

                rows.Add(values.ToArray());
            }

            WriteRows(filePath, rows);
        }

        private static string GetGroupName(ConfigSelectedSettings settings, ConfigSelectedItem item)
        {
            if (settings == null || item == null)
            {
                return string.Empty;
            }

            ConfigSelectedGroup group = settings.Groups
                .Where(x => x.StartCol.HasValue)
                .OrderBy(x => x.StartCol.Value)
                .LastOrDefault(x => x.StartCol.Value <= item.Index);

            return group == null ? string.Empty : group.Name;
        }

        private static void WriteRows(string filePath, IEnumerable<string[]> rows)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllLines(filePath, rows.Select(row => string.Join("\t", row ?? Array.Empty<string>())), Sjis);
        }
    }
}
