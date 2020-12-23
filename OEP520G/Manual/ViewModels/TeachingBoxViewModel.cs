using EPCIO;
using Prism.Commands;
using Prism.Mvvm;

namespace OEP520G.Manual.ViewModels
{
    public class TeachingBoxViewModel : BindableBase
    {
        private Epcio epcio = Epcio.Instance;

        /********************
         * 速度設定
         *******************/
        /// <summary>
        /// 速度設定的標籤
        /// </summary>
        private readonly string[] speedNameList = { "最低速", "低速", "中速", "高速", "最高速" };

        /// <summary>
        /// 速度設定的顏色
        /// </summary>
        private readonly string[] speedColorList = { "SlateGray", "Teal", "RoyalBlue", "OrangeRed", "Firebrick" };

        /// <summary>
        /// Jog移動距離級數(mm)
        /// </summary>
        private readonly double[] ResoultList = { 0.004, 0.02, 0.1, 1, 5, 10, 50 };

        /// <summary>
        /// Jog移動距離級數(度)
        /// </summary>
        private readonly double[] degreeList = { 0.2, 1, 2, 5, 10, 20, 45 };

        /// <summary>
        /// 目前速度Index(0~4)
        /// </summary>
        private EServoSpeed speedIndex;

        private int servoIdX;
        private int servoIdY;
        private int servoIdZ;
        private int servoIdR;
        private int servoIdTray;
        private int servoIdClamp;

        /// <summary>
        /// 目前速度值
        /// </summary>
        private double[] speedValue = new double[Epcio.MAX_OEP_AXIS];

        /// <summary>
        /// 目前速度比率
        /// </summary>
        private int[] speedRatio = new int[Epcio.MAX_OEP_AXIS];

        /********************
         * 繫結
         *******************/
        public DelegateCommand<string> SpeedCycleCommand { get; private set; }
        public DelegateCommand<string> JogCommand { get; private set; }
        public DelegateCommand MoveToCommand { get; private set; }
        public DelegateCommand StandbyPositionCommand { get; private set; }
        public DelegateCommand GetCoorFromCommand { get; private set; }
        public DelegateCommand ShiftToCommand { get; private set; }
        public DelegateCommand ClearShiftCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public TeachingBoxViewModel()
        {
            speedIndex = EServoSpeed.Middle; // 預設為中速

            servoIdX = epcio.ServoX.Id;
            servoIdY = epcio.ServoY.Id;
            servoIdZ = epcio.ServoZ.Id;
            servoIdR = epcio.ServoR.Id;
            servoIdTray = epcio.ServoTray.Id;
            servoIdClamp = epcio.ServoClamp.Id;

            SetSpeedValue();

            ResoultContent0 = $"{ResoultList[0]}\n{degreeList[0]}°";
            ResoultContent1 = $"{ResoultList[1]}\n{degreeList[1]}°";
            ResoultContent2 = $"{ResoultList[2]}\n{degreeList[2]}°";
            ResoultContent3 = $"{ResoultList[3]}\n{degreeList[3]}°";
            ResoultContent4 = $"{ResoultList[4]}\n{degreeList[4]}°";
            ResoultContent5 = $"{ResoultList[5]}\n{degreeList[5]}°";
            ResoultContent6 = $"{ResoultList[6]}\n{degreeList[6]}°";

            SetSpeedAppearance();
            ResoultOption = "0";
            SelectorXOption = "PP";
            SelectorYOption = "STAGE";

            SpeedCycleCommand = new DelegateCommand<string>(SpeedCycle);
            JogCommand = new DelegateCommand<string>(Jog);
            MoveToCommand = new DelegateCommand(MoveTo);
            StandbyPositionCommand = new DelegateCommand(StandbyPosition);
            GetCoorFromCommand = new DelegateCommand(GetCoorFrom);
            ShiftToCommand = new DelegateCommand(ShiftTo);
            ClearShiftCommand = new DelegateCommand(ClearShift);
        }

