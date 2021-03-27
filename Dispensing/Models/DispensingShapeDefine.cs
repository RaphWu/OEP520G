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
    /// 點膠品種
    /// </summary>
    [Table(DB.TABLE_NAME_DISPENSE_SHAPE)]
    public class DispensingShapeDefine
    {
        /// <summary>
        /// 品種ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 品種標題
        /// </summary>
        public string Title { get; set; }
    }
}
