using CSharpCore.Database;
using EPCIO;
using EpcioSeries;
using Imageproject.Contracts;
using Imageproject.Models;
using Imageproject.Services;
using OEP520G.Core;
using OEP520G.Functions;
using OEP520G.Product;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using TcpipServer.Services;
using StatusBar = OEP520G.Core.StatusBar;

namespace OEP520G.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        //// Prism的IoC及Region
        //public readonly IContainerExtension container;
        //public readonly IRegionManager regionManager;
        //public IRegion region;

        private readonly Epcio epcio = Epcio.Instance;
        private readonly StatusBar statusBar = StatusBar.Instance;
        private readonly ProductManager pm = new ProductManager();
        private readonly SQLiteService sqlite = new SQLiteService();
        private readonly TcpipServerService _tcpipServer;
        private readonly ImageService _camera;

        // StatusBar更新Timer
        private readonly Timer StatusBarUpdateTimer = new Timer { Interval = 500 };

        public DelegateCommand ShowTraySettingCommand { get; private set; }

        // 公開(expose)並實例化IApplicationCommands
        //private IApplicationCommands _applicationCommands;
        //public IApplicationCommands ApplicationCommands
        //{
        //    get { return _applicationCommands; }
        //    set { SetProperty(ref _applicationCommands, value); }
        //}

        // Window Loaded
        public DelegateCommand HandleLoadedCommand { get; set; }

        // Window Closing
        public DelegateCommand HandleClosingCommand { get; set; }

        //private readonly TcpipServerClass _tcpipServer;
        //private readonly ImageClass _image;


        private readonly IEventAggregator _ea;
        private readonly IDialogService _ds;

        /// <summary>
        /// 建構函式
        /// </summary>
        public MainWindowViewModel(IEventAggregator ea, IDialogService ds)
        {
            // 設定共用IEventAggregator
            _ea = ea;
            Common.EA = ea;

            _ds = ds;
            Common.DS = ds;

            _tcpipServer = new TcpipServerService(_ea);
            _camera = new ImageService(_ea, _tcpipServer);

            statusBar.ShowStatusBarMessage("系統初始化中...");

            int retVal;

            // 檢查Images資料夾是否存在
            if (!Directory.Exists("Images"))
            {
                MessageBox.Show("Images folder is missing.", "System Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            //else
            //{
            //    Common.log.WriteLine("Images folder exists.");
            //}

            // 確認資料庫檔案已建立
            sqlite.CreateDatabase(SQLite.DB_FILE_NAME_SYSTEM);

            // 註冊
            _ = new SQLite();
            _ = new ISR();

            // EventAggregator依賴項注入
            // 細節參見Oep520Core.cs
            //Oep520Core.EA = _ea;

            // 註冊主視窗Title標題的事件聚合器
            _ea.GetEvent<WindowTitleSetter>().Subscribe(TitleReceiver);

            // 設定主標題
            TitleReceiver("00 MainWindowTitle", "OEP520G - UI [0.00 2020/11/01]");

            // 運動控制卡控制程式版本
            TitleReceiver("02 EPCIO", $"EPCIO [{Epcio.EpcioLibVersion}]");

            // StatusBar更新Timer
            StatusBarUpdateTimer.Elapsed += new ElapsedEventHandler(StatusBarUpdateEvent);
            StatusBarUpdateTimer.AutoReset = true;
            StatusBarUpdateTimer.Start();

            // 註冊StatusBar的事件聚合器
            _ea.GetEvent<StatusBarSetter>().Subscribe(StatusBarReceiver);

            // 在開發機台上沒有軸卡，故在Debug模式是把軸卡初始化Disable掉
            // 要發布給作業機台時，要切到Release模式編譯


            // EPCIO初始化
            retVal = epcio.Init();
            if (retVal == EpcioErrorCode.IniFileNotFound)
            {
                MsgDialog.Show("無參數檔，以預設值重建參數檔。請先確認參數。", "參數錯誤", MsgDialogButtons.OK, MsgDialogIcon.Warning);
                //MessageBox.Show("無參數檔，以預設值重建參數檔。請先確認參數。", "參數錯誤", MessageBoxButton.OK, MessageBoxImage.Warning);
                //region.Activate(viewServoParameter);
            }
            else if (retVal != MCCL.NO_ERR)
            {
                //MsgDialog.Show($"EPCIO運動控制卡初始化失敗: {retVal}", "EPCIO錯誤", MsgDialogButtons.OK, MsgDialogIcon.Error);
                MessageBox.Show($"EPCIO運動控制卡初始化失敗: {retVal}", "EPCIO錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                //Application.Current.Shutdown();
            }

            // 自訂Fluent.Ribbon Themes
            //ThemeManager.AddTheme(new Uri("pack://application:,,,/OEP520G;component/Themes/FluentRibbonTheme.xaml"));
            //var theme = ThemeManager.DetectTheme(Application.Current);
            //ThemeManager.ChangeTheme(Application.Current, ThemeManager.GetTheme("FluentRibbonTheme"));

            // Window Loaded
            HandleLoadedCommand = new DelegateCommand(HandleLoaded);

            // Window Closing
            HandleClosingCommand = new DelegateCommand(HandleClosing);

            statusBar.ShowStatusBarMessage("系統初始化完成.");
            Common.log.WriteLine("MainWindowViewModel.cs Load Finished.");
        }

        /// <summary>
        /// Window Loaded
        /// </summary>
        private void HandleLoaded()
        {
            pm.SwitchProduct("");
        }

        /// <summary>
        /// Window Closing
        /// </summary>
        private void HandleClosing()
        {

            // 關閉EPCIO軸卡
            Common.log.ImportantEvent("EPCIO Close.");
            epcio.Close();


            Common.log.ImportantEvent("OEP520G Program Close.");
            // 強制關閉所有Thread (ShowDialog()殘留問題，參考Core.DialogCloser.cs中的說明)
            //Environment.Exit(Environment.ExitCode);
            Application.Current.Shutdown();
        }

        /********************
         * 主視窗Title標題處理
         * 細節參見Oep520Core.cs
         *******************/
        /// <summary>
        /// 主視窗Title儲存區
        /// </summary>
        private SortedDictionary<string, string> WindowTitle = new SortedDictionary<string, string>();

        /// <summary>
        /// Title接收處理(使用WindowTitleData結構)
        /// </summary>
        /// <param name="wt">WindowTitleData結構</param>
        private void TitleReceiver(WindowTitleData wt)
            => TitleReceiver(wt.Key, wt.Title);

        /// <summary>
        /// Title接收處理(使用 鍵=值)
        /// </summary>
        /// <param name="key">鍵值</param>
        /// <param name="title">標題</param>
        private void TitleReceiver(string key, string title)
        {
            if (WindowTitle.ContainsKey(key))
                WindowTitle[key] = title;
            else
                WindowTitle.Add(key, title);

            Title = string.Join(" - ", WindowTitle.Values.ToArray());
        }

        /********************
         * StatusBar
         *******************/
        private void StatusBarReceiver(StatusBarData sbd) => StatusBarReceiver(sbd.Name, sbd.Message);

        /// <summary>
        /// Title接收處理(使用 鍵=值)
        /// </summary>
        /// <param name="name">名稱</param>
        /// <param name="msg">顯示內容</param>
        private void StatusBarReceiver(EStatusBarContextName name, string msg)
        {
            switch (name)
            {
                case EStatusBarContextName.MESSAGE:
                    StatusBar_Message = msg;
                    break;
                case EStatusBarContextName.AUTHORITY:
                    break;
                case EStatusBarContextName.PRODUCT:
                    StatusBar_Product = msg;
                    break;
                case EStatusBarContextName.COORDINATION:
                    StatusBar_AxisCoordination = msg;
                    break;
                case EStatusBarContextName.TIME:
                    StatusBar_Time = msg;
                    break;
                case EStatusBarContextName.DEBUG:
                    StatusBar_DebugMessage = msg;
                    break;
            }
        }

        /// <summary>
        /// 訊息更新事件
        /// </summary>
        private void StatusBarUpdateEvent(object sender, ElapsedEventArgs e)
        {
            // 顯示軸座標            
            string msgCoor = "[座標]";
            msgCoor += $"X:{epcio.ServoX.GetCurrentCommandPosition():F3},";
            msgCoor += $"Y:{epcio.ServoY.GetCurrentCommandPosition():F3},";
            msgCoor += $"Z:{epcio.ServoZ.GetCurrentCommandPosition():F3},";
            msgCoor += $"R:{epcio.ServoR.GetCurrentCommandPosition():F1},";
            msgCoor += $"CX:{epcio.ServoClamp.GetCurrentCommandPosition():F3},";
            msgCoor += $"TY:{epcio.ServoTray.GetCurrentCommandPosition():F3}";

            _ea.GetEvent<StatusBarSetter>().Publish(new StatusBarData()
            {
                Name = EStatusBarContextName.COORDINATION,
                Message = msgCoor
            });

            // 顯示時間
            _ea.GetEvent<StatusBarSetter>().Publish(new StatusBarData()
            {
                Name = EStatusBarContextName.TIME,
                Message = $"[{DateTime.Now:HH:mm}]"
            });


            // 顯示DEBUG資訊
            string msgDebug = "[錯誤碼]";
            msgDebug += $"X:0x{epcio.ServoX.GetErrorCode():X},";
            msgDebug += $"Y:0x{epcio.ServoY.GetErrorCode():X},";
            msgDebug += $"Z:0x{epcio.ServoZ.GetErrorCode():X},";
            msgDebug += $"R:0x{epcio.ServoR.GetErrorCode():X},";
            msgDebug += $"CX:0x{epcio.ServoClamp.GetErrorCode():X},";
            msgDebug += $"TY:0x{epcio.ServoTray.GetErrorCode():X}";
            msgDebug += $" Pulse Stock:{epcio.GetPulseStock()}";

            _ea.GetEvent<StatusBarSetter>().Publish(new StatusBarData()
            {
                Name = EStatusBarContextName.DEBUG,
                Message = msgDebug
            });

        }

        /********************
         * 繫結
         *******************/

        private DelegateCommand _FixCameraDeviceSetting;
        public DelegateCommand FixCameraDeviceSetting
            => _FixCameraDeviceSetting ??= new DelegateCommand(ExecuteFixCameraDeviceSetting);
        void ExecuteFixCameraDeviceSetting()
        {
            TcpipServerService tcpip = new TcpipServerService(_ea);
            ImageService image = new ImageService(_ea, tcpip);
            image.FixCameraDeviceSetting();
        }

        private DelegateCommand _MoveCameraDeviceSetting;
        public DelegateCommand MoveCameraDeviceSetting
            => _MoveCameraDeviceSetting ??= new DelegateCommand(ExecuteMoveCameraDeviceSetting);
        void ExecuteMoveCameraDeviceSetting()
        {
            TcpipServerService tcpip = new TcpipServerService(_ea);
            ImageService image = new ImageService(_ea, tcpip);
            image.MoveCameraDeviceSetting();
        }

        private DelegateCommand _FixCameraPropertSetting;
        public DelegateCommand FixCameraPropertSetting
            => _FixCameraPropertSetting ??= new DelegateCommand(ExecuteFixCameraPropertSetting);
        void ExecuteFixCameraPropertSetting()
        {
            TcpipServerService tcpip = new TcpipServerService(_ea);
            ImageService image = new ImageService(_ea, tcpip);
            image.FixCameraPropertSetting();
        }

        private DelegateCommand _MoveCameraPropertSetting;
        public DelegateCommand MoveCameraPropertSetting
            => _MoveCameraPropertSetting ??= new DelegateCommand(ExecuteMoveCameraPropertSetting);
        void ExecuteMoveCameraPropertSetting()
        {
            TcpipServerService tcpip = new TcpipServerService(_ea);
            ImageService image = new ImageService(_ea, tcpip);
            image.MoveCameraPropertSetting();
        }

        // Title
        private string _title = "";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        // StatusBar
        private string _statusBar_Message;
        public string StatusBar_Message
        {
            get { return _statusBar_Message; }
            set { SetProperty(ref _statusBar_Message, value); }
        }

        private string _statusBar_AxisCoordination;
        public string StatusBar_AxisCoordination
        {
            get { return _statusBar_AxisCoordination; }
            set { SetProperty(ref _statusBar_AxisCoordination, value); }
        }

        private string _statusBar_Product;
        public string StatusBar_Product
        {
            get { return _statusBar_Product; }
            set { SetProperty(ref _statusBar_Product, value); }
        }

        private string _statusBar_Time;
        public string StatusBar_Time
        {
            get { return _statusBar_Time; }
            set { SetProperty(ref _statusBar_Time, value); }
        }

        private string _statusBar_DebugMessage;
        public string StatusBar_DebugMessage
        {
            get { return _statusBar_DebugMessage; }
            set { SetProperty(ref _statusBar_DebugMessage, value); }
        }

        //private string _id;
        //public string Id
        //{
        //    get { return _id; }
        //    set
        //    {
        //        SetProperty(ref _id, value);
        //        pm.machine.SaveParameter();
        //    }
        //}

        //private string _name;
        //public string Name
        //{
        //    get { return _name; }
        //    set
        //    {
        //        SetProperty(ref _name, value);
        //        pm.machine.SaveParameter();
        //    }
        //}
    }
}
