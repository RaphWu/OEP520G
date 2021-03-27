using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OEP520G.Dispensing.Constants
{
    public enum ActionType
    {
        /// <summary>
        /// 無
        /// <br/>此動作Disable
        /// </summary>
        None,

        /// <summary>
        /// 移動
        /// <br/>純粹移動，不吐膠
        /// </summary>
        Move,

        /// <summary>
        /// 點
        /// </summary>
        Dot,

        /// <summary>
        /// 畫線
        /// </summary>
        Line,

        /// <summary>
        /// 畫弧
        /// </summary>
        Arc,

        /// <summary>
        /// 中點
        /// </summary>
        Midpoint,

        /// <summary>
        /// 旋轉
        /// <br/>點膠座旋轉
        /// </summary>
        Rotate,

        /// <summary>
        /// 旋轉中心
        /// </summary>
        RtCntr,

        /// <summary>
        /// 畫像結果
        /// </summary>
        RtImg
    }
}
