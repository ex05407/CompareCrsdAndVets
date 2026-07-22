using CheckTestProcedures.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CheckTestProcedures.Project
{
    internal sealed class ResourceReader
    {
        public List<ExtractedResource> Read(IReadOnlyList<string> sourcePaths, InputResolution resolution, ConfigAllSettings settings)
        {
            List<ExtractedResource> resources = new List<ExtractedResource>();
            if (sourcePaths == null || sourcePaths.Count == 0)
            {
                return resources;
            }

            foreach (string sourcePath in sourcePaths)
            {
                if (File.Exists(sourcePath) && Path.GetExtension(sourcePath).Equals(".tsv", StringComparison.OrdinalIgnoreCase))
                {
                    resources.AddRange(ReadFromTsv(sourcePath));
                    continue;
                }

                if (File.Exists(sourcePath))
                {
                    resources.AddRange(ReadFromFile(sourcePath));
                    continue;
                }

                if (Directory.Exists(sourcePath))
                {
                    resources.AddRange(ReadFromDirectory(sourcePath));
                }
            }

            return resources;
        }

        private static IEnumerable<ExtractedResource> ReadFromDirectory(string directoryPath)
        {
            List<ExtractedResource> result = new List<ExtractedResource>();
            IEnumerable<string> files = Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                .Where(path => IsTargetFile(path));

            foreach (string file in files)
            {
                result.AddRange(ReadFromFile(file));
            }

            return result;
        }

        private static IEnumerable<ExtractedResource> ReadFromFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (extension == ".xml")
            {
                return ReadFromXml(filePath);
            }

            if (extension == ".bin")
            {
                return new[] { new ExtractedResource { ProcedureName = Path.GetFileNameWithoutExtension(filePath), SourcePath = filePath } };
            }

            return Enumerable.Empty<ExtractedResource>();
        }

        private static IEnumerable<ExtractedResource> ReadFromXml(string filePath)
        {
            XDocument document = XDocument.Load(filePath);
            string procedureName = GetProcedureName(document, filePath);
            ExtractedResource resource = new ExtractedResource
            {
                ProcedureName = procedureName,
                SourcePath = filePath
            };

            foreach (XElement element in document.Descendants().Where(x => !x.HasElements))
            {
                string value = (element.Value ?? string.Empty).Trim();
                string[] pathParts = element.Ancestors().Select(x => x.Name.LocalName).Reverse().ToArray();
                string category = pathParts.Length > 1 ? pathParts[Math.Max(0, pathParts.Length - 4)] : string.Empty;
                string subCategory = pathParts.Length > 2 ? pathParts[Math.Max(0, pathParts.Length - 3)] : string.Empty;
                string group = pathParts.Length > 3 ? pathParts[Math.Max(0, pathParts.Length - 2)] : string.Empty;

                resource.Records.Add(new ResourceRecord
                {
                    ProcedureName = procedureName,
                    Category = category,
                    SubCategory = subCategory,
                    Group = group,
                    Name = element.Name.LocalName,
                    Value = value,
                    FieldName = element.Name.LocalName
                });
            }

            return new[] { resource };
        }

        private static IEnumerable<ExtractedResource> ReadFromTsv(string filePath)
        {
            List<ExtractedResource> resources = new List<ExtractedResource>();
            string[] lines = File.ReadAllLines(filePath, Encoding.GetEncoding(932));
            if (lines.Length <= 1)
            {
                return resources;
            }

            foreach (string line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] columns = line.Split('\t');
                string procedureName = columns.Length > 0 ? columns[0] : Path.GetFileNameWithoutExtension(filePath);
                ExtractedResource resource = resources.FirstOrDefault(x => x.ProcedureName == procedureName);
                if (resource == null)
                {
                    resource = new ExtractedResource
                    {
                        ProcedureName = procedureName,
                        SourcePath = filePath
                    };
                    resources.Add(resource);
                }

                if (columns.Length >= 6)
                {
                    resource.Records.Add(new ResourceRecord
                    {
                        ProcedureName = procedureName,
                        Category = columns[1],
                        SubCategory = columns[2],
                        Group = columns[3],
                        Name = columns[4],
                        Value = columns[5],
                        FieldName = columns[4]
                    });
                }
            }

            return resources;
        }

        private static bool IsTargetFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension == ".xml" || extension == ".bin" || extension == ".tsv";
        }

        private static string GetProcedureName(XDocument document, string filePath)
        {
            XElement root = document.Root;
            if (root != null)
            {
                XAttribute nameAttribute = root.Attributes().FirstOrDefault(x => x.Name.LocalName == "Name");
                if (nameAttribute != null && !string.IsNullOrWhiteSpace(nameAttribute.Value))
                {
                    return nameAttribute.Value.Trim();
                }

                XElement nameElement = root.Elements().FirstOrDefault(x => x.Name.LocalName == "Name");
                if (nameElement != null && !string.IsNullOrWhiteSpace(nameElement.Value))
                {
                    return nameElement.Value.Trim();
                }
            }

            return Path.GetFileNameWithoutExtension(filePath);
        }
    }
}
