using EPCIO;
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
    public class NozzleMeasuringHeightData
    {
        public double Height { get; set; }
        public double NewH { get; set; }
        public bool Measure { get; set; }
        public bool Update { get; set; }
    }

    public class MeasuringHeightViewModel : BindableBase, IActiveAware
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly Machine machine = Machine.Instance;
        private readonly Nozzle nozzle = Nozzle.Instance;

        private readonly ObjectMotion objectMotion = new ObjectMotion();
        private readonly Nozzle ioAction = new Nozzle();
        private readonly MeasureHeight measureHeight = new MeasureHeight();

        // 吸嘴1~11
        private int nozzleId;

        // 吸嘴內容
        public NozzleMeasuringHeightData[] nozzleDatas { get; set; }

        // 有效吸嘴
        private readonly List<ENozzleId> nozzleForMeasure = new List<ENozzleId>();

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
        public DelegateCommand StartMeasureNozzleCommand { get; private set; }
        public DelegateCommand UpdateDataCommand { get; private set; }
        public DelegateCommand MoveCameraToPlatformCommand { get; private set; }
        public DelegateCommand GetCoorCommand { get; private set; }
        public DelegateCommand StartMeasurePlatformCommand { get; private set; }
        public DelegateCommand CancelMeasureNozzleCommand { get; private set; }

        public DelegateCommand SelectAllNozzleCommand { get; private set; }
        public DelegateCommand UnselectAllNozzleCommand { get; private set; }
        public DelegateCommand SelectAllUpdateCommand { get; private set; }
        public DelegateCommand UnselectAllUpdateCommand { get; private set; }
        public DelegateCommand SelectAllResolutionCommand { get; private set; }
        public DelegateCommand UnselectAllResolutionCommand { get; private set; }

        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public MeasuringHeightViewModel()
        {
            nozzleDatas = new NozzleMeasuringHeightData[Nozzle.MAX_NOZZLE];
            for (int noz = 0; noz < Nozzle.MAX_NOZZLE; noz++)
                nozzleDatas[noz] = new NozzleMeasuringHeightData();

            ReadData();

            // 解析度
            Resolution0Label = resolution[0].ToString() + "mm";
            Resolution1Label = resolution[1].ToString() + "mm";
            Resolution2Label = resolution[2].ToString() + "mm";
            Resolution3Label = resolution[3].ToString() + "mm";
            Resolution4Label = resolution[4].ToString() + "mm";
            Resolution5Label = resolution[5].ToString() + "mm";
            SelectAllResolution();

            // 測高平台
            //NozzleOfPlatformList = nozzle.NOZZLE_NAME_LIST;
            //NozzleOfPlatformItem = nozzle.NOZZLE_NAME_LIST[nozzle.BaseNozzleId];
            BaseNozzleName = nozzle.NOZZLE_NAME_LIST[(int)nozzle.DatumNozzleId];

            // 按鍵
            InRunningEnable = false;
            InStopEnable = true;

            StartMeasureNozzleCommand = new DelegateCommand(StartMeasureNozzle);
            UpdateDataCommand = new DelegateCommand(UpdateData);
            MoveCameraToPlatformCommand = new DelegateCommand(MoveCameraToPlatform);
            GetCoorCommand = new DelegateCommand(GetCoor);
            StartMeasurePlatformCommand = new DelegateCommand(StartMeasurePlatform);
            CancelMeasureNozzleCommand = new DelegateCommand(CancelMeasureNozzle);

            SelectAllNozzleCommand = new DelegateCommand(SelectAllNozzle);
            UnselectAllNozzleCommand = new DelegateCommand(UnselectAllNozzle);
            SelectAllUpdateCommand = new DelegateCommand(SelectAllUpdate);
            UnselectAllUpdateCommand = new DelegateCommand(UnselectAllUpdate);
            SelectAllResolutionCommand = new DelegateCommand(SelectAllResolution);
            UnselectAllResolutionCommand = new DelegateCommand(UnselectAllResolution);

            // 全域Save事件
            WriteDataCommand = new DelegateCommand(WriteData);
            ApplicationCommands.WriteCommand.RegisterCommand(WriteDataCommand);
        }

        /********************
         * 參數作業
         *******************/
        private void UpdateData() => WriteData();

        /// <summary>
        /// 全域Save事件
        /// </summary>
        private void WriteData()
        {
            if (IsActive)
            {
                // 測高平台
                machine.AssembleMeasureHeightPlatform.X = MeasuringPlatformX;
                machine.AssembleMeasureHeightPlatform.Y = MeasuringPlatformY;
                machine.AssembleMeasureHeightPlatform.Z = MeasuringPlatformZ;
                machine.WriteParameter();

                // 吸嘴
                if (NozzleUpdate1)
                    nozzle.NozzleList[0].MeasureHeight = NozzleNewH1;
                if (NozzleUpdate2)
                    nozzle.NozzleList[1].MeasureHeight = NozzleNewH2;
                if (NozzleUpdate3)
                    nozzle.NozzleList[2].MeasureHeight = NozzleNewH3;
                if (NozzleUpdate4)
                    nozzle.NozzleList[3].MeasureHeight = NozzleNewH4;
                if (NozzleUpdate5)
                    nozzle.NozzleList[4].MeasureHeight = NozzleNewH5;
                if (NozzleUpdate6)
                    nozzle.NozzleList[5].MeasureHeight = NozzleNewH6;
                if (NozzleUpdate7)
                    nozzle.NozzleList[6].MeasureHeight = NozzleNewH7;
                if (NozzleUpdate8)
                    nozzle.NozzleList[7].MeasureHeight = NozzleNewH8;
                if (NozzleUpdate9)
                    nozzle.NozzleList[8].MeasureHeight = NozzleNewH9;
                if (NozzleUpdate10)
                    nozzle.NozzleList[9].MeasureHeight = NozzleNewH10;
                if (NozzleUpdate11)
                    nozzle.NozzleList[10].MeasureHeight = NozzleNewH11;

                nozzle.WriteParameter();
                GetDate();
            }
        }

        /// <summary>
        /// 取得參數值
        /// </summary>
        public void ReadData()
        {
            machine.ReadParameter();
            nozzle.ReadParameter();
            GetDate();
        }

        private void GetDate()
        {
            // 測高平台
            MeasuringPlatformX = machine.AssembleMeasureHeightPlatform.X;
            MeasuringPlatformY = machine.AssembleMeasureHeightPlatform.Y;
            MeasuringPlatformZ = machine.AssembleMeasureHeightPlatform.Z;

            // 吸嘴
            NozzleHeight1 = nozzle.NozzleList[0].MeasureHeight;
            NozzleHeight2 = nozzle.NozzleList[1].MeasureHeight;
            NozzleHeight3 = nozzle.NozzleList[2].MeasureHeight;
            NozzleHeight4 = nozzle.NozzleList[3].MeasureHeight;
            NozzleHeight5 = nozzle.NozzleList[4].MeasureHeight;
            NozzleHeight6 = nozzle.NozzleList[5].MeasureHeight;
            NozzleHeight7 = nozzle.NozzleList[6].MeasureHeight;
            NozzleHeight8 = nozzle.NozzleList[7].MeasureHeight;
            NozzleHeight9 = nozzle.NozzleList[8].MeasureHeight;
            NozzleHeight10 = nozzle.NozzleList[9].MeasureHeight;
            NozzleHeight11 = nozzle.NozzleList[10].MeasureHeight;
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

        private void CreateNozzleList()
        {
            nozzleForMeasure.Clear();

            if (NozzleMeasure1)
                nozzleForMeasure.Add(ENozzleId.Nozzle01);
            if (NozzleMeasure2)
                nozzleForMeasure.Add(ENozzleId.Nozzle02);
            if (NozzleMeasure3)
                nozzleForMeasure.Add(ENozzleId.Nozzle03);
            if (NozzleMeasure4)
                nozzleForMeasure.Add(ENozzleId.Nozzle04);
            if (NozzleMeasure5)
                nozzleForMeasure.Add(ENozzleId.Nozzle05);
            if (NozzleMeasure6)
                nozzleForMeasure.Add(ENozzleId.Nozzle06);
            if (NozzleMeasure7)
                nozzleForMeasure.Add(ENozzleId.Nozzle07);
            if (NozzleMeasure8)
                nozzleForMeasure.Add(ENozzleId.Nozzle08);
            if (NozzleMeasure9)
                nozzleForMeasure.Add(ENozzleId.Nozzle09);
            if (NozzleMeasure10)
                nozzleForMeasure.Add(ENozzleId.Nozzle10);
            if (NozzleMeasure11)
                nozzleForMeasure.Add(ENozzleId.Nozzle11);
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
         * 吸嘴測高
         *******************/
        // 啟動吸嘴測高
        private async void StartMeasureNozzle()
        {
            InRunningEnable = true;
            InStopEnable = false;

            cts.Dispose();
            cts = new CancellationTokenSource();

            CreateNozzleList();
            CreateResolutionList();

            await epcio.SafetyPosition();

            try
            {
                // 遍歷吸嘴
                foreach (var noz in nozzleForMeasure)
                {
                    // 安全高度
                    await epcio.MoveServoZToSafety();

                    // 作業是否取消
                    if (cts.Token.IsCancellationRequested)
                        cts.Token.ThrowIfCancellationRequested();

                    // 測高吸嘴移至測高平台上方
                    await objectMotion.NozzleToMeasurePlatform(noz);

                    // 吸嘴伸出
                    ioAction.NozzleDown(noz);

                    // 測高
                     measureHeight.Start(resForMeasure, epcio.MeasureHighPlatform, cts);

                    // 吸嘴縮回
                    ioAction.NozzleUp(noz);

                    // 儲存結果
                    if (measureHeight.IsFinished)
                        UpdateNewHeight(noz, measureHeight.Result - machine.AssembleMeasureHeightPlatform.Z);
                }
            }
            catch (OperationCanceledException)
            {
            }

            // 所有吸嘴測完
            MeasureFinished();
        }

        /// <summary>
        /// 更新吸嘴測高值
        /// </summary>
        /// <param name="nozzleId">吸嘴ID(0~10)</param>
        /// <param name="newHeight">新測高值</param>
        private void UpdateNewHeight(ENozzleId nozzleId, double newHeight)
        {
            switch (nozzleId)
            {
                case ENozzleId.Nozzle01:
                    NozzleNewH1 = newHeight;
                    break;
                case ENozzleId.Nozzle02:
                    NozzleNewH2 = newHeight;
                    break;
                case ENozzleId.Nozzle03:
                    NozzleNewH3 = newHeight;
                    break;
                case ENozzleId.Nozzle04:
                    NozzleNewH4 = newHeight;
                    break;
                case ENozzleId.Nozzle05:
                    NozzleNewH5 = newHeight;
                    break;
                case ENozzleId.Nozzle06:
                    NozzleNewH6 = newHeight;
                    break;
                case ENozzleId.Nozzle07:
                    NozzleNewH7 = newHeight;
                    break;
                case ENozzleId.Nozzle08:
                    NozzleNewH8 = newHeight;
                    break;
                case ENozzleId.Nozzle09:
                    NozzleNewH9 = newHeight;
                    break;
                case ENozzleId.Nozzle10:
                    NozzleNewH10 = newHeight;
                    break;
                case ENozzleId.Nozzle11:
                    NozzleNewH11 = newHeight;
                    break;
            }
        }

        /********************
         * 測高平台測高
         *******************/
        private async void StartMeasurePlatform()
        {
            InRunningEnable = true;
            InStopEnable = false;

            cts.Dispose();
            cts = new CancellationTokenSource();

            CreateResolutionList();

            // 安全高度
            await epcio.SafetyPosition();

            // 基準吸嘴移至測高平台上方
            await objectMotion.NozzleToMeasurePlatform(nozzle.DatumNozzleId);

            // 吸嘴伸出
            ioAction.NozzleDown(nozzle.DatumNozzleId);

            // 測高
            measureHeight.Start(resForMeasure, epcio.MeasureHighPlatform, cts);

            // 吸嘴縮回
            ioAction.NozzleUp(nozzle.DatumNozzleId);

            // 儲存結果
            if (measureHeight.IsFinished)
                MeasuringPlatformZ = measureHeight.Result;

            // 所有吸嘴測完
            MeasureFinished();
        }

        /// <summary>
        /// 測高結束處理
        /// </summary>
        private void MeasureFinished()
        {
            epcio.SafetyPosition();

            InRunningEnable = false;
            InStopEnable = true;
        }

        /// <summary>
        /// 將移動相機移至測高平台
        /// </summary>
        private async void MoveCameraToPlatform()
            => await objectMotion.MoveCameraToPlatform();

        /// <summary>
        /// 取得目前座標
        /// </summary>
        private void GetCoor()
        {
            MeasuringPlatformX = epcio.ServoX.GetCurrentPosition();
            MeasuringPlatformY = epcio.ServoY.GetCurrentPosition();
        }

        /********************
         * 滑鼠右鍵
         *******************/
        private void SelectAllNozzle()
        {
            NozzleMeasure1 = true;
            NozzleMeasure2 = true;
            NozzleMeasure3 = true;
            NozzleMeasure4 = true;
            NozzleMeasure5 = true;
            NozzleMeasure6 = true;
            NozzleMeasure7 = true;
            NozzleMeasure8 = true;
            NozzleMeasure9 = true;
            NozzleMeasure10 = true;
            NozzleMeasure11 = true;
        }

        private void UnselectAllNozzle()
        {
            NozzleMeasure1 = false;
            NozzleMeasure2 = false;
            NozzleMeasure3 = false;
            NozzleMeasure4 = false;
            NozzleMeasure5 = false;
            NozzleMeasure6 = false;
            NozzleMeasure7 = false;
            NozzleMeasure8 = false;
            NozzleMeasure9 = false;
            NozzleMeasure10 = false;
            NozzleMeasure11 = false;
        }

        private void SelectAllUpdate()
        {
            NozzleUpdate1 = true;
            NozzleUpdate2 = true;
            NozzleUpdate3 = true;
            NozzleUpdate4 = true;
            NozzleUpdate5 = true;
            NozzleUpdate6 = true;
            NozzleUpdate7 = true;
            NozzleUpdate8 = true;
            NozzleUpdate9 = true;
            NozzleUpdate10 = true;
            NozzleUpdate11 = true;
        }

        private void UnselectAllUpdate()
        {
            NozzleUpdate1 = false;
            NozzleUpdate2 = false;
            NozzleUpdate3 = false;
            NozzleUpdate4 = false;
            NozzleUpdate5 = false;
            NozzleUpdate6 = false;
            NozzleUpdate7 = false;
            NozzleUpdate8 = false;
            NozzleUpdate9 = false;
            NozzleUpdate10 = false;
            NozzleUpdate11 = false;
        }

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

        // 吸嘴1
        private double _nozzleHeight1;
        public double NozzleHeight1
        {
            get { return _nozzleHeight1; }
            set { SetProperty(ref _nozzleHeight1, value); }
        }

        private double _nozzleNewH1;
        public double NozzleNewH1
        {
            get { return _nozzleNewH1; }
            set { SetProperty(ref _nozzleNewH1, value); }
        }

        private bool _nozzleMeasure1;
        public bool NozzleMeasure1
        {
            get { return _nozzleMeasure1; }
            set { SetProperty(ref _nozzleMeasure1, value); }
        }

        private bool _nozzleUpdate1;
        public bool NozzleUpdate1
        {
            get { return _nozzleUpdate1; }
            set { SetProperty(ref _nozzleUpdate1, value); }
        }

        // 吸嘴2
        private double _nozzleHeight2;
        public double NozzleHeight2
        {
            get { return _nozzleHeight2; }
            set { SetProperty(ref _nozzleHeight2, value); }
        }

        private double _nozzleNewH2;
        public double NozzleNewH2
        {
            get { return _nozzleNewH2; }
            set { SetProperty(ref _nozzleNewH2, value); }
        }

        private bool _nozzleMeasure2;
        public bool NozzleMeasure2
        {
            get { return _nozzleMeasure2; }
            set { SetProperty(ref _nozzleMeasure2, value); }
        }

        private bool _nozzleUpdate2;
        public bool NozzleUpdate2
        {
            get { return _nozzleUpdate2; }
            set { SetProperty(ref _nozzleUpdate2, value); }
        }

        // 吸嘴3
        private double _nozzleHeight3;
        public double NozzleHeight3
        {
            get { return _nozzleHeight3; }
            set { SetProperty(ref _nozzleHeight3, value); }
        }

        private double _nozzleNewH3;
        public double NozzleNewH3
        {
            get { return _nozzleNewH3; }
            set { SetProperty(ref _nozzleNewH3, value); }
        }

        private bool _nozzleMeasure3;
        public bool NozzleMeasure3
        {
            get { return _nozzleMeasure3; }
            set { SetProperty(ref _nozzleMeasure3, value); }
        }

        private bool _nozzleUpdate3;
        public bool NozzleUpdate3
        {
            get { return _nozzleUpdate3; }
            set { SetProperty(ref _nozzleUpdate3, value); }
        }

        // 吸嘴4
        private double _nozzleHeight4;
        public double NozzleHeight4
        {
            get { return _nozzleHeight4; }
            set { SetProperty(ref _nozzleHeight4, value); }
        }

        private double _nozzleNewH4;
        public double NozzleNewH4
        {
            get { return _nozzleNewH4; }
            set { SetProperty(ref _nozzleNewH4, value); }
        }

        private bool _nozzleMeasure4;
        public bool NozzleMeasure4
        {
            get { return _nozzleMeasure4; }
            set { SetProperty(ref _nozzleMeasure4, value); }
        }

        private bool _nozzleUpdate4;
        public bool NozzleUpdate4
        {
            get { return _nozzleUpdate4; }
            set { SetProperty(ref _nozzleUpdate4, value); }
        }

        // 吸嘴5
        private double _nozzleHeight5;
        public double NozzleHeight5
        {
            get { return _nozzleHeight5; }
            set { SetProperty(ref _nozzleHeight5, value); }
        }

        private double _nozzleNewH5;
        public double NozzleNewH5
        {
            get { return _nozzleNewH5; }
            set { SetProperty(ref _nozzleNewH5, value); }
        }

        private bool _nozzleMeasure5;
        public bool NozzleMeasure5
        {
            get { return _nozzleMeasure5; }
            set { SetProperty(ref _nozzleMeasure5, value); }
        }

        private bool _nozzleUpdate5;
        public bool NozzleUpdate5
        {
            get { return _nozzleUpdate5; }
            set { SetProperty(ref _nozzleUpdate5, value); }
        }

        // 吸嘴6
        private double _nozzleHeight6;
        public double NozzleHeight6
        {
            get { return _nozzleHeight6; }
            set { SetProperty(ref _nozzleHeight6, value); }
        }

        private double _nozzleNewH6;
        public double NozzleNewH6
        {
            get { return _nozzleNewH6; }
            set { SetProperty(ref _nozzleNewH6, value); }
        }

        private bool _nozzleMeasure6;
        public bool NozzleMeasure6
        {
            get { return _nozzleMeasure6; }
            set { SetProperty(ref _nozzleMeasure6, value); }
        }

        private bool _nozzleUpdate6;
        public bool NozzleUpdate6
        {
            get { return _nozzleUpdate6; }
            set { SetProperty(ref _nozzleUpdate6, value); }
        }

        // 吸嘴7
        private double _nozzleHeight7;
        public double NozzleHeight7
        {
            get { return _nozzleHeight7; }
            set { SetProperty(ref _nozzleHeight7, value); }
        }

        private double _nozzleNewH7;
        public double NozzleNewH7
        {
            get { return _nozzleNewH7; }
            set { SetProperty(ref _nozzleNewH7, value); }
        }

        private bool _nozzleMeasure7;
        public bool NozzleMeasure7
        {
            get { return _nozzleMeasure7; }
            set { SetProperty(ref _nozzleMeasure7, value); }
        }

        private bool _nozzleUpdate7;
        public bool NozzleUpdate7
        {
            get { return _nozzleUpdate7; }
            set { SetProperty(ref _nozzleUpdate7, value); }
        }

        // 吸嘴8
        private double _nozzleHeight8;
        public double NozzleHeight8
        {
            get { return _nozzleHeight8; }
            set { SetProperty(ref _nozzleHeight8, value); }
        }

        private double _nozzleNewH8;
        public double NozzleNewH8
        {
            get { return _nozzleNewH8; }
            set { SetProperty(ref _nozzleNewH8, value); }
        }

        private bool _nozzleMeasure8;
        public bool NozzleMeasure8
        {
            get { return _nozzleMeasure8; }
            set { SetProperty(ref _nozzleMeasure8, value); }
        }

        private bool _nozzleUpdate8;
        public bool NozzleUpdate8
        {
            get { return _nozzleUpdate8; }
            set { SetProperty(ref _nozzleUpdate8, value); }
        }

        // 吸嘴9
        private double _nozzleHeight9;
        public double NozzleHeight9
        {
            get { return _nozzleHeight9; }
            set { SetProperty(ref _nozzleHeight9, value); }
        }

        private double _nozzleNewH9;
        public double NozzleNewH9
        {
            get { return _nozzleNewH9; }
            set { SetProperty(ref _nozzleNewH9, value); }
        }

        private bool _nozzleMeasure9;
        public bool NozzleMeasure9
        {
            get { return _nozzleMeasure9; }
            set { SetProperty(ref _nozzleMeasure9, value); }
        }

        private bool _nozzleUpdate9;
        public bool NozzleUpdate9
        {
            get { return _nozzleUpdate9; }
            set { SetProperty(ref _nozzleUpdate9, value); }
        }

        // 吸嘴10
        private double _nozzleHeight10;
        public double NozzleHeight10
        {
            get { return _nozzleHeight10; }
            set { SetProperty(ref _nozzleHeight10, value); }
        }

        private double _nozzleNewH10;
        public double NozzleNewH10
        {
            get { return _nozzleNewH10; }
            set { SetProperty(ref _nozzleNewH10, value); }
        }

        private bool _nozzleMeasure10;
        public bool NozzleMeasure10
        {
            get { return _nozzleMeasure10; }
            set { SetProperty(ref _nozzleMeasure10, value); }
        }

        private bool _nozzleUpdate10;
        public bool NozzleUpdate10
        {
            get { return _nozzleUpdate10; }
            set { SetProperty(ref _nozzleUpdate10, value); }
        }

        // 吸嘴11
        private double _nozzleHeight11;
        public double NozzleHeight11
        {
            get { return _nozzleHeight11; }
            set { SetProperty(ref _nozzleHeight11, value); }
        }

        private double _nozzleNewH11;
        public double NozzleNewH11
        {
            get { return _nozzleNewH11; }
            set { SetProperty(ref _nozzleNewH11, value); }
        }

        private bool _nozzleMeasure11;
        public bool NozzleMeasure11
        {
            get { return _nozzleMeasure11; }
            set { SetProperty(ref _nozzleMeasure11, value); }
        }

        private bool _nozzleUpdate11;
        public bool NozzleUpdate11
        {
            get { return _nozzleUpdate11; }
            set { SetProperty(ref _nozzleUpdate11, value); }
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

        // 測高平台
        private double _measuringPlatformX;
        public double MeasuringPlatformX
        {
            get { return _measuringPlatformX; }
            set { SetProperty(ref _measuringPlatformX, value); }
        }

        private double _measuringPlatformY;
        public double MeasuringPlatformY
        {
            get { return _measuringPlatformY; }
            set { SetProperty(ref _measuringPlatformY, value); }
        }

        private double _measuringPlatformZ;
        public double MeasuringPlatformZ
        {
            get { return _measuringPlatformZ; }
            set { SetProperty(ref _measuringPlatformZ, value); }
        }

        // 基準吸嘴選擇
        //private List<string> _nozzleOfPlatformList;
        //public List<string> NozzleOfPlatformList
        //{
        //    get { return _nozzleOfPlatformList; }
        //    set { SetProperty(ref _nozzleOfPlatformList, value); }
        //}

        //private int _nozzleOfPlatformIndex;
        //public int NozzleOfPlatformIndex
        //{
        //    get { return _nozzleOfPlatformIndex; }
        //    set { SetProperty(ref _nozzleOfPlatformIndex, value); }
        //}

        //private string _nozzleOfPlatformItem;
        //public string NozzleOfPlatformItem
        //{
        //    get { return _nozzleOfPlatformItem; }
        //    set { SetProperty(ref _nozzleOfPlatformItem, value); }
        //}

        private string _baseNozzleName;
        public string BaseNozzleName
        {
            get { return _baseNozzleName; }
            set { SetProperty(ref _baseNozzleName, value); }
        }
    }
}
