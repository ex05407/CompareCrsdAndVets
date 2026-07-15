using CompareCrsdAndVets.Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace CompareCrsdAndVets.Common
{
    internal static class Xml
    {
        public static List<ParameterDefinition> ParseParameters(XElement parametersNode)
        {
            return parametersNode.Elements()
                .Where(x => x.Name.LocalName == "Parameter")
                .Select(x => new ParameterDefinition
                {
                    ParameterId = GetElementValue(x, "ParameterID"),
                    Unit = GetElementValue(x, "Unit"),
                    Value = GetElementValue(x, "Value")
                })
                .ToList();
        }

        public static XElement FindElement(XElement parent, string localName)
        {
            if (parent == null)
            {
                return null;
            }

            return parent.Elements().FirstOrDefault(x => x.Name.LocalName == localName);
        }

        public static string GetElementValue(XElement parent, params string[] localNames)
        {
            XElement current = parent;
            foreach (string localName in localNames)
            {
                current = FindElement(current, localName);
                if (current == null)
                {
                    return string.Empty;
                }
            }

            return current.Value;
        }
    }
}
