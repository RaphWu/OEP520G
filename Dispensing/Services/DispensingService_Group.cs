using Dapper;
using Dapper.Contrib.Extensions;
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
    public partial class DispensingService : IDispensingService_Group
    {
        /********************
         * Database
         ********************/
        /// <inheritdoc/>
        private bool WriteGroupToDb(SQLiteConnection conn)
        {
            try
            {
                conn.UpdateAsync(DispensingParameters.Group);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        private bool ReadGroupFromDb(SQLiteConnection conn)
        {
            try
            {
                if (_sqlite.IsTableExist(conn, DB.TABLE_NAME_DISPENSE_GROUP))
                {
                    string sql = $"SELECT * FROM {DB.TABLE_NAME_DISPENSE_GROUP} ORDER BY ShapeId, GroupNo;";
                    DispensingParameters.Group = conn.Query<DispensingGroupDefine>(sql).ToList();
                }
                else
                {
                    _sqlite.CreateTable(conn, DB.CREATE_TABLE_DISPENSE_GROUP);
                    DispensingParameters.Group = new List<DispensingGroupDefine>();
                }
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }
    }
}
