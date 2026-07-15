using CompareCrsdAndVets.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareCrsdAndVets.Class
{
    internal class CrsdBase
    {


        /// <summary>
        /// ファイルを解析して、セクションごとのキーと値の辞書を返す
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        protected static Dictionary<string, Dictionary<string, string>> ParseIniLikeFile(string filePath, bool ConmaFlg = false)
        {
            var valuesBySection = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            string currentSection = string.Empty;
            int i = 1;

            foreach (string rawLine in File.ReadLines(filePath, Const.SJiEnc))
            {
                string line = rawLine.Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith(";") || line.StartsWith("#"))
                {
                    continue;
                }

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentSection = line.Substring(1, line.Length - 2);
                    if (!valuesBySection.ContainsKey(currentSection))
                    {
                        valuesBySection[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }

                    continue;
                }

                int separatorIndex = line.IndexOf('=');
                int separatorIndexComma = line.IndexOf(',');
                if (string.IsNullOrEmpty(currentSection))
                {
                    continue;
                }
                else if(ConmaFlg && separatorIndexComma > 0)
                {
                    string key = $"data{i}";
                    string value = line.Substring(separatorIndexComma + 1).Trim();
                    valuesBySection[currentSection][key] = value;
                    i++;
                }
                else if(separatorIndex <= 0)
                {
                    string key = $"data{i}";
                    string value = line.Trim();
                    valuesBySection[currentSection][key] = value;
                    i++;
                }
                else
                {
                    string key = line.Substring(0, separatorIndex).Trim();
                    string value = line.Substring(separatorIndex + 1).Trim();
                    valuesBySection[currentSection][key] = value;
                }
            }

            return valuesBySection;
        }
    }
}
