using OEP520G.Core;
using OEP520G.Parameter;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Windows;

namespace OEP520G.Product.ViewModels
{
    public class ProductViewModel : BindableBase, IActiveAware
    {
        // Model
        private readonly ProductManager pm = new ProductManager();
        private readonly ProductExchange returnData = new ProductExchange();

        // 是否有接收到返回值
        private bool returnDataReceived = false;

        // 功能鍵
        public DelegateCommand ActiveProductCommand { get; set; }
        public DelegateCommand NewProductCommand { get; set; }
        public DelegateCommand CopyProductCommand { get; set; }
        public DelegateCommand RenameProductCommand { get; set; }
        public DelegateCommand DeleteProductCommand { get; set; }
        public DelegateCommand UpdateListCommand { get; set; }

        // 按鍵文字
        private readonly string[] ButtonCaption = new string[] { "新增品種", "複製品種", "變更名稱", "刪除品種" };
        private readonly string[] DialogButtonCaption = new string[] { "品種名稱", "新品種名稱", "註解", "機台名稱" };

        // 視窗Active/Deactive
        public event EventHandler IsActiveChanged;
        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                if (value)
                {
                    ButtonStateChange();
                    _ea.GetEvent<CrudDialogReceiver>().Subscribe(ReceiveReturnData);
                }
                else
                {
                    _ea.GetEvent<CrudDialogReceiver>().Unsubscribe(ReceiveReturnData);
                }
            }
        }

        // Window Loaded
        public DelegateCommand HandleLoadedCommand { get; private set; }

        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        // 事件聚合器引用
        private readonly IEventAggregator _ea;

        // Dialog
        private readonly IDialogService _ds;

        /// <summary>
        /// 建構函式
        /// </summary>
        public ProductViewModel(IEventAggregator ea, IDialogService ds)
        {
            _ea = ea;
            _ds = ds;

            // 功能鍵
            ActiveProductCommand = new DelegateCommand(ActiveProduct);
            NewProductCommand = new DelegateCommand(NewProduct);
            CopyProductCommand = new DelegateCommand(CopyProduct);
            RenameProductCommand = new DelegateCommand(RenameProduct);
            DeleteProductCommand = new DelegateCommand(DeleteProduct);
            UpdateListCommand = new DelegateCommand(UpdateList);

            // 功能鍵文字
            NewProductCaption = ButtonCaption[0];
            CopyProductCaption = ButtonCaption[1];
            RenameProductCaption = ButtonCaption[2];
            DeleteProductCaption = ButtonCaption[3];

            // DataGrid資料來源.
            RefreshDataGrid();

            // Window Loaded
            HandleLoadedCommand = new DelegateCommand(HandleLoaded);

            // 全域Save事件
            WriteDataCommand = new DelegateCommand(WriteData);
            ApplicationCommands.WriteCommand.RegisterCommand(WriteDataCommand);
        }

        /// <summary>
        /// Window Loaded
        /// </summary>
        private void HandleLoaded()
        {
        }

        /// <summary>
        /// 儲存參數
        /// </summary>
        private void WriteData()
        {
            if (IsActive)
                pm.WriteParameter();
        }

        /********************
         * CRUD
         * Field1=原品種
         * Field2=新品種
         * Field3=註解
         * Field4=機台ID
         *******************/
        /// <summary>
        /// 新增品種
        /// </summary>
        private void NewProduct()
        {
            //MessageBox.Show(MsgDialog.ShowDialog("TEST", "TEST2", MsgDialogButtons.YesNo, MsgDialogIcon.Error).ToString());

            string para = $"Title={ButtonCaption[0]}";

            para += "&Field1=";
            para += "&Field1Label=";
            para += "&Field1Visibility=Collapsed";
            para += "&Field1Enabled=False";

            para += "&Field2=";
            para += $"&Field2Label={DialogButtonCaption[1]}";
            para += "&Field2Visibility=Visible";
            para += "&Field2Enabled=True";

            para += "&Field3=";
            para += $"&Field3Label={DialogButtonCaption[2]}";
            para += "&Field3Visibility=Visible";
            para += "&Field3Enabled=True";

            para += $"&Field4={Machine.MachineId}";
            para += $"&Field4Label={DialogButtonCaption[3]}";
            para += "&Field4Visibility=Visible";
            para += "&Field4Enabled=False";

            _ds.ShowDialog("CrudDialog", new DialogParameters(para), r =>
            {
                if ((r.Result == ButtonResult.OK) && returnDataReceived)
                    CreateNewFunction();
            });
        }

        /// <summary>
        /// 複製品種
        /// </summary>
        private void CopyProduct()
        {
            string para = $"Title={ButtonCaption[1]}";

            para += $"&Field1={SelectedItem.ProductId}";
            para += $"&Field1Label={DialogButtonCaption[0]}";
            para += "&Field1Visibility=Visible";
            para += "&Field1Enabled=False";

            para += "&Field2=";
            para += $"&Field2Label={DialogButtonCaption[1]}";
            para += "&Field2Visibility=Visible";
            para += "&Field2Enabled=True";

            para += $"&Field3={SelectedItem.Memo}";
            para += $"&Field3Label={DialogButtonCaption[2]}";
            para += "&Field3Visibility=Visible";
            para += "&Field3Enabled=True";

            para += $"&Field4={Machine.MachineId}";
            para += $"&Field4Label={DialogButtonCaption[3]}";
            para += "&Field4Visibility=Visible";
            para += "&Field4Enabled=False";

            _ds.ShowDialog("CrudDialog", new DialogParameters(para), r =>
            {
                if ((r.Result == ButtonResult.OK) && returnDataReceived)
                    CopyFunction();
            });
        }

        /// <summary>
        /// 變更名稱
        /// </summary>
        private void RenameProduct()
        {
            string para = $"Title={ButtonCaption[2]}";

            para += $"&Field1={SelectedItem.ProductId}";
            para += $"&Field1Label={DialogButtonCaption[0]}";
            para += "&Field1Visibility=Visible";
            para += "&Field1Enabled=False";

            para += "&Field2=";
            para += $"&Field2Label={DialogButtonCaption[1]}";
            para += "&Field2Visibility=Visible";
            para += "&Field2Enabled=True";

            para += $"&Field3={SelectedItem.Memo}";
            para += $"&Field3Label={DialogButtonCaption[2]}";
            para += "&Field3Visibility=Visible";
            para += "&Field3Enabled=True";

            para += $"&Field4={Machine.MachineId}";
            para += $"&Field4Label={DialogButtonCaption[3]}";
            para += "&Field4Visibility=Visible";
            para += "&Field4Enabled=False";

            _ds.ShowDialog("CrudDialog", new DialogParameters(para), r =>
            {
                if ((r.Result == ButtonResult.OK) && returnDataReceived)
                    RenameFunction();
            });
        }

        /// <summary>
        /// 刪除品種
        /// </summary>
        private void DeleteProduct()
        {
            string productId = SelectedItem.ProductId;

            // 刪除確認
            if (MessageBox.Show($"確定刪除嗎？\n品種名稱：{productId}", "刪除品種",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                pm.DeleteProduct(productId);
                RefreshDataGrid();
            }
        }

        /// <summary>
        /// 訂閱事件，接收回傳的資料
        /// </summary>
        /// <param name="dpdData">Dialog回傳的資料內容</param>
        private void ReceiveReturnData(CrudDialogData dpdData)
        {
            returnDataReceived = false;

            if (dpdData.Result == ButtonResult.OK)
            {
                returnData.ProductId = dpdData.Field1;
                returnData.NewProductId = dpdData.Field2;
                returnData.Memo = dpdData.Field3;

                returnDataReceived = true;
            }
        }

        /********************
         * 功能
         *******************/
        /// <summary>
        /// 開啟品種
        /// </summary>
        private void ActiveProduct() => pm.SwitchProduct(SelectedItem.ProductId);

        private void CreateNewFunction()
        {
            if (returnDataReceived)
            {
                if (pm.IsProductExist(returnData.NewProductId))
                {
                    MessageBox.Show("品種名稱重覆，請重新輸入！", "輸入錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    //NewProduct();
                }
                else
                {
                    pm.NewProduct(returnData);
                    RefreshDataGrid();
                }
            }
        }

        private void CopyFunction()
        {
            if (returnDataReceived)
            {
                if (pm.IsProductExist(returnData.NewProductId))
                {
                    MessageBox.Show("新品種名稱已存在，請重新輸入！", "輸入錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    //CopyProduct();
                }
                else
                {
                    pm.CopyProduct(returnData);
                    RefreshDataGrid();
                }
            }
        }

        private void RenameFunction()
        {
            if (returnDataReceived)
            {
                if (pm.IsProductExist(returnData.NewProductId))
                {
                    MessageBox.Show("新品種名稱已存在，請另取一個名稱！", "輸入錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    //RenameProduct();
                }
                else
                {
                    pm.RenameProduct(returnData);
                    RefreshDataGrid();
                }
            }
        }

        /// <summary>
        /// 重建品種清單並更新畫面
        /// </summary>
        private void UpdateList()
        {
            pm.RebuildProductList();
            RefreshDataGrid();
        }

        /********************
         * 狀態
         *******************/
        /// <summary>
        /// 按鍵狀態設定
        /// </summary>
        private void ButtonStateChange()
        {
            bool cond1 = SelectedItem != null;
            //bool cond2 = ()

            ActiveProductEnable = cond1;
            NewProductEnable = true;
            CopyProductEnable = cond1;
            RenameProductEnable = cond1;
            DeleteProductEnable = cond1;

            ImportEnabled = true;
            ExportEnabled = cond1;
        }

        /********************
         * DataGrid
         *******************/
        private void RefreshDataGrid()
        {
            ProductSource = null;
            ProductSource = ProductManager.ProductList;
        }

        private List<ProductDefine> _productSource;
        public List<ProductDefine> ProductSource
        {
            get { return _productSource; }
            set { SetProperty(ref _productSource, value); }
        }

        private ProductDefine _selectedItem;
        public ProductDefine SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                SetProperty(ref _selectedItem, value);
                ButtonStateChange();
            }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { SetProperty(ref _selectedIndex, value); }
        }

        /********************
         * 檔案匯出入
         *******************/
        private bool _importEnabled;
        public bool ImportEnabled
        {
            get { return _importEnabled; }
            set { SetProperty(ref _importEnabled, value); }
        }

        private bool _exportEnabled;
        public bool ExportEnabled
        {
            get { return _exportEnabled; }
            set { SetProperty(ref _exportEnabled, value); }
        }

        /********************
         * 繫結
         *******************/
        // 功能鍵
        private bool _activeProductEnable;
        public bool ActiveProductEnable
        {
            get { return _activeProductEnable; }
            set { SetProperty(ref _activeProductEnable, value); }
        }

        // 新增
        private string _newProductCaption;
        public string NewProductCaption
        {
            get { return _newProductCaption; }
            set { SetProperty(ref _newProductCaption, value); }
        }

        private bool _newProductEnable;
        public bool NewProductEnable
        {
            get { return _newProductEnable; }
            set { SetProperty(ref _newProductEnable, value); }
        }

        // 複製
        private string _copyProductCaption;
        public string CopyProductCaption
        {
            get { return _copyProductCaption; }
            set { SetProperty(ref _copyProductCaption, value); }
        }

        private bool _copyProductEnable;
        public bool CopyProductEnable
        {
            get { return _copyProductEnable; }
            set { SetProperty(ref _copyProductEnable, value); }
        }

        // 更名
        private string _renameProductCaption;
        public string RenameProductCaption
        {
            get { return _renameProductCaption; }
            set { SetProperty(ref _renameProductCaption, value); }
        }

        private bool _renameProductEnable;
        public bool RenameProductEnable
        {
            get { return _renameProductEnable; }
            set { SetProperty(ref _renameProductEnable, value); }
        }

        // 刪除
        private string _deleteProductCaption;
        public string DeleteProductCaption
        {
            get { return _deleteProductCaption; }
            set { SetProperty(ref _deleteProductCaption, value); }
        }

        private bool _deleteProductEnable;
        public bool DeleteProductEnable
        {
            get { return _deleteProductEnable; }
            set { SetProperty(ref _deleteProductEnable, value); }
        }






        //private string _FuncButtonCaption0;
        //public string FuncButtonCaption0
        //{
        //    get { return _FuncButtonCaption0; }
        //    set { SetProperty(ref _FuncButtonCaption0, value); }
        //}

        //private string _FuncButtonCaption1;
        //public string FuncButtonCaption1
        //{
        //    get { return _FuncButtonCaption1; }
        //    set { SetProperty(ref _FuncButtonCaption1, value); }
        //}

        //private string _FuncButtonCaption2;
        //public string FuncButtonCaption2
        //{
        //    get { return _FuncButtonCaption2; }
        //    set { SetProperty(ref _FuncButtonCaption2, value); }
        //}

        //private string _FuncButtonCaption3;
        //public string FuncButtonCaption3
        //{
        //    get { return _FuncButtonCaption3; }
        //    set { SetProperty(ref _FuncButtonCaption3, value); }
        //}

        //private bool _funcButtonEnable0;
        //public bool FuncButtonEnable0
        //{
        //    get { return _funcButtonEnable0; }
        //    set { SetProperty(ref _funcButtonEnable0, value); }
        //}

        //private bool _funcButtonEnable1;
        //public bool FuncButtonEnable1
        //{
        //    get { return _funcButtonEnable1; }
        //    set { SetProperty(ref _funcButtonEnable1, value); }
        //}

        //private bool _funcButtonEnable2;
        //public bool FuncButtonEnable2
        //{
        //    get { return _funcButtonEnable2; }
        //    set { SetProperty(ref _funcButtonEnable2, value); }
        //}

        //private bool _funcButtonEnable3;
        //public bool FuncButtonEnable3
        //{
        //    get { return _funcButtonEnable3; }
        //    set { SetProperty(ref _funcButtonEnable3, value); }
        //}
    }
}
