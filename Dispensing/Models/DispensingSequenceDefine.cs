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
    /// 點膠動作順序
    /// </summary>
    /// <remarks>注意：此資料庫請保持依SeqNo排序狀態</remarks>
    [Table(DB.TABLE_NAME_DISPENSE_SEQUENCE)]
    public class DispensingSequenceDefine
    {
        /// <summary>
        /// 動作編號
        /// </summary>
        [ExplicitKey]
        public int SeqNo { get; set; }

        /// <summary>
        /// 關聯的點膠品種ID
        /// </summary>
        public int ShapeId { get; set; }

        /// <summary>
        /// 動作類別
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 參數群組編號
        /// </summary>
        public int GroupNo { get; set; }

        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public double OffsetZ { get; set; }
        public double OffsetR { get; set; }
    }
}
