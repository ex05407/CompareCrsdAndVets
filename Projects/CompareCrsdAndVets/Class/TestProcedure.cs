using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using static CompareCrsdAndVets.Common.Xml;
using static CompareCrsdAndVets.Common.CommonMethods;
using CompareCrsdAndVets.Common;

namespace CompareCrsdAndVets.Class
{
    /// <summary>
    /// テストプロシージャ情報格納用クラス
    /// </summary>
    internal class TestProcedure
    {
        /// <summary>
        /// イベントの時間情報格納用クラス
        /// </summary>
        internal class EventTimeEx
        {
            /// <summary>ドライブユニットのインクリメント値</summary>
            public int DriveUnitCount { get; set; }
            /// <summary>サンプル番号</summary>
            public int SampleNumber { get; set; }
            /// <summary>開始時間</summary>
            public double StartTime { get; set; }
            /// <summary>終了時間</summary>
            public double EndTime { get; set; }
            /// <summary>サンプル時間</summary>
            public double SampleTime { get => EndTime - StartTime; }
            /// <summary>エラーメッセージ</summary>
            public string ErrorMessage { get; set; } = string.Empty;

            /// <summary>
            /// 比較処理
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public int CompareTo(EventTimeEx value)
            {
                if (DriveUnitCount < value.DriveUnitCount) return -1;
                if (DriveUnitCount > value.DriveUnitCount) return 1;

                if (StartTime < value.StartTime) return -1;
                if (StartTime > value.StartTime) return 1;

                if (EndTime < value.EndTime) return -1;
                if (EndTime > value.EndTime) return 1;

                if (SampleNumber < value.SampleNumber) return -1;
                if (SampleNumber > value.SampleNumber) return 1;

                return 0;
            }
        }

        /// <summary>ファイルパス</summary>
        public string FilePath { get; set; } = string.Empty;
        
        /// <summary>ファイル名</summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>名前</summary>
        public string Name { get; set; }
        /// <summary>テストモードファイル名</summary>
        public string TestModeFileName { get; set; }

        /// <summary>速度単位</summary>
        public string CycleSpeedUnits { get; set; }

        /// <summary>ブロック情報</summary>
        public List<BaseBlock> CycleBlocks { get; set; } = new List<BaseBlock>();
        
        /// <summary>TraceStart</summary>
        public string TraceStart { get; set; }

        /// <summary>イベントの時間リスト</summary>
        public List<EventTimeEx> EventTimeList { get; set; } = new List<EventTimeEx>();

