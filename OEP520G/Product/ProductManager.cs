/****************************
 * 《品種管理員》
 *   Module    -> ProductManager (ProductManager.cs)
 *   ViewModel -> ProductViewModel (ProductViewModel.cs)
 *   View      -> ProductView (Product.xaml)
 * 
 * ========== ProductManager部分 ==========
 * 
 * 重點:
 *   - 保持以下屬性隨時為有效值
 *     - string ActiveProductId
 *     - bool IsProductIdActived
 *     - List<ProductData> ProductList
 *    
 * 建構:
 *   - 建立ProductList
 *   - new管理的物件，並將this傳給所有被管理物件(中介者模式)
 *   
 * ========== 各物件共同部分 ==========
 * 
 * 品種資料夾結構：
 *    [程式所在資料夾]
 *        ├─ Product(品種資料夾) ─┬─ [品種ID] ─┬─ Tray(托盤資料夾) ─┬─ ［托盤ID.ini]
 *        ├─ Machine.ini        │           ├─ Product.ini     ├─ ［托盤ID.ini]
 *        ├─ System.ini         │           │
 *        └─ Epcio.ini          │           ├─ Tray(托盤資料夾) ─┬─ ［托盤ID.ini]
 *                              │           ├─ Product.ini     ├─ ［托盤ID.ini]
 *                              │
 *                              ├─ [品種ID] ─┬─ Tray(托盤資料夾) ─┬─ ［托盤ID.ini]
 *                              │           ├─ Product.ini     ├─ ［托盤ID.ini]
 *                              │           │
 *                              │           ├─ Tray(托盤資料夾) ─┬─ ［托盤ID.ini]
 *                              │           ├─ Product.ini     ├─ ［托盤ID.ini]
 *
 * 開放的屬性及方法(繼承IProductManager介面):
 *   static string ActiveProductId  => 取得目前有效旳品種ID
 *   static bool IsProductIdActived => 品種ID是否有效
 *   bool IsProductExist(string id) => 檢查品種ID是否已存在
 *   ProductManager.tray            => 取用托盤處理物件
 * 
 * 事件聚合器:
 *   - ProductSwitch(string 品種ID) => Product.xaml啟用不同品種時
 *     - Define => Oep520Core.cs
 *     - Publish => ProductManager.cs
 *     - Subscribe => Tray.cs
 *     
 *   - RequestTrayListUpdate(string 品種ID) => TrayList有變動時
 *     - Define => Oep520Core.cs
 *     - Publish => ProductManager.cs
 *     - Subscribe => Tray.cs
 *   - OnTraySwitch(int 點位表數) => 1.TrayList有變動時; 2.TraySetting.xaml的Tray盤選擇變更時
 *     - Define => Tray.cs
 *     - Publish => Tray.cs, TraySettingViewModel.cs
 *     - Subscribe => Tray.cs
 *   - OnTrayGroupListsSwitch("") => TrayGroupList變更時
 *     - Define => Tray.cs
 *     - Publish => Tray.cs
 *     - Subscribe => TraySettingViewModel.cs
 *   - OnTrayGroupSwitch("") => TrayGroup選擇有變更時
 *     - Define => Tray.cs
 *     - Publish => TraySettingViewModel.cs
 *     - Subscribe => 
 *
 ***************************/
