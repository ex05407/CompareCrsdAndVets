using System.Collections.Generic;
using System.Linq;
using static CompareCrsdAndVets.Common.CommonMethods;
using static CompareCrsdAndVets.Common.Const;

namespace CompareCrsdAndVets.Class
{
    /// <summary>
    /// イベント定義
    /// </summary>
    internal class EventDefinition
    {
        /// <summary>トリガー</summary>
        public string TriggerName { get; set; }

        /// <summary>トリガーパラメータ</summary>
        public List<ParameterDefinition> TriggerParameters { get; set; } = new List<ParameterDefinition>();
        /// <summary>イベントアクション</summary>
        public List<EventActionDefinition> EventActions { get; set; } = new List<EventActionDefinition>();

        /// <summary>PendantButton</summary>
        private string _pendantButton;
        /// <summary>PendantButton</summary>
        public string PendantButton
        {
            get
            {
                if (_pendantButton == null)
                {
                    _pendantButton = GetParameterValue("PendantButton");
                }

                return _pendantButton;
            }
        }

        /// <summary>時間(s)</summary>
        private double? _time = null;
        /// <summary>時間(s)</summary>
        public double Time
        {
            get
            {
                _time = ToDouble(GetParameterValue("Time"), 0);
                return _time.Value;
            }
        }

        /// <summary>トリガーが時間系かどうか</summary>
        public bool IsTimeTrigger { get => (TriggerName == TriggerName_Time || TriggerName == TriggerName_TraceSegmentStart || TriggerName == TriggerName_TraceFinish); }

        /// <summary>
        /// 指定されたパラメータIDに対応するパラメータの値を取得
        /// </summary>
        /// <param name="pParam">パラメータID</param>
        /// <returns>パラメータの値</returns>
        private string GetParameterValue(string pParameterId)
        {
            if (TriggerParameters.Exists(x => x.ParameterId == pParameterId))
            {
                return TriggerParameters.Where(x => x.ParameterId == pParameterId).First().Value;
            }
            else
            {
                return string.Empty;
            }
        }

    }
}
