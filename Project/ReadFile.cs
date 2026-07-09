using CompareCrsdAndVets.Class;
using CompareCrsdAndVets.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CompareCrsdAndVets.Common.Const;

namespace CompareCrsdAndVets.Project
{
    internal static class ReadFile
    {
        #region "VETSファイルの読み込み"
        /// <summary>
        /// VETSファイルの読み込み
        /// </summary>
        /// <param name="pFilePath"></param>
        /// <param name="oMessage"></param>
        /// <returns></returns>
        internal static TestProcedure LoadVetsFile(Logger logger, string pFilePath, out string oMessage)
        {
            // XMLファイルを読み込み
            oMessage = string.Empty;
            TestProcedure VetsProcedure = new TestProcedure();
            try
            {
                VetsProcedure.LoadXml(pFilePath);
            }
            catch
            {
                oMessage = string.Format(Common.Message.Error_ReadFile, Const.VetsName);
                logger.OutputLog($"{oMessage} ファイルパス：{pFilePath}", Logger.LogType.Error);
                return null;
            }

            if (string.IsNullOrEmpty(VetsProcedure.TestModeFileName))
            {
                oMessage = string.Format(Common.Message.Error_EmptyData, "テストモードファイル名");
                logger.OutputLog($"{oMessage} ファイルパス：{pFilePath}", Logger.LogType.Warning);
                return null;
            }

            return VetsProcedure;
        }
        #endregion

        #region "CRSDファイルの読み込み"
        /// <summary>
        /// CRSDファイルの読み込み
        /// </summary>
        /// <param name="pFilePath"></param>
        /// <param name="oMessage"></param>
        /// <returns></returns>
        internal static CrsdFileData LoadCrsdFile(Logger logger, string pTdFilePath, string pTd2FilePath, out string oMessage)
        {
            // Tdファイル、Td2ファイルを読み込み
            oMessage = string.Empty;

            CrsdFileData CrsdFileData = new CrsdFileData();

            if (File.Exists(pTdFilePath))
            {
                try
                {
                    CrsdFileData.LoadTd(pTdFilePath);
                }
                catch
                {
                    oMessage = string.Format(Common.Message.Error_ReadFile, CrsdName);
                    logger.OutputLog($"{oMessage} ファイルパス：{pTdFilePath}", Logger.LogType.Error);
                    return null;
                }
            }
            else
            {
                logger.OutputLog($"{string.Format(Message.Error_NoSuchFile, CrsdName, "td")} ファイルパス：{pTdFilePath}",
                    Logger.LogType.Warning);
            }

            if (File.Exists(pTd2FilePath))
            {
                try
                {
                    CrsdFileData.LoadTd2(pTd2FilePath);
                }
                catch
                {
                    oMessage = string.Format(Common.Message.Error_ReadFile, CrsdName);
                    logger.OutputLog($"{oMessage} ファイルパス：{pTd2FilePath}", Logger.LogType.Error);
                    return null;
                }
            }
            else
            {
                logger.OutputLog($"{string.Format(Message.Error_NoSuchFile, CrsdName, "td2")} ファイルパス：{pTd2FilePath}",
                    Logger.LogType.Warning);
            }

            return CrsdFileData;
        }
        #endregion

        #region "CRSDイベントファイルの読み込み"
        /// <summary>
        /// CRSDイベントファイルの読み込み
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="pEventFolderPath"></param>
        /// <param name="oMessage"></param>
        /// <returns></returns>
        public static CrsdEventListData LoadCrsdEventFile(Logger logger, string pEventFolderPath, CrsdFileData pCrsdFileData,
            out string oMessage, out List<string> elFiles)
        {
            CrsdEventListData crsdEventlist = new CrsdEventListData();
            string EventListDirPath = Path.Combine(pEventFolderPath, pCrsdFileData.EventListDir);
            string EventListFilePath = string.Empty;
            elFiles = new List<string>();

            if (!Directory.Exists(EventListDirPath))
            {
                oMessage = string.Format(Common.Message.Error_NoSuchDir, "イベント");
                logger.OutputLog($"{oMessage} フォルダパス：{EventListDirPath}", Logger.LogType.Warning);
                return null;
            }

            string[] EventFiles = Directory.GetFiles(EventListDirPath, "*.el");
            string Std_FillFilePath = string.Empty;
            string LEFilePath = string.Empty;
            string NOCVSFilePath = string.Empty;
            string DefaultFilePath = string.Empty;
            oMessage = string.Empty;
            if (EventFiles.Length == 1)
            {
                EventListFilePath = EventFiles.First();
            }
            else
            {
                foreach (string file in EventFiles)
                {
                    elFiles.Add(System.IO.Path.GetFileName(file));
                    switch (Path.GetFileName(file).ToUpper())
                    {
                        case "STD_FILL.EL":
                            Std_FillFilePath = file;
                            break;
                        case "LE.EL":
                            LEFilePath = file;
                            break;
                        case "NOCVS.EL":
                            NOCVSFilePath = file;
                            break;
                        case "DEFAULT.EL":
                            DefaultFilePath = file;
                            break;
                    }
                }
                if (Std_FillFilePath != string.Empty) EventListFilePath = Std_FillFilePath;
                else if (LEFilePath != string.Empty) EventListFilePath = LEFilePath;
                else if (NOCVSFilePath != string.Empty) EventListFilePath = NOCVSFilePath;
                else if (DefaultFilePath != string.Empty) EventListFilePath = DefaultFilePath;
            }

            if (EventListFilePath == string.Empty)
            {
                oMessage = string.Format(Common.Message.Error_CheckFile, "イベント");
                logger.OutputLog($"{oMessage} フォルダパス：{pEventFolderPath}", Logger.LogType.Warning);
                return null;
            }

            // イベントリストファイルを読み込み
            try
            {
                crsdEventlist.Load(EventListFilePath);
            }
            catch
            {
                oMessage = string.Format(Common.Message.Error_ReadFile, "イベントリスト");
                logger.OutputLog($"{oMessage} ファイルパス：{EventListFilePath}", Logger.LogType.Error);
                return null;
            }
            if(crsdEventlist.EventFileList.Count == 0)
            {
                oMessage = string.Format(Common.Message.Error_EmptyData, "イベントリスト");
                logger.OutputLog($"{oMessage} ファイルパス：{EventListFilePath}", Logger.LogType.Warning);
                return null;
            }

            // イベントファイルを読み込み
            crsdEventlist.EventList = new List<CrsdEventData>();
            for(int i = 0;i < crsdEventlist.EventFileList.Count;i++)
            {
                string EventFileName = crsdEventlist.EventFileList[i];
                string BlockType = string.Empty;
                if (i < pCrsdFileData.Blocks.Count) BlockType = pCrsdFileData.Blocks[i].BlockType;

                if (EventFileName == "NULL")
                {
                    crsdEventlist.EventList.Add(new CrsdEventData(BlockType));
                    continue;
                }

                string EventFilePath = Path.Combine(pEventFolderPath, EventFileName);
                if (!File.Exists(EventFilePath))
                {
                    crsdEventlist.EventList.Add(new CrsdEventData(BlockType));
                    continue;
                }

                try
                {
                    crsdEventlist.EventList.Add(new CrsdEventData(EventFilePath, BlockType));
                }
                catch
                {
                    oMessage = string.Format(Common.Message.Error_ReadFile, "イベント");
                    logger.OutputLog($"{oMessage} ファイルパス：{EventListFilePath}", Logger.LogType.Error);
                    return null;
                }
            }

            return crsdEventlist;
        }
        #endregion
    }
}