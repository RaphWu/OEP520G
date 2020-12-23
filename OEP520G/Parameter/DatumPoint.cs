using OEP520G.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace OEP520G.Parameter
{
    /// <summary>
    /// 基準點資料結構
    /// </summary>
    public class DatumPoint
    {
        /**********
         * 基準座標為校正時，所取得的絕對座標
         * 偏移座標即與基準座標偏移量，可人工修改，在校正座標值寫入DB後自動歸零
         * 在校正畫面，XY顯示位移值，Z顯示絕對值
         **********/
        public PointXYZ Position { get; set; } // 基準座標
        public int ImageId { get; set; } // 畫像ID

        // 與固定相機的距離
        public PointXY DistanceToFixCamera { get; set; }

        public int Frequency { get; set; }    // 確認頻率
        public double Tolerance { get; set; } // 容許誤差

        public DatumPoint()
        {
            Position = new PointXYZ();
            DistanceToFixCamera = new PointXY();
        }
    }
}
