using OEP520G.Core;
using OEP520G.Functions;
using OEP520G.Parameter;
using OEP520G.Product.Views;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Windows;

namespace OEP520G.Product.ViewModels
{
    /// <summary>
    /// 繪製方塊的資料
    /// </summary>
    public class Block
    {
        public string Title { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public string BgColor { get; set; }
        public bool Mask { get; set; }
        public int Tag { get; set; }
    }

    public class TraySettingViewModel : BindableBase, IActiveAware, IProductManager
    {
        //private readonly ProductManager pm;
        private readonly Tray tray = Tray.Instance;

        //
        private readonly TrayExchange te = new TrayExchange();

        // 是否有接收到返回值
        private bool returnDataReceived = false;

        // 按鍵文字
        private readonly string[] ButtonCaption = new string[] { "新增", "複製", "更名", "刪除" };
        private readonly string[] DialogButtonCaption = new string[] { "托盤名稱", "新托盤名稱", "註解", "機台名稱" };

        // PanAndZoom;
        //private TraySetting view;
        //private ZoomBorder zb;
        public PointXY AxisOrigin { get; } = new PointXY { X = 100.0, Y = 100.0 };

        // 繪圖區座標, 左上/右下
        //private double ZoneX1;
        //private double ZoneY1;
        //private double ZoneX2;
        //private double ZoneY2;

        // 全部方塊
        private readonly List<Block> Blocks = new List<Block>();

        // 功能鍵
        public DelegateCommand ActivateTrayArrangementCommand { get; private set; }
        public DelegateCommand NewTrayCommand { get; private set; }
        public DelegateCommand CopyTrayCommand { get; private set; }
        public DelegateCommand RenameTrayCommand { get; private set; }
        public DelegateCommand DeleteTrayCommand { get; private set; }
        public DelegateCommand RenewPointMatrixCommand { get; private set; }

        // DataGrid Command
        public DelegateCommand ChangeTrayCommand { get; private set; }

        /// <summary>
        /// Pan And Zoom
        /// </summary>
        public class PanAndZoomRightClick : PubSubEvent<int> { }

        // Window Loaded
        //public DelegateCommand HandleLoadedCommand { get; private set; }

        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        // 事件聚合器引用
        private readonly IEventAggregator _ea;

        // Dialog
        private readonly IDialogService _ds;

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
                    SetButtonState();
                    tray.RearrangePointMatrix(TrayListSelectedItem);
                    DrawBlock(TrayListSelectedItem);

                    // 對話框事件聚合器
                    _ea.GetEvent<CrudDialogReceiver>().Subscribe(ReceiveReturnData);

