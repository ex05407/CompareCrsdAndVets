using CompareCrsdAndVets.Class;
using CompareCrsdAndVets.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CompareCrsdAndVets.Common.Const;
using static CompareCrsdAndVets.Common.CommonMethods;
using static CompareCrsdAndVets.Project.CreateData;
using Message = CompareCrsdAndVets.Common.Message;

namespace CompareCrsdAndVets.Project
{
    internal class CompareData
    {
        #region "外部変数"
        /// <summary>ログ関連クラス</summary>
        Logger logger;

        /// <summary>単位変換用クラス</summary>
        QuantitiesUnitsTable UnitConvet;
        #endregion

        #region "クラス"
        /// <summary>
        /// CRSD-7000のフェーズ情報を、NormalとSoakに分けて保持するためのクラス
        /// </summary>
        class CrsdEx
        {
            public List<List<CrsdPhaseData>> NormalPhaseList = new List<List<CrsdPhaseData>>();
            public List<List<CrsdPhaseData>> SoakPhaseList = new List<List<CrsdPhaseData>>();
            public List<List<CrsdPhaseData>> IdleCheckPhaseList = new List<List<CrsdPhaseData>>();
            public List<KeyValuePair<List<CrsdPhaseData>, PhaseType>> PhaseList = new List<KeyValuePair<List<CrsdPhaseData>, PhaseType>>();
            public void SetPhaseList(List<CrsdPhaseData> pPhaseList, PhaseType pPhaseType)
            {
                PhaseList.Add(new KeyValuePair<List<CrsdPhaseData>, PhaseType>(pPhaseList, pPhaseType));

                switch (pPhaseType)
                {
                    case PhaseType.Normal:
                    case PhaseType.Warmup:
                        NormalPhaseList.Add(pPhaseList);
                        break;
                    case PhaseType.Soak:
                        SoakPhaseList.Add(pPhaseList);
                        break;
                    case PhaseType.IdleCheck:
                        IdleCheckPhaseList.Add(pPhaseList);
                        break;
                }
            }
        }
        #endregion

        #region "コンストラクタ"
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="unitConvert"></param>
        public CompareData(Logger logger, QuantitiesUnitsTable unitConvert)
        {
            this.logger = logger;
            this.UnitConvet = unitConvert;
        }
        #endregion

