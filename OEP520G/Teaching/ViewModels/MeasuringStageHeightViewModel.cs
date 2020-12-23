using EPCIO;
using EPCIO.IoSystem;
using OEP520G.Core;
using OEP520G.Functions;
using OEP520G.Parameter;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OEP520G.Teaching.ViewModels
{
    public class MeasureStageData
    {
        public int StageNo { get; set; }
        public string StageName { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Height { get; set; }
        public double NewHeight { get; set; }
        public bool Update { get; set; }
    }

    public class MeasuringStageHeightViewModel : BindableBase, IActiveAware
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly Machine machine = Machine.Instance;
        private readonly Nozzle nozzle = Nozzle.Instance;
        private readonly Stage stage = Stage.Instance;

        private readonly ObjectMotion objectMotion = new ObjectMotion();
        private readonly Nozzle ioAction = new Nozzle();
        private readonly MeasureHeight measureHeight = new MeasureHeight();

        //
        private List<MeasureStageData> stageDatas = new List<MeasureStageData>();

        // 解析度
        private readonly double[] resolution = new double[] { 1, 0.5, 0.1, 0.05, 0.01, 0.005 };
        private const int MAX_RESOLUTION = 6;

        // 有效解析度
        private readonly List<double> resForMeasure = new List<double>();

        // Cancellation Token
        private CancellationTokenSource cts = new CancellationTokenSource();

        // 視窗Active/Deactive
        public event EventHandler IsActiveChanged;
        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        // 按鍵
        public DelegateCommand StartMeasureCommand { get; private set; }
        public DelegateCommand UpdateDataCommand { get; private set; }
        public DelegateCommand MoveHereCommand { get; private set; }
        public DelegateCommand StopMeasureCommand { get; private set; }

        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public MeasuringStageHeightViewModel()
        {
            ReadData();

            // 目前只有一個台車
            stageDatas = new List<MeasureStageData>
            {
                new MeasureStageData()
                {
                    StageNo = stage.Id,
                    StageName = stage.Name,
                    X = stage.RotateCenter.X,
                    Y = stage.RotateCenter.Y,
                    Height = stage.HeightZ,
                    Update = false,
                    NewHeight = 0
                }
            };

            DataGridUpdate();

            // 解析度
            Resolution0Label = resolution[0].ToString() + "mm";
            Resolution1Label = resolution[1].ToString() + "mm";
            Resolution2Label = resolution[2].ToString() + "mm";
            Resolution3Label = resolution[3].ToString() + "mm";
            Resolution4Label = resolution[4].ToString() + "mm";
            Resolution5Label = resolution[5].ToString() + "mm";
            SelectAllResolution();

            // 按鍵
            InRunningEnable = false;
            InStopEnable = true;

            StartMeasureCommand = new DelegateCommand(StartMeasure);
            UpdateDataCommand = new DelegateCommand(UpdateData);
            MoveHereCommand = new DelegateCommand(MoveHere);
            StopMeasureCommand = new DelegateCommand(StopMeasure);

            // 全域Save事件
            WriteDataCommand = new DelegateCommand(WriteData);
            ApplicationCommands.WriteCommand.RegisterCommand(WriteDataCommand);
        }

        /********************
         * 參數作業
         *******************/
        /// <summary>
        /// 全域Save事件
        /// </summary>
        private void WriteData()
        {
            if (IsActive)
            {
                stage.HeightZ = stageDatas[0].NewHeight;
                stage.WriteParameter();
            }
        }

        /// <summary>
        /// 取得參數值
        /// </summary>
        public void ReadData()
        {
            stage.ReadParameter();
        }

        /********************
         * DataGrid
         *******************/
        private void DataGridUpdate()
        {
            MeasureStageSource = null;
            MeasureStageSource = stageDatas;
        }

        /********************
         * 測高共用部分
         *******************/
        /// <summary>
        /// 停止測高
        /// </summary>
        private void CancelMeasureNozzle()
        {
            if (cts != null && !cts.IsCancellationRequested)
                cts.Cancel();
        }

        private void CreateResolutionList()
        {
            resForMeasure.Clear();

            if (Resolution0)
                resForMeasure.Add(resolution[0]);
            if (Resolution1)
                resForMeasure.Add(resolution[1]);
            if (Resolution2)
                resForMeasure.Add(resolution[2]);
            if (Resolution3)
                resForMeasure.Add(resolution[3]);
            if (Resolution4)
                resForMeasure.Add(resolution[4]);
            if (Resolution5)
                resForMeasure.Add(resolution[5]);
        }

        /********************
         * 按鍵
         *******************/
        private async void StartMeasure()
        {
            InRunningEnable = true;
            InStopEnable = false;

            cts.Dispose();
            cts = new CancellationTokenSource();

            CreateResolutionList();

            await epcio.SafetyPosition();

            try
            {
                // 安全高度
                await epcio.MoveServoZToSafety();

                // 作業是否取消
                if (cts.Token.IsCancellationRequested)
                    cts.Token.ThrowIfCancellationRequested();

                // 基準吸嘴移至測高平台上方
                await objectMotion.NozzleToStage(nozzle.DatumNozzleId);

                // 台車夾片開啟&真空關閉
                stage.StageVaccumOff();
                stage.StageClampOpen();

                // 吸嘴伸出
                ioAction.NozzleDown(nozzle.DatumNozzleId);

                // 測高
                RemoteIo sensor = nozzle.DatumNozzleId switch
                {
                    ENozzleId.Nozzle01 => epcio.Nozzle01_DownLs,
                    ENozzleId.Nozzle02 => epcio.Nozzle02_DownLs,
                    ENozzleId.Nozzle03 => epcio.Nozzle03_DownLs,
                    ENozzleId.Nozzle04 => epcio.Nozzle04_DownLs,
                    ENozzleId.Nozzle05 => epcio.Nozzle05_DownLs,
                    ENozzleId.Nozzle06 => epcio.Nozzle06_DownLs,
                    ENozzleId.Nozzle07 => epcio.Nozzle07_DownLs,
                    ENozzleId.Nozzle08 => epcio.Nozzle08_DownLs,
                    ENozzleId.Nozzle09 => epcio.Nozzle09_DownLs,
                    ENozzleId.Nozzle10 => epcio.Nozzle10_DownLs,
                    ENozzleId.Nozzle11 => epcio.Nozzle11_DownLs,
                    _ => null
                };

                // 執行測高
                if (sensor != null)
                    measureHeight.Start(resForMeasure, sensor, cts, true);

                // 吸嘴縮回
                //ioAction.NozzleUp(nozzle.DatumNozzleId);

                // 台車夾片閉回
                await Task.Delay(100);
                //ioAction.StageClampClose();

                // 儲存結果
                if (measureHeight.IsFinished)
                    stageDatas[0].NewHeight = measureHeight.Result - machine.AssembleMeasureHeightPlatform.Z;
            }
            catch (OperationCanceledException)
            {
            }

            // 測完
            await epcio.SafetyPosition();
            DataGridUpdate();

            InRunningEnable = false;
            InStopEnable = true;
        }

        private void UpdateData()
        {
            if (stageDatas[0].Update)
            {
                stageDatas[0].Height = stageDatas[0].NewHeight;
                DataGridUpdate();
            }
        }

        private void MoveHere()
        {
            objectMotion.MoveCameraToStage();
        }

        private void StopMeasure()
        {
            if (cts != null && !cts.IsCancellationRequested)
                cts.Cancel();
        }

        /********************
         * 滑鼠右鍵
         *******************/
        private void SelectAllResolution()
        {
            Resolution0 = true;
            Resolution1 = true;
            Resolution2 = true;
            Resolution3 = true;
            Resolution4 = true;
            Resolution5 = true;
        }

        private void UnselectAllResolution()
        {
            Resolution0 = false;
            Resolution1 = false;
            Resolution2 = false;
            Resolution3 = false;
            Resolution4 = false;
            Resolution5 = false;
        }

        /********************
         * 繫結
         *******************/
        // DataGrid
        private List<MeasureStageData> _measureStageSource;
        public List<MeasureStageData> MeasureStageSource
        {
            get { return _measureStageSource; }
            set { SetProperty(ref _measureStageSource, value); }
        }

        private MeasureStageData _measureStageItem;
        public MeasureStageData MeasureStageItem
        {
            get { return _measureStageItem; }
            set { SetProperty(ref _measureStageItem, value); }
        }

        private int _measureStageIndex;
        public int MeasureStageIndex
        {
            get { return _measureStageIndex; }
            set { SetProperty(ref _measureStageIndex, value); }
        }

        // 解析度
        private bool _resolution0;
        public bool Resolution0
        {
            get { return _resolution0; }
            set { SetProperty(ref _resolution0, value); }
        }

        private string _resolution0Label;
        public string Resolution0Label
        {
            get { return _resolution0Label; }
            set { SetProperty(ref _resolution0Label, value); }
        }

        private bool _resolution1;
        public bool Resolution1
        {
            get { return _resolution1; }
            set { SetProperty(ref _resolution1, value); }
        }

        private string _resolution1Label;
        public string Resolution1Label
        {
            get { return _resolution1Label; }
            set { SetProperty(ref _resolution1Label, value); }
        }

        private bool _resolution2;
        public bool Resolution2
        {
            get { return _resolution2; }
            set { SetProperty(ref _resolution2, value); }
        }

        private string _resolution2Label;
        public string Resolution2Label
        {
            get { return _resolution2Label; }
            set { SetProperty(ref _resolution2Label, value); }
        }

        private bool _resolution3;
        public bool Resolution3
        {
            get { return _resolution3; }
            set { SetProperty(ref _resolution3, value); }
        }

        private string _resolution3Label;
        public string Resolution3Label
        {
            get { return _resolution3Label; }
            set { SetProperty(ref _resolution3Label, value); }
        }

        private bool _resolution4;
        public bool Resolution4
        {
            get { return _resolution4; }
            set { SetProperty(ref _resolution4, value); }
        }

        private string _resolution4Label;
        public string Resolution4Label
        {
            get { return _resolution4Label; }
            set { SetProperty(ref _resolution4Label, value); }
        }

        private bool _resolution5;
        public bool Resolution5
        {
            get { return _resolution5; }
            set { SetProperty(ref _resolution5, value); }
        }

        private string _resolution5Label;
        public string Resolution5Label
        {
            get { return _resolution5Label; }
            set { SetProperty(ref _resolution5Label, value); }
        }

        // 按鍵有效性
        private bool _inRunningEnable;
        public bool InRunningEnable
        {
            get { return _inRunningEnable; }
            set { SetProperty(ref _inRunningEnable, value); }
        }

        private bool _inStopEnable;
        public bool InStopEnable
        {
            get { return _inStopEnable; }
            set { SetProperty(ref _inStopEnable, value); }
        }
    }
}
