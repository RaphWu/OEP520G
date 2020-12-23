using System.Collections.Generic;

namespace OEP520G.Parameter
{
    public class Device
    {
        public List<string> CoordinateModeList { get; set; } = new List<string> { "絕對座標", "相對座標" };

        // Limit Switch
        public List<string> LimitSwitchlList = new List<string> { "常開 NO", "常閉 NC", "不使用" };

        /********************
         * 伺服馬達參數用
         * *****************/
        // 命令模式
        public List<string> CommandModeList = new List<string> { "脈衝命令(Pulse Command)", "速度命令(Velocity Command)" };

        // Pulse Mode 脈衝輸出格式
        public List<string> PulseModeList = new List<string> { "Pulse/Direction", "CW/CCW", "A/B phase" };

        // Encoder Type 編碼器種類
        public List<string> EncoderTypeList = new List<string> { "A/B Phase", "CW/CCW", "Pulse/Direction" };

        // 加減速型式
        public List<string> CurveTypeList = new List<string> { "梯形加速曲線", "S形加速曲線" };

        // 加減速模式
        public List<string> AccDecModeList = new List<string> { "後加減速模式", "前加減速模式" };

        // In Position 定位控制模式
        public List<string> InPositionModeList = new List<string> { "IPM_SETTLE_BLOCK", "IPM_ONETIME_UNBLOCK", "IPM_SETTLE_BLOCK", "IPM_SETTLE_UNBLOCK" };

        // In Position 定位控制模式
        public List<ushort> InputRateList = new List<ushort> { 1, 2, 4 };

        // 復歸模式
        public List<ushort> GoHomeModeList = new List<ushort> { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };

        // 復歸方向
        public List<string> GoHomeDirectList = new List<string> { "正方向", "負方向" };

        /********************
         * 共用
         * *****************/
        public List<string> EnableDisableList = new List<string> { "開啟", "關閉" };
        public List<string> DirectionList = new List<string> { "不反相", "反相" };
        public List<string> ExchangeList = new List<string> { "不交換", "交換" };
    }
}