        /********************
         * Motion
         *******************/
        /// <summary>
        /// JOG
        /// </summary>
        /// <param name="parameter">[軸,移動方向]</param>
        private void Jog(string parameter)
        {
            int index = int.Parse(ResoultOption);
            double offset = ResoultList[index];
            double degree = degreeList[index];

            // [0]=軸 [1]=方向
            string[] paras = parameter.Split(',');

            // 軸運動
            switch (paras[0])
            {
                case "ASSEMBLY":
                    switch (paras[1])
                    {
                        case "LEFT":
                            epcio.JogTo(EServoId.X, offset * -1, speedRatio[servoIdX]);
                            break;
                        case "RIGHT":
                            epcio.JogTo(EServoId.X, offset, speedRatio[servoIdX]);
                            break;
                        case "UP":
                            epcio.JogTo(EServoId.Y, offset * -1, speedRatio[servoIdY]);
                            break;
                        case "DOWN":
                            epcio.JogTo(EServoId.Y, offset, speedRatio[servoIdY]);
                            break;
                    }
                    break;

                case "TRAY_CLAMP":
                    switch (paras[1])
                    {
                        case "LEFT":
                            epcio.JogTo(EServoId.Clamp, offset, speedRatio[servoIdClamp]);
                            break;
                        case "RIGHT":
                            epcio.JogTo(EServoId.Clamp, offset * -1, speedRatio[servoIdClamp]);
                            break;
                        case "UP":
                            epcio.JogTo(EServoId.Tray, offset * -1, speedRatio[servoIdTray]);
                            break;
                        case "DOWN":
                            epcio.JogTo(EServoId.Tray, offset, speedRatio[servoIdTray]);
                            break;
                    }
                    break;

                case "Z":
                    switch (paras[1])
                    {
                        case "UP":
                            epcio.JogTo(EServoId.Z, offset * -1, speedRatio[servoIdZ]);
                            break;

                        case "DOWN":
                            epcio.JogTo(EServoId.Z, offset, speedRatio[servoIdZ]);
                            break;
                    }
                    break;

                case "R":
                    switch (paras[1])
                    {
                        case "CCW":
                            epcio.JogTo(EServoId.R, degree, speedRatio[servoIdR]);
                            break;
                        case "CW":
                            epcio.JogTo(EServoId.R, degree * -1, speedRatio[servoIdR]);
                            break;
                    }
                    break;
            }
        }

        /********************
         * 速度
         *******************/
        private void SetSpeedValue()
        {
            epcio.SetSpeed(speedIndex);

            foreach (Servo servo in epcio.ServoList)
            {
                int idx = servo.Id;

                switch (speedIndex)
                {
                    case EServoSpeed.UltraLow:
                        speedValue[idx] = servo.UltraLowSpeed;
                        speedRatio[idx] = servo.UltraLowSpeedRate;
                        break;
                    case EServoSpeed.Low:
                        speedValue[idx] = servo.LowSpeed;
                        speedRatio[idx] = servo.LowSpeedRate;
                        break;
                    case EServoSpeed.High:
                        speedValue[idx] = servo.HighSpeed;
                        speedRatio[idx] = servo.HighSpeedRate;
                        break;
                    case EServoSpeed.UltraHigh:
                        speedValue[idx] = servo.UltraHighSpeed;
                        speedRatio[idx] = servo.UltraHighSpeedRate;
                        break;
                    default:
                        speedValue[idx] = servo.MiddleSpeed;
                        speedRatio[idx] = servo.MiddleSpeedRate;
                        break;
                }
            }
        }

        /// <summary>
        /// 速度循環
        /// </summary>
        private void SpeedCycle(string accdec)
        {
            if (accdec == "+")
            {
                if (++speedIndex > EServoSpeed.UltraHigh)
                    speedIndex = EServoSpeed.UltraLow;
            }
            else if (accdec == "-")
            {
                if (--speedIndex < EServoSpeed.UltraLow)
                    speedIndex = EServoSpeed.UltraHigh;
            }

            SetSpeedValue();
            SetSpeedAppearance();
        }

        /// <summary>
        /// 設定速度按鍵文字及顏色
        /// </summary>
        private void SetSpeedAppearance()
        {
            SpeedContent = speedNameList[(int)speedIndex];
            SpeedColor = speedColorList[(int)speedIndex];
        }

        /********************
         * 擴充功能
         *******************/
        /// <summary>
        /// 待機
        /// </summary>
        private async void StandbyPosition()
        {
            await epcio.SafetyPosition();

            epcio.SetSpeed(EServoSpeed.Middle);

            // TODO: 走不到AXIS_TRAY_POSITION_MAX位置，老是差1um
            var servo = epcio.ServoTray;
            double targetPos = servo.GetCurrentPosition() > (servo.MaxPosition - 1)
                              ? servo.MinPosition
                              : servo.MaxPosition;
            epcio.MoveTo(positionTray: targetPos);

            // 
            servo = epcio.ServoY;
            targetPos = servo.GetCurrentPosition() > (servo.MaxPosition - 1)
                             ? servo.MinPosition
                             : servo.MaxPosition;
            epcio.MoveTo(positionY: targetPos);

            //epcio.SetSpeed(speedValue);
        }

        /********************
         * 下方功能鍵
         *******************/
        /// <summary>
        /// 移動
        /// </summary>
        /// <remarks>
        /// 若Z軸不在安全高度內
        /// </remarks>
        private async void MoveTo()
        {
            await epcio.MoveServoZToSafety();

            epcio.MoveTo(
                positionX: MoveToX,
                positionY: MoveToY,
                degreeR: (float)MoveToR,
                positionTray: MoveToTray,
                positionClamp: MoveToClamp);

            await epcio.WaitingForAllServoMotionStop();

            epcio.MoveTo(positionZ: MoveToZ);
        }

        /// <summary>
        /// 取得
        /// </summary>
        private void GetCoorFrom()
        {
            MoveToX = epcio.ServoX.GetCurrentPosition();
            MoveToY = epcio.ServoY.GetCurrentPosition();
            MoveToZ = epcio.ServoZ.GetCurrentPosition();
            MoveToR = epcio.ServoR.GetCurrentPosition();
            MoveToClamp = epcio.ServoClamp.GetCurrentPosition();
            MoveToTray = epcio.ServoTray.GetCurrentPosition();
        }

