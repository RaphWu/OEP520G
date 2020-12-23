using System;
using System.Collections.Generic;
using System.Text;

namespace OEP520G.Functions
{
    /// <summary>
    /// 飛行基本資料
    /// </summary>
    public class FlyDataStruct
    {
        public string NozzleName { get; set; } // N1~N11吸嘴名稱
        public double X { get; set; }    // 吸嘴座標X
        public double Y { get; set; }    // 吸嘴座標Y
        public double NewX { get; set; } // 重算後時間標記
        public double NewY { get; set; } // 重算後座標標記
        public double TimeMarker { get; set; }

        // 以下為測試資料，正式版本須刪除
        public double NozzleX { get; set; } // 吸嘴中心座標
        public double NozzleY { get; set; }
    }
}