                    // 縮放事件聚合器
                    _ea.GetEvent<PanAndZoomRightClick>().Subscribe(ReceiveRightClick);
                }
                else
                {
                    ReadData();
                    _ea.GetEvent<CrudDialogReceiver>().Unsubscribe(ReceiveReturnData);
                    _ea.GetEvent<PanAndZoomRightClick>().Unsubscribe(ReceiveRightClick);
                }
            }
        }

        /// <summary>
        /// 建構函式
        /// </summary>
        public TraySettingViewModel(IEventAggregator ea, IDialogService ds, IContainerExtension ce, IRegionManager rm)
        {
            _ea = ea;
            _ds = ds;
            container = ce;
            regionManager = rm;

            // 原點
            ZoomOriginX = AxisOrigin.X;
            ZoomOriginY = AxisOrigin.Y;
            //ZoomOriginX = 0;
            //ZoomOriginY = 0;

            // Zoom區功能鍵
            ShowAxisLine = true;
            AuxiliaryDisplay = true;

            // 按鍵文字
            NewTrayCaption = ButtonCaption[0];
            CopyTrayCaption = ButtonCaption[1];
            RenameTrayCaption = ButtonCaption[2];
            DeleteTrayCaption = ButtonCaption[3];

            DirectionSource = new List<string>
            {
                "水平排列",
                "垂直排列"
            };

            OrientationSource = new List<string>
            {
                "交錯排列",
                "依序排列"
            };

            // 品種切換
            _ea.GetEvent<OnSwitchProduct>().Subscribe(onProductChangeover);
            _ea.GetEvent<AfterSwitchProduct>().Subscribe(afterProductChangeover);

            // 功能鍵
            ActivateTrayArrangementCommand = new DelegateCommand(ActivateTrayArrangement);
            NewTrayCommand = new DelegateCommand(NewTray);
            CopyTrayCommand = new DelegateCommand(CopyTray);
            RenameTrayCommand = new DelegateCommand(RenameTray);
            DeleteTrayCommand = new DelegateCommand(DeleteTray);

            // DataGrid Command
            ChangeTrayCommand = new DelegateCommand(ChangeTray);
            RenewPointMatrixCommand = new DelegateCommand(RenewPointMatrix);

            // Window Loaded
            //HandleLoadedCommand = new DelegateCommand(HandleLoaded);

            // 全域Save事件
            WriteDataCommand = new DelegateCommand(WriteData);
            ApplicationCommands.WriteCommand.RegisterCommand(WriteDataCommand);
        }

        ///// <summary>
        ///// Window Loaded
        ///// </summary>
        //private void HandleLoaded()
        //{
        //}

        /********************
         * 檔案作業
         *******************/
        /// <summary>
        /// 資料存檔
        /// </summary>
        private void WriteData()
        {
            if (IsActive)
            {
                tray.WriteParameter();
            }
        }

        private void ReadData()
        {
            tray.ReadParameter();
            RefreshDataGrid();
        }

        /********************
        * 品種切換作業
        *******************/
        /// <inheritdoc/>
        /// <param name="productId">切換後的品種ID</param>
        public void onProductChangeover(string productId)
        {
        }

        /// <inheritdoc/>
        /// <param name="productId">切換後的品種ID</param>
        public void afterProductChangeover(string productId)
        {
            // 切換TrayList
            tray.ReadParameter();
            RefreshDataGrid();
            DrawBlock(TrayListSelectedItem);

            //// 臨時修正BUG用
            //for (int idxPm = 0; idxPm < TrayList.Count; idxPm++)
            //{
            //    List<CoorTableDefine> ctd = TrayList[idxPm].CoorTable;
            //    for (int idxCt = 0; idxCt < ctd.Count; idxCt++)
            //        RearrangeCoorTable(ctd[idxCt]);

            //    RearrangePointMatrix(TrayList[idxPm]);
            //}

            // 發佈TrayList變更
            //if ((TrayList != null) && (TrayList.Count > 0))
            //    Common.EA.GetEvent<OnTrayListSwitch>().Publish(TrayList[0]);
            //else
            //    Common.EA.GetEvent<OnTrayListSwitch>().Publish(null);
        }

        /********************
         * 頁面切換
         *******************/
        private readonly IContainerExtension container;
        private readonly IRegionManager regionManager;
        private void ActivateTrayArrangement()
        {
            IRegion region = regionManager.Regions["MainRegion"];
            var view = container.Resolve<TrayArrangement>();
            region.Activate(view);
        }

        /********************
         * TrayList
         *******************/
        ///// <summary>
        ///// TrayList變動後處理
        ///// </summary>
        //public void OnTrayListUpdate(TrayObject to)
        //{
        //    RefreshDataGrid();
        //    //GetTrayData(to);
        //}

        ///// <summary>
        ///// 更新Tray列表Source
        ///// </summary>
        private void RefreshDataGrid()
        {
            TrayListSource = null;
            TrayListSource = tray.TrayList;

            TrayListSelectedIndex = 0;
        }

        ///// <summary>
        ///// 取TrayList更新Tray資料顯示
        ///// </summary>
        //private void GetTrayData(TrayObject to)
        //{
        //    if (to != null)
        //    {
        //        FieldMemo = to.Memo;
        //        FieldDirection = to.Direction;
        //        FieldOffsetX = to.OffsetX;
        //        FieldOffsetY = to.OffsetY;
        //    }
        //    else
        //    {
        //        FieldMemo = string.Empty;
        //        FieldDirection = string.Empty;
        //        FieldOffsetX = 0;
        //        FieldOffsetY = 0;
        //    }

        //    SetButtonState();
        //    //tray.RebuildCoorTableList(to);
        //    Common.EA.GetEvent<OnTraySelectChanged>().Publish(to);
        //}

        /********************
         * 功能鍵
         *******************/
        /// <summary>
        /// 按鍵狀態設定
        /// </summary>
        private void SetButtonState()
        {
            bool cond1 = TrayListSelectedItem != null;
            bool cond2 = ProductManager.HasProductActived;

            NewTrayEnabled = cond2;
            CopyTrayEnabled = cond1;
            RenameTrayEnabled = cond1;
            DeleteTrayEnabled = cond1;
        }

        ///// <summary>
        ///// 開啟並傳送資料給FieldDialog
        ///// </summary>
        //private void OpenEditWindow(string func)
        //{
        //    // 對話框互傳用資料
        //    CrudDialogData dpdData = new CrudDialogData
        //    {
        //        // 傳遞指令給後續接收的函式用
        //        //BuffString = func
        //    };

        //    string TrayId = (TrayListSelectedItem != null) ? TrayListSelectedItem.Name : "";

        //    //switch (func)
        //    //{
        //    //    case "NEW":
        //    //        fData.Title = productConst.DataProcessingName[0];

        //    //        fData.Caption_1 = "托盤ID";
        //    //        fData.Data_1 = "";
        //    //        fData.Visibility_1 = "Visible";
        //    //        fData.Enabled_1 = "True";

        //    //        fData.Caption_2 = "註解";
        //    //        fData.Data_2 = "";
        //    //        fData.Visibility_2 = "Visible";
        //    //        fData.Enabled_2 = "True";

        //    //        fData.Caption_3 = "";
        //    //        fData.Data_3 = "";
        //    //        fData.Visibility_3 = "Hidden";
        //    //        fData.Enabled_3 = "False";

        //    //        fData.Caption_4 = "";
        //    //        fData.Data_4 = "";
        //    //        fData.Visibility_4 = "Hidden";
        //    //        fData.Enabled_4 = "False";
        //    //        break;
        //    //    case "COPY":
        //    //        fData.Title = productConst.DataProcessingName[1];

        //    //        fData.Caption_1 = "托盤ID";
        //    //        fData.Data_1 = TrayId;
        //    //        fData.Visibility_1 = "Visible";
        //    //        fData.Enabled_1 = "False";

        //    //        fData.Caption_2 = "新托盤ID";
        //    //        fData.Data_2 = "";
        //    //        fData.Visibility_2 = "Visible";
        //    //        fData.Enabled_2 = "True";

        //    //        fData.Caption_3 = "註解";
        //    //        fData.Data_3 = TrayListSelectedItem.Memo;
        //    //        fData.Visibility_3 = "Visible";
        //    //        fData.Enabled_3 = "True";

        //    //        fData.Caption_4 = "";
        //    //        fData.Data_4 = "";
        //    //        fData.Visibility_4 = "Hidden";
        //    //        fData.Enabled_4 = "False";
        //    //        break;
        //    //    case "DELETE":
        //    //        break;
        //    //}

        //    if (func == "DELETE")
        //    {
        //        // 刪除確認
        //        if (MessageBox.Show($"確定刪除嗎？\n托盤名稱：{TrayId}", "刪除托盤",
        //            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        //        {
        //            tray.DeleteTray(TrayListSelectedItem);
        //        }
        //    }
        //    else
        //    {
        //        // 發佈資料給編輯視窗並顯示
        //        FieldDialog FieldWindow = new FieldDialog();
        //        //Common.EA.GetEvent<SendToFieldDialog>().Publish(fData);
        //        FieldWindow.ShowDialog();
        //    }
        //}

        /********************
         * Tray CRUD
         *******************/
        /// <summary>
        /// 新增Tray
        /// </summary>
        private void NewTray()
        {
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

            para += "&Field4Visibility=Collapsed";
            para += "&Field4Enabled=False";

            _ds.ShowDialog("CrudDialog", new DialogParameters(para), r =>
            {
                if ((r.Result == ButtonResult.OK) && returnDataReceived)
                    NewFunction();
            });
        }

        /// <summary>
        /// 複製Tray
        /// </summary>
        private void CopyTray()
        {
            string para = $"Title={ButtonCaption[1]}";

            para += $"&Field1={TrayListSelectedItem.Name}";
            para += "&Field1Label=";
            para += "&Field1Visibility=Visible";
            para += "&Field1Enabled=False";

            para += "&Field2=";
            para += $"&Field2Label={DialogButtonCaption[1]}";
            para += "&Field2Visibility=Visible";
            para += "&Field2Enabled=True";

            para += $"&Field3={TrayListSelectedItem.Memo}";
            para += $"&Field3Label={DialogButtonCaption[2]}";
            para += "&Field3Visibility=Visible";
            para += "&Field3Enabled=True";

            para += "&Field4Visibility=Collapsed";
            para += "&Field4Enabled=False";

            _ds.ShowDialog("CrudDialog", new DialogParameters(para), r =>
            {
                if ((r.Result == ButtonResult.OK) && returnDataReceived)
                    CopyFunction();
            });
        }

        /// <summary>
        /// Tray更名
        /// </summary>
        private void RenameTray()
        {
            string para = $"Title={ButtonCaption[2]}";

            para += $"&Field1={TrayListSelectedItem.Name}";
            para += "&Field1Label=";
            para += "&Field1Visibility=Visible";
            para += "&Field1Enabled=False";

            para += "&Field2=";
            para += $"&Field2Label={DialogButtonCaption[1]}";
            para += "&Field2Visibility=Visible";
            para += "&Field2Enabled=True";

            para += $"&Field3={TrayListSelectedItem.Memo}";
            para += $"&Field3Label={DialogButtonCaption[2]}";
            para += "&Field3Visibility=Visible";
            para += "&Field3Enabled=True";

            para += "&Field4Visibility=Collapsed";
            para += "&Field4Enabled=False";

            _ds.ShowDialog("CrudDialog", new DialogParameters(para), r =>
            {
                if ((r.Result == ButtonResult.OK) && returnDataReceived)
                    RenameFunction();
            });
        }

        /// <summary>
        /// 刪除Tray
        /// </summary>
        private void DeleteTray()
        {
            TrayData to = TrayListSelectedItem;

            // 刪除確認
            if (MessageBox.Show($"確定刪除嗎？\n托盤名稱：{to.Name}", "刪除托盤",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                tray.DeleteTray(to);
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
                te.TrayName = dpdData.Field1;
                te.NewTrayName = dpdData.Field2;
                te.Memo = dpdData.Field3;

                returnDataReceived = true;
            }
        }

        /********************
         * 功能
         *******************/
        private void NewFunction()
        {
            if (tray.IsTrayExist(te.NewTrayName))
            {
                MessageBox.Show("托盤名稱重覆，請重新輸入！", "輸入錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                //OpenEditWindow(te.Mode);
            }
            else
            {
                tray.NewTray(te);
                RefreshDataGrid();
            }
        }

        private void CopyFunction()
        {
            if (tray.IsTrayExist(te.NewTrayName))
            {
                MessageBox.Show("托盤名稱重覆，請重新輸入！", "輸入錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                //OpenEditWindow(te.Mode);
            }
            else
            {
                tray.CopyTray(te);
                RefreshDataGrid();
            }
        }

        private void RenameFunction()
        {
            if (tray.IsTrayExist(te.NewTrayName))
            {
                MessageBox.Show("托盤名稱重覆，請重新輸入！", "輸入錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                //OpenEditWindow(te.Mode);
            }
            else
            {
                tray.RenameTray(te);
                RefreshDataGrid();
            }
        }

        //********************
        // * DataGrid選擇變更動作
        // *******************/
        /// <summary>
        /// TrayList選擇變更動作
        /// </summary>
        private void ChangeTray()
        {
            TrayData to = TrayListSelectedItem;
            if (to != null)
            {
                //tray.RearrangePointMatrix(to);
                //WriteData();
                DrawBlock(to);
            }
        }

        private void RenewPointMatrix()
        {
            tray.RearrangePointMatrix(TrayListSelectedItem);
            DrawBlock(TrayListSelectedItem);
        }

        /********************
         * 繪圖區
         *******************/
        /// <summary>
        /// 更新Tray繪圖區
        /// </summary>
        public void RefreshTrayVisualizeSource()
        {
            TrayVisualizeSource = null;
            TrayVisualizeSource = Blocks;
        }

        /// <summary>
        /// 座標軸顯示/隱藏
        /// </summary>
        /// <param name="OnOff">true:ON false:OFF</param>
        public void AxisLineOnOff(bool OnOff)
            => ShowAxisLine = OnOff;

        //private void AutoFit()
        //{
        //    //ShowAxisLine = false;
        //    //zb.Uniform();
        //    //ShowAxisLine = true;
        //}

        /********************
         * 右下繪圖區
         *******************/
        ///// <summary>
        ///// 自ViewModel取得ZoomBorder
        ///// </summary>
        ///// <param name="_zb"></param>
        //public void SetZoomBorder(ZoomBorder _zb)
        //    => zb = _zb;

        /// <summary>
        /// 滑鼠右鍵切換MASK
        /// </summary>
        /// <param name="blockId">方格id</param>
        public void ReceiveRightClick(int blockId)
        {
            Block block = Blocks[blockId];
            TrayData to = TrayListSelectedItem;

            block.Mask = !block.Mask;

            if (block.Mask)
            {
                to.Mask.Add(blockId);
                block.BgColor = "DarkGray";
            }
            else
            {
                to.Mask.Remove(blockId);
                block.BgColor = "WhiteSmoke";
            }

            RefreshTrayVisualizeSource();
        }

        /// <summary>
        /// 繪製Tray點位
        /// </summary>
        private void DrawBlock(TrayData trayData)
        {
            // 頁面未顯示中會例外
            if (IsActive)
            {
                double zoom = 10;
                Blocks.Clear();

                // 方格尺寸
                //double gridSize = 40;
                double halfGridSize = 20;

                if (trayData != null)
                {
                    // 繪圖區座標
                    //ZoneX1 = StartX;
                    //ZoneX2 = ZoneX1 + (gridSize * to.NumX) + gapX * (to.NumX - 1);
                    //ZoneY1 = StartY;
                    //ZoneY2 = ZoneY1 + (gridSize * to.NumY) + gapY * (to.NumY - 1);

                    // 繪製方格矩陣
                    foreach (var pMatrix in trayData.PointMatrix)
                    {
                        Blocks.Add(new Block()
                        {
                            PosX = pMatrix.PointMatrixX * zoom - halfGridSize + AxisOrigin.X,
                            PosY = pMatrix.PointMatrixY * zoom * -1 - halfGridSize + AxisOrigin.Y,
                            Title = pMatrix.PointNo.ToString(),
                            BgColor = "WhiteSmoke",
                            Mask = false,
                            Tag = pMatrix.PointNo
                        });
                    }

                    // MASK
                    foreach (int cell in trayData.Mask)
                    {
                        Blocks[cell].Mask = true;
                        Blocks[cell].BgColor = "DarkGray";
                    }
                }

                TrayVisualizeSource = null;
                TrayVisualizeSource = Blocks;
            }
        }

        /********************
         * 左面版功能鍵
         *******************/
        private string _newTrayCaption;
        public string NewTrayCaption
        {
            get { return _newTrayCaption; }
            set { SetProperty(ref _newTrayCaption, value); }
        }

        private bool _newTrayEnabled;
        public bool NewTrayEnabled
        {
            get { return _newTrayEnabled; }
            set { SetProperty(ref _newTrayEnabled, value); }
        }

        private string _copyTrayCaption;
        public string CopyTrayCaption
        {
            get { return _copyTrayCaption; }
            set { SetProperty(ref _copyTrayCaption, value); }
        }

        private bool _copyTrayEnabled;
        public bool CopyTrayEnabled
        {
            get { return _copyTrayEnabled; }
            set { SetProperty(ref _copyTrayEnabled, value); }
        }

        private string _renameTrayCaption;
        public string RenameTrayCaption
        {
            get { return _renameTrayCaption; }
            set { SetProperty(ref _renameTrayCaption, value); }
        }

        private bool _renameTrayEnabled;
        public bool RenameTrayEnabled
        {
            get { return _renameTrayEnabled; }
            set { SetProperty(ref _renameTrayEnabled, value); }
        }

        private string _deleteTrayCaption;
        public string DeleteTrayCaption
        {
            get { return _deleteTrayCaption; }
            set { SetProperty(ref _deleteTrayCaption, value); }
        }

        private bool _deleteTrayEnabled;
        public bool DeleteTrayEnabled
        {
            get { return _deleteTrayEnabled; }
            set { SetProperty(ref _deleteTrayEnabled, value); }
        }

        /********************
         * DataGrid
         *******************/
        private List<TrayData> _trayListSource;
        public List<TrayData> TrayListSource
        {
            get { return _trayListSource; }
            set { SetProperty(ref _trayListSource, value); }
        }

        private TrayData _trayListSelectedItem;
        public TrayData TrayListSelectedItem
        {
            get { return _trayListSelectedItem; }
            set
            {
                SetProperty(ref _trayListSelectedItem, value);
                SetButtonState();
            }
        }

        private int _trayListSelectedIndex;
        public int TrayListSelectedIndex
        {
            get { return _trayListSelectedIndex; }
            set { SetProperty(ref _trayListSelectedIndex, value); }
        }

        /********************
         * 右上面版資料區繫結
         *******************/
        // 排列方向
        private List<string> _arrangementDirection;
        public List<string> DirectionSource
        {
            get { return _arrangementDirection; }
            set { SetProperty(ref _arrangementDirection, value); }
        }

        // 排列方式
        private List<string> _sourceOrientation;
        public List<string> OrientationSource
        {
            get { return _sourceOrientation; }
            set { SetProperty(ref _sourceOrientation, value); }
        }

        // 排列方式
        private string _fieldOrientation;
        public string FieldOrientation
        {
            get { return _fieldOrientation; }
            set { SetProperty(ref _fieldOrientation, value); }
        }

        /********************
         * 右下面版Zoom區繫結
         *******************/
        // 輔助顯示
        private bool _auxiliaryDisplay;
        public bool AuxiliaryDisplay
        {
            get { return _auxiliaryDisplay; }
            set { SetProperty(ref _auxiliaryDisplay, value); }
        }

        // 繪圖區
        private List<Block> _trayVisualizeSource;
        public List<Block> TrayVisualizeSource
        {
            get { return _trayVisualizeSource; }
            set { SetProperty(ref _trayVisualizeSource, value); }
        }

        // 座標軸原點位於Canvas的位置
        private double _zoomOriginX;
        public double ZoomOriginX
        {
            get { return _zoomOriginX; }
            set { SetProperty(ref _zoomOriginX, value); }
        }

        private double _zoomOriginY;
        public double ZoomOriginY
        {
            get { return _zoomOriginY; }
            set { SetProperty(ref _zoomOriginY, value); }
        }

        // 是否顯示座標軸
        private bool _showAxisLine;
        public bool ShowAxisLine
        {
            get { return _showAxisLine; }
            set { SetProperty(ref _showAxisLine, value); }
        }

        // 縮放比率
        private double _scaleRate;
        public double ScaleRate
        {
            get { return _scaleRate; }
            set { SetProperty(ref _scaleRate, value); }
        }
    }
}
