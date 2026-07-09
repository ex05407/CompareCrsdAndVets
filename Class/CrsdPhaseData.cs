using CompareCrsdAndVets.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CompareCrsdAndVets.Common.CommonMethods;
using static CompareCrsdAndVets.Common.Const;
using static CompareCrsdAndVets.Common.Message;

namespace CompareCrsdAndVets.Class
{
    /// <summary>
    /// CRSDファイルのフェーズ情報を格納するクラス
    /// </summary>
    internal class CrsdPhaseData
    {
        /// <summary>フェーズ番号</summary>
        public string PhaseNo { get; set; } = string.Empty;
        /// <summary>フェーズ番号</summary>
        public int PhaseNoInt { get => ToInt(PhaseNo, -1); }
        /// <summary>前のフェーズ番号</summary>
        public int BeforePhaseNo { get; set; } = -1;
        /// <summary>バックペア番号</summary>
        public string BagPairNo { get; set; } = string.Empty;
        /// <summary>前のバッグペア番号</summary>
        public int BeforeBagPairNo { get; set; } = -1;
        public CrsdEventData EventData { get; set; } = new CrsdEventData("");
        /// <summary>フェーズ時間</summary>
        public double PhaseTimeDbl { get => ToDouble(PhaseTime, -1); }

        /// <summary>フェーズ種類</summary>
        public Const.PhaseType PhaseTypeEnum
        {
            get
            {
                switch (PhaseType)
                {
                    case "Normal":
                        return Const.PhaseType.Normal;
                    case "Soak":
                        return Const.PhaseType.Soak;
                    case "IdleCheck":
                        return Const.PhaseType.IdleCheck;
                    case "CoastDown":
                        return Const.PhaseType.CoastDown;
                    case "Warmup":
                        return Const.PhaseType.Warmup;
                    default:
                        return Const.PhaseType.Unknown;
                }
            }
        }

        /// <summary>フェーズ種類</summary>
        public string PhaseType { get; set; }
        /// <summary>フェーズ時間</summary>
        public string PhaseTime { get; set; }
        /// <summary>開始時間</summary>
        public double StartTime { get; set; }
        /// <summary>フェーズ距離</summary>
        public string PhaseDistance { get; set; }
        /// <summary>WaitStart</summary> 
        public string WaitStart { private get; set; }
        public int WaitStartInt { get => EventData == null ? 0 : (EventData.EventDataList.Contains(WST) ? 1 : 0); }
        //public int WaitStartInt { get => ToInt(WaitStart, -1); }
        /// <summary>WaitConti</summary> 
        public string WaitConti { private get; set; }
        public int WaitContiInt { get => EventData == null ? 0 : (EventData.EventDataList.Contains(WCO) ? 1 : 0); }
        //public int WaitContiInt { get => ToInt(WaitConti, -1); }
        /// <summary>ソーク開始</summary>
        public string SoakMinTime { get; set; }
        /// <summary>ソーク終了</summary>
        public string SoakMaxTime { get; set; }
        /// <summary>トリガー名</summary>
        public string TriggerName { get; set; } = TriggerName_Time;
        /// <summary>空白データ</summary>
        public bool IsEmpty { get => (PhaseNoInt == -1 && BeforeBagPairNo == -1); }
        /// <summary>時間入力エラー</summary>
        public bool IsTimeError { get; set; }

        /// <summary>トレース開始</summary>
        public string TraceStartMode
        {
            get
            {
                if (WaitStartInt == -1 || WaitContiInt == -1) return string.Empty;
                if (WaitStartInt == 1 && WaitContiInt == 1) return PendantRun;
                if (WaitStartInt == 0 && WaitContiInt == 1) return PendantStart;
                if (WaitStartInt == 1 && WaitContiInt == 0) return PendantStart;
                if (WaitStartInt == 0 && WaitContiInt == 0) return PendantStart;
                return string.Empty;
            }
        }

        /// <summary>トレース開始エラー</summary>
        public string TraceStartModeError
        {
            get
            {
                if (IsEmpty) return string.Empty;
                if (!string.IsNullOrEmpty(WaitStart) && WaitStartInt == -1)
                    return string.Format(Error_NoNumber, "WaitStart");
                if (!string.IsNullOrEmpty(WaitConti) && WaitContiInt == -1)
                    return string.Format(Error_NoNumber, "Waitconti");
                if (WaitStartInt == 0 && WaitContiInt == 0) 
                    return "WaitStart=0, WaitConti=0が入力されています。";
                if (WaitStartInt == 0 && WaitContiInt == 1)
                    return "WaitStart=0, WaitConti=1が入力されています。";
                return string.Empty;
            }
        }

        /// <summary>時間系トリガーかどうか</summary>
        public bool IsTimeTrigger { get => (TriggerName == TriggerName_Time || TriggerName == TriggerName_TraceSegmentStart || TriggerName == TriggerName_TraceFinish); }
    }
}
