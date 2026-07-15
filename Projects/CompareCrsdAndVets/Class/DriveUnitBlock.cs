namespace CompareCrsdAndVets.Class
{
    /// <summary>
    /// ドライブユニットブロック
    /// </summary>
    internal class DriveUnitBlock : BaseBlock
    {
        /// <summary>速度許容差</summary>
        public double ViolationSpeedTolerance { get; set; }
        /// <summary>時間許容差</summary>
        public double ViolationTimeTolerance { get; set; }
        /// <summary>トレースの開始</summary>
        private string _TraceStartMode;
        /// <summary>トレースの開始</summary>
        public string TraceStartMode
        {
            get
            {
                if (_TraceStartMode == "PendantRun") return "ペンダントRun";
                else if (_TraceStartMode == "PendantStart") return "ペンダントStart";
                else return string.Empty;
            }
            set { _TraceStartMode = value; }
        }
    }
}
