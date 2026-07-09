using System;
using System.Collections.Generic;
using System.IO;

namespace CompareCrsdAndVets.Common
{
    internal class Logger
    {
        public enum LogType
        {
            Info,
            Warning,
            Error
        }

        /// <summary>
        /// ログフォルダパス
        /// </summary>
        static string LogDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Logger()
        {
            Directory.CreateDirectory(LogDirPath);
        }

        /// <summary>
        /// ログ出力を行う
        /// </summary>
        /// <param name="pMessage"></param>
        /// <param name="pLogType"></param>
        public void OutputLog(string pMessage, LogType pLogType)
        {
            string FileName = $"{pLogType.ToString()}_{DateTime.Now.ToString("yyyyMMdd")}.log";
            string FilePath = Path.Combine(LogDirPath, FileName);

            File.AppendAllLines(
                FilePath,
                new List<string> { string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), " ", pMessage) });
        }
    }
}
