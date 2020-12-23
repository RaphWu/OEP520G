using EPCIO;
using OEP520G.Core;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace OEP520G.Manual.ViewModels
{
    public class ServoTesterViewModel : BindableBase, IActiveAware
    {
        private readonly Epcio epcio = Epcio.Instance;

        // 取消權杖
        private CancellationTokenSource cts = null;

        /// <summary>
        /// 運動控制卡座標讀取Timer
        /// </summary>
        public System.Timers.Timer PositionReader = new System.Timers.Timer
        {
            Interval = 250
        };

        // 移動至...
        private bool RepeatAxisCheckedX = false;
        private bool RepeatAxisCheckedY = false;
        private bool RepeatAxisCheckedZ = false;
        private bool RepeatAxisCheckedTray = false;
        private bool RepeatAxisCheckedClamp = false;

        public DelegateCommand<string> MoveToCommand { get; private set; }
        public DelegateCommand<string> MoveAllToCommand { get; private set; }

        // 往返測試
        public DelegateCommand RepeatTestStartCommand { get; private set; }
        public DelegateCommand RepeatTestFinishCommand { get; private set; }
        public DelegateCommand<string> GetCoorCommand { get; private set; }

        // 視窗Active
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
                    SetSpeed();

                    epcio.SafetyPosition();

                    // 啟動座標讀取Timer
                    PositionReader.Start();
                }
                else
                {
                    // 離開畫面就停止測試
                    RepeatTestFinish();

                    // 停止座標讀取Timer
                    PositionReader.Stop();
                }
            }
        }

        // 事件聚合器引用
        private readonly IEventAggregator _ea;

        /// <summary>
        /// 建構函式
        /// </summary>
        public ServoTesterViewModel(IEventAggregator ea)
        {
            _ea = ea;

            // 基本參數
            //Interlinkage = false;
            SpeedSelected = "MIDDLE";

            // 各滑桿極限值
            AxisXMax = epcio.ServoX.MaxPosition;
            AxisXMin = epcio.ServoX.MinPosition;
            AxisYMax = epcio.ServoY.MaxPosition;
            AxisYMin = epcio.ServoY.MinPosition;
            AxisZMax = epcio.ServoZ.MaxPosition;
            AxisZMin = epcio.ServoZ.MinPosition;
            AxisTrayMax = epcio.ServoTray.MaxPosition;
            AxisTrayMin = epcio.ServoTray.MinPosition;
            AxisClampMax = epcio.ServoClamp.MaxPosition;
            AxisClampMin = epcio.ServoClamp.MinPosition;

            // 按鍵
            ButtonContentX = epcio.ServoX.Title + "移動";
            ButtonContentY = epcio.ServoY.Title + "移動";
            ButtonContentZ = epcio.ServoZ.Title + "移動";
            ButtonContentR = epcio.ServoR.Title + "移動";
            ButtonContentTray = epcio.ServoTray.Title + "移動";
            ButtonContentClamp = epcio.ServoClamp.Title + "移動";

            // 往返測試
            RepeatTestSwicth = false;
            RepeatTestTimes = 0;
            PauseTimes = 500;

            TestAxisSelected = "X";

            RepeatAxisEnabledX = true;
            RepeatAxisEnabledY = true;
            RepeatAxisEnabledZ = true;
            RepeatAxisEnabledTray = true;
            RepeatAxisEnabledClamp = true;

            RepeatStartButtonEnabled = true;
            StopTestButtonEnabled = false;

            // 註冊座標讀取Timer
            PositionReader.Elapsed += new ElapsedEventHandler(UpdateAxisCoor);

            // 移動至...
            MoveToCommand = new DelegateCommand<string>(MoveTo);
            MoveAllToCommand = new DelegateCommand<string>(MoveAllTo);

            // 往返測試
            RepeatTestStartCommand = new DelegateCommand(RepeatTestStart);
            RepeatTestFinishCommand = new DelegateCommand(RepeatTestFinish);
            GetCoorCommand = new DelegateCommand<string>(GetPosition);
        }

        /********************
         * 座標軸實際位置顯示
         *******************/
        public void UpdateAxisCoor(object sender, EventArgs e)
        {
            var servo = epcio.ServoX;
            CommandPositionAxisX = servo.GetCurrentCommandPosition().ToString("F3");
            PositionAxisX = servo.GetCurrentPosition().ToString("F3");
            PulseAxisX = servo.GetCurrentPulse().ToString("0");
            EncAxisX = servo.GetCurrentEncoder().ToString("0");

            servo = epcio.ServoY;
            CommandPositionAxisY = servo.GetCurrentCommandPosition().ToString("F3");
            PositionAxisY = servo.GetCurrentPosition().ToString("F3");
            PulseAxisY = servo.GetCurrentPulse().ToString("0");
            EncAxisY = servo.GetCurrentEncoder().ToString("0");

            servo = epcio.ServoZ;
            CommandPositionAxisZ = servo.GetCurrentCommandPosition().ToString("F3");
            PositionAxisZ = servo.GetCurrentPosition().ToString("F3");
            PulseAxisZ = servo.GetCurrentPulse().ToString("0");
            EncAxisZ = servo.GetCurrentEncoder().ToString("0");

            servo = epcio.ServoR;
            CommandPositionAxisR = Epcio.MmToDegree(servo.GetCurrentCommandPosition()).ToString("F1");
            PositionAxisR = Epcio.MmToDegree(servo.GetCurrentPosition()).ToString("F1");
            PulseAxisR = servo.GetCurrentPulse().ToString("0");
            EncAxisR = servo.GetCurrentEncoder().ToString("0");

            servo = epcio.ServoTray;
            CommandPositionAxisTray = servo.GetCurrentCommandPosition().ToString("F3");
            PositionAxisTray = servo.GetCurrentPosition().ToString("F3");
            PulseAxisTray = servo.GetCurrentPulse().ToString("0");
            EncAxisTray = servo.GetCurrentEncoder().ToString("0");

            servo = epcio.ServoClamp;
            CommandPositionAxisClamp = servo.GetCurrentCommandPosition().ToString("F3");
            PositionAxisClamp = servo.GetCurrentPosition().ToString("F3");
            PulseAxisClamp = servo.GetCurrentPulse().ToString("0");
            EncAxisClamp = servo.GetCurrentEncoder().ToString("0");
        }

        /********************
         * 參數設定
         *******************/
        /// <summary>
        /// 設定軸速度百分比
        /// </summary>
        private void SetSpeed()
        {
            switch (SpeedSelected)
            {
                case "ULTRA_HIGH":
                    epcio.SetSpeed(EServoSpeed.UltraHigh);
                    break;
                case "HIGH":
                    epcio.SetSpeed(EServoSpeed.High);
                    break;
                case "LOW":
                    epcio.SetSpeed(EServoSpeed.Low);
                    break;
                case "ULTRA_LOW":
                    epcio.SetSpeed(EServoSpeed.UltraLow);
                    break;
                default:
                    epcio.SetSpeed(EServoSpeed.Middle);
                    break;
            }
        }

        /********************
         * 移動至...
         *******************/
        /// <summary>
        /// 移動指定軸至指定座標
        /// </summary>
        /// <param name="paras">
        /// 格式=>"軸,移動位置"
        /// [0]:軸名稱
        /// [1]:移動位置=>起點(START) 終點(END) 設定值(SETTING/省略)
        /// </param>
        private void MoveTo(string paras)
        {
            RepeatStartButtonEnabled = false;
            StopTestButtonEnabled = true;

            // 參數解析
            string[] para = paras.Split(",");

            double pos = default;

            if (para[0] == "SafetyZ")
            {
                pos = epcio.SafetyZ;
                MoveToCoorZ = pos;

                paras = "Z,SETTING";
                para = paras.Split(",");
            }

            //if ((para[0] == "Z") || (servos[Motion.AXIS_ID_Z].CurrentPosition <= epcio.SafetyZ))
            //{
            switch (para[0])
            {
                case "X":
                    if (para[1] == "START")
                        pos = RepeatMoveToStartX;
                    else if (para[1] == "END")
                        pos = RepeatMoveToEndX;
                    else
                        pos = MoveToCoorX;

                    epcio.MoveTo(positionX: pos);
                    break;

                case "Y":
                    if (para[1] == "START")
                        pos = RepeatMoveToStartY;
                    else if (para[1] == "END")
                        pos = RepeatMoveToEndY;
                    else
                        pos = MoveToCoorY;

                    epcio.MoveTo(positionY: pos);
                    break;

                case "Z":
                    if (para[1] == "START")
                        pos = RepeatMoveToStartZ;
                    else if (para[1] == "END")
                        pos = RepeatMoveToEndZ;
                    else
                        pos = MoveToCoorZ;

                    epcio.MoveTo(positionZ: pos);
                    break;

                case "R":
                    if (para[1] == "START")
                        epcio.MoveTo(degreeR: 0);
                    else
                        epcio.MoveTo(degreeR: (float)MoveToCoorR);
                    break;

                case "TRAY":
                    if (para[1] == "START")
                        pos = RepeatMoveToStartTray;
                    else if (para[1] == "END")
                        pos = RepeatMoveToEndTray;
                    else
                        pos = MoveToCoorTray;

                    epcio.MoveTo(positionTray: pos);
                    break;

                case "CLAMP":
                    if (para[1] == "START")
                        pos = RepeatMoveToStartClamp;
                    else if (para[1] == "END")
                        pos = RepeatMoveToEndClamp;
                    else
                        pos = MoveToCoorClamp;

                    epcio.MoveTo(positionClamp: pos);
                    break;

                default:
                    return;
            }

            RepeatStartButtonEnabled = true;
            StopTestButtonEnabled = false;
        }

        /// <summary>
        /// 全部軸移動至指定座標
        /// </summary>
        private void MoveAllTo(string para)
        {
            if (MoveToCoorZ <= epcio.SafetyZ)
            {
                MoveTo($"X,{para}");
                MoveTo($"Y,{para}");
                MoveTo($"R,{para}");
                MoveTo($"TRAY,{para}");
                MoveTo($"CLAMP,{para}");
            }
        }

        /********************
         * 往返測試動作
         *******************/
        /// <summary>
        /// 往返測試停止
        /// </summary>
        private void RepeatTestFinish()
        {
            if (cts != null && !cts.IsCancellationRequested)
                cts.Cancel();
        }

        /// <summary>
        /// 往返測試啟動
        /// </summary>
        private async void RepeatTestStart()
        {
            RepeatStartButtonEnabled = false;
            StopTestButtonEnabled = true;

            cts = null;
            cts = new CancellationTokenSource();

            await Task.Run(() => RepeatTestTask());
        }

        //private void RepeatTestReady()
        //    => Task.Run(() => RepeatTestTask());

        /// <summary>
        /// 往返測試啟動(新執行緒部分)
        /// </summary>
        private async void RepeatTestTask()
        {
            // message
            AxisCounterX = "";
            AxisCounterY = "";
            AxisCounterZ = "";
            AxisCounterTray = "";
            AxisCounterClamp = "";
            string msgFinished = "結束";

            // 移動次數計數器
            int MovesRemainderX = RepeatAxisCheckedX ? 0 : RepeatTestTimes;
            int MovesRemainderY = RepeatAxisCheckedY ? 0 : RepeatTestTimes;
            int MovesRemainderZ = RepeatAxisCheckedZ ? 0 : RepeatTestTimes;
            int MovesRemainderTray = RepeatAxisCheckedTray ? 0 : RepeatTestTimes;
            int MovesRemainderClamp = RepeatAxisCheckedClamp ? 0 : RepeatTestTimes;

            // start position
            double NextPosX = epcio.ServoX.GetCurrentPosition();
            double NextPosY = epcio.ServoY.GetCurrentPosition();
            double NextPosZ = epcio.ServoZ.GetCurrentPosition();
            double NextPosTray = epcio.ServoTray.GetCurrentPosition();
            double NextPosClamp = epcio.ServoClamp.GetCurrentPosition();

            await Task.Run(() =>
            {
                bool thereAreAxisInTest = true;
                try
                {
                    while (thereAreAxisInTest)
                    {
                        // 作業是否取消
                        if (cts.Token.IsCancellationRequested)
                            cts.Token.ThrowIfCancellationRequested();

                        // 是否還有軸未測試完
                        thereAreAxisInTest = false;

                        // X軸
                        if (RepeatAxisCheckedX)
                            if ((RepeatTestTimes < 0) || (MovesRemainderX < RepeatTestTimes))
                            {
                                if (epcio.IsMotionStop(checkServoX: true))
                                {
                                    Task.Delay(PauseTimes).Wait();
                                    NextPosX = (Math.Floor(NextPosX) == Math.Floor(RepeatMoveToEndX))
                                        ? RepeatMoveToStartX : RepeatMoveToEndX;
                                    epcio.MoveTo(positionX: NextPosX);
                                    AxisCounterX = (++MovesRemainderX).ToString();
                                }
                                thereAreAxisInTest = true;
                            }
                            else if (MovesRemainderX == RepeatTestTimes)
                            {
                                AxisCounterX = msgFinished;
                                MovesRemainderX++;
                            }

                        // Y軸
                        if (RepeatAxisCheckedY)
                            if ((RepeatTestTimes < 0) || (MovesRemainderY < RepeatTestTimes))
                            {
                                if (epcio.IsMotionStop(checkServoX: true))
                                {
                                    Task.Delay(PauseTimes).Wait();
                                    NextPosY = (Math.Floor(NextPosY) == Math.Floor(RepeatMoveToEndY))
                                        ? RepeatMoveToStartY : RepeatMoveToEndY;
                                    epcio.MoveTo(positionY: NextPosY);
                                    AxisCounterY = (++MovesRemainderY).ToString();
                                }
                                thereAreAxisInTest = true;
                            }
                            else if (MovesRemainderY == RepeatTestTimes)
                            {
                                AxisCounterY = msgFinished;
                                MovesRemainderY++;
                            }

                        // Z軸
                        if (RepeatAxisCheckedZ)
                            if ((RepeatTestTimes < 0) || (MovesRemainderZ < RepeatTestTimes))
                            {
                                if (epcio.IsMotionStop(checkServoZ: true))
                                {
                                    Task.Delay(PauseTimes).Wait();
                                    NextPosZ = (Math.Floor(NextPosZ) == Math.Floor(RepeatMoveToEndZ))
                                        ? RepeatMoveToStartZ : RepeatMoveToEndZ;
                                    epcio.MoveTo(positionZ: NextPosZ);
                                    AxisCounterZ = (++MovesRemainderZ).ToString();
                                }
                                thereAreAxisInTest = true;
                            }
                            else if (MovesRemainderZ == RepeatTestTimes)
                            {
                                AxisCounterZ = msgFinished;
                                MovesRemainderZ++;
                            }

                        // Tray軸
                        if (RepeatAxisCheckedTray)
                            if ((RepeatTestTimes < 0) || (MovesRemainderTray < RepeatTestTimes))
                            {
                                if (epcio.IsMotionStop(checkServoTray: true))
                                {
                                    Task.Delay(PauseTimes).Wait();
                                    NextPosTray = (Math.Floor(NextPosTray) == Math.Floor(RepeatMoveToEndTray))
                                        ? RepeatMoveToStartTray : RepeatMoveToEndTray;
                                    epcio.MoveTo(positionTray: NextPosTray);
                                    AxisCounterTray = (++MovesRemainderTray).ToString();
                                }
                                thereAreAxisInTest = true;
                            }
                            else if (MovesRemainderTray == RepeatTestTimes)
                            {
                                AxisCounterTray = msgFinished;
                                MovesRemainderTray++;
                            }

                        // Clamp軸
                        if (RepeatAxisCheckedClamp)
                            if ((RepeatTestTimes < 0) || (MovesRemainderClamp < RepeatTestTimes))
                            {
                                if (epcio.IsMotionStop(checkServoClamp: true))
                                {
                                    Task.Delay(PauseTimes).Wait();
                                    NextPosClamp = (Math.Floor(NextPosClamp) == Math.Floor(RepeatMoveToEndClamp))
                                        ? RepeatMoveToStartClamp : RepeatMoveToEndClamp;
                                    epcio.MoveTo(positionClamp: NextPosClamp);
                                    AxisCounterClamp = (++MovesRemainderClamp).ToString();
                                }
                                thereAreAxisInTest = true;
                            }
                            else if (MovesRemainderClamp == RepeatTestTimes)
                            {
                                AxisCounterClamp = msgFinished;
                                MovesRemainderClamp++;
                            }
                    }
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                }
            });

            await epcio.WaitingForAllServoMotionStop();
            RepeatStartButtonEnabled = true;
            StopTestButtonEnabled = false;
        }

        /********************
         * UI操作
         *******************/
        /// <summary>
        /// 使用滑鼠右鍵，取得特定座標值
        /// </summary>
        /// <param name="paras">
        /// 格式=>"軸,呼叫者,取得何值"
        /// [0]:軸名稱
        /// [1]:呼叫者=>起點(START) 終點(END) 設定值(SETTING)
        /// [2]:取得何值=>軸座標(AXIS) 起點(START) 終點(END) 設定值(SETTING/省略)
        /// </param>
        private void GetPosition(string paras)
        {
            // 參數解析
            string[] para = paras.Split(",");

            double pos;

            // 觸發滑鼠事件的軸
            switch (para[0])
            {
                case "X":
                    if (para[2] == "START")
                        pos = RepeatMoveToStartX;
                    else if (para[2] == "END")
                        pos = RepeatMoveToEndX;
                    else if (para[2] == "AXIS")
                        pos = epcio.ServoX.GetCurrentPosition();
                    else // SETTING
                        pos = MoveToCoorX;

                    if (para[1] == "START")
                        RepeatMoveToStartX = pos;
                    else if (para[1] == "END")
                        RepeatMoveToEndX = pos;
                    else
                        MoveToCoorX = pos;

                    break;
                case "Y":
                    if (para[2] == "START")
                        pos = RepeatMoveToStartY;
                    else if (para[2] == "END")
                        pos = RepeatMoveToEndY;
                    else if (para[2] == "AXIS")
                        pos = epcio.ServoY.GetCurrentPosition();
                    else
                        pos = MoveToCoorY;

                    if (para[1] == "START")
                        RepeatMoveToStartY = pos;
                    else if (para[1] == "END")
                        RepeatMoveToEndY = pos;
                    else
                        MoveToCoorY = pos;

                    break;
                case "Z":
                    if (para[2] == "START")
                        pos = RepeatMoveToStartZ;
                    else if (para[2] == "END")
                        pos = RepeatMoveToEndZ;
                    else if (para[2] == "AXIS")
                        pos = epcio.ServoZ.GetCurrentPosition();
                    else
                        pos = MoveToCoorZ;

                    if (para[1] == "START")
                        RepeatMoveToStartZ = pos;
                    else if (para[1] == "END")
                        RepeatMoveToEndZ = pos;
                    else
                        MoveToCoorZ = pos;

                    break;
                case "R":
                    MoveToCoorR = epcio.ServoR.GetCurrentPosition();
                    break;
                case "TRAY":
                    if (para[2] == "START")
                        pos = RepeatMoveToStartTray;
                    else if (para[2] == "END")
                        pos = RepeatMoveToEndTray;
                    else if (para[2] == "AXIS")
                        pos = epcio.ServoTray.GetCurrentPosition();
                    else
                        pos = MoveToCoorTray;

                    if (para[1] == "START")
                        RepeatMoveToStartTray = pos;
                    else if (para[1] == "END")
                        RepeatMoveToEndTray = pos;
                    else
                        MoveToCoorTray = pos;

                    break;
                case "CLAMP":
                    if (para[2] == "START")
                        pos = RepeatMoveToStartClamp;
                    else if (para[2] == "END")
                        pos = RepeatMoveToEndClamp;
                    else if (para[2] == "AXIS")
                        pos = epcio.ServoClamp.GetCurrentPosition();
                    else
                        pos = MoveToCoorClamp;

                    if (para[1] == "START")
                        RepeatMoveToStartClamp = pos;
                    else if (para[1] == "END")
                        RepeatMoveToEndClamp = pos;
                    else
                        MoveToCoorClamp = pos;

                    break;
            }
        }

        /********************
         * 繫結
         *******************/
        private string _speedSelected;
        public string SpeedSelected
        {
            get { return _speedSelected; }
            set
            {
                SetProperty(ref _speedSelected, value);
                SetSpeed();
            }
        }

        // Position命令值
        private string _commandPositionAxisX;
        public string CommandPositionAxisX
        {
            get { return _commandPositionAxisX; }
            set { SetProperty(ref _commandPositionAxisX, value); }
        }

        private string _commandPositionAxisY;
        public string CommandPositionAxisY
        {
            get { return _commandPositionAxisY; }
            set { SetProperty(ref _commandPositionAxisY, value); }
        }

        private string _commandPositionAxisZ;
        public string CommandPositionAxisZ
        {
            get { return _commandPositionAxisZ; }
            set { SetProperty(ref _commandPositionAxisZ, value); }
        }

        private string _commandPositionAxisR;
        public string CommandPositionAxisR
        {
            get { return _commandPositionAxisR; }
            set { SetProperty(ref _commandPositionAxisR, value); }
        }

        private string _commandPositionAxisTray;
        public string CommandPositionAxisTray
        {
            get { return _commandPositionAxisTray; }
            set { SetProperty(ref _commandPositionAxisTray, value); }
        }

        private string _commandPositionAxisClamp;
        public string CommandPositionAxisClamp
        {
            get { return _commandPositionAxisClamp; }
            set { SetProperty(ref _commandPositionAxisClamp, value); }
        }

        // Position反映值
        private string _positionAxisX;
        public string PositionAxisX
        {
            get { return _positionAxisX; }
            set { SetProperty(ref _positionAxisX, value); }
        }

        private string _positionAxisY;
        public string PositionAxisY
        {
            get { return _positionAxisY; }
            set { SetProperty(ref _positionAxisY, value); }
        }

        private string _positionAxisZ;
        public string PositionAxisZ
        {
            get { return _positionAxisZ; }
            set { SetProperty(ref _positionAxisZ, value); }
        }

        private string _positionAxisR;
        public string PositionAxisR
        {
            get { return _positionAxisR; }
            set { SetProperty(ref _positionAxisR, value); }
        }

        private string _positionAxisTray;
        public string PositionAxisTray
        {
            get { return _positionAxisTray; }
            set { SetProperty(ref _positionAxisTray, value); }
        }

        private string _positionAxisClamp;
        public string PositionAxisClamp
        {
            get { return _positionAxisClamp; }
            set { SetProperty(ref _positionAxisClamp, value); }
        }

        // Pulse
        private string _pulseAxisX;
        public string PulseAxisX
        {
            get { return _pulseAxisX; }
            set { SetProperty(ref _pulseAxisX, value); }
        }

        private string _pulseAxisY;
        public string PulseAxisY
        {
            get { return _pulseAxisY; }
            set { SetProperty(ref _pulseAxisY, value); }
        }

        private string _pulseAxisZ;
        public string PulseAxisZ
        {
            get { return _pulseAxisZ; }
            set { SetProperty(ref _pulseAxisZ, value); }
        }

        private string _pulseAxisR;
        public string PulseAxisR
        {
            get { return _pulseAxisR; }
            set { SetProperty(ref _pulseAxisR, value); }
        }

        private string _pulseAxisTray;
        public string PulseAxisTray
        {
            get { return _pulseAxisTray; }
            set { SetProperty(ref _pulseAxisTray, value); }
        }

        private string _pulseAxisClamp;
        public string PulseAxisClamp
        {
            get { return _pulseAxisClamp; }
            set { SetProperty(ref _pulseAxisClamp, value); }
        }

        // Encoder
        private string _encAxisX;
        public string EncAxisX
        {
            get { return _encAxisX; }
            set { SetProperty(ref _encAxisX, value); }
        }

        private string _encAxisY;
        public string EncAxisY
        {
            get { return _encAxisY; }
            set { SetProperty(ref _encAxisY, value); }
        }

        private string _encAxisZ;
        public string EncAxisZ
        {
            get { return _encAxisZ; }
            set { SetProperty(ref _encAxisZ, value); }
        }

        private string _encAxisR;
        public string EncAxisR
        {
            get { return _encAxisR; }
            set { SetProperty(ref _encAxisR, value); }
        }

        private string _encAxisTray;
        public string EncAxisTray
        {
            get { return _encAxisTray; }
            set { SetProperty(ref _encAxisTray, value); }
        }

        private string _encAxisClamp;
        public string EncAxisClamp
        {
            get { return _encAxisClamp; }
            set { SetProperty(ref _encAxisClamp, value); }
        }

        // X
        private double _moveToCoorX;
        public double MoveToCoorX
        {
            get { return _moveToCoorX; }
            set { SetProperty(ref _moveToCoorX, value); }
        }
        // TODO: 安全極限值判斷

        private double _repeatMoveToStartX;
        public double RepeatMoveToStartX
        {
            get { return _repeatMoveToStartX; }
            set { SetProperty(ref _repeatMoveToStartX, value); }
        }

        private double _repeatMoveToEndX;
        public double RepeatMoveToEndX
        {
            get { return _repeatMoveToEndX; }
            set { SetProperty(ref _repeatMoveToEndX, value); }
        }

        // Y
        private double _moveToCoorY;
        public double MoveToCoorY
        {
            get { return _moveToCoorY; }
            set { SetProperty(ref _moveToCoorY, value); }
        }

        private double _repeatMoveToStartY;
        public double RepeatMoveToStartY
        {
            get { return _repeatMoveToStartY; }
            set { SetProperty(ref _repeatMoveToStartY, value); }
        }

        private double _repeatMoveToEndY;
        public double RepeatMoveToEndY
        {
            get { return _repeatMoveToEndY; }
            set { SetProperty(ref _repeatMoveToEndY, value); }
        }

        // Z
        private double _moveToCoorZ;
        public double MoveToCoorZ
        {
            get { return _moveToCoorZ; }
            set { SetProperty(ref _moveToCoorZ, value); }
        }

        private double _repeatMoveToStartZ;
        public double RepeatMoveToStartZ
        {
            get { return _repeatMoveToStartZ; }
            set { SetProperty(ref _repeatMoveToStartZ, value); }
        }

        private double _repeatMoveToEndZ;
        public double RepeatMoveToEndZ
        {
            get { return _repeatMoveToEndZ; }
            set { SetProperty(ref _repeatMoveToEndZ, value); }
        }

        // R
        private double _moveToCoorR;
        public double MoveToCoorR
        {
            get { return _moveToCoorR; }
            set { SetProperty(ref _moveToCoorR, value); }
        }

        //private double _repeatMoveToStartR;
        //public double RepeatMoveToStartR
        //{
        //    get { return _repeatMoveToStartR; }
        //    set { SetProperty(ref _repeatMoveToStartR, value); }
        //}

        //private double _repeatMoveToEndR;
        //public double RepeatMoveToEndR
        //{
        //    get { return _repeatMoveToEndR; }
        //    set { SetProperty(ref _repeatMoveToEndR, value); }
        //}

        // Tray
        private double _moveToCoorTray;
        public double MoveToCoorTray
        {
            get { return _moveToCoorTray; }
            set { SetProperty(ref _moveToCoorTray, value); }
        }

        private double _repeatMoveToStartTray;
        public double RepeatMoveToStartTray
        {
            get { return _repeatMoveToStartTray; }
            set { SetProperty(ref _repeatMoveToStartTray, value); }
        }

        private double _repeatMoveToEndTray;
        public double RepeatMoveToEndTray
        {
            get { return _repeatMoveToEndTray; }
            set { SetProperty(ref _repeatMoveToEndTray, value); }
        }

        // Clamp
        private double _moveToCoorClamp;
        public double MoveToCoorClamp
        {
            get { return _moveToCoorClamp; }
            set { SetProperty(ref _moveToCoorClamp, value); }
        }

        private double _repeatMoveToStartClamp;
        public double RepeatMoveToStartClamp
        {
            get { return _repeatMoveToStartClamp; }
            set { SetProperty(ref _repeatMoveToStartClamp, value); }
        }

        private double _repeatMoveToEndClamp;
        public double RepeatMoveToEndClamp
        {
            get { return _repeatMoveToEndClamp; }
            set { SetProperty(ref _repeatMoveToEndClamp, value); }
        }

        // 啟動開關
        private bool _positioningAccuracyTestSwicth;
        public bool RepeatTestSwicth
        {
            get { return _positioningAccuracyTestSwicth; }
            set { SetProperty(ref _positioningAccuracyTestSwicth, value); }
        }

        // 重覆次數
        private int _pATRepeatTimes;
        public int RepeatTestTimes
        {
            get { return _pATRepeatTimes; }
            set { SetProperty(ref _pATRepeatTimes, value); }
        }

        // 各軸開關
        /// <summary>
        /// 確認測試軸
        /// </summary>
        private void checkTestAxis()
        {
            RepeatAxisCheckedX = false;
            RepeatAxisCheckedY = false;
            RepeatAxisCheckedZ = false;
            RepeatAxisCheckedTray = false;
            RepeatAxisCheckedClamp = false;

            switch (TestAxisSelected)
            {
                case "X":
                    RepeatAxisCheckedX = true;
                    break;
                case "Y":
                    RepeatAxisCheckedY = true;
                    break;
                case "Z":
                    RepeatAxisCheckedZ = true;
                    break;
                case "TRAY":
                    RepeatAxisCheckedTray = true;
                    break;
                case "CLAMP":
                    RepeatAxisCheckedClamp = true;
                    break;
            }
        }

        private string _testAxisSelected;
        public string TestAxisSelected
        {
            get { return _testAxisSelected; }
            set
            {
                SetProperty(ref _testAxisSelected, value);
                checkTestAxis();
            }
        }

        //private bool _repeatAxisCheckedX;
        //public bool RepeatAxisCheckedX
        //{
        //    get { return _repeatAxisCheckedX; }
        //    set
        //    {
        //        SetProperty(ref _repeatAxisCheckedX, value);
        //        checkTestAxis();
        //    }
        //}

        //private bool _repeatAxisCheckedY;
        //public bool RepeatAxisCheckedY
        //{
        //    get { return _repeatAxisCheckedY; }
        //    set
        //    {
        //        SetProperty(ref _repeatAxisCheckedY, value);
        //        checkTestAxis();
        //    }
        //}

        //private bool _repeatAxisCheckedZ;
        //public bool RepeatAxisCheckedZ
        //{
        //    get { return _repeatAxisCheckedZ; }
        //    set
        //    {
        //        SetProperty(ref _repeatAxisCheckedZ, value);
        //        checkTestAxis();
        //    }
        //}

        //private bool _repeatAxisCheckedTray;
        //public bool RepeatAxisCheckedTray
        //{
        //    get { return _repeatAxisCheckedTray; }
        //    set
        //    {
        //        SetProperty(ref _repeatAxisCheckedTray, value);
        //        checkTestAxis();
        //    }
        //}

        //private bool _repeatAxisCheckedClamp;
        //public bool RepeatAxisCheckedClamp
        //{
        //    get { return _repeatAxisCheckedClamp; }
        //    set
        //    {
        //        SetProperty(ref _repeatAxisCheckedClamp, value);
        //        checkTestAxis();
        //    }
        //}

        // 各軸有效
        private bool _repeatAxisEnabledX;
        public bool RepeatAxisEnabledX
        {
            get { return _repeatAxisEnabledX; }
            set { SetProperty(ref _repeatAxisEnabledX, value); }
        }

        private bool _repeatAxisEnabledY;
        public bool RepeatAxisEnabledY
        {
            get { return _repeatAxisEnabledY; }
            set { SetProperty(ref _repeatAxisEnabledY, value); }
        }

        private bool _repeatAxisEnabledZ;
        public bool RepeatAxisEnabledZ
        {
            get { return _repeatAxisEnabledZ; }
            set { SetProperty(ref _repeatAxisEnabledZ, value); }
        }

        private bool _repeatAxisEnabledTray;
        public bool RepeatAxisEnabledTray
        {
            get { return _repeatAxisEnabledTray; }
            set { SetProperty(ref _repeatAxisEnabledTray, value); }
        }

        private bool _repeatAxisEnabledClamp;
        public bool RepeatAxisEnabledClamp
        {
            get { return _repeatAxisEnabledClamp; }
            set { SetProperty(ref _repeatAxisEnabledClamp, value); }
        }

        // 往返測試運轉中?
        private int _pauseTimes;
        public int PauseTimes
        {
            get { return _pauseTimes; }
            set { SetProperty(ref _pauseTimes, value); }
        }

        // 按鍵文字
        private string _buttonContentX;
        public string ButtonContentX
        {
            get { return _buttonContentX; }
            set { SetProperty(ref _buttonContentX, value); }
        }

        private string _buttonContentY;
        public string ButtonContentY
        {
            get { return _buttonContentY; }
            set { SetProperty(ref _buttonContentY, value); }
        }

        private string _buttonContentZ;
        public string ButtonContentZ
        {
            get { return _buttonContentZ; }
            set { SetProperty(ref _buttonContentZ, value); }
        }

        private string _buttonContentR;
        public string ButtonContentR
        {
            get { return _buttonContentR; }
            set { SetProperty(ref _buttonContentR, value); }
        }

        private string _buttonContentTray;
        public string ButtonContentTray
        {
            get { return _buttonContentTray; }
            set { SetProperty(ref _buttonContentTray, value); }
        }

        private string _buttonContentClamp;
        public string ButtonContentClamp
        {
            get { return _buttonContentClamp; }
            set { SetProperty(ref _buttonContentClamp, value); }
        }

        // 按鍵致能
        private bool _repeatStartButtonEnabled;
        public bool RepeatStartButtonEnabled
        {
            get { return _repeatStartButtonEnabled; }
            set { SetProperty(ref _repeatStartButtonEnabled, value); }
        }

        private bool _stopTestButtonEnabled;
        public bool StopTestButtonEnabled
        {
            get { return _stopTestButtonEnabled; }
            set { SetProperty(ref _stopTestButtonEnabled, value); }
        }

        // 往返測試計數器
        private string _axisCounterX;
        public string AxisCounterX
        {
            get { return _axisCounterX; }
            set { SetProperty(ref _axisCounterX, value); }
        }

        private string _axisCounterY;
        public string AxisCounterY
        {
            get { return _axisCounterY; }
            set { SetProperty(ref _axisCounterY, value); }
        }

        private string _axisCounterZ;
        public string AxisCounterZ
        {
            get { return _axisCounterZ; }
            set { SetProperty(ref _axisCounterZ, value); }
        }

        private string _axisCounterTray;
        public string AxisCounterTray
        {
            get { return _axisCounterTray; }
            set { SetProperty(ref _axisCounterTray, value); }
        }

        private string _axisCounterClamp;
        public string AxisCounterClamp
        {
            get { return _axisCounterClamp; }
            set { SetProperty(ref _axisCounterClamp, value); }
        }

        // Max Min
        private double _axisXMax;
        public double AxisXMax
        {
            get { return _axisXMax; }
            set { SetProperty(ref _axisXMax, value); }
        }

        private double _axisXMin;
        public double AxisXMin
        {
            get { return _axisXMin; }
            set { SetProperty(ref _axisXMin, value); }
        }

        private double _axisYMax;
        public double AxisYMax
        {
            get { return _axisYMax; }
            set { SetProperty(ref _axisYMax, value); }
        }

        private double _axisYMin;
        public double AxisYMin
        {
            get { return _axisYMin; }
            set { SetProperty(ref _axisYMin, value); }
        }

        private double _axisZMax;
        public double AxisZMax
        {
            get { return _axisZMax; }
            set { SetProperty(ref _axisZMax, value); }
        }

        private double _axisZMin;
        public double AxisZMin
        {
            get { return _axisZMin; }
            set { SetProperty(ref _axisZMin, value); }
        }

        private double _axisTrayMax;
        public double AxisTrayMax
        {
            get { return _axisTrayMax; }
            set { SetProperty(ref _axisTrayMax, value); }
        }

        private double _axisTrayMin;
        public double AxisTrayMin
        {
            get { return _axisTrayMin; }
            set { SetProperty(ref _axisTrayMin, value); }
        }

        private double _axisClampMax;
        public double AxisClampMax
        {
            get { return _axisClampMax; }
            set { SetProperty(ref _axisClampMax, value); }
        }

        private double _axisClampMin;
        public double AxisClampMin
        {
            get { return _axisClampMin; }
            set { SetProperty(ref _axisClampMin, value); }
        }
    }
}
