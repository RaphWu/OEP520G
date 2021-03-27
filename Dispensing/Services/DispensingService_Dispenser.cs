using Dapper.Contrib.Extensions;
using OEP520G.Core.Helpers;
using OEP520G.Dispensing.Constants;
using OEP520G.Dispensing.Contracts;
using OEP520G.Dispensing.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OEP520G.Dispensing.Services
{
    public partial class DispensingService : IDispensingService_Dispenser
    {
        /********************
         * Database
         ********************/
        /// <summary>
        /// 將點膠機參數寫入資料庫
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <returns>寫入是否成功</returns>
        private bool WriteDispenserToDb(SQLiteConnection conn)
        {
            try
            {
                conn.UpdateAsync(DispensingParameters.Dispenser);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 從資料庫讀出點膠機參數
        /// </summary>
        /// <param name="conn">資料庫連線</param>
        /// <returns>讀出是否成功</returns>
        private bool ReadDispenserFromDb(SQLiteConnection conn)
        {
            try
            {
                if (_sqlite.IsTableExist(conn, DB.TABLE_NAME_DISPENSER))
                {
                    // 目前只有1台點膠機，ID固定為1
                    DispensingParameters.Dispenser = conn.Get<DispenserDefine>(1);
                }
                else
                {
                    _sqlite.CreateTable(conn, DB.CREATE_TABLE_DISPENSER);
                    DispensingParameters.Dispenser = new DispenserDefine { Id = 1 };
                    conn.InsertAsync(new DispenserDefine { Id = 1, UvPosition = UVPosition.Stage });
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