        /// <summary>
        /// サンプル時間情報の作成
        /// </summary>
        public void CreateSampleTime()
        {
            int DriveUnitCount = 0;
            foreach (BaseBlock block in CycleBlocks)
            {
                DriveUnitCount++;
                // ドライブユニットの場合のみの処理
                if (block.GetType() != typeof(DriveUnitBlock)) continue;

                List<Tuple<EventDefinition, EventActionDefinition>> StartEventList = new List<Tuple<EventDefinition, EventActionDefinition>>();
                List<Tuple<EventDefinition, EventActionDefinition>> EndEventList = new List<Tuple<EventDefinition, EventActionDefinition>>();

                DriveUnitBlock driveUnit = (DriveUnitBlock)block;
                foreach(EventDefinition eventDefinition in driveUnit.Events)
                {
                    // イベントの開始時間を設定
                    switch(eventDefinition.TriggerName)
                    {
                        case Const.TriggerName_PendantButon:
                            eventDefinition.StartTime = 0;
                            break;

                        case Const.TriggerName_Time:
                            eventDefinition.StartTime = eventDefinition.Time;
                            break;

                        case Const.TriggerName_TraceSegmentStart:
                            // トレースファイルの確認
                            eventDefinition.StartTime = eventDefinition.Time;
                            if (eventDefinition.TraceSegment > 0 && block.TraceData.Count >= eventDefinition.TraceSegment)
                            {
                                int TraceCount = Math.Min(eventDefinition.TraceSegment - 1, block.TraceData.Count);
                                for (int i = 0; i < TraceCount; i++)
                                {
                                    eventDefinition.StartTime += block.TraceData[i].MaxTime;
                                }
                            }
                            break;

                        case Const.TriggerName_TraceFinish:
                            eventDefinition.StartTime = driveUnit.AllTime;
                            break;
                    }

                    // イベントアクションの仕分け
                    foreach (EventActionDefinition eventAction in eventDefinition.EventActions)
                    {
                        if (!eventAction.IsStartAction && !eventAction.IsEndAction) continue;

                        if (eventAction.IsStartAction) 
                            StartEventList.Add(new Tuple<EventDefinition, EventActionDefinition>(eventDefinition, eventAction));
                        if (eventAction.IsEndAction)
                            EndEventList.Add(new Tuple<EventDefinition, EventActionDefinition>(eventDefinition, eventAction));
                    }
                }

                // イベント時間リストの設定
                foreach(Tuple<EventDefinition, EventActionDefinition> StartEvent in StartEventList)
                {
                    int id = StartEvent.Item2.Id;
                    Tuple<EventDefinition, EventActionDefinition> EndEvent = EndEventList.Find(a => a.Item2.Id == id);

                    EventTimeList.Add(new EventTimeEx()
                    {
                        DriveUnitCount = DriveUnitCount,
                        SampleNumber = StartEvent.Item2.SampleNumber,
                        StartTime = StartEvent.Item1.StartTime,
                        EndTime = EndEvent == null ? 0 : EndEvent.Item1.StartTime,
                        ErrorMessage = EndEvent == null ? Message.Error_NoEndEvent : string.Empty
                    });
                }
                EventTimeList.Sort((a, b) => a.CompareTo(b));

                /*
                foreach (EventDefinition eventDefinition in driveUnit.Events)
                {
                    if (!eventDefinition.IsEmissionSample) continue;

                    if (eventDefinition.TriggerName == Const.TriggerName_Time)
                    {
                        eventDefinition.SampleTime = eventDefinition.Time - beforeTime;
                        beforeTime = eventDefinition.Time;
                    }
                    else if (eventDefinition.TriggerName == Const.TriggerName_TraceFinish)
                    {
                        // トレースファイルの確認
                        eventDefinition.SampleTime = driveUnit.AllTime - beforeTime;
                        //beforeTime = driveUnit.AllTime;
                        beforeTime = 0;
                    }
                    else if (eventDefinition.TriggerName == Const.TriggerName_TraceSegmentStart)
                    {
                        // トレースファイルの確認
                        int traceSegment = eventDefinition.TraceSegment;
                        if(traceSegment > 0 && block.TraceData.Count >= traceSegment)
                        {
                            double traceSegmenttime = eventDefinition.Time;
                            for(int i = 0;i < traceSegment - 1; i++)
                            {
                                traceSegmenttime += block.TraceData[i].MaxTime;
                            }

                            eventDefinition.SampleTime = traceSegmenttime - beforeTime;
                            beforeTime = traceSegmenttime;
                        }
                    }
                }
                */
            }
        }

