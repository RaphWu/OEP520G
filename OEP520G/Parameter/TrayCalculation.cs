using System;
using System.Collections.Generic;
using System.Text;

namespace OEP520G.Parameter
{
    public class TrayCalculation
    {
        /// <summary>
        /// 此Tray是原List第幾Tray
        /// </summary>
        public int SelectedIndex { get; set; }

        /// <summary>
        /// 總排數
        /// </summary>
        public int TotalLines { get; set; }

        /// <summary>
        /// 每排點位數
        /// </summary>
        public int PointsInLine { get; set; }

        // 第1點軸座標
        public double OriginX { get; set; }
        public double OriginY { get; set; }

        /// <summary>
        /// 與第一點位偏移值X
        /// </summary>
        public double[,] OffsetX { get; set; }

        /// <summary>
        /// 與第一點位偏移值Y
        /// </summary>
        public double[,] OffsetY { get; set; }
    }
}
