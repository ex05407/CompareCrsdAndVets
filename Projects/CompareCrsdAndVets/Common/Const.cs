using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareCrsdAndVets.Common
{
    internal static class Const
    {
        /// <summary>Excel出力列数</summary>
        public const int OutputExcelCol = 9;

        /// <summary>Excel出力列数(ファイルデータ用)</summary>
        public const int OutputFileExcelCol = 7;

        /// <summary>Excel出力列数(TraceStartデータ用)</summary>
        public const int OutputTraceStartExcelCol = 3;

        /// <summary>出力結果ファイル名</summary>
        public const string FileName_OutputTsv = "CompareResult.tsv";

        /// <summary>出力結果ファイル名(ファイルデータ用)</summary>
        public const string FileName_OutputFileTsv = "FileResult.tsv";

        /// <summary>出力結果ファイル名(TraceStartデータ用)</summary>
        public const string FileName_OutputTraceStartTsv = "TraceStart.tsv";

        /// <summary>出力結果ファイル名(TraceStartデータ用)</summary>
        public const string FileName_OutputSampleTimeTsv = "SampleTime.tsv";

        /// <summary>shift_jisエンコーディング</summary>
        public static readonly Encoding SJiEnc = Encoding.GetEncoding("shift_jis");

        #region "Enum定義"

        /// <summary>Excel列定義</summary>
        public enum ExcelCols
        {
            FileName=1,
            BlockName=2,
            EventName=3,
            ActionName=4,
            ItemName=5,
            Result=6,
            CrsdVal=7,
            VetsVal=8,
            Note=9
        };

        /// <summary>Excel列定義(ファイルデータ用)</summary>
        public enum ExcelCols_FileData
        {
            Name = 1,
            TestModeName = 2,
            Message = 3,
            VetsPath = 4,
            TdPath = 5,
            Td2Path = 6,
            EventPath = 7
        };

        /// <summary>Excel列定義(TraceStartデータ用)</summary>
        public enum ExcelCols_TraceStart
        {
            Name = 1,
            TestModeName = 2,
            TraceStart = 3
        };

        /// <summary>フェーズタイプ</summary>
        public enum PhaseType
        {
            Normal,
            Soak,
            IdleCheck,
            CoastDown,
            Warmup,
            Unknown
        };

        /// <summary>ブロックタイプ</summary>
        public enum BlockType
        {
            Normal,
            Soak,
            IdleCheck,
            CoastDown,
            Warmup,
            Unknown
        };

        #endregion

        public const string CrsdName = "CRSD-7000";
        public const string VetsName = "STARS VETS";

        public const string TestProcedure = "テストプロシージャ";
        public const string Trace = "トレース";
        public const string TraceVectors = "TraceVectors";
        public const string MassData = "MassData";

        #region "Excel出力定義"

        public const string Title_Name = "名前";
        public const string Title_FileName = "テストモードファイル名";
        public const string Title_BlockName = "ブロック";
        public const string Title_EventName = "イベント";
        public const string Title_ActionName = "アクション";
        public const string Title_ItemName = "項目名";
        public const string Title_Result = "比較結果";
        public const string Title_CrsdVal = "CRSD-7000";
        public const string Title_VetsVal = "STARS VETS";
        public const string Title_Note = "備考";

        public const string TitleFile_Name = "ファイル名";
        public const string TitleFile_TestModeName = "テストモードファイル名";
        public const string TitleFile_TdPath = "CRSD-7000パス(td)";
        public const string TitleFile_Td2Path = "CRSD-7000パス(td2)";
        public const string TitleFile_ElPath = "CRSD-7000パス(el)";
        public const string TitleFile_VetsPath = "STARS VETSパス";
        public const string TitleFile_Message = "メッセージ";

        public const string TitleTraceStart_Name = "ファイル名";
        public const string TitleTraceStart_TestModeName = "テストモードファイル名";
        public const string TitleTraceStart_TraceStart = "トレース開始";

        public const string TitleSample_Name = "VETSファイル名";
        public const string TitleSample_TestModeName = "テストモードファイル名";
        public const string TitleSample_BagSampleTime_CRSD = "BAGサンプル時間(CRSD)";
        public const string TitleSample_BagSampleTime_CRSD1 = TitleSample_BagSampleTime_CRSD + "1";
        public const string TitleSample_BagSampleTime_CRSD2 = TitleSample_BagSampleTime_CRSD + "2";
        public const string TitleSample_BagSampleTime_CRSD3 = TitleSample_BagSampleTime_CRSD + "3";
        public const string TitleSample_BagSampleTime_CRSD4 = TitleSample_BagSampleTime_CRSD + "4";
        public const string TitleSample_BagSampleTime_VETS = "BAGサンプル時間(VETS)";
        public const string TitleSample_BagSampleTime_VETS1 = TitleSample_BagSampleTime_VETS + "1";
        public const string TitleSample_BagSampleTime_VETS2 = TitleSample_BagSampleTime_VETS + "2";
        public const string TitleSample_BagSampleTime_VETS3 = TitleSample_BagSampleTime_VETS + "3";
        public const string TitleSample_BagSampleTime_VETS4 = TitleSample_BagSampleTime_VETS + "4";

        public const int SampleStart_Crsd = 2;
        public const int SampleStart_Vets = 6;
        public const int SampleMaxNum = 4;

        public const string ItemName_All = "全体結果";
        public const string BlockName_DriveUnit = "ドライブユニット設定";
        public const string BlockName_Soak = "ソーク";
        public const string BlockName_IdleCheck = "アイドルチェック";
        public const string BlockName_CoastDown = "コーストダウン";
        public const string EventName = "イベント";
        public const string ActionName_Trigger = "トリガー";

        public const string PendantStart = "ペンダントStart";
        public const string PendantRun = "ペンダントRun";
        public const string WST = "WST";
        public const string WCO = "WCO";

        public const string TriggerName_PendantButon = "PendantButton";
        public const string TriggerName_Time = "Time";
        public const string TriggerName_TraceFinish = "TraceFinish";
        public const string TriggerName_TraceSegmentStart = "TraceSegmentStart";

        #endregion
    }
}
