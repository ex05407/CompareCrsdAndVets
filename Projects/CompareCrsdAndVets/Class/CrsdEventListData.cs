using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareCrsdAndVets.Class
{
    internal class CrsdEventListData : CrsdBase
    {
        /// <summary>
        /// イベントファイルリスト
        /// </summary>
        public List<string> EventFileList { get; set; }

        /// <summary>
        /// イベントファイルリストのパス
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// イベントファイルリストのファイル名
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// イベントファイルリスト
        /// </summary>
        public List<CrsdEventData> EventList { get; set; } = new List<CrsdEventData>();

        /// <summary>
        /// elファイルを読み込み、CRSDファイルのデータを格納する
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public void Load(string filePath)
        {
            var valuesBySection = ParseIniLikeFile(filePath);
            FilePath = filePath;
            FileName = System.IO.Path.GetFileName(filePath);

            Dictionary<string, string> eventlist;
            EventFileList = new List<string>();
            if (valuesBySection.TryGetValue("EventList", out eventlist))
            {
                foreach(var kvp in eventlist)
                {
                    EventFileList.Add(kvp.Value);
                }
            }
        }
    }
}
