using CompareCrsdAndVets.Class;
using CompareCrsdAndVets.Common;
using System;
using System.Collections.Generic;
using static CompareCrsdAndVets.Common.CommonMethods;
using static CompareCrsdAndVets.Common.Const;

namespace CompareCrsdAndVets.Project
{
    internal static class CreateData
    {

        #region "1行ごとのExcel出力データ作成"
        /// <summary>
        /// Excel出力用のデータを作成する
        /// </summary>
        /// <param name="oResult"></param>
        /// <param name="pFileName"></param>
        /// <param name="pBlockName"></param>
        /// <param name="pEventName"></param>
        /// <param name="pActionName"></param>
        /// <param name="pItemName"></param>
        /// <param name="pResult"></param>
        /// <param name="pCrsdVal"></param>
        /// <param name="pVetsVal"></param>
        /// <param name="pNote"></param>
        /// <returns></returns>
        public static string[] CreateExcelItem(ref bool oResult, string pFileName = "", string pBlockName = "-", string pEventName = "-", string pActionName = "-", string pItemName = "-",
            bool? pResult = null, string pCrsdVal = "-", string pVetsVal = "-", string pNote = "", bool pCheckResultFlg = true, bool pIsNuber = false)
        {
            string[] strings = new string[OutputExcelCol];
            strings[(int)ExcelCols.FileName - 1] = pFileName;
            strings[(int)ExcelCols.BlockName - 1] = pBlockName;
            strings[(int)ExcelCols.EventName - 1] = pEventName;
            strings[(int)ExcelCols.ActionName - 1] = pActionName;
            strings[(int)ExcelCols.ItemName - 1] = pItemName;
            if (!pCheckResultFlg)
            {
                strings[(int)ExcelCols.Result - 1] = "-";
            }
            else if (pResult != null)
            {
                strings[(int)ExcelCols.Result - 1] = pResult.Value ? "OK" : "NG";
            }
            else if (pIsNuber)
            {
                bool result = false;
                if (string.IsNullOrEmpty(pCrsdVal) && string.IsNullOrEmpty(pVetsVal)) result = true;
                else if (string.IsNullOrEmpty(pCrsdVal)) result = false;
                else if (string.IsNullOrEmpty(pVetsVal)) result = false;
                else if (ToDouble(pCrsdVal, 0) == ToDouble(pVetsVal, 0)) result = true;
                strings[(int)ExcelCols.Result - 1] = result ? "OK" : "NG";
            }
            else
            {
                bool result = (pCrsdVal == pVetsVal);
                if (oResult && !result) oResult = false;
                strings[(int)ExcelCols.Result - 1] = result ? "OK" : "NG";
            }
            strings[(int)ExcelCols.CrsdVal - 1] = pCrsdVal;
            strings[(int)ExcelCols.VetsVal - 1] = pVetsVal;
            strings[(int)ExcelCols.Note - 1] = pNote;

            return strings;
        }
        #endregion

        #region "1行ごとのExcel出力データ作成(Vetsのみ)"
        /// <summary>
        /// Vetsの情報のみのExcel出力用のデータを作成する
        /// </summary>
        /// <returns></returns>
        public static List<string[]> CreateExcelItem_OnlyTestProcedure(TestProcedure testProcedure)
        {
            bool tmp = false;
            List<string[]> mainData = new List<string[]>();
            string FileName = string.Concat(testProcedure.FileName, "(※)");
            string TestModeFileName = string.Empty;
            string FileNameError = string.Empty;
            if (testProcedure.TestModeFileName != string.Empty)
            {
                TestModeFileName = testProcedure.TestModeFileName;
                FileNameError = string.Format(Message.Error_NoExistCrsd, "テストモードファイル名");
            }
            else
            {
                TestModeFileName = "※記載なし";
                FileNameError = string.Format(Message.Error_NoInput, "テストモードファイル名"); 
            }

            // ファイル名を設定
            mainData.Add(CreateData.CreateExcelItem(ref tmp, pFileName: FileName, pItemName: "ファイル名",
                        pCrsdVal: TestModeFileName, pVetsVal: testProcedure.FileName, pCheckResultFlg: false, pNote: FileNameError));

            // サンプル時間を設定
            if (testProcedure != null && testProcedure.CycleBlocks != null)
            {
                foreach (TestProcedure.EventTimeEx EventtimeData in testProcedure.EventTimeList)
                {
                    mainData.Add(CreateData.CreateExcelItem(ref tmp, pFileName: FileName,
                        pItemName: "サンプル時間", pVetsVal: EventtimeData.SampleTime.ToString(), pCheckResultFlg: false));
                }
            }

            return mainData;
        }
        #endregion

