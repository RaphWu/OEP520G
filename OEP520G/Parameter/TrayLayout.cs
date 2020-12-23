using System;
using System.Collections.Generic;
using System.Text;

namespace OEP520G.Parameter
{
    /// <summary>
    /// 各組Tray排列定義
    /// </summary>
    public class TrayLayout
    {
        // 排列別名
        public string Name { get; set; }

        /// <summary>
        /// 有效的
        /// </summary>
        public bool Effective { get; set; }

        /// <summary>
        /// 總排數
        /// </summary>
        public int TotalLines { get; set; }

        /// <summary>
        /// 每排點位數
        /// </summary>
        public int PointsInLine { get; set; }

        // 校正時，第1點絕對軸座標
        public double OriginX { get; set; }
        public double OriginY { get; set; }

        // 校正時，對角線點(非最後點)絕對軸座標
        public double DiagonalX { get; set; }
        public double DiagonalY { get; set; }

        // 每一點間距(程式計算)
        public double GapHorizontal { get; set; } // X間距
        public double GapVertical { get; set; } // Y間距
    }
}
