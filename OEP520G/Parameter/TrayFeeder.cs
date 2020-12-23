using System;
using System.Collections.Generic;
using System.Text;

namespace OEP520G.Parameter
{
    /// <summary>
    /// Tray描述
    /// </summary>
    public class TrayFeeder
    {
        public const int MAX_TRAY_FEEDER = 30;

        public int FeederId { get; set; } // 編碼(1~30)
        public bool Effective { get; set; } // 此Feeder是否有效
        public string Part { get; set; } // 零件編號
        public bool PartEnable { get; set; } // 零件編號啟用
        public string ImageBeforePickup { get; set; } // 吸著前對位
        public bool ImageBeforePickupEnable { get; set; } // 吸著前對位啟用
        public string ImageBeforeCarry { get; set; } // 搭載前對位
        public bool ImageBeforeCarryEnable { get; set; } // 搭載前對位啟用

        /// <summary>
        /// 返回新的空白Feeder資料
        /// </summary>
        /// <param name="feederNo"></param>
        /// <returns></returns>
        public static TrayFeeder GetEmptyFeeder(int feederNo)
        {
            return new TrayFeeder()
            {
                FeederId = feederNo,
                Effective = false,
                Part = "",
                PartEnable = false,
                ImageBeforePickup = "",
                ImageBeforePickupEnable = false,
                ImageBeforeCarry = "",
                ImageBeforeCarryEnable = false
            };
        }

    }
}