        #region "1行ごとのExcel出力データ作成(ファイル情報出力用)"
        /// <summary>
        /// Excel出力用のデータを作成する
        /// </summary>
        /// <param name="oResult"></param>
        /// <param name="pFileName"></param>
        /// <param name="pBlockName"></param>
        /// <param name="pEventName"></param>
        /// <param name="pActionName"></param>
        /// <param name="pItemName"></param>
        /// <param name="pResult"></param>
        /// <param name="pCrsdVal"></param>
        /// <param name="pVetsVal"></param>
        /// <param name="pNote"></param>
        /// <returns></returns>
        public static string[] CreateExcelItem_FileData(string pFileName = "", string pTestModeName = "-", string pVetsPath = "-", string pTdPath = "-", string pTd2Path = "-",
            string pEventPath = "-", string pMessage = "-")
        {
            string[] strings = new string[OutputFileExcelCol];
            strings[(int)ExcelCols_FileData.Name - 1] = pFileName;
            strings[(int)ExcelCols_FileData.TestModeName - 1] = pTestModeName;
            strings[(int)ExcelCols_FileData.VetsPath - 1] = pVetsPath;
            strings[(int)ExcelCols_FileData.TdPath - 1] = pTdPath;
            strings[(int)ExcelCols_FileData.Td2Path - 1] = pTd2Path;
            strings[(int)ExcelCols_FileData.EventPath - 1] = pEventPath;
            strings[(int)ExcelCols_FileData.Message - 1] = pMessage;

            return strings;
        }
        #endregion

        #region "1行ごとのExcel出力データ作成(トレース開始情報出力用)"
        /// <summary>
        /// Excel出力用のデータを作成する
        /// </summary>
        /// <returns></returns>
        public static string[] CreateExcelItem_TraceStartData(string pFileName = "", string pTestModeName = "-", string pTraceStart = "-")
        {
            string[] strings = new string[OutputTraceStartExcelCol];
            strings[(int)ExcelCols_TraceStart.Name - 1] = pFileName;
            strings[(int)ExcelCols_TraceStart.TestModeName - 1] = pTestModeName;
            strings[(int)ExcelCols_TraceStart.TraceStart - 1] = pTraceStart;

            return strings;
        }
        #endregion

        #region "1行ごとのExcel出力データ作成(サンプル情報出力用)"
        /// <summary>
        /// Excel出力用のデータを作成する
        /// </summary>
        /// <returns></returns>
        public static string[] CreateExcelItem_SampleData(TestProcedure testProcedure, CrsdFileData crsdFileData)
        {
            string[] strings = new string[11];
            strings[0] = testProcedure.Name;
            strings[1] = testProcedure.TestModeFileName;
            string ErrorMessage = string.Empty;

            // CRSDの情報を設定
            int count = 0;
            if( crsdFileData != null && crsdFileData.Phases != null)
            {
                foreach (var phase in crsdFileData.NormalPhaseList)
                {
                    if (count >= SampleMaxNum)
                    {
                        ErrorMessage = $"{Const.Title_CrsdVal}のサンプル時間が、最大値を超過しています";
                        break;
                    }

                    strings[Const.SampleStart_Crsd + count] = phase.PhaseTime;
                    count++;
                }
            }

            // VETSの情報を設定
            if (testProcedure != null && testProcedure.CycleBlocks != null)
            {
                count = 0;
                foreach(TestProcedure.EventTimeEx EventtimeData in testProcedure.EventTimeList)
                {
                    if (count >= SampleMaxNum)
                    {
                        if (string.IsNullOrEmpty(ErrorMessage)) ErrorMessage = $"{Const.VetsName}のサンプル時間が、最大値を超過しています";
                        else ErrorMessage = "サンプル時間が、最大値を超過しています";
                        break;
                    }

                    strings[Const.SampleStart_Vets + count] = EventtimeData.SampleTime.ToString();
                    if(EventtimeData.ErrorMessage != string.Empty)
                    {
                        ErrorMessage = EventtimeData.ErrorMessage;
                    }
                    count++;
                }
            }
            strings[10] = ErrorMessage;

            return strings;
        }
        #endregion
    }
}
