using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CompareCrsdAndVets.Common.Message;

namespace CompareCrsdAndVets.Common
{
    internal static class CommonMethods
    {
        /// <summary>
        /// 文字列を数値に変換する。変換できない場合は0を返す
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToInt(string value, int defaultValue = 0)
        {
            int parsed;
            return int.TryParse(value, out parsed) ? parsed : defaultValue;
        }

        public static string CheckInt(string value, string itemName)
        {
            int parsed;
            if(string.IsNullOrEmpty(value)) return string.Empty;
            return int.TryParse(value, out parsed) ? "" : string.Format(Error_NoNumber, itemName);
        }

        /// <summary>
        /// 文字列を数値に変換する。変換できない場合は0を返す
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double ToDouble(string value, double defaultValue = 0)
        {
            double parsed;
            return double.TryParse(
                value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out parsed)
                ? parsed
                : defaultValue;
        }

        public static string CheckDouble(string value, string itemName)
        {
            double parsed;
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return double.TryParse(value, out parsed) ? "" : string.Format(Error_NoNumber, itemName);
        }

        /// <summary>
        /// 文字列を真偽値に変換する。変換できない場合はfalseを返す
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ToBool(string value, bool defaultValue = false)
        {
            bool parsed;
            return bool.TryParse(value, out parsed) ? parsed : defaultValue;
        }

        /// <summary>
        /// 親ディレクトリのパスを取得する
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            if (Directory.Exists(path))
            {
                DirectoryInfo directory = new DirectoryInfo(path);
                return directory.Parent.FullName;
            }

            if (File.Exists(path))
            {
                FileInfo file = new FileInfo(path);
                return file.Directory.FullName;
            }

            return string.Empty;
        }
    }
}
