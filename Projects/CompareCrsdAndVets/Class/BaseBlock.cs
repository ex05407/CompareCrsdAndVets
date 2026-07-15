using System.Collections.Generic;
using System.Linq;

namespace CompareCrsdAndVets.Class
{
    /// <summary>
    /// ベースブロック
    /// </summary>
    abstract class BaseBlock
    {
        /// <summary>名前</summary>
        public string Name { get; set; }
        /// <summary>トレース名リスト</summary>
        public List<string> TraceNames { get; set; } = new List<string>();
        /// <summary>トレース情報リスト</summary>
        public List<Trace> TraceData { get; set; } = new List<Trace>();
        /// <summary>紐付くトレースファイルの合計時間</summary>
        public double AllTime { get => TraceData.Sum(a => a.MaxTime); }
        /// <summary>イベント定義のリスト</summary>
        public List<EventDefinition> Events { get; set; } = new List<EventDefinition>();
    }
}
