using OEP520G.Dispensing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OEP520G.Dispensing.Contracts
{
    public interface IDispensing : IDispensingService_Dispenser,
                                   IDispensingService_Shape,
                                   IDispensingService_Sequence
    {
        DispenserDefine Dispenser { get; }
        List<DispensingShapeDefine> Shape { get; }
        List<DispensingSequenceDefine> Sequence { get; }
        List<DispensingGroupDefine> Group { get; }

        /********************
         * Database
         ********************/
        /// <summary>
        /// 將點膠參數存入資料庫
        /// </summary>
        /// <returns>寫入是否成功</returns>
        bool WriteToDb();

        /// <summary>
        /// 從資料庫讀取點膠參數
        /// </summary>
        /// <returns>讀取是否完成</returns>
        bool ReadFromDb();
    }
}
