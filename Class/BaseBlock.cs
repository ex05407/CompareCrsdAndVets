using System.Collections.Generic;

namespace CompareCrsdAndVets.Class
{
    /// <summary>
    /// ベースブロック
    /// </summary>
    abstract class BaseBlock
    {
        public string Name { get; set; }
        public List<string> TraceNames { get; set; } = new List<string>();
        public List<EventDefinition> Events { get; set; } = new List<EventDefinition>();
    }
}
