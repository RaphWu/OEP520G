using OEP520G.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace OEP520G.Parameter
{
    public class ClampObject
    {
        // 基本資料
        public EClampId Id { get; set; }
        public string Title { get; set; }

        // 與移動相機換算
        public PointXY ConvertToMoveCamera { get; set; }

        // Stage取放料點
        public PointXY StageCoordination { get; set; }

        // 延遲時間設定
        public int DelayTime1 { get; set; }
        public int DelayTime2 { get; set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public ClampObject(EClampId id, string title)
        {
            Id = id;
            Title = title;
            ConvertToMoveCamera = new PointXY();
            StageCoordination = new PointXY();
        }
    }
}
