using Dapper;
using Dapper.Contrib.Extensions;
using OEP520G.Core.Helpers;
using OEP520G.Database.Models;
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
    public partial class DispensingService : IDispensingService_Shape
    {
        /********************
         * Database
         ********************/
        /// <inheritdoc/>
        private bool WriteShapeToDb(SQLiteConnection conn)
        {
            try
            {
                conn.UpdateAsync(DispensingParameters.Shape);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        private bool ReadShapeFromDb(SQLiteConnection conn)
        {
            try
            {
                if (_sqlite.IsTableExist(conn, DB.TABLE_NAME_DISPENSE_SHAPE))
                {
                    DispensingParameters.Shape = conn.GetAll<DispensingShapeDefine>().ToList();
                }
                else
                {
                    _sqlite.CreateTable(conn, DB.CREATE_TABLE_DISPENSE_SHAPE);
                    DispensingParameters.Shape = new List<DispensingShapeDefine>();
                }
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        /********************
         * CRUD
         ********************/
        /// <inheritdoc/>
        public int CreateNewShape(CrudInfo crudInfo)
        {
            if (!_pm.IsProductActive)
                return -1;

            try
            {
                using var conn = _sqlite.OpenConnection(_pm.DB_NAME_PRODUCT);
                if (conn == null)
                    return -1;

                using var tran = conn.BeginTransaction();
                DispensingParameters.Shape.Add(new DispensingShapeDefine { Title = crudInfo.NewName });
                conn.Insert(new DispensingShapeDefine { Title = crudInfo.NewName });
                tran.Commit();

                string msg = LocalizationProvider.GetValue<string>("Msg_WritingToDatabaseHasBeenCompleted");
                _statusBar.SystemMessage(msg);
                return DispensingParameters.Shape.FindIndex(x => x.Title == crudInfo.NewName);
            }
            catch
            {
                return -1;
            }
        }

        /********************
         * Tabel
         ********************/
        /// <inheritdoc/>
        public bool IsShapeExist(int dispId)
            => DispensingParameters.Shape.FindIndex(x => x.Id == dispId) >= 0;

        /// <inheritdoc/>
        public bool IsShapeExist(string dispName)
            => DispensingParameters.Shape.FindIndex(x => x.Title == dispName) >= 0;
    }
}
