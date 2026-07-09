using CompareCrsdAndVets.Class;
using CompareCrsdAndVets.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static CompareCrsdAndVets.Common.Const;
using static CompareCrsdAndVets.Common.CommonMethods;
using Message = CompareCrsdAndVets.Common.Message;
using System.Diagnostics.Eventing.Reader;

namespace CompareCrsdAndVets.Project
{
    /// <summary>
    /// ファイルの比較処理を行うためのクラス
    /// </summary>
    internal class CompareFiles
    {
        #region "定数"
        /// <summary>ログ関連クラス</summary>
        Logger logger;

        /// <summary>単位変換用クラス</summary>
        QuantitiesUnitsTable UnitConvet;

        /// <summary>メッセージ</summary>
        public string Mesage {  get; set; }

        Encoding sJisEnc = Encoding.GetEncoding("shift-jis");
        #endregion

        #region "コンストラクタ"
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CompareFiles()
        {
            UnitConvet = new QuantitiesUnitsTable();
            UnitConvet.LoadXml(@"Xml\QuantitiesUnits.xml");
        }
        #endregion

        #region "Open Method"

        #region "データチェック"
        /// <summary>
        /// データのチェック処理
        /// </summary>
        /// <returns></returns>
        internal bool CheckData(string pCrsdFolderPath, string pTestProcedureFolderPath, out string ErrorMessage)
        {
            ErrorMessage = string.Empty;

            // 入力値のチェック
            if (string.IsNullOrEmpty(pCrsdFolderPath))
            {
                ErrorMessage = string.Format(Message.Error_NoInput, "CRSD7000フォルダパス");
                return false;
            }

            if (string.IsNullOrEmpty(pTestProcedureFolderPath))
            {
                ErrorMessage = string.Format(Message.Error_NoInput, "TestProcedureフォルダパス");
                return false;
            }

            // フォルダの存在チェック
            if (!Directory.Exists(pCrsdFolderPath))
            {
                ErrorMessage = string.Format(Message.Error_NoSuchDir, "CRSD7000");
                return false;
            }

            if (!Directory.Exists(pTestProcedureFolderPath))
            {
                ErrorMessage = string.Format(Message.Error_NoSuchDir, "TestProcedure");
                return false;
            }

            // ファイルの存在チェック
            string[] CrsdFiles = Directory.GetFiles(pCrsdFolderPath, "*.td", SearchOption.TopDirectoryOnly);
            if (CrsdFiles == null || CrsdFiles.Length == 0)
            {
                ErrorMessage = string.Format(Message.Error_NoSuchFile, "CRSD7000");
                return false;
            }

            string[] TestProcedureFiles = Directory.GetFiles(pTestProcedureFolderPath, "*.xml", SearchOption.TopDirectoryOnly);
            if (TestProcedureFiles == null || TestProcedureFiles.Length == 0)
            {
                ErrorMessage = string.Format(Message.Error_NoSuchFile, "TestProcedure");
                return false;
            }

            return true;
        }
        #endregion