        #region "Excel出力用のデータ作成"
        /// <summary>
        /// Excelデータの作成
        /// </summary>
        /// <param name="pCrsdFileData"></param>
        /// <param name="pVetsFileData"></param>
        /// <returns></returns>
        public List<string[]> CreateExcelData(CrsdFileData pCrsdFileData, TestProcedure pVetsFileData)
        {
            List<string[]> retData = new List<string[]>();
            string FileName = pVetsFileData.TestModeFileName;

            bool Result = true;
            int DriveUnitCount = 0;
            int SoakCount = 0;
            int IdleCheckCount = 0;
            CrsdEx crsdPhase = CreatePhaseBlock(pCrsdFileData);
            //ConvertItem(pVetsFileData);

#if DEBUG
            string text = string.Empty;
            pCrsdFileData.Phases.ForEach(phase =>
            {
                text += phase.PhaseType + " > ";
            }) ;
            text = text.TrimEnd(' ', '>');
            retData.Add(CreateExcelItem(ref Result, pFileName: FileName, pItemName: "フェーズ種類", pCrsdVal: text, pVetsVal: string.Empty, pCheckResultFlg: false));
#endif

            // 速度単位
            retData.Add(CreateExcelItem(ref Result, pFileName: FileName, pItemName: "速度単位", pCrsdVal: pCrsdFileData.DisplayUnits, pVetsVal: pVetsFileData.CycleSpeedUnits));

            // トレースの開始
            string vetsTraceStartMode = string.Empty;
            string TraceStartModeError = string.Empty;
            foreach (BaseBlock baseBlock1 in pVetsFileData.CycleBlocks)
            {
                if (baseBlock1.GetType() == typeof(DriveUnitBlock))
                {
                    vetsTraceStartMode = ((DriveUnitBlock)baseBlock1).TraceStartMode;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(pCrsdFileData.TraceStartModeError)) TraceStartModeError = pCrsdFileData.TraceStartModeError;
            else if (string.IsNullOrEmpty(vetsTraceStartMode)) TraceStartModeError = string.Format(Message.Error_NoExistVets, "トレースの開始");
            else if(pCrsdFileData.Blocks.Count > pCrsdFileData.EventListData.EventList.Count) TraceStartModeError = string.Format(Message.Error_BigSize, "CRSDのブロック", "イベント");
            retData.Add(CreateExcelItem(ref Result, pFileName: pVetsFileData.TestModeFileName, pItemName: "トレースの開始",
                pCrsdVal: pCrsdFileData.TraceStartMode, pVetsVal: vetsTraceStartMode, pNote: TraceStartModeError));

            // データを確認
            //List<CrsdPhaseData> PhaseMinList = new List<CrsdPhaseData>();
            int MaxCount = Math.Max(pVetsFileData.CycleBlocks.Count, crsdPhase.PhaseList.Count);
            BaseBlock baseBlock = null;
            for (int i = 0; i < MaxCount; i++)
            {
                List<CrsdPhaseData> PhaseMinList = null;
                PhaseType PhaseMinType = PhaseType.Unknown;
                if (i < crsdPhase.PhaseList.Count)
                {
                    PhaseMinList = crsdPhase.PhaseList[i].Key;
                    PhaseMinType = crsdPhase.PhaseList[i].Value;
                }
                if (i < pVetsFileData.CycleBlocks.Count) baseBlock = pVetsFileData.CycleBlocks[i];

                if(baseBlock == null)
                {
                    if (PhaseMinType == PhaseType.Normal || PhaseMinType == PhaseType.Warmup)
                    {
                        retData.AddRange(CreateExcelDataForDriveUnit(ref Result, pVetsFileData.CycleSpeedUnits,
                            pCrsdFileData, PhaseMinList, DriveUnitCount, new DriveUnitBlock(), FileName));
                    }
                    else if(PhaseMinType == PhaseType.Soak)
                    {
                        retData.AddRange(CreateExcelDataForSoak(ref Result, pVetsFileData.CycleSpeedUnits, PhaseMinList.First(), null, FileName));
                    }
                    else if (PhaseMinType == PhaseType.IdleCheck)
                    {
                        retData.AddRange(CreateExcelDataForIdleCheck(ref Result, PhaseMinList.First(), null, FileName));
                        
                        //retData.Add(CreateExcelItem(ref Result, pFileName: FileName, pBlockName: BlockName_IdleCheck, pCheckResultFlg: false,
                        //    pNote: string.Format(Message.Error_NoExistVets, "ブロック")));
                    }
                }
                else if (baseBlock.GetType() == typeof(DriveUnitBlock))
                {
                    DriveUnitBlock block = (DriveUnitBlock)baseBlock;
                    DriveUnitCount++;

                    if (PhaseMinType == PhaseType.Normal || PhaseMinType == PhaseType.Warmup)
                    {
                        retData.AddRange(CreateExcelDataForDriveUnit(ref Result, pVetsFileData.CycleSpeedUnits,
                            pCrsdFileData, PhaseMinList, DriveUnitCount, block, FileName));
                    }
                    else if(PhaseMinType == PhaseType.Soak)
                    {
                        retData.AddRange(CreateExcelDataForDriveUnit(ref Result, pVetsFileData.CycleSpeedUnits,
                            pCrsdFileData, new List<CrsdPhaseData>(), DriveUnitCount, block, FileName));
                        retData.AddRange(CreateExcelDataForSoak(ref Result, pVetsFileData.CycleSpeedUnits, PhaseMinList.First(), null, FileName));
                    }
                    else if(PhaseMinType == PhaseType.IdleCheck)
                    {
                        retData.AddRange(CreateExcelDataForDriveUnit(ref Result, pVetsFileData.CycleSpeedUnits,
                            pCrsdFileData, new List<CrsdPhaseData>(), DriveUnitCount, block, FileName));

                        retData.AddRange(CreateExcelDataForIdleCheck(ref Result, PhaseMinList.First(), null, FileName));
                        //retData.Add(CreateExcelItem(ref Result, pFileName: FileName, pBlockName: BlockName_IdleCheck, pCheckResultFlg: false,
                        //    pNote: string.Format(Message.Error_NoExistVets, "ブロック")));
                    }
                    else
                    {
                        retData.AddRange(CreateExcelDataForDriveUnit(ref Result, pVetsFileData.CycleSpeedUnits,
                            pCrsdFileData, new List<CrsdPhaseData>(), DriveUnitCount, block, FileName));
                    }
                }
                else if(baseBlock.GetType() == typeof(SoakBlock))
                {
                    SoakBlock block = (SoakBlock)baseBlock;
                    SoakCount++;

                    if (PhaseMinType == PhaseType.Soak)
                    {
                        retData.AddRange(CreateExcelDataForSoak(ref Result, pVetsFileData.CycleSpeedUnits, PhaseMinList.First(), block, FileName));
                    }
                    else if (PhaseMinType == PhaseType.Normal || PhaseMinType == PhaseType.Warmup)
                    {
                        retData.AddRange(CreateExcelDataForSoak(ref Result, pVetsFileData.CycleSpeedUnits, null, block, FileName));
                        retData.AddRange(CreateExcelDataForDriveUnit(ref Result, pVetsFileData.CycleSpeedUnits,
                            pCrsdFileData, PhaseMinList, DriveUnitCount, new DriveUnitBlock(), FileName));
                    }
                    else if(PhaseMinType == PhaseType.IdleCheck)
                    {
                        retData.AddRange(CreateExcelDataForSoak(ref Result, pVetsFileData.CycleSpeedUnits, null, block, FileName));

                        retData.AddRange(CreateExcelDataForIdleCheck(ref Result, PhaseMinList.First(), null, FileName));
                        //retData.Add(CreateExcelItem(ref Result, pFileName: FileName, pBlockName: BlockName_IdleCheck, pCheckResultFlg: false,
                        //    pNote: string.Format(Message.Error_NoExistVets, "ブロック")));
                    }
                    else
                    {
                        retData.AddRange(CreateExcelDataForSoak(ref Result, pVetsFileData.CycleSpeedUnits, null, block, FileName));
                    }
                }
                else if(baseBlock.GetType() == typeof(IdleCheckBlock))
                {
                    IdleCheckBlock block = (IdleCheckBlock)baseBlock;
                    string note = string.Format(Message.Error_NoExistCrsd, "ブロック");
                    IdleCheckCount++;

                    if (PhaseMinType == PhaseType.Normal || PhaseMinType == PhaseType.Warmup)
                    {
                        retData.AddRange(CreateExcelDataForIdleCheck(ref Result, null, block, FileName));
                        //retData.Add(CreateExcelItem(ref Result, pFileName: FileName, pBlockName: BlockName_IdleCheck, pCheckResultFlg: false,
                        //    pNote: note));
                        retData.AddRange(CreateExcelDataForDriveUnit(ref Result, pVetsFileData.CycleSpeedUnits,
                            pCrsdFileData, PhaseMinList, DriveUnitCount, new DriveUnitBlock(), FileName));
                    }
                    else if (PhaseMinType == PhaseType.Soak)
                    {
                        retData.AddRange(CreateExcelDataForIdleCheck(ref Result, null, block, FileName));
                        //retData.Add(CreateExcelItem(ref Result, pFileName: FileName, pBlockName: BlockName_IdleCheck, pCheckResultFlg: false,
                        //    pNote: note));
                        retData.AddRange(CreateExcelDataForSoak(ref Result, pVetsFileData.CycleSpeedUnits, PhaseMinList.First(), null, FileName));
                    }
                    else if (PhaseMinType == PhaseType.IdleCheck)
                    {
                        retData.AddRange(CreateExcelDataForIdleCheck(ref Result, PhaseMinList.First(), block, FileName));
                        //retData.Add(CreateExcelItem(ref Result, pFileName: FileName, pBlockName: BlockName_IdleCheck, pCheckResultFlg: false));
                    }
                    else
                    {
                        retData.Add(CreateExcelItem(ref Result, pFileName: FileName, pBlockName: BlockName_IdleCheck, pCheckResultFlg: false,
                            pNote: note));
                    }
                }

                /*
                // ドライブユニット
                if (pVetsFileData.CycleBlocks[i].GetType() == typeof(DriveUnitBlock))
                {
                    if (crsdPhase.NormalPhaseList.Count() > DriveUnitCount)
                    {
                        PhaseMinList = crsdPhase.NormalPhaseList.ElementAt(DriveUnitCount);
                    }
                    else
                    {
                        PhaseMinList = new List<CrsdPhaseData>();
                    }

                    DriveUnitCount++;
                    DriveUnitBlock block = (DriveUnitBlock)pVetsFileData.CycleBlocks[i];
                    retData.AddRange(CreateExcelDataForDriveUnit(ref Result, pVetsFileData.CycleSpeedUnits,
                        pCrsdFileData, PhaseMinList, DriveUnitCount, block, FileName));
                }
                // ソーク
                else if (pVetsFileData.CycleBlocks[i].GetType() == typeof(SoakBlock))
                {
                    CrsdPhaseData Phase = null;
                    if (crsdPhase.SoakPhaseList.Count() > SoakCount)
                    {
                        Phase = crsdPhase.SoakPhaseList.ElementAt(SoakCount).First();
                    }

                    SoakCount++;
                    SoakBlock block = (SoakBlock)pVetsFileData.CycleBlocks[i];

                    retData.AddRange(CreateExcelDataForSoak(ref Result, Phase, block, FileName));
                }
                */
            }

            /*
            if (DriveUnitCount < crsdPhase.NormalPhaseList.Count)
            {
                retData.Add(CreateExcelItem(ref Result, pFileName: FileName, pBlockName: BlockName_DriveUnit, pCheckResultFlg: false,
                    pNote: string.Format(Message.Error_NoRead, "VETS", "Phase")));
            }
            if (SoakCount < crsdPhase.SoakPhaseList.Count)
            {
                retData.Add(CreateExcelItem(ref Result, pFileName: FileName, pBlockName: BlockName_Soak, pCheckResultFlg: false,
                    pNote: string.Format(Message.Error_NoRead, "VETS", "Phase")));
            }
            if (crsdPhase.IdleCheckPhaseList.Count != 0)
            {
                retData.Add(CreateExcelItem(ref Result, pFileName: FileName, pBlockName: BlockName_IdleCheck, pCheckResultFlg: false,
                    pNote: string.Format(Message.Error_ExistData, "アイドルチェック", "Phase")));
            }
            */

            // 全体結果
            retData.Insert(0, CreateExcelItem(ref Result, pFileName: FileName, pItemName: ItemName_All, pResult: Result));

            return retData;
        }
        #endregion

        #region "ドライブユニット情報のデータ作成"
        /// <summary>
        /// ドライブユニットの設定
        /// </summary>
        /// <param name="oResult"></param>
        /// <param name="pCrsdFileData"></param>
        /// <param name="pPhaseList"></param>
        /// <param name="pDriveUnitCount"></param>
        /// <param name="block"></param>
        /// <param name="pFileName"></param>
        /// <returns></returns>
        private List<string[]> CreateExcelDataForDriveUnit(ref bool oResult, string pCycleSpeedUnits,
            CrsdFileData pCrsdFileData, List<CrsdPhaseData> pPhaseList, int pDriveUnitCount, DriveUnitBlock block, string pFileName)
        {
            List<string[]> retData = new List<string[]>();
            CrsdPhaseData PhaseFirst = new CrsdPhaseData();
            bool IsExistPhase = (pPhaseList.Count == 0);
            if (pPhaseList.Count != 0) PhaseFirst = pPhaseList.First();


            // ドライブユニット名を設定
            string DriveUnitName = string.Concat(BlockName_DriveUnit, pDriveUnitCount);

            // 速度許容差
            double ViolationSpeedTolerance = ConvertUnit(block.ViolationSpeedTolerance, "Speed", pCycleSpeedUnits);
            double CrsdSpeedTolerance = -1;
            if (!IsExistPhase) CrsdSpeedTolerance = ToDouble(pCrsdFileData.SpeedTolerance, -1);
            if (CrsdSpeedTolerance != -1 && pCrsdFileData.DisplayUnits != pCycleSpeedUnits) CrsdSpeedTolerance = ConvertUnit(CrsdSpeedTolerance, pCrsdFileData.DisplayUnits, pCycleSpeedUnits);
            retData.Add(CreateExcelItem(ref oResult, pFileName: pFileName, pBlockName: DriveUnitName, pItemName: $"速度許容差({pCycleSpeedUnits})",
                pCrsdVal: IsExistPhase || CrsdSpeedTolerance == -1 ? string.Empty : CrsdSpeedTolerance.ToString("0.0"), pVetsVal: ViolationSpeedTolerance.ToString("0.0"),
                pNote: IsExistPhase ? string.Format(Message.Error_NoExistCrsd, "ブロック") : CheckDouble(pCrsdFileData.SpeedTolerance, $"速度許容差({pCycleSpeedUnits})")));

            // 時間許容差
            retData.Add(CreateExcelItem(ref oResult, pFileName: pFileName, pBlockName: DriveUnitName, pItemName: "時間許容差(s)",
                pCrsdVal: IsExistPhase ? string.Empty : pCrsdFileData.TimeTolerance, pVetsVal: block.ViolationTimeTolerance.ToString("0.0")));

            // トレース開始
            retData.Add(CreateExcelItem(ref oResult, pFileName: pFileName, pBlockName: DriveUnitName, pItemName: "トレースの開始",
                //pCrsdVal: PhaseFirst.TraceStartMode, pVetsVal: block.TraceStartMode, pNote: PhaseFirst.TraceStartModeError));
                pCrsdVal: PhaseFirst.EventData.TraceStartMode, pVetsVal: block.TraceStartMode, pNote: PhaseFirst.EventData.TraceStartModeError));

            // イベント情報の設定
            var events = block.Events.Where(x => x.EventActions.Exists(a => a.Name == "EmissionSample" || a.Name == string.Empty));
            var phases = pPhaseList.Where(x => x.PhaseTypeEnum != PhaseType.Warmup);
            int EventMaxCount = Math.Max(events.Count(), phases.Count());
            for (int i = 0; i < EventMaxCount; i++)
            {
                EventDefinition Event = null;
                if (i < events.Count()) Event = events.ElementAt(i);
                else Event = new EventDefinition();
                CrsdPhaseData Phase = null;
                if (i < phases.Count()) Phase = phases.ElementAt(i);
                else Phase = new CrsdPhaseData();
                retData.AddRange(CreateEvent(ref oResult, Event, block.Events, Phase, pPhaseList, pFileName, DriveUnitName, i + 1, PhaseFirst.IsEmpty));
            }

            return retData;
        }
        #endregion

        #region "ソーク情報のデータ作成"
        /// <summary>
        /// ソークの設定
        /// </summary>
        /// <param name="oResult"></param>
        /// <param name="pCrsdPhaseData"></param>
        /// <param name="block"></param>
        /// <param name="pFileName"></param>
        /// <returns></returns>
        private List<string[]> CreateExcelDataForSoak(ref bool oResult, string pCycleSpeedUnits, CrsdPhaseData pCrsdPhaseData, SoakBlock block, string pFileName)
        {
            List<string[]> retData = new List<string[]>();
            string SoakMinTime = pCrsdPhaseData == null ? string.Empty : pCrsdPhaseData.SoakMinTime;
            string SoakMaxTime = pCrsdPhaseData == null ? string.Empty : pCrsdPhaseData.SoakMaxTime;

            // 最小時間
            retData.Add(CreateExcelItem(ref oResult, pFileName: pFileName, pBlockName: BlockName_Soak, pItemName: "最小時間 (s)",
                pCrsdVal: SoakMinTime, pVetsVal: block == null ? string.Empty : block.MinimumDuration.ToString("0.0"),
                pNote: pCrsdPhaseData == null ? string.Format(Message.Error_NoExistCrsd, "ブロック") : CheckDouble(SoakMinTime, "最小時間"), pIsNuber: true));
            // 最大時間
            retData.Add(CreateExcelItem(ref oResult, pFileName: pFileName, pBlockName: BlockName_Soak, pItemName: "最大時間 (s)",
                pCrsdVal: SoakMaxTime, pVetsVal: block == null ? string.Empty : block.MaximumDuration.ToString("0.0"),
                pNote: pCrsdPhaseData == null ? string.Empty : CheckDouble(SoakMaxTime, "最大時間"), pIsNuber: true));

            return retData;
        }
        #endregion

        #region "アイドルチェック情報のデータ作成"
        /// <summary>
        /// アイドルチェックの設定
        /// </summary>
        /// <param name="oResult"></param>
        /// <param name="pCrsdPhaseData"></param>
        /// <param name="block"></param>
        /// <param name="pFileName"></param>
        /// <returns></returns>
        private List<string[]> CreateExcelDataForIdleCheck(ref bool oResult, CrsdPhaseData pCrsdPhaseData, IdleCheckBlock block, string pFileName)
        {
            List<string[]> retData = new List<string[]>();

            // 時間
            retData.Add(CreateExcelItem(ref oResult, pFileName: pFileName, pBlockName: BlockName_IdleCheck, pItemName: "時間 (s)",
                pCrsdVal: pCrsdPhaseData == null ? string.Empty : pCrsdPhaseData.PhaseTime, 
                pVetsVal: block == null ? string.Empty : block.MeasurementTime.ToString("0.0"),
                pNote: pCrsdPhaseData == null ? string.Empty : CheckDouble(pCrsdPhaseData.PhaseTime, "最大時間"), pIsNuber: true));

            return retData;
        }
        #endregion

        #region "イベント情報のデータ作成"
        /// <summary>
        /// イベントの設定
        /// </summary>
        /// <param name="oResult"></param>
        /// <param name="Event"></param>
        /// <param name="pFileName"></param>
        /// <param name="pDriveUnitName"></param>
        /// <param name="pEventCount"></param>
        /// <returns></returns>
        private List<string[]> CreateEvent(ref bool oResult, EventDefinition Event, List<EventDefinition> EventList,
            CrsdPhaseData Phase, List<CrsdPhaseData> pPhaseList, string pFileName, string pDriveUnitName, int pEventCount, bool IsPhaseEmpty)
        {
            List<string[]> retData = new List<string[]>();
            bool IsStart = false;
            bool IsEnd = false;

            string EventName = string.Concat(Const.EventName, pEventCount);
            bool NoEventFlg = string.IsNullOrEmpty(Event.TriggerName);

            // トリガーの設定
            string Note = string.Empty;
            if (!IsPhaseEmpty)
            {
                if (Phase.IsEmpty) Note = string.Format(Message.Error_NoExistCrsd, "イベント");
                if (NoEventFlg) Note = string.Format(Message.Error_NoExistVets, "イベント");
            }

            retData.Add(CreateExcelItem(ref oResult, pFileName: pFileName, pBlockName: pDriveUnitName, pEventName: EventName,
                pItemName: "トリガー", pCrsdVal: Phase.IsEmpty ? string.Empty : Phase.TriggerName, pVetsVal: Event.TriggerName, pNote: Note));

            if (Phase.TriggerName == TriggerName_PendantButon || Event.TriggerName == TriggerName_PendantButon)
            {
                retData.Add(CreateExcelItem(ref oResult, pFileName: pFileName, pBlockName: pDriveUnitName, pEventName: EventName,
                        pItemName: "PendantButton", pCrsdVal: Phase.IsEmpty ? string.Empty : "Start", pVetsVal: Event.PendantButton));
            }
            if ((!Phase.IsEmpty && (Phase.IsTimeTrigger || Event.IsTimeTrigger)))
            {
                retData.Add(CreateExcelItem(ref oResult, pFileName: pFileName, pBlockName: pDriveUnitName, pEventName: EventName,
                        pItemName: "時間(s)", pVetsVal: Event.IsTimeTrigger ? Event.Time.ToString() : string.Empty,
                        pCrsdVal: Phase.IsTimeTrigger ? (Phase.TriggerName == TriggerName_Time ? Phase.StartTime.ToString() : 0.ToString()) : string.Empty,
                        pNote: !Phase.IsTimeError ? string.Empty : string.Format(Message.Error_NoNumber, "時間"), pIsNuber: true));
            }

            // イベントアクションの設定
            if (NoEventFlg)
            {
                if (Phase.TriggerName == TriggerName_PendantButon)
                {
                    retData.AddRange(CreateEventActionForStart(ref oResult, null, Phase, pFileName, pDriveUnitName, EventName, string.Empty, true));
                }
            }
            else
            {
                // アクション
                foreach (EventActionDefinition Action in Event.EventActions)
                {
                    if (Action.Name == "EmissionSample")
                    {
                        retData.AddRange(CreateEventActionForStart(ref oResult, Action, Phase, pFileName, pDriveUnitName, EventName, string.Empty, Phase.IsEmpty));
                        IsStart = true;
                    }
                    else if (Action.Name == string.Empty)
                    {
                        if (Action.StartActionId != -1)
                        {
                            // 開始アクションを取得
                            EventDefinition StartEvent = EventList.Where(a =>
                            a.EventActions.Exists(x => x.Id == Action.StartActionId)).FirstOrDefault();
                            EventActionDefinition StartAction = StartEvent.EventActions.Where(x => x.Id == Action.StartActionId).FirstOrDefault();
                            if (StartAction.Id != Action.StartActionId) StartAction = null;
                            retData.AddRange(CreateEventActionForEnd(ref oResult, StartAction, Phase, pFileName, pDriveUnitName, EventName,
                               string.Empty, Phase.IsEmpty));
                            IsEnd = true;
                        }
                    }
                }
            }

            // フェーズにのみ存在するアクションの確認
            if (!Phase.IsEmpty && (Phase.TriggerName == TriggerName_Time || Phase.TriggerName == TriggerName_TraceFinish))
            {
                if (Phase.PhaseNoInt > 0 && !IsStart)
                {
                    // 開始アクションを設定
                    retData.AddRange(CreateEventActionForStart(ref oResult, null, Phase, pFileName, pDriveUnitName, EventName,
                        NoEventFlg ? string.Empty : string.Format(Message.Error_NoExistVets, "アクション"), NoEventFlg));
                }
                if (Phase.BeforePhaseNo > 0 && !IsEnd)
                {
                    // 終了アクションを設定
                    retData.AddRange(CreateEventActionForEnd(ref oResult, null, Phase, pFileName, pDriveUnitName, EventName,
                        NoEventFlg ? string.Empty : string.Format(Message.Error_NoExistVets, "アクション"), NoEventFlg));
                }
            }

            return retData;
        }
        #endregion

        #region "トレーススタートのデータ作成"
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
        public string[] CreateExcelItem_TraceStartData(string pFileName = "", string pTraceStart = "-")
        {
            string[] strings = new string[OutputTraceStartExcelCol];
            strings[(int)ExcelCols_TraceStart.Name - 1] = pFileName;
            strings[(int)ExcelCols_TraceStart.TraceStart - 1] = pTraceStart;

            return strings;
        }
        #endregion

        #region "イベントアクションの設定（開始）"
        /// <summary>
        /// イベントアクションの設定（開始）
        /// </summary>
        /// <param name="oTime"></param>
        /// <param name="oResult"></param>
        /// <param name="Action"></param>
        /// <param name="pPhaseList"></param>
        /// <param name="pFileName"></param>
        /// <param name="pDriveUnitName"></param>
        /// <param name="pEventName"></param>
        /// <returns></returns>
        private List<string[]> CreateEventActionForStart(ref bool oResult, EventActionDefinition Action,
            CrsdPhaseData pPhase, string pFileName, string pDriveUnitName, string pEventName, string pNote, bool pIsEmptyFlg)
        {
            List<string[]> retData = new List<string[]>();

            bool NoActionFlg = (Action == null);
            bool NoPhaseFlg = (pPhase.PhaseNo == string.Empty);
            string ActionName = "開始　Emission Sample";

            string Note = string.Empty;
            if (!pPhase.IsEmpty)
            {
                if (!string.IsNullOrEmpty(pNote)) Note = pNote;
                else if (pIsEmptyFlg) Note = string.Empty;
                else if (NoPhaseFlg) Note = string.Format(Message.Error_NoExistCrsd, "サンプル番号");
                else if (NoActionFlg) Note = string.Format(Message.Error_NoExistVets, "サンプル番号");
                else if (pPhase.PhaseNoInt == -1) Note = string.Format(Message.Error_NoNumber, "サンプル番号");
            }

            // サンプル番号
            retData.Add(CreateExcelItem(ref oResult, pFileName: pFileName, pBlockName: pDriveUnitName, pEventName: pEventName,
                //pActionName: ActionName, pItemName: "サンプル番号", pCrsdVal: pPhase.PhaseNo,
                pActionName: ActionName, pItemName: "サンプル番号", pCrsdVal: pPhase.BagPairNo,
                pVetsVal: NoActionFlg ? string.Empty : Action.SampleNumber.ToString(), pNote: Note));
            // バッグペア番号
            retData.Add(CreateExcelItem(ref oResult, pFileName: pFileName, pBlockName: pDriveUnitName, pEventName: pEventName,
                //pActionName: ActionName, pItemName: "バッグペア番号", pCrsdVal: pPhase.PhaseNo,
                pActionName: ActionName, pItemName: "バッグペア番号", pCrsdVal: pPhase.BagPairNo,
                pVetsVal: NoActionFlg ? string.Empty : Action.BagSampleNumber.ToString()));
            // PMフィルタ番号
            retData.Add(CreateExcelItem(ref oResult, pFileName: pFileName, pBlockName: pDriveUnitName, pEventName: pEventName,
                //pActionName: ActionName, pItemName: "PMフィルタ番号", pCrsdVal: pPhase.PhaseNo,
                pActionName: ActionName, pItemName: "PMフィルタ番号", pCrsdVal: pPhase.BagPairNo,
                pVetsVal: NoActionFlg ? string.Empty : Action.PmSampleNumber.ToString()));
            // 理論走行距離
            retData.Add(CreateExcelItem(ref oResult, pFileName: pFileName, pBlockName: pDriveUnitName, pEventName: pEventName,
                pActionName: ActionName, pItemName: "理論距離(km)", pCrsdVal: NoPhaseFlg ? string.Empty : pPhase.PhaseDistance.ToString(),
                //pVetsVal: NoActionFlg ? string.Empty : Action.NominalDistance.ToString(), pIsNuber: true));
                pVetsVal: NoActionFlg ? string.Empty : (Action.NominalDistance == 0 ? 0.ToString() : (Action.NominalDistance / 1000).ToString()), pIsNuber:true));

            return retData;
        }
        #endregion

        #region "イベントアクションの設定（終了）"
        /// <summary>
        /// イベントアクションの設定（終了）
        /// </summary>
        /// <param name="oResult"></param>
        /// <param name="Action"></param>
        /// <param name="pPhase"></param>
        /// <param name="pFileName"></param>
        /// <param name="pDriveUnitName"></param>
        /// <param name="pEventName"></param>
        /// <returns></returns>
        private List<string[]> CreateEventActionForEnd(ref bool oResult, EventActionDefinition Action, CrsdPhaseData pPhase,
            string pFileName, string pDriveUnitName, string pEventName, string pNote, bool pIsEmptyFlg)
        {
            List<string[]> retData = new List<string[]>();
            string ActionName = "終了　Emission Sample";

            bool IsEmptyAction = (Action == null);
            bool IsEmptyPhase = (pPhase.BeforeBagPairNo < 0);
            string Note = string.Empty;

            if (!string.IsNullOrEmpty(pNote)) Note = pNote;
            else if (pIsEmptyFlg) Note = string.Empty;
            else if (IsEmptyPhase) Note = string.Format(Message.Error_NoExistCrsd, "サンプル番号");
            else if (IsEmptyAction) Note = string.Format(Message.Error_NoExistVets, "サンプル番号");

            string CrsdPhaseNo = IsEmptyPhase ? string.Empty : pPhase.BeforeBagPairNo.ToString();

            // サンプル番号
            retData.Add(CreateExcelItem(ref oResult, pFileName: pFileName, pBlockName: pDriveUnitName, pEventName: pEventName,
                pActionName: ActionName, pItemName: "サンプル番号", pCrsdVal: CrsdPhaseNo,
                pVetsVal: IsEmptyAction ? "" : Action.SampleNumber.ToString(), pNote: Note));
            // バッグペア番号
            retData.Add(CreateExcelItem(ref oResult, pFileName: pFileName, pBlockName: pDriveUnitName, pEventName: pEventName,
                pActionName: ActionName, pItemName: "バッグペア番号", pCrsdVal: CrsdPhaseNo,
                pVetsVal: IsEmptyAction ? "" : Action.BagSampleNumber.ToString()));
            // PMフィルタ番号
            retData.Add(CreateExcelItem(ref oResult, pFileName: pFileName, pBlockName: pDriveUnitName, pEventName: pEventName,
                pActionName: ActionName, pItemName: "PMフィルタ番号", pCrsdVal: CrsdPhaseNo,
                pVetsVal: IsEmptyAction ? "" : Action.PmSampleNumber.ToString()));

            return retData;
        }
        #endregion

        #region "フェーズ情報からブロックを作成"
        /// <summary>
        /// CRSD-7000のフェーズ情報を、NormalとSoakに分けてブロック情報として保持するためのクラスを作成する
        /// </summary>
        /// <param name="pCrsdFileData"></param>
        /// <returns></returns>
        private CrsdEx CreatePhaseBlock(CrsdFileData pCrsdFileData)
        {
            // 同一ブロックのリストを作成
            CrsdEx crsdEx = new CrsdEx();
            List<CrsdPhaseData> PhaseMinList = new List<CrsdPhaseData>();
            PhaseType BeforePhase = PhaseType.Unknown;
            int BeforePhaseNo = -1;
            int BeforeBagPairNo = -1;
            double time_All = 0;
            bool IsError = false;
            int count = 0;
            foreach (CrsdPhaseData phase in pCrsdFileData.Phases)
            {
                if(count < pCrsdFileData.EventListData.EventList.Count) phase.EventData = pCrsdFileData.EventListData.EventList[count];
                count++;

                // 時間を追加する
                phase.StartTime = time_All;
                phase.IsTimeError = IsError;
                phase.BeforePhaseNo = BeforePhaseNo;
                phase.BeforeBagPairNo = BeforeBagPairNo;

                if (BeforePhase != phase.PhaseTypeEnum)
                {
                    // はじめがWarmup以外で、かつフェーズタイプが変更となった場合はトリガーを「PendantButton」とする。
                    if (BeforePhase != PhaseType.Warmup && !IsEneligiblePhase(phase.PhaseTypeEnum)) phase.TriggerName = "PendantButton";

                    // WarmupとNormalの切り替わりで、かつ「ペンダントStart」「ペンダントRun」以外(WSTなし)の場合は同一ブロック
                    if (((BeforePhase == PhaseType.Warmup && phase.PhaseTypeEnum == PhaseType.Normal) ||
                       (BeforePhase == PhaseType.Normal && phase.PhaseTypeEnum == PhaseType.Warmup)) &&
                       phase.WaitStartInt == 0)
                    {
                    }
                    else
                    {
                        if (IsEneligiblePhase(BeforePhase))
                        {
                            if(BeforePhase != PhaseType.Unknown) crsdEx.SetPhaseList(PhaseMinList, BeforePhase);
                            PhaseMinList = new List<CrsdPhaseData>();
                            time_All = 0;
                        }
                        else
                        {
                            // 最初のフェーズ以外の場合は、「Time」項目を追加後にフェーズリストにセットする。
                            if (!IsEneligiblePhase(BeforePhase) && BeforePhase != PhaseType.Warmup)
                            {
                                PhaseMinList.Add(new CrsdPhaseData()
                                {
                                    TriggerName = "Time",
                                    StartTime = time_All,
                                    IsTimeError = IsError,
                                    BeforePhaseNo = BeforePhaseNo,
                                    BeforeBagPairNo = BeforeBagPairNo
                                });
                            }

                            if (PhaseMinList.Count != 0)
                            {
                                crsdEx.SetPhaseList(PhaseMinList, BeforePhase);
                            }
                            PhaseMinList = new List<CrsdPhaseData>();
                            time_All = 0;
                        }
                    }
                }

                if (phase.PhaseTypeEnum != PhaseType.Unknown) PhaseMinList.Add(phase);
                BeforePhase = phase.PhaseTypeEnum;
                BeforePhaseNo = ToInt(phase.PhaseNo, -1);
                BeforeBagPairNo = ToInt(phase.BagPairNo, -1);
                if (!IsEneligiblePhase(phase.PhaseTypeEnum) && phase.PhaseTypeEnum != PhaseType.Soak && phase.PhaseTypeEnum != PhaseType.IdleCheck)
                {
                    double time = phase.PhaseTimeDbl;
                    if (!string.IsNullOrEmpty(phase.PhaseNo) && time == -1) IsError = true;
                    else time_All += time;
                }
            }

            // 最後に「TraceFinish」を入れる
            if (IsEneligiblePhase(BeforePhase))
            {
                crsdEx.SetPhaseList(PhaseMinList, BeforePhase);

                if (crsdEx.NormalPhaseList.Count != 0)
                {
                    crsdEx.NormalPhaseList.Last().Last().TriggerName = "TraceFinish";
                }
            }
            else
            {
                PhaseMinList.Add(new CrsdPhaseData()
                {
                    TriggerName = "TraceFinish",
                    StartTime = time_All,
                    IsTimeError = IsError,
                    BeforePhaseNo = BeforePhaseNo,
                    BeforeBagPairNo = BeforeBagPairNo
                });
                crsdEx.SetPhaseList(PhaseMinList, BeforePhase);
            }

            return crsdEx;
        }
        #endregion

        #region "単位変換"
        /// <summary>
        /// 単位の変換
        /// </summary>
        /// <param name="pBaseValue"></param>
        /// <param name="pBaseUnitName"></param>
        /// <param name="pConvertUnitName"></param>
        /// <returns></returns>
        private double ConvertUnit(double pBaseValue, string pBaseUnitName, string pConvertUnitName)
        {
            // 速度許容差の基準単位確認
            var speed = UnitConvet.Quantities.FirstOrDefault(q => q.Name == pBaseUnitName);
            var baseUnitName = speed?.BaseUnitName;

            // 換算係数を取得
            var baseUnit = UnitConvet.BaseUnits.FirstOrDefault(b => b.Name == baseUnitName);
            var kmh = baseUnit?.Units.FirstOrDefault(u => u.Name == pConvertUnitName);

            double gain = kmh?.Gain ?? 1.0;
            double offset = kmh?.Offset ?? 0.0;

            return pBaseValue * gain + offset;
        }
        #endregion

        #region "対象外フェーズの確認"
        /// <summary>対象外のフェーズ</summary>
        public bool IsEneligiblePhase(PhaseType pPhaseTypeEnum)
        {
            if (pPhaseTypeEnum == PhaseType.CoastDown ||
                pPhaseTypeEnum == PhaseType.Unknown)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region "TODO:修正要"
        /*
        private void ConvertItem(TestProcedure pVetsFileData)
        {
            List<double> list = new List<double>();
            foreach (BaseBlock block in pVetsFileData.CycleBlocks)
            {
                // ドライブユニットの場合のみの処理
                if (block.GetType() != typeof(DriveUnitBlock)) continue;

                DriveUnitBlock driveUnit = (DriveUnitBlock)block;
                foreach (EventDefinition eventDefinition in driveUnit.Events)
                {
                    if (eventDefinition.TriggerName == "Time")
                    {
                        list.Add(eventDefinition.Time);
                    }
                    else if (eventDefinition.TriggerName == "TraceFinish")
                    {
                        // トレースファイルの確認

                    }
                }
            }
        }
        */
        #endregion

    }
}
