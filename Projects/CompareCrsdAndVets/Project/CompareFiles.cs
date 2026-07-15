using CompareCrsdAndVets.Class;
using CompareCrsdAndVets.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using static CompareCrsdAndVets.Common.CommonMethods;
using static CompareCrsdAndVets.Common.Const;
using Message = CompareCrsdAndVets.Common.Message;

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
        QuantitiesUnitsTable UnitConvert;

        /// <summary>メッセージ</summary>
        public string Mesage { get; set; }

        Encoding sJisEnc = Encoding.GetEncoding("shift-jis");
        #endregion

        #region "クラス"
        private class ResultEx
        {
            public int SuccessCount { get; set; } = 0;
            public int ErrorCount { get; set; } = 0;
            public int VetsOnlyCount { get; set; } = -1;
            public int CrsdOnlyCount { get; set; } = -1;
            public int CrsdErrorCount { get; set; } = -1;

            public override string ToString()
            {
                return string.Concat($"成功：{SuccessCount}件, エラー：{ErrorCount}件\r\n",
                    VetsOnlyCount == -1 ? string.Empty : $"VETSのみ：{VetsOnlyCount}件, ",
                    CrsdOnlyCount == -1 ? string.Empty : $"CRSDのみ：{CrsdOnlyCount}件, ",
                    CrsdErrorCount == -1 ? string.Empty : $"CRSDエラー：{CrsdErrorCount}件").TrimEnd(' ').TrimEnd(',');
            }
        }
        #endregion

        #region "Enum"
        private enum OutputFileData
        {
            [Description(Const.FileName_OutputTsv)]
            OutputTsv = 1,
            [Description(Const.FileName_OutputFileTsv)]
            OutputFileTsv = 2,
            [Description(Const.FileName_OutputTraceStartTsv)]
            OutputTraceStartTsv = 3,
            [Description(Const.FileName_OutputSampleTimeTsv)]
            OutputSampleTimeTsv = 4
        };

        static string GetName(OutputFileData data)
        {
            var gm = data.GetType().GetMember(data.ToString());
            var attributes = gm[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            var description = ((DescriptionAttribute)attributes[0]).Description;
            return description;
        }
        #endregion

        #region "コンストラクタ"
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CompareFiles()
        {
            UnitConvert = new QuantitiesUnitsTable();
            UnitConvert.LoadXml(@"Xml\QuantitiesUnits.xml");
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
                ErrorMessage = string.Format(Message.Error_NoSuchFile, "CRSD7000", string.Empty);
                return false;
            }

            string[] TestProcedureFiles = Directory.GetFiles(pTestProcedureFolderPath, "*.xml", SearchOption.TopDirectoryOnly);
            if (TestProcedureFiles == null || TestProcedureFiles.Length == 0)
            {
                ErrorMessage = string.Format(Message.Error_NoSuchFile, "TestProcedure", string.Empty);
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

            ResultEx result = new ResultEx();

            Dictionary<OutputFileData, object> retData = CreateOutputFileData();
            List<string[]> mainData = (List<string[]>)retData[OutputFileData.OutputTsv];
            List<string[]> fileData = (List<string[]>)retData[OutputFileData.OutputFileTsv];
            List<string[]> sampleTimeData = (List<string[]>)retData[OutputFileData.OutputSampleTimeTsv];
            List<string[]> traceStartData = (List<string[]>)retData[OutputFileData.OutputTraceStartTsv];

            // テストプロシージャ情報の取得
            List<TestProcedure> testProcedures = LoadVetsTestProcedures(pTestProcedureFolderPath, out ErrorMessage, ref fileData, ref result);
            if(testProcedures == null || !string.IsNullOrEmpty(ErrorMessage)) return false;

            // トレース情報の取得
            string TraceFolderPath = Path.Combine(GetParentPath(pTestProcedureFolderPath), "Vetstraces");
            List<Trace> traces = LoadVetsTraces(TraceFolderPath, out ErrorMessage, ref fileData);
            if (traces == null || !string.IsNullOrEmpty(ErrorMessage)) return false;

            // CRSDのテストプロシージャ情報の取得
            List<CrsdFileData> crsdFiles = LoadCrsdTestProcedures(pCrsdFolderPath, out ErrorMessage, ref fileData, ref result);

            string MessageStr = string.Empty;
            string CrsdBaseDirPath = CommonMethods.GetParentPath(pCrsdFolderPath);
            string CrsdEventDirPath = Path.Combine(CrsdBaseDirPath, "Event");
            List<string> crsdSaveFileNames = new List<string>();
            foreach (TestProcedure testProcedure in testProcedures)
            {
                CompareData compareData = new CompareData(logger, UnitConvert);
                CrsdFileData CrsdFileData = null;

                try
                {
                    /*
                    foreach (BaseBlock block in testProcedure.CycleBlocks)
                    {
                        if (block.GetType() != typeof(DriveUnitBlock)) continue;

                        DriveUnitBlock drive = (DriveUnitBlock)block;
                        List<string> traceMinData = new List<string>()
                        {
                            testProcedure.Name,
                            drive.Name
                        };
                        traceMinData.AddRange(drive.TraceNames);
                        traceData.Add(traceMinData);
                    }
                    */

                    // トレースとテストプロシージャの紐付け
                    foreach (BaseBlock block in testProcedure.CycleBlocks)
                    {
                        foreach (string traceName in block.TraceNames)
                        {
                            if (traces.Exists(a => a.Name == traceName))
                            {
                                block.TraceData.Add(traces.Find(a => a.Name == traceName));
                            }
                            else
                            {
                                block.TraceData.Add(new Trace() { Name = traceName });
                            }
                        }
                    }

                    // サンプル時間の設定
                    testProcedure.CreateSampleTime();

                    // CRSD-7000ファイルの紐付け
                    CrsdFileData = crsdFiles.Find(a => CompareFileName(a.FileName, testProcedure.TestModeFileName));

                    if (CrsdFileData == null)
                    {
                        MessageStr = string.Format(Message.Error_NoSuchFile, Const.CrsdName, "td/td2");
                        logger.OutputLog($"{MessageStr} {CrsdName}フォルダ：{pCrsdFolderPath} " +
                            $"テストモードファイル名：{testProcedure.TestModeFileName}", Logger.LogType.Warning);
                        fileData.Add(CreateData.CreateExcelItem_FileData(pFileName: testProcedure.FileName, pTestModeName: testProcedure.TestModeFileName, 
                            pVetsPath: testProcedure.FilePath, pMessage: MessageStr));
                        traceStartData.Add(CreateData.CreateExcelItem_TraceStartData(pFileName: testProcedure.FileName));
                        result.VetsOnlyCount++;

                        // サンプル時間の出力
                        sampleTimeData.Add(CreateData.CreateExcelItem_SampleData(testProcedure, null));
                        mainData.AddRange(CreateData.CreateExcelItem_OnlyTestProcedure(testProcedure));

                        continue;
                    }

                    crsdSaveFileNames.Add(CrsdFileData.FileName);

                    // イベントファイルの読み込み 
                    CrsdEventListData crsdEventListData = ReadFile.LoadCrsdEventFile(logger, CrsdEventDirPath, CrsdFileData,
                        out MessageStr, out List<string> elFiles);
                    if (crsdEventListData == null)
                    {
                        fileData.Add(CreateData.CreateExcelItem_FileData(pFileName: testProcedure.FileName, pTestModeName: testProcedure.TestModeFileName, pVetsPath: testProcedure.FilePath,
                            pTdPath: CrsdFileData.TdFilePath, pTd2Path: CrsdFileData.Td2FilePath, pMessage: MessageStr));
                        traceStartData.Add(CreateData.CreateExcelItem_TraceStartData(pFileName: testProcedure.FileName));
                        result.VetsOnlyCount++;

                        // サンプル時間の出力
                        sampleTimeData.Add(CreateData.CreateExcelItem_SampleData(testProcedure, null));
                        mainData.AddRange(CreateData.CreateExcelItem_OnlyTestProcedure(testProcedure));

                        continue;
                    }
                    CrsdFileData.EventListData = crsdEventListData;
                    List<string> eventMinData = new List<string>()
                    {
                        testProcedure.FileName,
                        crsdEventListData.FileName
                    };
                    eventMinData.AddRange(elFiles);
                    //eventData.Add(eventMinData);

                    /*
                    int phaseCount = 0;
                    string[] phaseMinData = new string[7];
                    phaseMinData[0] = testProcedure.Name;
                    phaseMinData[1] = testProcedure.TestModeFileName;
                    double warmuptime = 0;
                    PhaseType beforePhaseType = PhaseType.Unknown;
                    foreach (CrsdPhaseData phase in CrsdFileData.Phases)
                    {
                        if (phase.PhaseTypeEnum == PhaseType.Normal || phase.PhaseTypeEnum == PhaseType.Warmup)
                        {
                            warmuptime++;
                            phaseMinData[phaseCount + 2] = phase.PhaseTime;
                            phaseCount++;
                        }
                        else if (phase.PhaseTypeEnum == PhaseType.Warmup)
                        {

                        }
                        beforePhaseType = phase.PhaseTypeEnum;
                    }
                    phaseData.Add(phaseMinData);
                    */

                    // Excel出力用のデータを作成
                    bool tmp = false;
                    mainData.Add(CreateData.CreateExcelItem(ref tmp, pFileName: testProcedure.TestModeFileName,
                        pItemName: "ファイル名", pCrsdVal: testProcedure.TestModeFileName, pVetsVal: testProcedure.FileName, pCheckResultFlg: false));
                    mainData.AddRange(compareData.CreateExcelData(CrsdFileData, testProcedure, out string[] sample));
                    traceStartData.Add(CreateData.CreateExcelItem_TraceStartData(pFileName: testProcedure.FileName, pTraceStart: CrsdFileData.TraceStartMode));
                    sampleTimeData.Add(sample);
                    result.SuccessCount++;

                    // ファイル情報を作成
                    fileData.Add(CreateData.CreateExcelItem_FileData(pFileName: testProcedure.FileName, pTestModeName: testProcedure.TestModeFileName, pVetsPath: testProcedure.FilePath,
                        pTdPath: CrsdFileData.TdFilePath, pTd2Path: CrsdFileData.Td2FilePath, pMessage: string.Empty));
                }
                catch (Exception ex)
                {
                    logger.OutputLog(
                        $"ファイル：{testProcedure.FileName} エラー：{ex.Message}",
                        Logger.LogType.Error);
                    fileData.Add(CreateData.CreateExcelItem_FileData(pFileName: testProcedure.FileName, pTestModeName: testProcedure.TestModeFileName, pVetsPath: testProcedure.FilePath,
                        pTdPath: CrsdFileData != null ? CrsdFileData.TdFilePath : string.Empty, pTd2Path: CrsdFileData != null ? CrsdFileData.Td2FilePath : string.Empty, 
                        pMessage: Message.Error_CompareFile));
                    result.ErrorCount++;
                }
            }

            result.CrsdOnlyCount = 0;
            foreach(CrsdFileData crsdFile in crsdFiles.Where(a => !crsdSaveFileNames.Contains(a.FileName)))
            {
                fileData.Add(CreateData.CreateExcelItem_FileData(pFileName: crsdFile.FileName, pTestModeName: crsdFile.FileName, pVetsPath: string.Empty,
                        pTdPath: crsdFile.TdFilePath, pTd2Path: crsdFile.Td2FilePath, pMessage: string.Format(Message.Error_NoExistVets, Const.CrsdName + "ファイル")));
                result.CrsdOnlyCount++;
            }

            foreach(KeyValuePair<OutputFileData, object> kvp in retData)
            {
                SaveFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, GetName(kvp.Key)), (List<string[]>)kvp.Value);
            }


            // メッセージの作成
            Mesage = string.Concat(Common.Message.Info_CompareFile, result.ToString());

            return true;
        }
        #endregion

        #endregion

        #region "Private Method"

        #region "出力ファイル情報の設定"
        /// <summary>
        /// 出力ファイル情報の設定
        /// </summary>
        /// <returns></returns>
        private Dictionary<OutputFileData, object> CreateOutputFileData()
        {
            Dictionary<OutputFileData, object> retData = new Dictionary<OutputFileData, object>();

            // メイン情報
            retData.Add(OutputFileData.OutputTsv, new List<string[]>()
            {
                new string[] { Title_FileName, Title_BlockName, Title_EventName, Title_ActionName, Title_ItemName, Title_Result, Title_CrsdVal, Title_VetsVal, Title_Note }
            });
            // ファイル情報
            retData.Add(OutputFileData.OutputFileTsv, new List<string[]>()
            {
                new string[] { TitleFile_Name, TitleFile_TestModeName, TitleFile_Message, TitleFile_VetsPath, TitleFile_TdPath, TitleFile_Td2Path, TitleFile_ElPath }
            });
            // トレース開始情報
            retData.Add(OutputFileData.OutputTraceStartTsv, new List<string[]>()
            {
                new string[] { TitleTraceStart_Name, TitleTraceStart_TestModeName, TitleTraceStart_TraceStart }
            });
            // サンプル時間情報
            retData.Add(OutputFileData.OutputSampleTimeTsv, new List<string[]>()
            {
                new string[] { TitleSample_Name, TitleSample_TestModeName, 
                    TitleSample_BagSampleTime_CRSD1, TitleSample_BagSampleTime_CRSD2, TitleSample_BagSampleTime_CRSD3, TitleSample_BagSampleTime_CRSD4,
                    TitleSample_BagSampleTime_VETS1, TitleSample_BagSampleTime_VETS2, TitleSample_BagSampleTime_VETS3, TitleSample_BagSampleTime_VETS4 }
            });


            /*
            List<List<string>> eventData = new List<List<string>>()
            {
                new List<string>(){ "ファイル名", "使用イベントリスト", "イベントリスト" }
            };
            List<string[]> phaseData = new List<string[]>()
            {
                new string[] { "テストプロシージャ名", "ファイル名", "BAGサンプル時間１", "BAGサンプル時間２", "BAGサンプル時間３", "BAGサンプル時間４" }
            };
            List<List<string>> traceData = new List<List<string>>()
            {
                new List<string>(){ "ファイル名", "ドライブユニット", "トレース" }
            };
            */

            return retData;
        }
        #endregion

        #region "テストプロシージャ情報のロード処理"
        /// <summary>
        /// テストプロシージャ情報のロード
        /// </summary>
        /// <param name="pTestProcedureFolderPath"></param>
        /// <param name="ErrorMessage"></param>
        /// <returns></returns>
        private List<TestProcedure> LoadVetsTestProcedures(string pTestProcedureFolderPath, out string ErrorMessage, ref List<string[]> fileData, ref ResultEx result)
        {
            ErrorMessage = string.Empty;
            List<TestProcedure> testProcedures = new List<TestProcedure>();

            // プロシージャフォルダ内のXMLファイルを取得
            string[] VetsFilePathes = Directory.GetFiles(pTestProcedureFolderPath, "*.xml", SearchOption.TopDirectoryOnly);
            if (VetsFilePathes == null || VetsFilePathes.Length == 0)
            {
                ErrorMessage = string.Format(Common.Message.Error_NoSuchFile, Const.TestProcedure, string.Empty);
                return null;
            }

            // 処理開始ログの設定
            logger.OutputLog(string.Concat(
                string.Format(Message.Info_Start, "TestProcedureフォルダの確認"),
                $" 対象件数：{VetsFilePathes.Length}件"), Logger.LogType.Info);

            // VETSファイルの読み込み
            foreach (string VetsPath in VetsFilePathes)
            {
                string FileName = Path.GetFileNameWithoutExtension(VetsPath);

                try
                {
                    TestProcedure VetsFileData = ReadFile.LoadVetsFile(logger, VetsPath, out ErrorMessage);
                    if (VetsFileData == null)
                    {
                        fileData.Add(CreateData.CreateExcelItem_FileData(pFileName: FileName, pVetsPath: VetsPath, pMessage: ErrorMessage));
                        result.ErrorCount++;
                        continue;
                    }
                    testProcedures.Add(VetsFileData);
                }
                catch
                {
                    result.ErrorCount++;
                }
            }

            // 処理終了ログの設定
            logger.OutputLog(string.Format(Message.Info_Complete, "TestProcedureフォルダの確認"), Logger.LogType.Info);

            return testProcedures;
        }
        #endregion

        #region "トレース情報のロード処理"
        /// <summary>
        /// トレース情報のロード
        /// </summary>
        /// <param name="pTestProcedureFolderPath"></param>
        /// <param name="ErrorMessage"></param>
        /// <returns></returns>
        private List<Trace> LoadVetsTraces(string pTraceFolderPath, out string ErrorMessage, ref List<string[]> fileData)
        {
            ErrorMessage = string.Empty;
            List<Trace> traces = new List<Trace>();
            ResultEx result = new ResultEx();

            if (!Directory.Exists(pTraceFolderPath))
            {
                ErrorMessage = string.Format(Common.Message.Error_NoSuchDir, Const.Trace);
                return null;
            }

            // トレースフォルダ内のXMLファイルを取得
            string[] VetsFilePathes = Directory.GetFiles(pTraceFolderPath, "*.xml", SearchOption.TopDirectoryOnly);
            if (VetsFilePathes == null || VetsFilePathes.Length == 0)
            {
                ErrorMessage = string.Format(Common.Message.Error_NoSuchFile, Const.Trace, string.Empty);
                return null;
            }

#if DEBUG
            // 出力用のトレースフォルダを作成
            string OutputTraceDir = Path.Combine(Environment.CurrentDirectory, "trace");
            if (Directory.Exists(OutputTraceDir)) Directory.Delete(OutputTraceDir, true);
            Directory.CreateDirectory(OutputTraceDir);
#endif

            // 処理開始ログの設定
            logger.OutputLog(string.Concat(
                string.Format(Message.Info_Start, "Traceフォルダの確認"),
                $" 対象件数：{VetsFilePathes.Length}件"), Logger.LogType.Info);

            foreach (string VetsPath in VetsFilePathes)
            {
                try
                {
                    // VETSトレースファイルの読み込み
                    Trace trace = ReadFile.LoadVetsTraceFile(logger, VetsPath, out ErrorMessage);
                    if (trace == null) 
                    {
                        result.ErrorCount++;
                        continue;
                    }
                    traces.Add(trace);

#if DEBUG
                    // トレースファイルの出力
                    string filePath = Path.Combine(OutputTraceDir, trace.Name + ".tsv");
                    List<string> outputData = new List<string>() { string.Join("\t", trace.Vectors.Select(x => x.Name).ToList()) };
                    for (int i = 0; i < trace.RowCount; i++)
                    {
                        List<string> rowData = new List<string>();
                        foreach (TraceVectorData data in trace.Vectors)
                        {
                            data.TypedData[i] = UnitConvert.ConvertUnit(data.TypedData[i], "Speed", data.Unit);
                            rowData.Add(data.TypedData[i].ToString());
                        }
                        outputData.Add(string.Join("\t", rowData));
                    }
                    File.WriteAllLines(filePath, outputData);
#endif
                    result.SuccessCount++;
                }
                catch
                {
                    result.ErrorCount++;
                }
            }

            // 処理終了ログの設定
            logger.OutputLog(string.Concat(string.Format(Message.Info_Complete, "TestProcedureフォルダの確認"), " ",  result.ToString()), Logger.LogType.Info);

            return traces;
        }
