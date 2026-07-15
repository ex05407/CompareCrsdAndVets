using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CompareCrsdAndVets.Common;
using static CompareCrsdAndVets.Common.CommonMethods;

namespace CompareCrsdAndVets.Class
{
    /// <summary>
    /// CRSDファイルのデータを格納するクラス
    /// </summary>
    internal class CrsdFileData : CrsdBase
    {
        /// <summary>ファイル名</summary>
        public string FileName { get; set; } = string.Empty;
        /// <summary>Tdファイルのパス</summary>
        public string TdFilePath { get; set; } = string.Empty;
        /// <summary>Td2ファイルのパス</summary>
        public string Td2FilePath { get; set; } = string.Empty;

        /// <summary>単位</summary>
        private string _DisplayUnits;
        /// <summary>単位</summary>
        public string DisplayUnits 
        {
            get 
            {
                if (_DisplayUnits == "KM") return "km/h";
                else if (_DisplayUnits == "MI") return "mph";
                else return string.Empty;
            }
            set { _DisplayUnits = value; }
        }
        /// <summary>イベントリストのディレクトリ</summary>
        public string EventListDir { get; set; }
        /// <summary>イベントリストのデータ</summary>
        public CrsdEventListData EventListData { get; set; } = new CrsdEventListData();
        /// <summary>PhaseType＝"Normal"のフェーズリスト</summary>
        public List<CrsdPhaseData> NormalPhaseList { get => Phases.Where(a => a.PhaseTypeEnum == Const.PhaseType.Normal).ToList(); }

        /// <summary>最初のイベント</summary>
        private CrsdEventData _FirstEvent;
        /// <summary>最初のイベント</summary>
        private CrsdEventData FirstEvent
        {
            get
            {
                if (_FirstEvent != null) return _FirstEvent;
                if (EventListData.EventList.Count == 0) return null;

                foreach (var eventData in EventListData.EventList)
                {
                    if(eventData.BlockTypeEnum == Const.BlockType.Normal || eventData.BlockTypeEnum == Const.BlockType.Warmup)
                    {
                        _FirstEvent = eventData;
                        return _FirstEvent;
                    }
                }
                return null;
            }
        }

        /// <summary>トレース開始</summary>
        public string TraceStartMode
        {
            get
            {
                if (FirstEvent == null) return string.Empty;
                else return FirstEvent.TraceStartMode;
            }
        }

        /// <summary>トレース開始エラー</summary>
        public string TraceStartModeError
        {
            get
            {

                if (FirstEvent == null) return string.Empty;
                else return FirstEvent.TraceStartModeError;
            }
        }
        /// <summary>速度許容差</summary>
        public string SpeedTolerance { get; set; }
        /// <summary>時間許容差</summary>
        public string TimeTolerance { get; set; }

        /// <summary>CRSDファイルのフェーズデータのリスト</summary>
        public List<CrsdPhaseData> Phases { get; private set; } = new List<CrsdPhaseData>();

        /// <summary>CRSDファイルのブロックデータのリスト</summary>
        public List<CrsdBlockData> Blocks { get; private set; } = new List<CrsdBlockData>();

        /// <summary>フェーズ時間に数値が存在するか否か</summary>
        public bool PhaseTimeError { get => Phases.Any(phase => ToDouble(phase.PhaseTime, -1) == -1 ); }

        /// <summary>
        /// Tdファイルを読み込み、CRSDファイルのデータを格納する
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public void LoadTd(string filePath)
        {
            TdFilePath = filePath;
            FileName = Path.GetFileNameWithoutExtension(filePath);

            var valuesBySection = ParseIniLikeFile(filePath);

            Dictionary<string, string> header;
            if (valuesBySection.TryGetValue("Header", out header))
            {
                EventListDir = GetValue(header, "EventListDir");
                DisplayUnits = GetValue(header, "DisplayUnits");
                SpeedTolerance = GetValue(header, "SpeedTolerance");
                TimeTolerance = GetValue(header, "TimeTolerance");
            }

            foreach (var section in valuesBySection
                .Where(x => x.Key.StartsWith("Block", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
            {
                var blockValues = section.Value;

                Blocks.Add(new CrsdBlockData()
                {
                    PhaseNo = GetValue(blockValues, "PhaseNo"),
                    BlockType = GetValue(blockValues, "BlockType"),
                    BlockLength = GetValue(blockValues, "BlockLength"),
                    TraceFile = GetValue(blockValues, "TraceFile")
                });
            }
        }

        /// <summary>
        /// Td2ファイルを読み込み、CRSDファイルのデータを格納する
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public void LoadTd2(string filePath)
        {
            Td2FilePath = filePath;
            Phases.Clear();

            var valuesBySection = ParseIniLikeFile(filePath);

            foreach (var section in valuesBySection
                .Where(x => x.Key.StartsWith("Phase", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
            {
                var phaseValues = section.Value;

                Phases.Add(new CrsdPhaseData
                {
                    PhaseNo = GetValue(phaseValues, "PhaseNo"),
                    PhaseType = GetValue(phaseValues, "PhaseType"),
                    PhaseTime = GetValue(phaseValues, "PhaseTime"),
                    PhaseDistance = GetValue(phaseValues, "PhaseDistance"),
                    BagPairNo = GetValue(phaseValues, "BagPairNo"),
                    WaitStart = GetValue(phaseValues, "WaitStart"),
                    WaitConti = GetValue(phaseValues, "WaitConti"),
                    SoakMinTime = GetValue(phaseValues, "SoakMinTime"),
                    SoakMaxTime = GetValue(phaseValues, "SoakMaxTime")
                });
            }
        }

        /// <summary>
        /// 値の取得
        /// </summary>
        /// <param name="values"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetValue(Dictionary<string, string> values, string key)
        {
            string value;
            return values.TryGetValue(key, out value) ? value : string.Empty;
        }
    }
}
