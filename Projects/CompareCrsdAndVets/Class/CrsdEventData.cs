using CompareCrsdAndVets.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CompareCrsdAndVets.Common.Const;

namespace CompareCrsdAndVets.Class
{
    internal class CrsdEventData : CrsdBase
    {
        public string BlockType { get; set; } = string.Empty;
        public BlockType BlockTypeEnum
        {
            get
            {
                switch(BlockType.ToUpper())
                {
                    case "NORMAL":
                        return Const.BlockType.Normal;
                    case "SOAK":
                        return Const.BlockType.Soak;
                    case "WARMUP":
                        return Const.BlockType.Warmup;
                    case "IDLECHECK":
                        return Const.BlockType.IdleCheck;
                    case "COASTDOWN":
                        return Const.BlockType.CoastDown;
                    default:
                        return Const.BlockType.Unknown;
                }
            }
        }
        public List<string> EventDataList { get; set; } = new List<string>();

        public string TraceStartMode
        {
            get
            {
                if (EventDataList.Contains(WST) && EventDataList.Contains(WCO)) return PendantRun;
                else return PendantStart;
            }
        }

        public string TraceStartModeError
        {
            get
            {
                if (!EventDataList.Contains(WST) && !EventDataList.Contains(WCO))
                    return "イベントファイルに、WST,WCOがありません。";
                if (!EventDataList.Contains(WST) && EventDataList.Contains(WCO))
                    return "イベントファイルに、WCOがありません。";
                return string.Empty;
            }
        }

        public CrsdEventData(string pBlockType)
        {
            BlockType = pBlockType;
        }

        public CrsdEventData(string pEventFilePath, string pBlockType)
        {
            BlockType = pBlockType;
            var valuesBySection = ParseIniLikeFile(pEventFilePath, true);

            Dictionary<string, string> eventlist;
            if (valuesBySection.TryGetValue("Event", out eventlist))
            {
                foreach (var kvp in eventlist)
                {
                    EventDataList.Add(kvp.Value);
                }
            }
        }
    }
}
