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
    /// 點膠基本資料
    /// </summary>
    [Table(DB.TABLE_NAME_DISPENSER)]
    public class DispenserDefine
    {
        /// <summary>
        /// 點膠品種ID
        /// </summary>
        [ExplicitKey]
        public int Id { get; set; }

        /// <summary>
        /// 校正座標X
        /// </summary>
        public double DatumX { get; set; }

        /// <summary>
        /// 校正座標Y
        /// </summary>
        public double DatumY { get; set; }

        /// <summary>
        /// 校正座標Z
        /// </summary>
        public double DatumZ { get; set; }

        /// <summary>
        /// 點膠位置
        /// </summary>
        public int UvPosition { get; set; }

        /// <summary>
        /// UV照射時間
        /// </summary>
        public double UvTime { get; set; }

        /// <summary>
        /// UV前等待時間
        /// </summary>
        public double UvPreWait { get; set; }

        /// <summary>
        /// UV後等待時間
        /// </summary>
        public double UvPostWait { get; set; }
    }
}
