using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace OEP520G.Automatic
{
    public enum EAction
    {
        PlacePart = 0,        // 置件
        PickUp01 = 1,         // 取料1
        PickUp02 = 2,         // 取料2
        PickUp03 = 3,         // 取料3
        PickUp04 = 4,         // 取料4
        PickUp05 = 5,         // 取料5
        PickUp06 = 6,         // 取料6
        PickUp07 = 7,         // 取料7
        PickUp08 = 8,         // 取料8
        PickUp09 = 9,         // 取料9
        PickUp10 = 10,        // 取料10
        PickUp11 = 11,        // 取料11
        ImageCheck = 30,      // 畫像檢查
        Dispensing = 40,      // 點膠
        GlueAmountCheck = 41, // 膠量檢查
        PreDispensing = 42,   // 預吐膠
        ClearGlue = 43        // 清膠
    };
}
