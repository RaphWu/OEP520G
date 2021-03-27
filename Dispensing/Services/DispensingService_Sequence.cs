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
    public partial class DispensingService : IDispensingService_Sequence
    {
        /********************
         * Database
         ********************/
        /// <inheritdoc/>
        private bool WriteSequenceToDb(SQLiteConnection conn)
        {
            try
            {
                conn.UpdateAsync(DispensingParameters.Sequence);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        private bool ReadSequenceFromDb(SQLiteConnection conn)
        {
            try
            {
                if (_sqlite.IsTableExist(conn, DB.TABLE_NAME_DISPENSE_SEQUENCE))
                {
                    string sql = $"SELECT * FROM {DB.TABLE_NAME_DISPENSE_SEQUENCE} ORDER BY ShapeId, SeqNo;";
                    DispensingParameters.Sequence = conn.Query<DispensingSequenceDefine>(sql).ToList();
                }
                else
                {
                    _sqlite.CreateTable(conn, DB.CREATE_TABLE_DISPENSE_SEQUENCE);
                    DispensingParameters.Sequence = new List<DispensingSequenceDefine>();
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
