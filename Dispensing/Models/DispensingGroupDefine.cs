using Dapper.Contrib.Extensions;
using OEP520G.Dispensing.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OEP520G.Dispensing.Models
{
    /// <summary>
    /// 點膠參數群組
    /// </summary>
    [Table(DB.TABLE_NAME_DISPENSE_GROUP)]
    public class DispensingGroupDefine
    {
        /// <summary>
        /// 群組編號
        /// </summary>
        [ExplicitKey]
        public int GroupNo { get; set; }

        /// <summary>
        /// 關聯的點膠品種ID
        /// </summary>
        public int ShapeId { get; set; }

        // 以下參數參考說明Note
        public double DspSpeed { get; set; }
        public double SpeedR { get; set; }
        public int SWait { get; set; }
        public int EShot { get; set; }
        public double PreStop { get; set; }
        public int EWait { get; set; }
        public double UpXY { get; set; }
        public double UpZ { get; set; }
        public double UpSpeed { get; set; }
        public int UpDelay { get; set; }
        public int UpWay { get; set; }
    }
}