        /// <summary>
        /// XMLファイルの読み込み
        /// </summary>
        /// <param name="pFilePath">XMLファイルパス</param>
        public void LoadXml(string pFilePath)
        {
            FilePath = pFilePath;
            FileName = Path.GetFileNameWithoutExtension(pFilePath);

            // XMLファイルの読み込み
            XDocument doc = XDocument.Load(pFilePath);
            
            XElement resource = doc.Root;
            if (resource == null)
            {
                throw new InvalidDataException("XMLのルート要素(Resource)が見つかりません。");
            }

            Name = GetElementValue(resource, "VisionStructure", "Administration", "Name");

            // 基本情報の取得
            XElement privateNode = FindElement(resource, "Private");
            if (privateNode == null)
            {
                throw new InvalidDataException("XMLのルート要素(Resource)が見つかりません。");
            }

            // 速度単位
            CycleSpeedUnits = GetElementValue(privateNode, "CycleSpeedUnits");
            // テストモードファイル名
            TestModeFileName = GetElementValue(privateNode, "TestModeFileName");

            // ブロック情報の取得
            XElement blocksNode = FindElement(privateNode, "CycleCycleBlocks");
            if (blocksNode == null) return;

            foreach (XElement block in blocksNode.Descendants().Where(x => x.Name.LocalName == "BaseBlock"))
            {
                BaseBlock baseBlock = CreateBaseBlock(block);
                if (baseBlock == null) continue;

                XElement traceNamesNode = FindElement(block, "TraceNames");
                if (traceNamesNode != null)
                {
                    baseBlock.TraceNames = traceNamesNode.Elements()
                        .Select(x => x.Value)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToList();
                }

                XElement eventsNode = FindElement(block, "Events");
                if (eventsNode != null)
                {
                    foreach (XElement eventNode in eventsNode.Elements().Where(x => x.Name.LocalName == "Event"))
                    {
                        var eventDefinition = new EventDefinition();

                        XElement triggerNode = FindElement(eventNode, "Trigger");
                        if (triggerNode != null)
                        {
                            eventDefinition.TriggerName = GetElementValue(triggerNode, "Name");

                            XElement triggerParametersNode = FindElement(triggerNode, "Parameters");
                            if (triggerParametersNode != null)
                            {
                                eventDefinition.TriggerParameters = ParseParameters(triggerParametersNode);
                            }
                        }

                        XElement eventActionsNode = FindElement(eventNode, "EventActions");
                        if (eventActionsNode != null)
                        {
                            foreach (XElement actionNode in eventActionsNode.Elements().Where(x => x.Name.LocalName == "EventAction"))
                            {
                                var eventAction = new EventActionDefinition
                                {
                                    Id = ToInt(GetElementValue(actionNode, "Id")),
                                    Name = GetElementValue(actionNode, "Name")
                                };

                                XElement actionParametersNode = FindElement(actionNode, "Parameters");
                                if (actionParametersNode != null)
                                {
                                    eventAction.Parameters = ParseParameters(actionParametersNode);
                                }

                                eventDefinition.EventActions.Add(eventAction);
                            }
                        }

                        baseBlock.Events.Add(eventDefinition);
                    }
                }

                CycleBlocks.Add(baseBlock);
            }
        }

        /// <summary>
        /// ブロック要素の取得
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        private BaseBlock CreateBaseBlock(XElement block)
        {
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            string baseBlockType = (string)block.Attribute(xsi + "type") ?? string.Empty;

            switch (baseBlockType)
            {
                case "DriveUnitBlock":
                    DriveUnitBlock driveUnitBlock = new DriveUnitBlock
                    {
                        Name = GetElementValue(block, "Name"),
                        TraceStartMode = GetElementValue(block, "TraceStartMode"),
                        ViolationSpeedTolerance = ToDouble(GetElementValue(block, "ViolationSpeedTolerance")),
                        ViolationTimeTolerance = ToDouble(GetElementValue(block, "ViolationTimeTolerance"))
                    };
                    return driveUnitBlock;

                case "SoakBlock":
                    SoakBlock soakBlock = new SoakBlock
                    {
                        Name = GetElementValue(block, "Name"),
                        MaximumDuration = ToDouble(GetElementValue(block, "MaximumDuration")),
                        MinimumDuration = ToDouble(GetElementValue(block, "MinimumDuration"))
                    };
                    return soakBlock;

                case "IdleCheckBlock":
                    IdleCheckBlock idleCheckBlock = new IdleCheckBlock
                    {
                        Name = GetElementValue(block, "Name"),
                        MeasurementTime = ToDouble(GetElementValue(block, "MeasurementTime"))
                    };
                    return idleCheckBlock;

                default:
                    return null;
            }
        }
    }
    
}
