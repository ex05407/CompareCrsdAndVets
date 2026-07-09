using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareCrsdAndVets.Class
{
    /// <summary>
    /// ソークブロック
    /// </summary>
    internal class SoakBlock : BaseBlock
    {
        /// <summary>最大時間</summary>
        public double MaximumDuration { get; set; }

        /// <summary>最小時間</summary>
        public double MinimumDuration { get; set; }
    }
}