        private double finalZ; // Z軸最終座標
        /// <summary>
        /// 位移
        /// </summary>
        private async void ShiftTo()
        {
            epcio.SetSpeed(EServoSpeed.Middle);
            finalZ = epcio.ServoZ.GetCurrentPosition() + ShiftToZ;

            await epcio.MoveServoZToSafety();

            epcio.MoveToRelative(
                distanceX: ShiftToX,
                distanceY: ShiftToY,
                degreeR: (float)ShiftToR,
                distanceTray: ShiftToTray,
                distanceClamp: ShiftToClamp);

            await epcio.WaitingForAllServoMotionStop();

            epcio.MoveTo(positionZ: finalZ);
            //epcio.SetSpeed(speedValue);
        }

        /// <summary>
        /// 清除
        /// </summary>
        private void ClearShift()
        {
            ShiftToX = 0.0;
            ShiftToY = 0.0;
            ShiftToZ = 0.0;
            ShiftToR = 0.0;
            ShiftToTray = 0.0;
            ShiftToClamp = 0.0;
        }

        /********************
         * 繫結
         *******************/
        private string _resoultContent0;
        public string ResoultContent0
        {
            get { return _resoultContent0; }
            set { SetProperty(ref _resoultContent0, value); }
        }

        private string _resoultContent1;
        public string ResoultContent1
        {
            get { return _resoultContent1; }
            set { SetProperty(ref _resoultContent1, value); }
        }

        private string _resoultContent2;
        public string ResoultContent2
        {
            get { return _resoultContent2; }
            set { SetProperty(ref _resoultContent2, value); }
        }

        private string _resoultContent3;
        public string ResoultContent3
        {
            get { return _resoultContent3; }
            set { SetProperty(ref _resoultContent3, value); }
        }

        private string _resoultContent4;
        public string ResoultContent4
        {
            get { return _resoultContent4; }
            set { SetProperty(ref _resoultContent4, value); }
        }

        private string _resoultContent5;
        public string ResoultContent5
        {
            get { return _resoultContent5; }
            set { SetProperty(ref _resoultContent5, value); }
        }

        private string _resoultContent6;
        public string ResoultContent6
        {
            get { return _resoultContent6; }
            set { SetProperty(ref _resoultContent6, value); }
        }

        private string _speedContent;
        public string SpeedContent
        {
            get { return _speedContent; }
            set { SetProperty(ref _speedContent, value); }
        }

        private string _speedColor;
        public string SpeedColor
        {
            get { return _speedColor; }
            set { SetProperty(ref _speedColor, value); }
        }

        private string _resoultOption;
        public string ResoultOption
        {
            get { return _resoultOption; }
            set { SetProperty(ref _resoultOption, value); }
        }

        private string _selectorXOption;
        public string SelectorXOption
        {
            get { return _selectorXOption; }
            set { SetProperty(ref _selectorXOption, value); }
        }

        private string _selectorYOption;
        public string SelectorYOption
        {
            get { return _selectorYOption; }
            set { SetProperty(ref _selectorYOption, value); }
        }

        private double _moveToX;
        public double MoveToX
        {
            get { return _moveToX; }
            set { SetProperty(ref _moveToX, value); }
        }

        private double _moveToY;
        public double MoveToY
        {
            get { return _moveToY; }
            set { SetProperty(ref _moveToY, value); }
        }

        private double _moveToZ;
        public double MoveToZ
        {
            get { return _moveToZ; }
            set { SetProperty(ref _moveToZ, value); }
        }

        private double _moveToR;
        public double MoveToR
        {
            get { return _moveToR; }
            set { SetProperty(ref _moveToR, value); }
        }

        private double _moveToTray;
        public double MoveToTray
        {
            get { return _moveToTray; }
            set { SetProperty(ref _moveToTray, value); }
        }

        private double _moveToClamp;
        public double MoveToClamp
        {
            get { return _moveToClamp; }
            set { SetProperty(ref _moveToClamp, value); }
        }

        private double _shiftToX;
        public double ShiftToX
        {
            get { return _shiftToX; }
            set { SetProperty(ref _shiftToX, value); }
        }

        private double _shiftToY;
        public double ShiftToY
        {
            get { return _shiftToY; }
            set { SetProperty(ref _shiftToY, value); }
        }

        private double _shiftToZ;
        public double ShiftToZ
        {
            get { return _shiftToZ; }
            set { SetProperty(ref _shiftToZ, value); }
        }

        private double _shiftToR;
        public double ShiftToR
        {
            get { return _shiftToR; }
            set { SetProperty(ref _shiftToR, value); }
        }

        private double _shiftToTray;
        public double ShiftToTray
        {
            get { return _shiftToTray; }
            set { SetProperty(ref _shiftToTray, value); }
        }

        private double _shiftToClamp;
        public double ShiftToClamp
        {
            get { return _shiftToClamp; }
            set { SetProperty(ref _shiftToClamp, value); }
        }

    }
}
