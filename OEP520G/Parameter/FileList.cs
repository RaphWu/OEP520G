/****************************
 * 檔案名稱表
 ***************************/
using OEP520G.Core;
using OEP520G.Product;

namespace OEP520G.Parameter
{
    /************************
     * 程式會用到的檔案名稱集中在此管理
     ***********************/
    public class FileList : IProductManager
    {
        /********************
         * 資料夾
         *******************/
        // 產品品種資料
        public const string DIRECTORY_PRODUCT = "Product";
        public static string DIRECTORY_ACTIVE_PRODUCT = "Product"; // 注意：此變數會被ProductManager.cs改變

        public const string DIRECTORY_TRAY = "Tray";

        /********************
         * 系統檔案
         *******************/
        // 機台硬體參數
        public const string INI_MACHINE = "Machine.ini";

        // 軟體系統參數
        public const string INI_SYSTEM = "System.ini";

        /********************
         * 產品資料檔案
         *******************/
        // 軟體系統參數
        public const string INI_PRODUCT_INFO = "Product.ini";

        public const string INI_CAMERA = "Camera.ini";
        public const string INI_STAGE = "Stage.ini";
        public const string INI_NOZZLE = "Nozzle.ini";
        public const string INI_DISPENSER = "Dispenser.ini";
        public const string INI_CLAMP = "Clamp.ini";

        // 自動作業
        public const string INI_AUTO_SEQUENCE = "AutoSequence.ini";
        public const string INI_AUTO_OPERATION = "AutoOperation.ini";

        /// <summary>
        /// 建構函式
        /// </summary>
        public FileList()
        {
            // 訂閱事件，並設定呼叫相對應函式
            Common.EA.GetEvent<OnSwitchProduct>().Subscribe(onProductChangeover);
            Common.EA.GetEvent<AfterSwitchProduct>().Subscribe(afterProductChangeover);
        }

        /// <inheritdoc/>
        /// <param name="productId">切換後的品種ID</param>
        public void onProductChangeover(string productId)
        {
            if (productId != "")
                DIRECTORY_ACTIVE_PRODUCT = $"{DIRECTORY_PRODUCT}\\{productId}";
            else
                DIRECTORY_ACTIVE_PRODUCT = DIRECTORY_PRODUCT;
        }

        /// <inheritdoc/>
        /// <param name="productId">切換後的品種ID</param>
        public void afterProductChangeover(string productId)
        {
            // TODO: 品種切換作業
        }
    }
}