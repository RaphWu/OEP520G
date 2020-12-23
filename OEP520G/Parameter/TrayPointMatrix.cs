using System;
using System.Collections.Generic;
using System.Text;

namespace OEP520G.Parameter
{
    /// <summary>
    /// 排列矩陣(PointMatrix)
    /// </summary>
    public class TrayPointMatrix
    {
        /// <summary>
        /// 點位序號列表
        /// </summary>
        public int PointNo { get; set; }

        /// <summary>
        /// 點位實際軸座標X
        /// </summary>
        public double PointMatrixX { get; set; }

        /// <summary>
        /// 點位實際軸座標Y
        /// </summary>
        public double PointMatrixY { get; set; }
    }
}