#endregion

        #region "CRSD情報のロード処理"
        /// <summary>
        /// CRSD情報のロード
        /// </summary>
        /// <param name="pCrsdFolderPath"></param>
        /// <param name="ErrorMessage"></param>
        /// <param name="fileData"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private List<CrsdFileData> LoadCrsdTestProcedures(string pCrsdFolderPath, out string ErrorMessage, ref List<string[]> fileData, ref ResultEx result)
        {
            List<CrsdFileData> crsdFiles = new List<CrsdFileData>();
            ErrorMessage = string.Empty;

            // CRSDフォルダ内のtdファイル、td2ファイルを取得
            string[] TdFilePathes = Directory.GetFiles(pCrsdFolderPath, "*.td", SearchOption.TopDirectoryOnly);
            string[] Td2FilePathes = Directory.GetFiles(pCrsdFolderPath, "*.td2", SearchOption.TopDirectoryOnly);
            Dictionary<string, string> Td2FileDic = new Dictionary<string, string>();
            foreach(string Td2FilePath in Td2FilePathes)
            {
                Td2FileDic.Add(Path.GetFileNameWithoutExtension(Td2FilePath).ToUpper(), Td2FilePath);
            }

            if (TdFilePathes == null || TdFilePathes.Length == 0)
            {
                ErrorMessage = string.Format(Common.Message.Error_NoSuchFileDetail, Const.CrsdName, Const.TestProcedure, "td");
                return null;
            }

            if(Td2FilePathes == null || Td2FilePathes.Length == 0)
            {
                ErrorMessage = string.Format(Common.Message.Error_NoSuchFileDetail, Const.CrsdName, Const.TestProcedure, "td2");
                return null;
            }

            // 処理開始ログの設定
            logger.OutputLog(string.Concat(
                string.Format(Message.Info_Start, $"{Const.CrsdName}の{Const.TestProcedure}フォルダの確認"),
                $" 対象件数 tdファイル：{TdFilePathes.Length}件 td2ファイル：{Td2FilePathes.Length}件"), Logger.LogType.Info);

            result.CrsdErrorCount = 0;
            foreach (string TdFilePath in TdFilePathes)
            {
                // Td2ファイルの検索
                string TdFileName = Path.GetFileNameWithoutExtension(TdFilePath).ToUpper();
                string Td2FilePath = string.Empty;
                if (Td2FileDic.ContainsKey(TdFileName))
                {
                    Td2FilePath = Td2FileDic[TdFileName];
                    Td2FileDic.Remove(TdFileName);
                }

                // CRSD-7000ファイルの読み込み
                CrsdFileData CrsdFileData = ReadFile.LoadCrsdFile(logger, TdFilePath, Td2FilePath, out string MessageStr);
                if (CrsdFileData == null)
                {
                    result.CrsdErrorCount++;
                    continue;
                }
                crsdFiles.Add(CrsdFileData);
            }

            return crsdFiles;
        }
        #endregion

        #region "ファイル名同士の比較処理"
        /// <summary>
        /// ファイル名同士の比較処理
        /// (アンダーバーと空白の違いは許容、大文字同士で比較）
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        private bool CompareFileName(string value1, string value2)
        {
            string checkValue1 = value1.Replace("_", " ").ToUpper();
            string checkValue2 = value2.Replace("_", " ").ToUpper();

            return checkValue1 == checkValue2;
        }
        #endregion

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
