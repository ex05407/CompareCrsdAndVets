using CheckTestProcedures.Class;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CheckTestProcedures.Project
{
    internal sealed class ConfigReader
    {
        public ConfigAllSettings LoadAll(string filePath)
        {
            XDocument document = LoadDocument(filePath);
            ConfigAllSettings settings = new ConfigAllSettings();

            foreach (XElement item in document.Descendants().Where(x => x.Name.LocalName == "Item"))
            {
                settings.Items.Add(new ConfigAllItem
                {
                    ConfigType = GetValue(item, "ConfigType"),
                    Name = GetValue(item, "Name"),
                    Title = GetValue(item, "Title"),
                    FieldType = GetValue(item, "FieldType")
                });
            }

            return settings;
        }

        public ConfigSelectedSettings LoadSelected(string filePath)
        {
            XDocument document = LoadDocument(filePath);
            ConfigSelectedSettings settings = new ConfigSelectedSettings();

            XElement groups = document.Descendants().FirstOrDefault(x => x.Name.LocalName == "Groups");
            if (groups != null)
            {
                foreach (XElement item in groups.Elements().Where(x => x.Name.LocalName == "Group"))
                {
                    settings.Groups.Add(new ConfigSelectedGroup
                    {
                        Index = ParseInt(GetValue(item, "Index")),
                        StartCol = ParseNullableInt(GetValue(item, "StartCol")),
                        Name = GetValue(item, "Name")
                    });
                }
            }

            XElement items = document.Descendants().FirstOrDefault(x => x.Name.LocalName == "Items");
            if (items != null)
            {
                foreach (XElement item in items.Elements().Where(x => x.Name.LocalName == "Item"))
                {
                    settings.Items.Add(new ConfigSelectedItem
                    {
                        Index = ParseInt(GetValue(item, "Index")),
                        Category = GetValue(item, "Category"),
                        SubCategory = GetValue(item, "SubCategory"),
                        Group = GetValue(item, "Group"),
                        Name = GetValue(item, "Name"),
                        FieldName = GetValue(item, "FieldName")
                    });
                }
            }

            return settings;
        }

        private static XDocument LoadDocument(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("設定ファイルが指定されていません。", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format("設定ファイルが存在しません: {0}", filePath), filePath);
            }

            return XDocument.Load(filePath);
        }

        private static string GetValue(XElement element, string name)
        {
            XElement child = element.Elements().FirstOrDefault(x => x.Name.LocalName == name);
            return child == null ? string.Empty : (child.Value ?? string.Empty).Trim();
        }

        private static int ParseInt(string value)
        {
            int parsed;
            return int.TryParse(value, out parsed) ? parsed : 0;
        }

        private static int? ParseNullableInt(string value)
        {
            int parsed;
            return int.TryParse(value, out parsed) ? (int?)parsed : null;
        }
    }
}
