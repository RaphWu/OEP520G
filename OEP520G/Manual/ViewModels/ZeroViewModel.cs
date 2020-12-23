using EPCIO;
using Imageproject.Contracts;
using OEP520G.Core;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Timers;

namespace OEP520G.Manual.ViewModels
{
    public class ZeroViewModel : BindableBase, IActiveAware
    {
        // 物件引入
        private readonly Epcio epcio = Epcio.Instance;
        private readonly StatusBar statusBar = StatusBar.Instance;

        // 復歸是否正在執行中?
        // 防止重覆執行
        private static bool GoHomeInProcessing { get; set; }

        /// <summary>
        /// 運動控制卡座標讀取Timer
        /// </summary>
        private readonly Timer AxisStatusUpdate = new Timer { Interval = 200 };

        public string[] ResetButtonCaption { get; private set; }

        public DelegateCommand<string> GoHomeCommand { get; private set; }
        public DelegateCommand GoHomeAllAxisCommand { get; private set; }
        public DelegateCommand GoHomeAbortCommand { get; private set; }
        public DelegateCommand ServoResetCommand { get; private set; }
        public DelegateCommand TestCommand { get; private set; }
        // 各軸狀態訊息
        private const string msgGoingHome = "復歸中";
        private const string msgAtHome = "復歸完成";
        private const string msgGoHomeAbort = "復歸中斷";
        private const string msgCheckHomeSensor = "原點檢查";
        private const string msgCheckHomeSensorFinished = "原點檢查完成";
        private const string msgError = "伺服軸錯誤";
        private const string msgErrorReset = "伺服軸錯誤重置";

        // 座標值備份
        private double[] coorCommandPosition = new double[Epcio.MAX_OEP_AXIS];
        private double[] coorPosition = new double[Epcio.MAX_OEP_AXIS];
        private long[] coorPulse = new long[Epcio.MAX_OEP_AXIS];
        private int[] coorEncoder = new int[Epcio.MAX_OEP_AXIS];

        // 視窗Active/Deactive
        private bool _isActive = false;
        public event EventHandler IsActiveChanged;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;

