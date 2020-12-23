using Imageproject.Contracts;
using OEP520G.Core;
using OEP520G.Parameter;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OEP520G.Automatic.ViewModels
{
    public class AutoOperationViewModel : BindableBase, IActiveAware
    {
        private readonly ActionGroup aob = new ActionGroup();
        private readonly AutoOperation autoOperation = new AutoOperation();
        private readonly AutoProcedure autoProcedure;
        private readonly PickUpPart pickUpPart = new PickUpPart();

        // 運轉模式
        public EAutomaticOperationMode AutoOperateMode { get; private set; }
        private readonly List<string> autoOperationModeList = new List<string> { "自動生產", "生產運作測試" };
        private readonly List<string> operationModeCaption = new List<string> { "自動運轉", "結束運轉" };

        public DelegateCommand AutomaticRuningCommand { get; private set; }
        public DelegateCommand QuantityChangeCommand { get; private set; }
        public DelegateCommand IncreaseQuantityCommand { get; private set; }
        public DelegateCommand DecreaseQuantityCommand { get; private set; }
        public DelegateCommand ResetCounterCommand { get; private set; }

        // TEST
        public DelegateCommand Test1Command { get; private set; }
        public DelegateCommand Test2Command { get; private set; }
        public DelegateCommand Test3Command { get; private set; }

        // 視窗Active/Deactive
        public event EventHandler IsActiveChanged;
        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                if (!value)
                    WriteData();
            }
        }

        // 取消權杖
        private CancellationTokenSource cts = null;

        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        private readonly IImage _image;

        /// <summary>
        /// 建構函式
        /// </summary>
        public AutoOperationViewModel(IImage image)
        {
            _image = image;
            autoProcedure = new AutoProcedure(_image);

            ReadData();

            AutoOperateMode = EAutomaticOperationMode.RunningTest;

            AutoOperateModeSource = autoOperationModeList;
            AutoOperateModeItem = autoOperationModeList[(int)AutoOperateMode];
            OperationModeCaption = operationModeCaption[0];

            PickupAfterAssembly = false;
            InformationToggle = false;

            AutomaticRuningCommand = new DelegateCommand(AutomaticRuning);
            QuantityChangeCommand = new DelegateCommand(QuantityChange);
            IncreaseQuantityCommand = new DelegateCommand(IncreaseQuantity);
            DecreaseQuantityCommand = new DelegateCommand(DecreaseQuantity);
            ResetCounterCommand = new DelegateCommand(ResetCounter);

            // TEST
            Test1Command = new DelegateCommand(Test1);
            Test2Command = new DelegateCommand(Test2);
            Test3Command = new DelegateCommand(Test3);

            // 全域Save事件
            WriteDataCommand = new DelegateCommand(WriteData);
            ApplicationCommands.WriteCommand.RegisterCommand(WriteDataCommand);
        }

        /// <summary>
        /// 1.儲存至Module
        /// 2.利用CompositeCommand執行Save按鈕
        /// </summary>
        public void WriteData()
        {
            if (IsActive)
            {
                // 自動運轉模式
                autoOperation.Quantity = _productionQuantity;

                // 生產計數
                autoOperation.EndProductThisRun = _endProductThisRun;
                autoOperation.DiscardThisRun = _discardThisRun;
                autoOperation.PickUpMaterialThisRun = _pickUpMaterialThisRun;
                autoOperation.EndProductTotal = _endProductTotal;
                autoOperation.DiscardTotal = _discardTotal;
                autoOperation.PickUpMaterialTotal = _pickUpMaterialTotal;
                autoOperation.RecountTime = _recountTime;

                // 工作時間
                autoOperation.DiscardPartWhenPhotoFailed = _discardPartWhenPhotoFailed;
                autoOperation.NoStopWhenPhotoFailed = _noStopWhenPhotoFailed;
                autoOperation.TrayTimesWhenPhotoFailed = _trayTimesWhenPhotoFailed;
                autoOperation.ResetWhenNoTray = _resetWhenNoTray;
                autoOperation.MeasureHeightAfterAssembly = _measureHeightAfterAssembly;
                autoOperation.ProgressInfo = _progressInfo;

                autoOperation.WriteParameter();
            }
        }

        /// <summary>
        /// 讀取參數
        /// </summary>
        public void ReadData()
        {
            // 生產數量
            ProductionQuantity = autoOperation.Quantity;

            // 生產計數
            EndProductThisRun = autoOperation.EndProductThisRun;
            DiscardThisRun = autoOperation.DiscardThisRun;
            PickUpMaterialThisRun = autoOperation.PickUpMaterialThisRun;
            EndProductTotal = autoOperation.EndProductTotal;
            DiscardTotal = autoOperation.DiscardTotal;
            PickUpMaterialTotal = autoOperation.PickUpMaterialTotal;
            RecountTime = autoOperation.RecountTime;

            // 選項
            DiscardPartWhenPhotoFailed = autoOperation.DiscardPartWhenPhotoFailed;
            NoStopWhenPhotoFailed = autoOperation.NoStopWhenPhotoFailed;
            TrayTimesWhenPhotoFailed = autoOperation.TrayTimesWhenPhotoFailed;
            ResetWhenNoTray = autoOperation.ResetWhenNoTray;
            MeasureHeightAfterAssembly = autoOperation.MeasureHeightAfterAssembly;
            ProgressInfo = autoOperation.ProgressInfo;

            // 工作時間
            LastTimeCt = 0;
            ThisTimeCt = 0;
            StandbyTime = DateTime.Now;
            TotalStandbyTime = DateTime.Now;
            RunningTime = DateTime.Now;
            TotalRunningTime = DateTime.Now;
        }

        /********************
         * 自動運轉作業
         *******************/
        /// <summary>
        /// 自動運轉
        /// </summary>
        private async void AutomaticRuning()
        {
            if (aob.IsRunning())
            {
                ProductionQuantity = autoProcedure.StopAutoOperation();

                //// 中斷運轉
                //if (cts != null && !cts.IsCancellationRequested)
                //{
                //    cts.Cancel();
                //    OperationModeCaption = operationModeCaption[0];
                //}
            }
            else
            {
                // 重置取消權杖
                cts = null;
                cts = new CancellationTokenSource();

                OperationModeCaption = operationModeCaption[1];
                await autoProcedure.StartRunning(AutoOperateMode, ProductionQuantity, cts);
                OperationModeCaption = operationModeCaption[0];
            }
        }

        /// <summary>
        /// 變更生產數量
        /// </summary>
        private void QuantityChange()
            => ProductionQuantity = autoProcedure.StopAutoOperation(ProductionQuantity);

        /********************
         * 生產數量設定
         *******************/
        private List<string> _autoOperateModeSource;
        public List<string> AutoOperateModeSource
        {
            get { return _autoOperateModeSource; }
            set { SetProperty(ref _autoOperateModeSource, value); }
        }

        private int _autoOperateModeIndex;
        public int AutoOperateModeIndex
        {
            get { return _autoOperateModeIndex; }
            set
            {
                SetProperty(ref _autoOperateModeIndex, value);
                AutoOperateMode = (EAutomaticOperationMode)value;
            }
        }

        private string _autoOperateModeItem;
        public string AutoOperateModeItem
        {
            get { return _autoOperateModeItem; }
            set { SetProperty(ref _autoOperateModeItem, value); }
        }

        private string _operationModeCaption;
        public string OperationModeCaption
        {
            get { return _operationModeCaption; }
            set { SetProperty(ref _operationModeCaption, value); }
        }

        /********************
         * 生產數量設定
         *******************/
        private int _productionQuantity;
        public int ProductionQuantity
        {
            get { return _productionQuantity; }
            set { SetProperty(ref _productionQuantity, value); }
        }

        // 生產數量設定 加減鍵
        private void IncreaseQuantity()
        {
            if (ProductionQuantity >= 0)
                ++ProductionQuantity;
            else
                ProductionQuantity = 0;
        }

        private void DecreaseQuantity()
        {
            if (ProductionQuantity > 0)
                --ProductionQuantity;
            else
                ProductionQuantity = 0;
        }

        /********************
         * 生產計數
         *******************/
        // 成品
        private long _endProductThisRun;
        public long EndProductThisRun
        {
            get { return _endProductThisRun; }
            set { SetProperty(ref _endProductThisRun, value); }
        }

        // 抛料數
        private long _discardThisRun;
        public long DiscardThisRun
        {
            get { return _discardThisRun; }
            set { SetProperty(ref _discardThisRun, value); }
        }

        // 取料數
        private long _pickUpMaterialThisRun;
        public long PickUpMaterialThisRun
        {
            get { return _pickUpMaterialThisRun; }
            set { SetProperty(ref _pickUpMaterialThisRun, value); }
        }

        // 總計-成品
        private long _endProductTotal;
        public long EndProductTotal
        {
            get { return _endProductTotal; }
            set { SetProperty(ref _endProductTotal, value); }
        }

        // 總計-抛料數
        private long _discardTotal;
        public long DiscardTotal
        {
            get { return _discardTotal; }
            set { SetProperty(ref _discardTotal, value); }
        }

        // 總計-取料數
        private long _pickUpMaterialTotal;
        public long PickUpMaterialTotal
        {
            get { return _pickUpMaterialTotal; }
            set { SetProperty(ref _pickUpMaterialTotal, value); }
        }

        // 重置時間
        private DateTime _recountTime;
        public DateTime RecountTime
        {
            get { return _recountTime; }
            set { SetProperty(ref _recountTime, value); }
        }

        // 時間重置鍵
        private void ResetCounter()
        {
            EndProductThisRun = 0;
            DiscardThisRun = 0;
            PickUpMaterialThisRun = 0;
            EndProductTotal = 0;
            DiscardTotal = 0;
            PickUpMaterialTotal = 0;
            RecountTime = DateTime.Now;
        }

        /********************
         * 工作時間
         *******************/
        private long _lastTimeCt;
        public long LastTimeCt
        {
            get { return _lastTimeCt; }
            set { SetProperty(ref _lastTimeCt, value); }
        }

        private long _thisTimeCt;
        public long ThisTimeCt
        {
            get { return _thisTimeCt; }
            set { SetProperty(ref _thisTimeCt, value); }
        }

        private DateTime _standbyTime;
        public DateTime StandbyTime
        {
            get { return _standbyTime; }
            set { SetProperty(ref _standbyTime, value); }
        }

        private DateTime _totalStandbyTime;
        public DateTime TotalStandbyTime
        {
            get { return _totalStandbyTime; }
            set { SetProperty(ref _totalStandbyTime, value); }
        }

        private DateTime _runningTime;
        public DateTime RunningTime
        {
            get { return _runningTime; }
            set { SetProperty(ref _runningTime, value); }
        }

        private DateTime _totalRunningTime;
        public DateTime TotalRunningTime
        {
            get { return _totalRunningTime; }
            set { SetProperty(ref _totalRunningTime, value); }
        }

        /********************
         * 選項
         *******************/
        private bool _discardPartWhenPhotoFailed;
        public bool DiscardPartWhenPhotoFailed
        {
            get { return autoOperation.DiscardPartWhenPhotoFailed; }
            set { SetProperty(ref _discardPartWhenPhotoFailed, value); }
        }

        private bool _noStopWhenPhotoFailed;
        public bool NoStopWhenPhotoFailed
        {
            get { return autoOperation.NoStopWhenPhotoFailed; }
            set { SetProperty(ref _noStopWhenPhotoFailed, value); }
        }

        private int _trayTimesWhenPhotoFailed;
        public int TrayTimesWhenPhotoFailed
        {
            get { return autoOperation.TrayTimesWhenPhotoFailed; }
            set { SetProperty(ref _trayTimesWhenPhotoFailed, value); }
        }

        private bool _resetWhenNoTray;
        public bool ResetWhenNoTray
        {
            get { return autoOperation.ResetWhenNoTray; }
            set { SetProperty(ref _resetWhenNoTray, value); }
        }

        private bool _measureHeightAfterAssembly;
        public bool MeasureHeightAfterAssembly
        {
            get { return autoOperation.MeasureHeightAfterAssembly; }
            set { SetProperty(ref _measureHeightAfterAssembly, value); }
        }

        private bool _progressInfo;
        public bool ProgressInfo
        {
            get { return autoOperation.ProgressInfo; }
            set { SetProperty(ref _progressInfo, value); }
        }

        /********************
         * 取置件
         *******************/
        private bool _pickupAfterAssembly;
        public bool PickupAfterAssembly
        {
            get { return _pickupAfterAssembly; }
            set { SetProperty(ref _pickupAfterAssembly, value); }
        }

        /********************
         * 資訊顯示
         *******************/
        private bool _informationToggle;
        public bool InformationToggle
        {
            get { return _informationToggle; }
            set
            {
                SetProperty(ref _informationToggle, value);
                if (value)
                {
                    InformationDisplay = "自動作業";
                    AutoInfoVisibility = true;
                    ProofVisibility = false;
                }
                else
                {
                    InformationDisplay = "打樣測試";
                    AutoInfoVisibility = false;
                    ProofVisibility = true;
                }
            }
        }

        private string _informationDisplay;
        public string InformationDisplay
        {
            get { return _informationDisplay; }
            set { SetProperty(ref _informationDisplay, value); }
        }

        private bool _autoInfoVisibility;
        public bool AutoInfoVisibility
        {
            get { return _autoInfoVisibility; }
            set { SetProperty(ref _autoInfoVisibility, value); }
        }

        private bool _proofVisibility;
        public bool ProofVisibility
        {
            get { return _proofVisibility; }
            set { SetProperty(ref _proofVisibility, value); }
        }

        /********************
         * Test
         *******************/
        private readonly Dump discard = new Dump();
        private readonly PickUpPart getPart = new PickUpPart();
        private readonly ActionGroup sg = new ActionGroup();

        private async void Test1()
        {
            //_image.DeviceSetting();
        }
        private async void Test2()
        {
            await sg.SwitchActionGroup(EActionGroup.XTray_ClampY);
            //await getPart.ClampGetProduct();
            await discard.DiscardAll();
        }
        private async void Test3()
        {
            await sg.SwitchActionGroup(EActionGroup.XY_ClampTray);
            await pickUpPart.ClampPickUpBarrel(11);
            
            var clamp = Clamp.Instance;
            clamp.AllClampOpen();
        }
    }
}