using CSharpCore;
using CSharpCore.FileSystem;
using OEP520G.Core;
using OEP520G.Parameter;
using Prism.Events;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace OEP520G.Product
{
    /// <summary>
    /// 品種資料結構
    /// </summary>
    public class ProductDefine
    {
        public string ProductId { get; set; } // 品種ID
        public string MachineId { get; set; } // 機台ID
        public string Memo { get; set; }      // 註解
    }

    /// <summary>
    /// 資料交換用
    /// </summary>
    public class ProductExchange : ProductDefine
    {
        //public int Mode { get; set; }            // 模式
        public string NewProductId { get; set; } // 新產品ID
    }

    public class ProductManager
    {
        // Singleton單例模式
        //private static readonly Lazy<ProductManager> lazy = new Lazy<ProductManager>(() => new ProductManager());
        //public static ProductManager Instance => lazy.Value;

        public Dictionary<string, bool> RequiresCompleteChangeover { get; set; } = new Dictionary<string, bool>();

        /// <summary>
        /// 有效品種ID
        /// </summary>
        public static string ActiveProductId { get; private set; } = "";

        /// <summary>
        /// 品種ID是否有效
        /// </summary>
        public static bool HasProductActived { get; private set; } = false;

        /// <summary>
        /// 品種列表
        /// </summary>
        public static List<ProductDefine> ProductList { get; private set; } = new List<ProductDefine>();

        // ini檔名 = 品種名稱
        private readonly string dirName = FileList.DIRECTORY_PRODUCT;
        private readonly string iniName = FileList.INI_PRODUCT_INFO;
        private readonly string sectionName = "ProductInfo";

        // 被管理的物件
        //public Product product { get; }
        //private readonly Tray tray = Tray.Instance;

        //public VisionObj Vision { get; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public ProductManager()
        {
            RebuildProductList();
        }

        /********************
         * 檔案作業
         *   讀取品種
         *   1. ReadParameter(string id, ref ProductData prod)
         *      從預設資料夾讀入品種為ID的資料
         *   2. ReadParameter(string id, ref ProductData prod, string dirPath)
         *      從dirPath資料夾讀入品種為ID的資料
         * 
         *   儲存品種
         *   1. WriteParameter()
         *      無參數=>儲存目前作業中的品種至預設資料夾
         *   2. WriteParameter(string id)
         *      儲存品種id至預設資料夾
         *   3. WriteParameter(string id, string dirPath, ProductData pd = null)
         *      
         *   4. WriteParameter(ProductExchange pe, string dirPath, bool SaveNewProductId)
         *      使用ProductExchange結構。
         *        SaveNewProductId==false => 品種ID 為 ProductExchange 中的 PrductId
         *        SaveNewProductId==true  => 品種ID 為 ProductExchange 中的 NewPrductId
         *******************/
        /// <summary>
        /// 從參數檔讀取品種資料，使用預設品種儲存資料夾
        /// </summary>
        /// <param name="id">品種ID</param>
        /// <param name="prod">回傳的Buffer</param>
        public void ReadParameter(string id, ref ProductDefine prod)
        {
            ReadParameter(id, ref prod, dirName);
        }

        /// <summary>
        /// 從參數檔讀取品種資料
        /// </summary>
        /// <param name="id">品種ID</param>
        /// <param name="prod">回傳的Buffer</param>
        /// <param name="dirPath">品種儲存資料夾'</param>
        /// <returns>讀檔是否成功</returns>
        public void ReadParameter(string id, ref ProductDefine prod, string dirPath)
        {
            string fullPath = $"{dirPath}\\{id}";

            // 若品種資料夾不存在則離開
            if (!Directory.Exists(fullPath))
                return;

            string fullFileName = $"{fullPath}\\{iniName}";
            IniFile iniFile = new IniFile(fullFileName);

            prod.ProductId = iniFile.ReadIniFile(sectionName, "ProductId", "");
            prod.MachineId = Machine.MachineId;
            prod.Memo = iniFile.ReadIniFile(sectionName, "Memo", "");
        }

        /// <summary>
        /// 將品種資料寫回參數檔，使用目前品種及預設品種儲存資料夾
        /// </summary>
        public void WriteParameter()
        {
            if (HasProductActived)
                WriteParameter(ActiveProductId, dirName);
        }

        /// <summary>
        /// 將品種資料寫回參數檔，使用預設品種儲存資料夾
        /// </summary>
        /// <param name="id">品種ID</param>
        public void WriteParameter(string id)
            => WriteParameter(id, dirName);

        /// <summary>
        /// 將品種資料寫回參數檔
        /// </summary>
        /// <param name="id">品種ID</param>
        /// <param name="dirPath">品種儲存資料夾</param>
        /// <param name="pd">品種資料。若不為null，可另指定要儲存的ProductData</param>
        public void WriteParameter(string id, string dirPath, ProductDefine pd = null)
        {
            if (IsProductExist(id))
            {
                // 品種儲存資料夾不存在則新建(Export時不一定在預設位置)
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                string fullPath = $"{dirPath}\\{id}";

                // 品種資料夾不存在則新建
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);

                string fullFileName = $"{fullPath}\\{iniName}";
                IniFile iniFile = new IniFile(fullFileName);

                // 若無來源資料，則以id做儲存
                if (pd == null)
                    pd = ProductList.Find(x => x.ProductId == id);

                iniFile.WriteIniFile(sectionName, "ProductId", id);
                iniFile.WriteIniFile(sectionName, "Memo", pd.Memo ?? "");
            }
        }

        /// <summary>
        /// 將品種資料(EditableData)寫回參數檔
        /// </summary>
        /// <param name="pe">EditableData資料</param>
        /// <param name="dirPath">品種儲存資料夾</param>
        /// <param name="SaveNewProductId">是否使用NewProductId做為Id</param>
        private void WriteParameter(ProductExchange pe, string dirPath, bool SaveNewProductId)
        {
            // 將 ProductExchange 轉成 ProductData
            ProductDefine pd = new ProductDefine
            {
                ProductId = SaveNewProductId ? pe.NewProductId : pe.ProductId,
                MachineId = Machine.MachineId,
                Memo = pe.Memo
            };

            WriteParameter(pd.ProductId, dirPath, pd);
        }

        /********************
         * 共用函式
         *******************/
        /// <summary>
        /// 品種ID是否已存在
        /// </summary>
        /// <param name="id">品種ID</param>
        /// <returns>True:存在  False:不存在</returns>
        public bool IsProductExist(string id)
            => ProductList.Exists(x => x.ProductId == id);

        /// <summary>
        /// 取得品種ID所在Index
        /// </summary>
        /// <param name="id">品種ID</param>
        /// <returns>Index</returns>
        public int GetIndex(string id)
            => ProductList.FindIndex(x => x.ProductId == id);

        /********************
         * 品種作業
         *******************/
        /// <summary>
        /// 切換品種
        /// </summary>
        /// <param name="productId">品種ID。空字串表示無品種ID有效</param>
        /// <returns>切換是否完成</returns>
        public bool SwitchProduct(string productId)
        {
            string megProduct;

            int idx = (productId == "") ? -1 : GetIndex(productId);
            if (idx >= 0)
            {
                ActiveProductId = productId;
                HasProductActived = true;
                megProduct = productId;
            }
            else
            {
                ActiveProductId = "";
                HasProductActived = false;
                megProduct = "None";
            }

            // 發佈品種更換訊息
            Common.EA.GetEvent<OnSwitchProduct>().Publish(ActiveProductId);
            
            // 顯示品種訊息
            Common.EA.GetEvent<StatusBarSetter>().Publish(new StatusBarData()
            {
                Name = EStatusBarContextName.PRODUCT,
                Message = "[品種]" + megProduct
            });

            // 發佈品種更換訊息 (後續作業)
            Common.EA.GetEvent<AfterSwitchProduct>().Publish(ActiveProductId);

            return idx >= 0;
        }

        /// <summary>
        /// 重建品種列表
        /// </summary>
        public bool RebuildProductList()
        {
            try
            {
                ProductList.Clear();

                // 讀取全部品種資料夾
                List<string> dirs = new List<string>(Directory.EnumerateDirectories(dirName,
                    "*", SearchOption.TopDirectoryOnly));

                foreach (string dir in dirs)
                {
                    string[] dirString = dir.Split('\\');

                    ProductDefine p = new ProductDefine();
                    ReadParameter(dirString[^1], ref p);
                    ProductList.Add(p);
                }

                return true;
            }
            catch (DirectoryNotFoundException)
            {
                try
                {
                    // 存放品種的資料夾不存在
                    Directory.CreateDirectory(dirName);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /********************
         * CRUD
         *******************/
        /// <summary>
        /// 建立新品種
        /// </summary>
        /// <param name="pe">資料物件</param>
        /// <returns>執行是否完成</returns>
        public bool NewProduct(ProductExchange pe)
        {
            string id = pe.NewProductId;

            if (IsProductExist(id))
                return false;

            try
            {
                string fullPath = $"{dirName}\\{id}";

                ProductList.Add(new ProductDefine()
                {
                    ProductId = pe.NewProductId,
                    MachineId = Machine.MachineId,
                    Memo = pe.Memo
                });

                Directory.CreateDirectory(fullPath);

                pe.MachineId = Machine.MachineId;
                WriteParameter(pe, dirName, true);
                RebuildProductList();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 複製品種
        /// </summary>
        /// <param name="pe">資料物件</param>
        /// <returns>複製是否完成</returns>
        public bool CopyProduct(ProductExchange pe)
            => CopyProduct(dirName, dirName, pe);

        /// <summary>
        /// 複製品種
        /// </summary>
        /// <param name="fromDir">被複製的品種資料夾</param>
        /// <param name="toDir">複製目的品種資料夾</param>
        /// <param name="pe">資料物件</param>
        /// <returns>複製是否完成</returns>
        public bool CopyProduct(string fromDir, string toDir, ProductExchange pe)
        {
            string fromId = pe.ProductId;
            string toId = pe.NewProductId;

            if (IsProductExist(fromId) && !IsProductExist(toId))
            {
                string fromPath = $"{fromDir}\\{fromId}";
                string toPath = $"{toDir}\\{toId}";

                // 複製整個資料夾
                DirectoryOperate.DirectoryCopy(fromPath, toPath, true);

                ProductList.Add(new ProductDefine()
                {
                    ProductId = pe.NewProductId,
                    MachineId = Machine.MachineId,
                    Memo = pe.Memo
                });

                // 使用新ID儲存一次
                WriteParameter(pe, dirName, true);

                //RebuildProductList();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 複製品種(兩品種完全複製，不可在同一資料夾)
        /// </summary>
        /// <param name="fromDir">被複製的品種資料夾</param>
        /// <param name="toDir">複製目的品種資料夾</param>
        /// <param name="toId">品種ID</param>
        /// <returns>複製是否完成</returns>
        public bool CopyProduct(string fromDir, string fromId, string toDir, string toId)
        {
            if (fromDir == toDir)
                return false;

            if (IsProductExist(fromId) && !IsProductExist(toId))
            {
                string fromPath = $"{fromDir}\\{fromId}";
                string toPath = $"{toDir}\\{toId}";

                // 複製整個資料夾
                DirectoryOperate.DirectoryCopy(fromPath, toPath, true);

                // 以來源資料建立參數檔
                WriteParameter(toId, toDir);

                RebuildProductList();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 品種更名
        /// </summary>
        /// <param name="pe">資料物件</param>
        /// <returns>更名是否完成</returns>
        public bool RenameProduct(ProductExchange pe)
        {
            // 是否為作業中品種
            bool isActiveProduct = ActiveProductId == pe.ProductId;

            string oldId = pe.ProductId;
            string newId = pe.NewProductId;

            if (IsProductExist(newId))
                return false;

            int idx = ProductList.FindIndex(x => x.ProductId == oldId);
            if (idx >= 0)
            {
                // 更名
                Directory.Move($"{dirName}\\{oldId}", $"{dirName}\\{newId}");

                // 更新
                ProductList[idx].ProductId = pe.NewProductId;
                ProductList[idx].Memo = pe.Memo;

                // 寫回
                WriteParameter(pe, dirName, true);
                //RebuildProductList();

                // 若為作業中品種，切換至新名稱
                if (isActiveProduct)
                    SwitchProduct(pe.NewProductId);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 刪除品種
        /// </summary>
        /// <param name="id">品種ID</param>
        /// <returns>刪除是否完成</returns>
        public bool DeleteProduct(string id)
        {
            if (id == "")
                return false;

            int idx = ProductList.FindIndex(x => x.ProductId == id);
            if (idx >= 0)
            {
                ProductList.RemoveAt(idx);
                Directory.Delete($"{dirName}\\{id}", true);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 從外部讀入品種資料
        /// </summary>
        /// <param name="id">讀入品種ID</param>
        /// <param name="dirPath">讀入品種資料夾</param>
        public void ImportProduct(string id, string dirPath)
        {
            string toDir = $"{FileList.DIRECTORY_PRODUCT}\\{id}";

            // 若品種ID已存在
            if (Directory.Exists(toDir))
            {
                if (MessageBox.Show($"品種已存在，是否要覆蓋？\n品種名稱: {id}", "品種覆蓋警告", MessageBoxButton.YesNo,
                    MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
                    return;
            }

            string fromDir = $"{dirPath}\\{id}";

            // double check
            if (!Directory.Exists(fromDir))
                return;

            CopyProduct(dirPath, id, FileList.DIRECTORY_PRODUCT, id);

            // 覆蓋的是目前品種?
            if (id == ActiveProductId)
                SwitchProduct(ActiveProductId);
        }

        ///// <summary>
        ///// 品種資料輸出，使用目前品種ID及預設品種儲存資料夾
        ///// </summary>
        //public void ExportProduct()
        //{
        //    product.SaveParameter(ActiveProduct, FileList.PRODUCT_DIRECTORY);
        //}

        ///// <summary>
        ///// 品種資料輸出，使用預設品種儲存資料夾
        ///// </summary>
        ///// <param name="id">品種ID</param>
        //public void ExportProduct(string id)
        //{
        //    product.SaveParameter(id, FileList.PRODUCT_DIRECTORY);
        //}

        /// <summary>
        /// 品種資料輸出
        /// </summary>
        /// <param name="id">品種ID</param>
        /// <param name="ditPath">品種儲存資料夾</param>
        public void ExportProduct(string id, string ditPath)
            => WriteParameter(id, ditPath);

        ///// <summary>
        ///// ProductManager內部請求執行
        ///// </summary>
        ///// <param name="command">請求指令</param>
        ///// <remarks>
        ///// 為保持此函數為private，不暴露給其他物件，故採用事件聚合器呼叫
        ///// </remarks>
        //private void PMRequest(string command)
        //{
        //    switch (command)
        //    {
        //        case "NewProduct":
        //            break;
        //        default:
        //            new Exception($"ProductManager內部請求指令錯誤: ProductRequest->{command}");
        //            break;
        //    }
        //}
    }
}