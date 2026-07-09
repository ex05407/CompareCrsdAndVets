using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using static CompareCrsdAndVets.Common.Xml;
using static CompareCrsdAndVets.Common.CommonMethods;

namespace CompareCrsdAndVets.Class
{
    /// <summary>
    /// テストプロシージャ情報格納用クラス
    /// </summary>
    internal class TestProcedure
    {
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

        /// <summary>
        /// XMLファイルの読み込み
        /// </summary>
        /// <param name="pFilePath">XMLファイルパス</param>
        public void LoadXml(string pFilePath)
        {
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
