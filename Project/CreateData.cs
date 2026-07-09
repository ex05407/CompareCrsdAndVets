using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CompareCrsdAndVets.Common.Const;
using static CompareCrsdAndVets.Common.CommonMethods;

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
    }
}