        #region "比較処理"
        /// <summary>
        /// 比較処理
        /// </summary>
        /// <param name="pCrsdFolderPath"></param>
        /// <param name="pTestProcedureFolderPath"></param>
        public bool Compare(string pCrsdFolderPath, string pTestProcedureFolderPath, out string ErrorMessage)
        {

            ErrorMessage = string.Empty;
            logger = new Logger();

            logger.OutputLog(string.Format(Message.Info_Start, "処理"), Logger.LogType.Info);

            // プロシージャフォルダ内のXMLファイルを取得
            string[] VetsFilePathes = Directory.GetFiles(pTestProcedureFolderPath, "*.xml", SearchOption.TopDirectoryOnly);
            if(VetsFilePathes == null || VetsFilePathes.Length == 0)
            {
                ErrorMessage = string.Format(Common.Message.Error_NoSuchFile, "TestProcedure");
                return false;
            }

            logger.OutputLog(string.Concat(
                string.Format(Message.Info_Complete, "TestProcedureフォルダの確認"),
                $" 対象件数：{VetsFilePathes.Length}件"), Logger.LogType.Info);

            ProgressBar bar = new ProgressBar();
            bar.Maximum = VetsFilePathes.Length;
            bar.Show();

            // タイトル設定
            List<string[]> retData = new List<string[]>()
            {
                new string[] { Title_FileName, Title_BlockName, Title_EventName, Title_ActionName, Title_ItemName, Title_Result, Title_CrsdVal, Title_VetsVal, Title_Note }
            };
            List<string[]> fileData = new List<string[]>()
            {
                new string[] { TitleFile_Name, TitleFile_TestModeName, TitleFile_Message, TitleFile_VetsPath, TitleFile_TdPath, TitleFile_Td2Path, TitleFile_ElPath }
            };
            List<string[]> traceStartData = new List<string[]>()
            {
                new string[] { TitleTraceStart_Name, TitleTraceStart_TraceStart }
            };
            List<List<string>> eventData = new List<List<string>>()
            {
                new List<string>(){ "ファイル名", "使用イベントリスト", "イベントリスト" }
            };


            int SuccessCount = 0;
            int FailureCount = 0;
            int NoOutputCount = 0;
            string MessageStr = string.Empty;
            string CrsdBaseDirPath = CommonMethods.GetParentPath(pCrsdFolderPath);
            string CrsdEventDirPath = Path.Combine(CrsdBaseDirPath, "Event");
            List<string> CompareCrsdFiles = new List<string>();
            foreach (string VetsPath in VetsFilePathes)
            {
                CompareData compareData = new CompareData(logger, UnitConvet);
                string FileName = Path.GetFileNameWithoutExtension(VetsPath);
                string TdFilePath = string.Empty;
                string Td2FilePath = string.Empty;
                bool TdFileExists = false;
                bool Td2FileExists = false;

                try
                {
                    // VETSファイルの読み込み
                    TestProcedure VetsFileData = ReadFile.LoadVetsFile(logger, VetsPath, out MessageStr);
                    if (VetsFileData == null)
                    {
                        fileData.Add(CreateData.CreateExcelItem_FileData(pFileName: FileName, pVetsPath:VetsPath, pMessage: MessageStr));
                        traceStartData.Add(compareData.CreateExcelItem_TraceStartData(pFileName: FileName));
                        NoOutputCount++;
                        continue;
                    }

                    // ファイルパスの取得
                    TdFilePath = Path.Combine(pCrsdFolderPath, $"{VetsFileData.TestModeFileName}.td");
                    Td2FilePath = Path.Combine(pCrsdFolderPath, $"{VetsFileData.TestModeFileName}.td2");
                    TdFileExists = File.Exists(TdFilePath);
                    Td2FileExists = File.Exists(Td2FilePath);

                    if (!TdFileExists && !Td2FileExists)
                    {
                        MessageStr  = string.Format(Message.Error_NoSuchFile, "CRSD-7000", "td/td2");
                        logger.OutputLog($"{MessageStr} {CrsdName}フォルダ：{pCrsdFolderPath} テストモードファイル名：{VetsFileData.TestModeFileName}", Logger.LogType.Warning);
                        fileData.Add(CreateData.CreateExcelItem_FileData(pFileName: FileName, pTestModeName: VetsFileData.TestModeFileName, pVetsPath: VetsPath, pMessage: MessageStr));
                        traceStartData.Add(compareData.CreateExcelItem_TraceStartData(pFileName: FileName));
                        NoOutputCount++;
                        continue;
                    }

                    CompareCrsdFiles.Add(TdFilePath);

                    // CRSD-7000ファイルの読み込み
                    CrsdFileData CrsdFileData = ReadFile.LoadCrsdFile(logger, TdFilePath, Td2FilePath, out MessageStr);
                    if (CrsdFileData == null)
                    {
                        fileData.Add(CreateData.CreateExcelItem_FileData(pFileName: FileName, pTestModeName: VetsFileData.TestModeFileName, pVetsPath: VetsPath, 
                            pTdPath: TdFileExists ? TdFilePath : string.Empty, pTd2Path: Td2FileExists ? Td2FilePath : string.Empty, pMessage: MessageStr));
                        traceStartData.Add(compareData.CreateExcelItem_TraceStartData(pFileName: FileName));
                        NoOutputCount++;
                        continue;
                    }

                    // イベントファイルの読み込み 
                    CrsdEventListData crsdEventListData = ReadFile.LoadCrsdEventFile(logger, CrsdEventDirPath, CrsdFileData,
                        out MessageStr, out List<string> elFiles);
                    if (crsdEventListData == null)
                    {
                        fileData.Add(CreateData.CreateExcelItem_FileData(pFileName: FileName, pTestModeName: VetsFileData.TestModeFileName, pVetsPath: VetsPath,
                            pTdPath: TdFileExists ? TdFilePath : string.Empty, pTd2Path: Td2FileExists ? Td2FilePath : string.Empty, pMessage: MessageStr));
                        traceStartData.Add(compareData.CreateExcelItem_TraceStartData(pFileName: FileName));
                        NoOutputCount++;
                        continue;
                    }
                    CrsdFileData.EventListData = crsdEventListData;
                    List<string> eventMinData = new List<string>()
                    {
                        FileName,
                        crsdEventListData.FileName
                    };
                    eventMinData.AddRange(elFiles);
                    eventData.Add(eventMinData);

                    // Excel出力用のデータを作成
                    bool tmp = false;
                    retData.Add(CreateData.CreateExcelItem(ref tmp, pFileName: VetsFileData.TestModeFileName, 
                        pItemName: "ファイル名", pCrsdVal: VetsFileData.TestModeFileName, pVetsVal: FileName, pCheckResultFlg: false));
                    retData.AddRange(compareData.CreateExcelData(CrsdFileData, VetsFileData));
                    traceStartData.Add(compareData.CreateExcelItem_TraceStartData(pFileName: FileName, pTraceStart: CrsdFileData.TraceStartMode));
                    SuccessCount++;

                    // ファイル情報を作成
                    fileData.Add(CreateData.CreateExcelItem_FileData(pFileName: FileName, pTestModeName: VetsFileData.TestModeFileName, pVetsPath: VetsPath,
                        pTdPath: TdFileExists ? TdFilePath : string.Empty, pTd2Path: Td2FileExists ? Td2FilePath : string.Empty, pMessage: string.Empty));
                }
                catch (Exception ex)
                {
                    logger.OutputLog(
                        $"ファイル：{FileName} エラー：{ex.Message}",
                        Logger.LogType.Error);
                    fileData.Add(CreateData.CreateExcelItem_FileData(pFileName: FileName, pVetsPath: VetsPath,
                        pTdPath: TdFileExists ? TdFilePath : string.Empty, pTd2Path: Td2FileExists ? Td2FilePath : string.Empty, pMessage: Message.Error_CompareFile));
                    FailureCount++;
                }
            }

            int CrsdNoExistCount = 0;
            foreach(string crsdFile in Directory.GetFiles(pCrsdFolderPath, "*.td", SearchOption.TopDirectoryOnly))
            {
                if (!CompareCrsdFiles.Contains(crsdFile))
                {
                    string fileName = Path.GetFileNameWithoutExtension(crsdFile);
                    string Td2FilePath = crsdFile.ToUpper().Replace("TD", "TD2");
                    FileInfo file = null;
                    if (File.Exists(Td2FilePath)) file = new FileInfo(Td2FilePath);
                    fileData.Add(CreateData.CreateExcelItem_FileData(pFileName: fileName, pTestModeName: fileName, pVetsPath: string.Empty,
                        pTdPath: crsdFile, pTd2Path: file == null ? string.Empty : file.FullName, pMessage: "VETSに存在しないCRSD-7000ファイルです"));
                    CrsdNoExistCount++;
                }
            }

            // 結果ファイルの保存
            SaveFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName_OutputTsv), retData);
            // ファイル情報の保存
            SaveFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName_OutputFileTsv), fileData);
            // トレース開始情報の保存
            SaveFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName_OutputTraceStartTsv), traceStartData);
            // イベントリストの保存
            File.WriteAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EventList.tsv"), 
                eventData.ConvertAll(x => string.Join("\t", x)), sJisEnc);

            // メッセージの作成
            Mesage = string.Format(Common.Message.Info_CompareFile, SuccessCount, FailureCount, NoOutputCount, CrsdNoExistCount);

            return true;
        }
        #endregion

        #endregion

        #region "Private Method"

        #region "ファイルの保存"
        /// <summary>
        /// ファイルの保存
        /// </summary>
        /// <param name="pFilePath"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private void SaveFile(string pFilePath, List<string[]> pData)
        {
            try
            {
                File.WriteAllLines(pFilePath, pData.ConvertAll(x => string.Join("\t", x)), sJisEnc);
            }
            catch (IOException ex)
            {
                // Win32エラーコード（下位16bit）
                int win32 = ex.HResult & 0xFFFF;

                // 共有違反、ロック違反の場合は、ファイルが使用中のため保存できなかったことを通知する
                if (win32 == 32 || win32 == 33)
                {
                    throw new Exception(
                        $"保存先ファイルが使用中のため、ファイルが保存できませんでした。\nファイルを閉じて再実行してください。\n\n" +
                        $"{pFilePath}");
                }
                else
                {
                    throw new Exception(
                        $"ファイルの保存に失敗しました。\n\n" +
                        $"{pFilePath}");
                }
            }
        }
        #endregion

        #endregion
    }
}
