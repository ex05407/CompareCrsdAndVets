using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using static CompareCrsdAndVets.Common.Xml;
using static CompareCrsdAndVets.Common.CommonMethods;

namespace CompareCrsdAndVets.Class
{
    /// <summary>
    /// QuantitiesUnits.xml 読み込み用クラス
    /// </summary>
    internal class QuantitiesUnitsTable
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public int VersionMajor { get; set; }

        public List<BaseUnitDefinition> BaseUnits { get; private set; } = new List<BaseUnitDefinition>();
        public List<QuantityDefinition> Quantities { get; private set; } = new List<QuantityDefinition>();

        public void LoadXml(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("ファイルパスが空です。", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("指定されたファイルが見つかりません。", filePath);
            }

            BaseUnits.Clear();
            Quantities.Clear();

            XDocument doc = XDocument.Load(filePath);
            XElement root = doc.Root;
            if (root == null)
            {
                throw new InvalidDataException("XMLのルート要素が見つかりません。");
            }

            Name = GetElementValue(root, "VisionStructure", "Administration", "Name");
            Id = GetElementValue(root, "VisionStructure", "Administration", "ID");
            Type = GetElementValue(root, "VisionStructure", "Administration", "Type");
            VersionMajor = ToInt(GetElementValue(root, "VisionStructure", "Administration", "Version", "Major"));

            XElement privateNode = FindElement(root, "Private");
            if (privateNode == null)
            {
                return;
            }

            LoadBaseUnits(privateNode);
            LoadQuantities(privateNode);
        }

        private void LoadBaseUnits(XElement privateNode)
        {
            XElement baseUnitsNode = FindElement(privateNode, "BaseUnits");
            if (baseUnitsNode == null)
            {
                return;
            }

            foreach (XElement baseUnitNode in baseUnitsNode.Elements().Where(x => x.Name.LocalName == "BaseUnit"))
            {
                var baseUnit = new BaseUnitDefinition
                {
                    Name = GetElementValue(baseUnitNode, "Name"),
                    Type = GetElementValue(baseUnitNode, "Type"),
                    SubType = GetElementValue(baseUnitNode, "SubType")
                };

                XElement unitsNode = FindElement(baseUnitNode, "Units");
                if (unitsNode != null)
                {
                    foreach (XElement unitNode in unitsNode.Elements().Where(x => x.Name.LocalName == "Unit"))
                    {
                        baseUnit.Units.Add(new UnitDefinition
                        {
                            Name = GetElementValue(unitNode, "Name"),
                            DisplayName = GetDisplayName(unitNode),
                            Gain = ToDouble(GetElementValue(unitNode, "Gain")),
                            Offset = ToDouble(GetElementValue(unitNode, "Offset"))
                        });
                    }
                }

                BaseUnits.Add(baseUnit);
            }
        }

        private void LoadQuantities(XElement privateNode)
        {
            XElement quantitiesNode = FindElement(privateNode, "Quantities");
            if (quantitiesNode == null)
            {
                return;
            }

            foreach (XElement quantityNode in quantitiesNode.Elements().Where(x => x.Name.LocalName == "Quantity"))
            {
                Quantities.Add(new QuantityDefinition
                {
                    Name = GetElementValue(quantityNode, "Name"),
                    DisplayName = GetDisplayName(quantityNode),
                    VarType = GetElementValue(quantityNode, "VarType"),
                    BaseUnitName = GetElementValue(quantityNode, "BaseUnitName")
                });
            }
        }

        private static string GetDisplayName(XElement node)
        {
            XElement displayNameNode = FindElement(node, "DisplayName");
            if (displayNameNode == null)
            {
                return string.Empty;
            }

            string childValue = displayNameNode.Elements()
                .Select(x => x.Value)
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            if (!string.IsNullOrWhiteSpace(childValue))
            {
                return childValue;
            }

            return displayNameNode.Value;
        }

        #region "単位変換"
        /// <summary>
        /// 単位の変換
        /// </summary>
        /// <param name="pBaseValue"></param>
        /// <param name="pBaseUnitName"></param>
        /// <param name="pConvertUnitName"></param>
        /// <returns></returns>
        public double ConvertUnit(double pBaseValue, string pBaseUnitName, string pConvertUnitName)
        {
            // 速度許容差の基準単位確認
            var speed = Quantities.FirstOrDefault(q => q.Name == pBaseUnitName);
            var baseUnitName = speed?.BaseUnitName;

            // 換算係数を取得
            var baseUnit = BaseUnits.FirstOrDefault(b => b.Name == baseUnitName);
            var kmh = baseUnit?.Units.FirstOrDefault(u => u.Name == pConvertUnitName);

            double gain = kmh?.Gain ?? 1.0;
            double offset = kmh?.Offset ?? 0.0;

            return pBaseValue * gain + offset;
        }
        #endregion
    }

    internal class BaseUnitDefinition
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
        public List<UnitDefinition> Units { get; private set; } = new List<UnitDefinition>();
    }

    internal class UnitDefinition
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public double Gain { get; set; }
        public double Offset { get; set; }
    }

    internal class QuantityDefinition
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string VarType { get; set; }
        public string BaseUnitName { get; set; }
    }
}