                if (value)
                {
                    // 復原速度設定
                    //SetSpeed();

                    // 啟動座標讀取Timer
                    AxisStatusUpdate.Start();
                }
                else
                {
                    // 停止座標讀取Timer
                    AxisStatusUpdate.Stop();
                }
            }
        }

        // 事件聚合器引用
        private readonly IEventAggregator _ea;
        private readonly IImage _image;

        /// <summary>
        /// 建構函式
        /// </summary>
        public ZeroViewModel(IEventAggregator ea,
            IImage image)
        {
            _ea = ea;
            _image = image;
            GoHomeInProcessing = false;

            // 基本參數
            //SpeedRate = 100;

            // 按鍵
            ResetButtonCaption = new string[Epcio.MAX_OEP_AXIS]
            {
                string.Concat(epcio.ServoX.Title, "復歸"),
                string.Concat(epcio.ServoY.Title, "復歸"),
                string.Concat(epcio.ServoZ.Title, "復歸"),
                string.Concat(epcio.ServoR.Title, "復歸"),
                string.Concat(epcio.ServoTray.Title, "復歸"),
                string.Concat(epcio.ServoClamp.Title, "復歸")
            };

            InGoHomeRunningEnabled = false;
            InGoHomeStopEnabled = true;

            // 繫結
            GoHomeCommand = new DelegateCommand<string>(GoHome);
            GoHomeAllAxisCommand = new DelegateCommand(GoHomeAllServo);
            GoHomeAbortCommand = new DelegateCommand(GoHomeAbort);
            ServoResetCommand = new DelegateCommand(ServoReset);
            //TestCommand = new DelegateCommand(Test);
            // 註冊座標讀取Timer
            AxisStatusUpdate.Elapsed += new ElapsedEventHandler(ScreenUpdatePolling);
        }

        /********************
         * epcio
         *******************/
        /// <summary>
        /// 記錄復歸軸
        /// </summary>
        private bool[] goHomeAxis = new bool[Epcio.MAX_OEP_AXIS];

        /// <summary>
        /// 各軸前狀態
        /// </summary>
        private EMotionStatus[] axisStatus = new EMotionStatus[Epcio.MAX_OEP_AXIS];

        /// <summary>
        /// 是否按中斷復歸
        /// </summary>
        private bool pressGoHomeAbort = false;

        /// <summary>
        /// 單軸復歸
        /// </summary>
        /// <param name="servoId">要復歸的軸ID</param>
        private async void GoHome(string servoId)
        {
            if (!GoHomeInProcessing)
            {
                GoHomeInProcessing = true;
                pressGoHomeAbort = false;

                InGoHomeRunningEnabled = true;
                InGoHomeStopEnabled = false;

                goHomeAxis[EServoId.X] = false;
                goHomeAxis[EServoId.Y] = false;
                goHomeAxis[EServoId.Z] = false;
                goHomeAxis[EServoId.R] = false;
                goHomeAxis[EServoId.Tray] = false;
                goHomeAxis[EServoId.Clamp] = false;

                switch (servoId)
                {
                    case "X":
                        goHomeAxis[EServoId.X] = true;
                        axisStatus[EServoId.X] = EMotionStatus.Unknow;
                        break;
                    case "Y":
                        goHomeAxis[EServoId.Y] = true;
                        axisStatus[EServoId.Y] = EMotionStatus.Unknow;
                        break;
                    case "Z":
                        goHomeAxis[EServoId.Z] = true;
                        axisStatus[EServoId.Z] = EMotionStatus.Unknow;
                        break;
                    case "R":
                        goHomeAxis[EServoId.R] = true;
                        axisStatus[EServoId.R] = EMotionStatus.Unknow;
                        break;
                    case "TRAY":
                        goHomeAxis[EServoId.Tray] = true;
                        axisStatus[EServoId.Tray] = EMotionStatus.Unknow;
                        break;
                    case "CLAMP":
                        goHomeAxis[EServoId.Clamp] = true;
                        axisStatus[EServoId.Clamp] = EMotionStatus.Unknow;
                        break;
                }

                //epcio.SafetyPosition();

                await epcio.GoHome(
                    servoXGoHome: goHomeAxis[EServoId.X],
                    servoYGoHome: goHomeAxis[EServoId.Y],
                    servoZGoHome: goHomeAxis[EServoId.Z],
                    servoRGoHome: goHomeAxis[EServoId.R],
                    servoTrayGoHome: goHomeAxis[EServoId.Tray],
                    servoClampGoHome: goHomeAxis[EServoId.Clamp]);

                AfterGoHomeFinish();
            }
        }

        /// <summary>
        /// 全軸復歸
        /// </summary>
        private async void GoHomeAllServo()
        {
            if (!GoHomeInProcessing)
            {
                GoHomeInProcessing = true;
                pressGoHomeAbort = false;

                InGoHomeRunningEnabled = true;
                InGoHomeStopEnabled = false;

                goHomeAxis[EServoId.X] = true;
                goHomeAxis[EServoId.Y] = true;
                goHomeAxis[EServoId.Z] = true;
                goHomeAxis[EServoId.R] = true;
                goHomeAxis[EServoId.Tray] = true;
                goHomeAxis[EServoId.Clamp] = true;

                axisStatus[0] = EMotionStatus.Unknow;
                axisStatus[1] = EMotionStatus.Unknow;
                axisStatus[2] = EMotionStatus.Unknow;
                axisStatus[3] = EMotionStatus.Unknow;
                axisStatus[4] = EMotionStatus.Unknow;
                axisStatus[5] = EMotionStatus.Unknow;

                await epcio.GoHome();

                AfterGoHomeFinish();
            }
        }

        /// <summary>
        /// 中斷復歸
        /// </summary>
        private void GoHomeAbort()
        {
            pressGoHomeAbort = true;
            epcio.GoHomeAbort();
            AfterGoHomeFinish();
        }

        /// <summary>
        /// 復歸動作結束後處理
        /// </summary>
        private void AfterGoHomeFinish()
        {
            goHomeAxis[EServoId.X] = false;
            goHomeAxis[EServoId.Y] = false;
            goHomeAxis[EServoId.Z] = false;
            goHomeAxis[EServoId.R] = false;
            goHomeAxis[EServoId.Tray] = false;
            goHomeAxis[EServoId.Clamp] = false;

            InGoHomeRunningEnabled = false;
            InGoHomeStopEnabled = true;
            GoHomeInProcessing = false;
        }

        /// <summary>
        /// 伺服軸重置
        /// </summary>
        private void ServoReset()
            => epcio.ErrorReset();

        /********************
         * Polling
         *******************/
        /// <summary>
        /// 畫面顯示更新
        /// </summary>
        public void ScreenUpdatePolling(object sender, EventArgs e)
        {
            // 顯示命令座標
            double pos = epcio.ServoX.GetCurrentCommandPosition();
            if (coorCommandPosition[EServoId.X] != pos)
            {
                coorCommandPosition[EServoId.X] = pos;
                CommandPositionAxisX = pos;
            }

            pos = epcio.ServoY.GetCurrentCommandPosition();
            if (coorCommandPosition[EServoId.Y] != pos)
            {
                coorCommandPosition[EServoId.Y] = pos;
                CommandPositionAxisY = pos;
            }

            pos = epcio.ServoZ.GetCurrentCommandPosition();
            if (coorCommandPosition[EServoId.Z] != pos)
            {
                coorCommandPosition[EServoId.Z] = pos;
                CommandPositionAxisZ = pos;
            }

            pos = epcio.ServoR.GetCurrentCommandPosition();
            if (coorCommandPosition[EServoId.R] != pos)
            {
                coorCommandPosition[EServoId.R] = pos;
                CommandPositionAxisR = pos;
            }

            pos = epcio.ServoTray.GetCurrentCommandPosition();
            if (coorCommandPosition[EServoId.Tray] != pos)
            {
                coorCommandPosition[EServoId.Tray] = pos;
                CommandPositionAxisTray = pos;
            }

            pos = epcio.ServoClamp.GetCurrentCommandPosition();
            if (coorCommandPosition[EServoId.Clamp] != pos)
            {
                coorCommandPosition[EServoId.Clamp] = pos;
                CommandPositionAxisClamp = pos;
            }

            // 顯示反映座標
            pos = epcio.ServoX.GetCurrentPosition();
            if (coorPosition[EServoId.X] != pos)
            {
                coorPosition[EServoId.X] = pos;
                PositionAxisX = pos;
            }

            pos = epcio.ServoY.GetCurrentPosition();
            if (coorPosition[EServoId.Y] != pos)
            {
                coorPosition[EServoId.Y] = pos;
                PositionAxisY = pos;
            }

            pos = epcio.ServoZ.GetCurrentPosition();
            if (coorPosition[EServoId.Z] != pos)
            {
                coorPosition[EServoId.Z] = pos;
                PositionAxisZ = pos;
            }

            pos = epcio.ServoR.GetCurrentPosition();
            if (coorPosition[EServoId.R] != pos)
            {
                coorPosition[EServoId.R] = pos;
                PositionAxisR = Epcio.MmToDegree(pos);
            }

            pos = epcio.ServoTray.GetCurrentPosition();
            if (coorPosition[EServoId.Tray] != pos)
            {
                coorPosition[EServoId.Tray] = pos;
                PositionAxisTray = pos;
            }

            pos = epcio.ServoClamp.GetCurrentPosition();
            if (coorPosition[EServoId.Clamp] != pos)
            {
                coorPosition[EServoId.Clamp] = pos;
                PositionAxisClamp = pos;
            }

            // 顯示各軸Pluse
            long pulse = epcio.ServoX.GetCurrentPulse();
            if (coorPulse[EServoId.X] != pulse)
            {
                coorPulse[EServoId.X] = pulse;
                PulseAxisX = pulse;
            }

            pulse = epcio.ServoY.GetCurrentPulse();
            if (coorPulse[EServoId.Y] != pulse)
            {
                coorPulse[EServoId.Y] = pulse;
                PulseAxisY = pulse;
            }

            pulse = epcio.ServoZ.GetCurrentPulse();
            if (coorPulse[EServoId.Z] != pulse)
            {
                coorPulse[EServoId.Z] = pulse;
                PulseAxisZ = pulse;
            }

            pulse = epcio.ServoR.GetCurrentPulse();
            if (coorPulse[EServoId.R] != pulse)
            {
                coorPulse[EServoId.R] = pulse;
                PulseAxisR = pulse;
            }

            pulse = epcio.ServoTray.GetCurrentPulse();
            if (coorPulse[EServoId.Tray] != pulse)
            {
                coorPulse[EServoId.Tray] = pulse;
                PulseAxisTray = pulse;
            }

            pulse = epcio.ServoClamp.GetCurrentPulse();
            if (coorPulse[EServoId.Clamp] != pulse)
            {
                coorPulse[EServoId.Clamp] = pulse;
                PulseAxisClamp = pulse;
            }

            // 顯示各軸Encoder
            int enc = epcio.ServoX.GetCurrentEncoder();
            if (coorEncoder[EServoId.X] != enc)
            {
                coorEncoder[EServoId.X] = enc;
                EncAxisX = enc;
            }

            enc = epcio.ServoY.GetCurrentEncoder();
            if (coorEncoder[EServoId.Y] != enc)
            {
                coorEncoder[EServoId.Y] = enc;
                EncAxisY = enc;
            }

            enc = epcio.ServoZ.GetCurrentEncoder();
            if (coorEncoder[EServoId.Z] != enc)
            {
                coorEncoder[EServoId.Z] = enc;
                EncAxisZ = enc;
            }

            enc = epcio.ServoR.GetCurrentEncoder();
            if (coorEncoder[EServoId.R] != enc)
            {
                coorEncoder[EServoId.R] = enc;
                EncAxisR = enc;
            }

            enc = epcio.ServoTray.GetCurrentEncoder();
            if (coorEncoder[EServoId.Tray] != enc)
            {
                coorEncoder[EServoId.Tray] = enc;
                EncAxisTray = enc;
            }

            enc = epcio.ServoClamp.GetCurrentEncoder();
            if (coorEncoder[EServoId.Clamp] != enc)
            {
                coorEncoder[EServoId.Clamp] = enc;
                EncAxisClamp = enc;
            }

            // 顯示復歸軸狀態
            foreach (var servo in epcio.ServoList)
            {
                int id = servo.Id;

                EMotionStatus ms = servo.GetMotionStatus();
                if (axisStatus[id] != ms)
                {
                    // 狀態有變動
                    axisStatus[id] = ms;
                    string msg = ms switch
                    {
                        EMotionStatus.Stop => msgAtHome,
                        EMotionStatus.GoingHome => msgGoingHome,
                        EMotionStatus.MoveAwayFromHome => msgCheckHomeSensor,
                        EMotionStatus.MoveAwayFromHomeFinished => msgCheckHomeSensorFinished,
                        EMotionStatus.GoHomeAbort => msgGoHomeAbort,
                        EMotionStatus.Error => msgError,
                        EMotionStatus.ErrorReset => msgErrorReset,
                        _ => string.Empty,
                    };

                    switch (id)
                    {
                        case EServoId.X:
                            StatusAxisX = msg;
                            break;

                        case EServoId.Y:
                            StatusAxisY = msg;
                            break;

                        case EServoId.Z:
                            StatusAxisZ = msg;
                            break;

                        case EServoId.R:
                            StatusAxisR = msg;
                            break;

                        case EServoId.Tray:
                            StatusAxisTray = msg;
                            break;

                        case EServoId.Clamp:
                            StatusAxisClamp = msg;
                            break;
                    }
                }
            }
        }

        /********************
         * 繫結
         *******************/
        // 命令座標
        private double _commandPositionAxisX;
        public double CommandPositionAxisX
        {
            get { return _commandPositionAxisX; }
            set { SetProperty(ref _commandPositionAxisX, value); }
        }

        private double _commandPositionAxisY;
        public double CommandPositionAxisY
        {
            get { return _commandPositionAxisY; }
            set { SetProperty(ref _commandPositionAxisY, value); }
        }

        private double _commandPositionAxisZ;
        public double CommandPositionAxisZ
        {
            get { return _commandPositionAxisZ; }
            set { SetProperty(ref _commandPositionAxisZ, value); }
        }

        private double _commandPositionAxisR;
        public double CommandPositionAxisR
        {
            get { return _commandPositionAxisR; }
            set { SetProperty(ref _commandPositionAxisR, value); }
        }

        private double _commandPositionAxisTray;
        public double CommandPositionAxisTray
        {
            get { return _commandPositionAxisTray; }
            set { SetProperty(ref _commandPositionAxisTray, value); }
        }

        private double _commandPositionAxisClamp;
        public double CommandPositionAxisClamp
        {
            get { return _commandPositionAxisClamp; }
            set { SetProperty(ref _commandPositionAxisClamp, value); }
        }

        // 反映座標
        private double _positionAxisX;
        public double PositionAxisX
        {
            get { return _positionAxisX; }
            set { SetProperty(ref _positionAxisX, value); }
        }

        private double _positionAxisY;
        public double PositionAxisY
        {
            get { return _positionAxisY; }
            set { SetProperty(ref _positionAxisY, value); }
        }

        private double _positionAxisZ;
        public double PositionAxisZ
        {
            get { return _positionAxisZ; }
            set { SetProperty(ref _positionAxisZ, value); }
        }

        private double _positionAxisR;
        public double PositionAxisR
        {
            get { return _positionAxisR; }
            set { SetProperty(ref _positionAxisR, value); }
        }

        private double _positionAxisTray;
        public double PositionAxisTray
        {
            get { return _positionAxisTray; }
            set { SetProperty(ref _positionAxisTray, value); }
        }

        private double _positionAxisClamp;
        public double PositionAxisClamp
        {
            get { return _positionAxisClamp; }
            set { SetProperty(ref _positionAxisClamp, value); }
        }

        // Pulse
        private long _pulseAxisX;
        public long PulseAxisX
        {
            get { return _pulseAxisX; }
            set { SetProperty(ref _pulseAxisX, value); }
        }

        private long _pulseAxisY;
        public long PulseAxisY
        {
            get { return _pulseAxisY; }
            set { SetProperty(ref _pulseAxisY, value); }
        }

        private long _pulseAxisZ;
        public long PulseAxisZ
        {
            get { return _pulseAxisZ; }
            set { SetProperty(ref _pulseAxisZ, value); }
        }

        private long _pulseAxisR;
        public long PulseAxisR
        {
            get { return _pulseAxisR; }
            set { SetProperty(ref _pulseAxisR, value); }
        }

        private long _pulseAxisTray;
        public long PulseAxisTray
        {
            get { return _pulseAxisTray; }
            set { SetProperty(ref _pulseAxisTray, value); }
        }

        private long _pulseAxisClamp;
        public long PulseAxisClamp
        {
            get { return _pulseAxisClamp; }
            set { SetProperty(ref _pulseAxisClamp, value); }
        }

        // Encoder
        private int _encAxisX;
        public int EncAxisX
        {
            get { return _encAxisX; }
            set { SetProperty(ref _encAxisX, value); }
        }

        private int _encAxisY;
        public int EncAxisY
        {
            get { return _encAxisY; }
            set { SetProperty(ref _encAxisY, value); }
        }

        private int _encAxisZ;
        public int EncAxisZ
        {
            get { return _encAxisZ; }
            set { SetProperty(ref _encAxisZ, value); }
        }

        private int _encAxisR;
        public int EncAxisR
        {
            get { return _encAxisR; }
            set { SetProperty(ref _encAxisR, value); }
        }

        private int _encAxisTray;
        public int EncAxisTray
        {
            get { return _encAxisTray; }
            set { SetProperty(ref _encAxisTray, value); }
        }

        private int _encAxisClamp;
        public int EncAxisClamp
        {
            get { return _encAxisClamp; }
            set { SetProperty(ref _encAxisClamp, value); }
        }


        private string _statusAxisX;
        public string StatusAxisX
        {
            get { return _statusAxisX; }
            set { SetProperty(ref _statusAxisX, value); }
        }

        private string _statusAxisY;
        public string StatusAxisY
        {
            get { return _statusAxisY; }
            set { SetProperty(ref _statusAxisY, value); }
        }

        private string _statusAxisZ;
        public string StatusAxisZ
        {
            get { return _statusAxisZ; }
            set { SetProperty(ref _statusAxisZ, value); }
        }

        private string _statusAxisR;
        public string StatusAxisR
        {
            get { return _statusAxisR; }
            set { SetProperty(ref _statusAxisR, value); }
        }

        private string _statusAxisTray;
        public string StatusAxisTray
        {
            get { return _statusAxisTray; }
            set { SetProperty(ref _statusAxisTray, value); }
        }

        private string _statusAxisClamp;
        public string StatusAxisClamp
        {
            get { return _statusAxisClamp; }
            set { SetProperty(ref _statusAxisClamp, value); }
        }

        /*****/

        private bool _inGoHomeRunningEnabled;
        public bool InGoHomeRunningEnabled
        {
            get { return _inGoHomeRunningEnabled; }
            set { SetProperty(ref _inGoHomeRunningEnabled, value); }
        }

        private bool _inGoHomeStopEnabled;
        public bool InGoHomeStopEnabled
        {
            get { return _inGoHomeStopEnabled; }
            set { SetProperty(ref _inGoHomeStopEnabled, value); }
        }
    }
}
