using Imageproject.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Imageproject.Services
{
    public class ShapeManager : IShapeManager
    {
        // ctor
        public ShapeManager()
        {
        }

        /********************
		 * Database
		 ********************/
        /// <inheritdoc/>
        public bool WriteToDb()
        {
            //using var conn = _sqlite.OpenConnection(_pm.DB_NAME_PRODUCT);
            //if (conn == null)
            //	return false;

            //using var tran = conn.BeginTransaction();

            //conn.UpdateAsync(NozzleParameters.NozzleList);

            //string sql = $"UPDATE {DB.TABLE_NAME_NOZZLE} SET DatumNozzleId = {NozzleParameters.DatumNozzleId}";
            //conn.ExecuteAsync(sql);

            //tran.Commit();

            /***** for TCP/IP *****/

            /***** for TCP/IP *****/

            return true;
        }

        /// <inheritdoc/>
        public bool ReadFromDb()
        {
            //using var conn = _sqlite.OpenConnection(_pm.DB_NAME_PRODUCT);
            //if (conn == null)
            //	return false;

            //using var tran = conn.BeginTransaction();

            //NozzleParameters.NozzleList = conn.GetAll<NozzlesDefine>().ToList();

            //string sql = $"SELECT DatumNozzleId FROM {DB.TABLE_NAME_NOZZLE}";
            //NozzleParameters.DatumNozzleId = conn.QueryFirst(sql);

            //tran.Commit();

            /***** for TCP/IP *****/

            /***** for TCP/IP *****/

            return true;
        }


    }
}
