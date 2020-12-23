using System;
using System.Collections.Generic;
using System.Text;

namespace OEP520G.Parameter
{
    /// <summary>
    /// Tray主資料
    /// </summary>
    public class TrayData
    {
        /// <summary>
        /// Tray名稱
        /// </summary>
        /// <remarks>此名稱必須為唯一值，以做為關聯KEY</remarks>
        public string Name { get; set; }

        /// <summary>
        /// 註解
        /// </summary>
        public string Memo { get; set; }

        /// <summary>
        /// 總點位數 (不含Mask)
        /// </summary>
        public int TotalPoints { get; set; }

        /// <summary>
        /// 排列方向
        /// </summary>
        public EDirection Direction { get; set; }

        /// <summary>
        /// 排列方式
        /// </summary>
        public EArrangement Arrangement { get; set; }

        // 基準點 = 與第一基準點相對座標
        public double DatumX { get; set; }
        public double DatumY { get; set; }

        // 整體偏移
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }

        /// <summary>
        /// 目前點位
        /// </summary>
        /// <remarks>可計算NowX/Y,NextX/Y</remarks>
        public int CurrentPoint { get; set; }

        /// <summary>
        /// 交替Tray
        /// </summary>
        public int NextTray { get; set; }

        /// <summary>
        /// 各組Tray排列定義
        /// </summary>
        public List<TrayLayout> Layout { get; set; }

        /// <summary>
        /// 全部排列完成的點位
        /// </summary>
        public List<TrayPointMatrix> PointMatrix { get; set; }

        /// <summary>
        /// 無效點位
        /// </summary>
        /// <remarks>PointMatrix.PointNo</remarks>
        public List<int> Mask { get; set; }
    }
}
