using System.Collections.ObjectModel;

namespace OEP520G.Core
{
    public static class GlobalString
    {
        // 相機名稱
        public static readonly string FixedCamera = "固定相機";
        public static readonly string MovingCamera = "移動相機";

        /// <summary>
        /// 軸速度名稱
        /// </summary>
        /// <remarks>
        /// 0:最高速 1:高速 2:中速 3:低速 4:最低速
        /// </remarks>
        public readonly static ObservableCollection<string> SpeedName = new ObservableCollection<string>
        {
            "最高速",
            "高速",
            "中速",
            "低速",
            "最低速"
        };
    }
}