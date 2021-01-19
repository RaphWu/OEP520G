using System;
using System.Collections.Generic;
using System.Text;

namespace Imageproject.Constants
{
    /// <summary>
    /// 畫像參數類別
    /// </summary>
    public enum ShapeType
    {
        /// <summary>
        /// 異形
        /// </summary>
        ShieldingCase,

        /// <summary>
        /// 白件缺角
        /// </summary>
        CrackWhite,

        /// <summary>
        /// 黑件缺角
        /// </summary>
        CrackBlack,

        /// <summary>
        /// 圖片模板比對 Picture Template 'N' Matching
        /// </summary>
        PTNM,

        /// <summary>
        /// 基準點 Fiducial
        /// </summary>
        FID,

        /// <summary>
        /// 特徵/外觀/方向
        /// </summary>
        Aspect,

        /// <summary>
        /// 極性
        /// </summary>
        Polarity,

        /// <summary>
        /// 膠量檢查
        /// </summary>
        Glue
    }
}
