using OEP520G.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace OEP520G.Parameter
{
    public class NozzleObject
    {
        // 基本資料
        public string Name { get; set; }

        /// <summary>
        /// 吸嘴校正座標(座標值)
        /// </summary>
        public PointXYZ Position { get; set; }
        public LongPointXYZ Pulse { get; set; }
        public IntPointXYZ Encoder { get; set; }

        // 各吸嘴與移動相機距離
        public PointXY DistanceToMoveCamera { get; set; }

        // Encoder飛行補正
        public IntPointXY UltraHighEncMarker { get; set; }
        public IntPointXY HighEncMarker { get; set; }
        public IntPointXY MiddleEncMarker { get; set; }

        // Time飛行補正
        public PointXY UltraHighTimeMarker { get; set; }
        public PointXY HighTimeMarker { get; set; }
        public PointXY MiddleTimeMarker { get; set; }

        // 測高
        public double MeasureHeight { get; set; }

        public NozzleObject()
        {
            Position = new PointXYZ();
            Pulse = new LongPointXYZ();
            DistanceToMoveCamera = new PointXY();
            UltraHighEncMarker = new IntPointXY();
            HighEncMarker = new IntPointXY();
            MiddleEncMarker = new IntPointXY();
            UltraHighTimeMarker = new PointXY();
            HighTimeMarker = new PointXY();
            MiddleTimeMarker = new PointXY();
        }
    }
}