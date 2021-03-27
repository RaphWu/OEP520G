using OEP520G.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OEP520G.Dispensing.Contracts
{
    public interface IDispensingService_Shape
    {
        /********************
         * CRUD
         ********************/
        /// <summary>
        /// 建立新Dispensing Shape資料
        /// </summary>
        /// <param name="crudInfo">CrudInfo物件</param>
        /// <returns>新Shape的ID</returns>
        int CreateNewShape(CrudInfo crudInfo);

        /********************
         * Tabel
         ********************/
        /// <summary>
        /// 檢查Dispensing Shape Id是否已存在資料庫
        /// </summary>
        /// <param name="dispId">Dispensing Id</param>
        /// <returns>true: Dispensing Shape Id已存在<br/>false: Dispensing Shape Id不存在</returns>
        bool IsShapeExist(int dispId);

        /// <summary>
        /// 檢查檢查Dispensing Shape Name是否已存在資料庫
        /// </summary>
        /// <param name="dispName">檢查Dispensing Shape Name</param>
        /// <returns>true: 檢查Dispensing Shape Name已存在<br/>false: 檢查Dispensing Shape Name不存在</returns>
        bool IsShapeExist(string dispName);
    }
}
