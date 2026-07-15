using System.Collections.Generic;
using System.Linq;
using static CompareCrsdAndVets.Common.CommonMethods;

namespace CompareCrsdAndVets.Class
{
    /// <summary>
    /// イベントアクション定義
    /// </summary>
    internal class EventActionDefinition
    {
        /// <summary>イベントアクションID</summary>
        public int Id { get; set; }

        /// <summary>
        /// イベントアクション名</summary>
        public string Name { get; set; }

        /// <summary>開始アクションか否か</summary>
        public bool IsStartAction { get => Name == "EmissionSample"; }

        /// <summary>終了アクションか否か</summary>
        public bool IsEndAction { get => Name == "" && StartActionId != -1; }

        /// <summary>エラーメッセージ</summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// パラメータリスト
        /// </summary>
        public List<ParameterDefinition> Parameters { private get; set; } = new List<ParameterDefinition>();

        /// <summary>サンプル番号</summary>
        private int? _sampleNumber;
        /// <summary>サンプル番号</summary>
        public int SampleNumber
        {
            get
            {
                if (_sampleNumber == null)
                {
                    _sampleNumber = ToInt(GetParameterValue("SampleNumber"), 1);
                }

                return _sampleNumber.Value;
            }
        }

        /// <summary>バッグペア番号</summary>
        private int? _bagSampleNumber;
        /// <summary>バッグペア番号</summary>
        public int BagSampleNumber
        {
            get
            {
                if (_bagSampleNumber == null)
                {
                    _bagSampleNumber = ToInt(GetParameterValue("BagSampleNumber"), 1);
                }

                return _bagSampleNumber.Value;
            }
        }

        /// <summary>PMフィルタ番号</summary>
        private int? _pmSampleNumber;
        /// <summary>PMフィルタ番号</summary>
        public int PmSampleNumber
        {
            get
            {
                if (_pmSampleNumber == null)
                {
                    _pmSampleNumber = ToInt(GetParameterValue("PmSampleNumber"), 1);
                }

                return _pmSampleNumber.Value;
            }
        }

        /// <summary>理論走行距離</summary>
        private double? _nominalDistance;
        /// <summary>理論走行距離</summary>
        public double NominalDistance
        {
            get
            {
                if (_nominalDistance == null)
                {
                    _nominalDistance = ToDouble(GetParameterValue("NominalDistance"), 0);
                }

                return _nominalDistance.Value;
            }
        }

        /// <summary>開始アクションID</summary>
        private int? _startActionId;
        public int StartActionId
        {
            get
            {
                if (_startActionId == null)
                {
                    _startActionId = ToInt(GetParameterValue("StartActionId"), -1);
                }

                return _startActionId.Value;
            }
        }

        /// <summary>
        /// 指定されたパラメータIDに対応するパラメータの値を取得
        /// </summary>
        /// <param name="pParam">パラメータID</param>
        /// <returns>パラメータの値</returns>
        private string GetParameterValue(string pParameterId)
        {
            if (Parameters.Exists(x => x.ParameterId == pParameterId))
            {
                return Parameters.Where(x => x.ParameterId == pParameterId).First().Value;
            }
            else
            {
                return string.Empty;
            }
        }

    }
}
