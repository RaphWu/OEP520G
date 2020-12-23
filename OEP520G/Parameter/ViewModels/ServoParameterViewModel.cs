using EPCIO;
using OEP520G.Core;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace OEP520G.Parameter.ViewModels
{
    class ServoParameterViewModel : BindableBase, IActiveAware
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly Device module = new Device();

        // 選擇顯示的軸
        private int axisSelected;

        // 視窗Active/Deactive
        public event EventHandler IsActiveChanged;
        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                //if (!value)
                //epcio.UpdateMcclVar();
            }
        }

        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        // 事件聚合器引用
        private IEventAggregator _ea;

        /// <summary>
        /// 建構函式
        /// </summary>
        public ServoParameterViewModel(IEventAggregator ea)
        {
            _ea = ea;

            AxisSelected = "X";
            SelectAxis();

            GetParameter();

            // TODO: Button改RadioButton
            RadioCaptionX = epcio.ServoX.Title;
            RadioCaptionY = epcio.ServoY.Title;
            RadioCaptionZ = epcio.ServoZ.Title;
            RadioCaptionR = epcio.ServoR.Title;
            RadioCaptionTray = epcio.ServoTray.Title;
            RadioCaptionClamp = epcio.ServoClamp.Title;

            // 全域Save事件
            WriteDataCommand = new DelegateCommand(WriteData);
            ApplicationCommands.WriteCommand.RegisterCommand(WriteDataCommand);
        }

        /********************
         * 參數作業
         *******************/
        /// <summary>
        /// 取得參數值
        /// </summary>
        public void GetParameter()
        {
            DirectionList = module.DirectionList;
            ExchangeList = module.ExchangeList;

            CommandModeList = module.CommandModeList;
            PulseModeList = module.PulseModeList;
            LimitSwitchlList = module.LimitSwitchlList;
            EncoderTypeList = module.EncoderTypeList;
            InputRateList = module.InputRateList;

            CurveTypeList = module.CurveTypeList;
            AccDecModeList = module.AccDecModeList;

            GoHomeModeList = module.GoHomeModeList;
            GoHomeDirectList = module.GoHomeDirectList;
            HomeSensorList = module.LimitSwitchlList;

            InPositionModeList = module.InPositionModeList;

            for (int servo = 0; servo < Epcio.MAX_OEP_AXIS; servo++)
            {
                _commandModeSelected[servo] = epcio.ServoList[servo].Axis.CommandMode;
                _posToEncoderDirSelected[servo] = epcio.ServoList[servo].Axis.PosToEncoderDir;
                _rpm[servo] = epcio.ServoList[servo].Axis.RPM;
                _ppr[servo] = epcio.ServoList[servo].Axis.PPR;
                _gearRatio[servo] = epcio.ServoList[servo].Axis.GearRatio;
                _pitch[servo] = epcio.ServoList[servo].Axis.Pitch;
                _highLimit[servo] = epcio.ServoList[servo].Axis.HighLimit;
                _lowLimit[servo] = epcio.ServoList[servo].Axis.LowLimit;
                _pulseModeSelected[servo] = epcio.ServoList[servo].Axis.PulseMode;
                _pulseWidth[servo] = epcio.ServoList[servo].Axis.PulseWidth;
                _overTravelUpSensorModeSelected[servo] = epcio.ServoList[servo].Axis.OverTravelUpSensorMode;
                _overTravelDownSensorModeSelected[servo] = epcio.ServoList[servo].Axis.OverTravelDownSensorMode;

                _encoderTypeSelected[servo] = epcio.ServoList[servo].Axis.EncoderType;
                _aBSwapSelected[servo] = epcio.ServoList[servo].Axis.ABSwap;
                _aInverseSelected[servo] = epcio.ServoList[servo].Axis.AInverse;
                _bInverseSelected[servo] = epcio.ServoList[servo].Axis.BInverse;
                _cInverseSelected[servo] = epcio.ServoList[servo].Axis.CInverse;
                _inputRateSelected[servo] = (ushort)InputRateList.FindIndex(x => x == epcio.ServoList[servo].Axis.InputRate);

                _accCurveSelected[servo] = epcio.ServoList[servo].Axis.AccelerationCurve;
                _accelerationTime[servo] = epcio.ServoList[servo].Axis.AccelerationTime;
                _decCurveSelected[servo] = epcio.ServoList[servo].Axis.DecelerationCurve;
                _decelerationTime[servo] = epcio.ServoList[servo].Axis.DecelerationTime;
                _accDecModeSelected[servo] = epcio.ServoList[servo].Axis.AccDecMode;

                _goHomeModeSelected[servo] = epcio.ServoList[servo].Axis.HomeMode;
                _goHomeDirectSelected[servo] = epcio.ServoList[servo].Axis.HomeDirection;
                _homeSensorSelected[servo] = epcio.ServoList[servo].Axis.HomeSensorMode;
                _goHomeIndex[servo] = epcio.ServoList[servo].Axis.IndexCount;
                _goHomeHightSpeed[servo] = epcio.ServoList[servo].Axis.HomeHighSpeed;
                _goHomeLowSpeed[servo] = epcio.ServoList[servo].Axis.HomeLowSpeed;
                _goHomeAccTime[servo] = epcio.ServoList[servo].Axis.HomeAccTime;
                _goHomeDecTime[servo] = epcio.ServoList[servo].Axis.HomeDecTime;
                _logicHomeOffset[servo] = epcio.ServoList[servo].Axis.HomeOffset;

                _inPositionOnOff[servo] = epcio.ServoList[servo].Axis.InPosition;
                _inPosMode[servo] = epcio.ServoList[servo].Axis.InPosMode;
                _inPosMaxCheckTime[servo] = epcio.ServoList[servo].Axis.InPosMaxCheckTime;
                _inPosSettleTime[servo] = epcio.ServoList[servo].Axis.InPosSettleTime;
                _inPosTolerance[servo] = epcio.ServoList[servo].Axis.InPosTolerance;

                _trackErrorOnOff[servo] = epcio.ServoList[servo].Axis.TrackError;
                _trackErrorLimit[servo] = epcio.ServoList[servo].Axis.TrackErrorLimit;

                // 速度比例
                _baseSpeed[servo] = epcio.ServoList[servo].BaseSpeed;
                _ultraHighSpeedRate[servo] = epcio.ServoList[servo].UltraHighSpeedRate;
                _highSpeedRate[servo] = epcio.ServoList[servo].HighSpeedRate;
                _middleSpeedRate[servo] = epcio.ServoList[servo].MiddleSpeedRate;
                _lowSpeedRate[servo] = epcio.ServoList[servo].LowSpeedRate;
                _ultraLowSpeedRate[servo] = epcio.ServoList[servo].UltraLowSpeedRate;
            }
        }

        /// <summary>
        /// 存回參數值
        /// </summary>
        public void WriteData()
        {
            if (IsActive)
            {
                for (int axis = 0; axis < Epcio.MAX_OEP_AXIS; axis++)
                {
                    epcio.ServoList[axis].Axis.CommandMode = _commandModeSelected[axis];
                    epcio.ServoList[axis].Axis.PosToEncoderDir = _posToEncoderDirSelected[axis];
                    epcio.ServoList[axis].Axis.RPM = _rpm[axis];
                    epcio.ServoList[axis].Axis.PPR = _ppr[axis];
                    epcio.ServoList[axis].Axis.GearRatio = _gearRatio[axis];
                    epcio.ServoList[axis].Axis.Pitch = _pitch[axis];
                    epcio.ServoList[axis].Axis.HighLimit = _highLimit[axis];
                    epcio.ServoList[axis].Axis.LowLimit = _lowLimit[axis];
                    epcio.ServoList[axis].Axis.PulseMode = _pulseModeSelected[axis];
                    epcio.ServoList[axis].Axis.PulseWidth = _pulseWidth[axis];
                    epcio.ServoList[axis].Axis.OverTravelUpSensorMode = _overTravelUpSensorModeSelected[axis];
                    epcio.ServoList[axis].Axis.OverTravelDownSensorMode = _overTravelDownSensorModeSelected[axis];

                    epcio.ServoList[axis].Axis.EncoderType = _encoderTypeSelected[axis];
                    epcio.ServoList[axis].Axis.ABSwap = _aBSwapSelected[axis];
                    epcio.ServoList[axis].Axis.AInverse = _aInverseSelected[axis];
                    epcio.ServoList[axis].Axis.BInverse = _bInverseSelected[axis];
                    epcio.ServoList[axis].Axis.CInverse = _cInverseSelected[axis];
                    epcio.ServoList[axis].Axis.InputRate = InputRateList[_inputRateSelected[axis]];

                    epcio.ServoList[axis].Axis.AccelerationCurve = _accCurveSelected[axis];
                    epcio.ServoList[axis].Axis.AccelerationTime = _accelerationTime[axis];
                    epcio.ServoList[axis].Axis.DecelerationCurve = _decCurveSelected[axis];
                    epcio.ServoList[axis].Axis.DecelerationTime = _decelerationTime[axis];
                    epcio.ServoList[axis].Axis.AccDecMode = _accDecModeSelected[axis];

                    epcio.ServoList[axis].Axis.HomeMode = _goHomeModeSelected[axis];
                    epcio.ServoList[axis].Axis.HomeDirection = _goHomeDirectSelected[axis];
                    epcio.ServoList[axis].Axis.HomeSensorMode = _homeSensorSelected[axis];
                    epcio.ServoList[axis].Axis.IndexCount = _goHomeIndex[axis];
                    epcio.ServoList[axis].Axis.HomeHighSpeed = _goHomeHightSpeed[axis];
                    epcio.ServoList[axis].Axis.HomeLowSpeed = _goHomeLowSpeed[axis];
                    epcio.ServoList[axis].Axis.HomeAccTime = _goHomeAccTime[axis];
                    epcio.ServoList[axis].Axis.HomeDecTime = _goHomeDecTime[axis];
                    epcio.ServoList[axis].Axis.HomeOffset = _logicHomeOffset[axis];

                    epcio.ServoList[axis].Axis.InPosition = _inPositionOnOff[axis];
                    epcio.ServoList[axis].Axis.InPosMode = _inPosMode[axis];
                    epcio.ServoList[axis].Axis.InPosMaxCheckTime = _inPosMaxCheckTime[axis];
                    epcio.ServoList[axis].Axis.InPosSettleTime = _inPosSettleTime[axis];
                    epcio.ServoList[axis].Axis.InPosTolerance = _inPosTolerance[axis];

                    epcio.ServoList[axis].Axis.TrackError = _trackErrorOnOff[axis];
                    epcio.ServoList[axis].Axis.TrackErrorLimit = _trackErrorLimit[axis];

                    // 速度比例
                    epcio.ServoList[axis].BaseSpeed = _baseSpeed[axis];
                    epcio.ServoList[axis].UltraHighSpeedRate = _ultraHighSpeedRate[axis];
                    epcio.ServoList[axis].HighSpeedRate = _highSpeedRate[axis];
                    epcio.ServoList[axis].MiddleSpeedRate = _middleSpeedRate[axis];
                    epcio.ServoList[axis].LowSpeedRate = _lowSpeedRate[axis];
                    epcio.ServoList[axis].UltraLowSpeedRate = _ultraLowSpeedRate[axis];
                }

                epcio.WriteParameter();

                // 重設新參數(不可在此呼叫，否則存檔動作會將軸卡資料歸零)
                //epcio.UpdateParam();
            }
        }

        /********************
         * 資料/方法繫結
         *******************/
        // 軸名稱
        //public string AxisToDisplay { get; set; }

        private void SelectAxis()
        {
            axisSelected = AxisSelected switch
            {
                "Y" => EServoId.Y,
                "Z" => EServoId.Z,
                "R" => EServoId.R,
                "TRAY" => EServoId.Tray,
                "CLAMP" => EServoId.Clamp,
                _ => EServoId.X
            };

            RaisePropertyChanged(null);
        }

        private string _axisSelected;
        public string AxisSelected
        {
            get { return _axisSelected; }
            set
            {
                SetProperty(ref _axisSelected, value);
                SelectAxis();
            }
        }

        /***  機構  ***/
        // 方向
        private List<string> _directionList;
        public List<string> DirectionList
        {
            get { return _directionList; }
            set { SetProperty(ref _directionList, value); }
        }

        private ushort[] _posToEncoderDirSelected = new ushort[6];
        public ushort PosToEncoderDirSelected
        {
            get { return _posToEncoderDirSelected[axisSelected]; }
            set { SetProperty(ref _posToEncoderDirSelected[axisSelected], value); }
        }

        // 命令模式
        private List<string> _commandModeList;
        public List<string> CommandModeList
        {
            get { return _commandModeList; }
            set { SetProperty(ref _commandModeList, value); }
        }

        private ushort[] _commandModeSelected = new ushort[6];
        public ushort CommandModeSelected
        {
            get { return _commandModeSelected[axisSelected]; }
            set { SetProperty(ref _commandModeSelected[axisSelected], value); }
        }

        private ushort[] _rpm = new ushort[6];
        public ushort Rpm
        {
            get { return _rpm[axisSelected]; }
            set { SetProperty(ref _rpm[axisSelected], value); }
        }

        private uint[] _ppr = new uint[6];
        public uint Ppr
        {
            get { return _ppr[axisSelected]; }
            set { SetProperty(ref _ppr[axisSelected], value); }
        }

        private double[] _gearRatio = new double[6];
        public double GearRatio
        {
            get { return _gearRatio[axisSelected]; }
            set { SetProperty(ref _gearRatio[axisSelected], value); }
        }

        private double[] _pitch = new double[6];
        public double Pitch
        {
            get { return _pitch[axisSelected]; }
            set { SetProperty(ref _pitch[axisSelected], value); }
        }

        private double[] _highLimit = new double[6];
        public double HighLimit
        {
            get { return _highLimit[axisSelected]; }
            set { SetProperty(ref _highLimit[axisSelected], value); }
        }

        private double[] _lowLimit = new double[6];
        public double LowLimit
        {
            get { return _lowLimit[axisSelected]; }
            set { SetProperty(ref _lowLimit[axisSelected], value); }
        }

        private List<string> _pulseModeList;
        public List<string> PulseModeList
        {
            get { return _pulseModeList; }
            set { SetProperty(ref _pulseModeList, value); }
        }

        private ushort[] _pulseModeSelected = new ushort[6];
        public ushort PulseModeSelected
        {
            get { return _pulseModeSelected[axisSelected]; }
            set { SetProperty(ref _pulseModeSelected[axisSelected], value); }
        }

        private ushort[] _pulseWidth = new ushort[6];
        public ushort PulseWidth
        {
            get { return _pulseWidth[axisSelected]; }
            set { SetProperty(ref _pulseWidth[axisSelected], value); }
        }

        private List<string> _limitSwitchlList;
        public List<string> LimitSwitchlList
        {
            get { return _limitSwitchlList; }
            set { SetProperty(ref _limitSwitchlList, value); }
        }

        private ushort[] _overTravelUpSensorModeSelected = new ushort[6];
        public ushort OverTravelUpSensorModeSelected
        {
            get { return _overTravelUpSensorModeSelected[axisSelected]; }
            set { SetProperty(ref _overTravelUpSensorModeSelected[axisSelected], value); }
        }

        private ushort[] _overTravelDownSensorModeSelected = new ushort[6];
        public ushort OverTravelDownSensorModeSelected
        {
            get { return _overTravelDownSensorModeSelected[axisSelected]; }
            set { SetProperty(ref _overTravelDownSensorModeSelected[axisSelected], value); }
        }

        /***  機構  ***/
        private List<string> _exchangeList;
        public List<string> ExchangeList
        {
            get { return _exchangeList; }
            set { SetProperty(ref _exchangeList, value); }
        }

        private List<string> _encoderTypeList;
        public List<string> EncoderTypeList
        {
            get { return _encoderTypeList; }
            set { SetProperty(ref _encoderTypeList, value); }
        }

        // 編碼器格式
        private ushort[] _encoderTypeSelected = new ushort[6];
        public ushort EncoderTypeSelected
        {
            get { return (_encoderTypeSelected[axisSelected]); }
            set { SetProperty(ref _encoderTypeSelected[axisSelected], value); }
        }

        private ushort[] _aBSwapSelected = new ushort[6];
        public ushort ABSwapSelected
        {
            get { return (_aBSwapSelected[axisSelected]); }
            set { SetProperty(ref _aBSwapSelected[axisSelected], value); }
        }

        private ushort[] _aInverseSelected = new ushort[6];
        public ushort AInverseSelected
        {
            get { return (_aInverseSelected[axisSelected]); }
            set { SetProperty(ref _aInverseSelected[axisSelected], value); }
        }

        private ushort[] _bInverseSelected = new ushort[6];
        public ushort BInverseSelected
        {
            get { return (_bInverseSelected[axisSelected]); }
            set { SetProperty(ref _bInverseSelected[axisSelected], value); }
        }

        private ushort[] _cInverseSelected = new ushort[6];
        public ushort CInverseSelected
        {
            get { return (_cInverseSelected[axisSelected]); }
            set { SetProperty(ref _cInverseSelected[axisSelected], value); }
        }

        private List<ushort> _inputRateList;
        public List<ushort> InputRateList
        {
            get { return _inputRateList; }
            set { SetProperty(ref _inputRateList, value); }
        }

        private ushort[] _inputRateSelected = new ushort[6];
        public ushort InputRateSelected
        {
            get { return (_inputRateSelected[axisSelected]); }
            set { SetProperty(ref _inputRateSelected[axisSelected], value); }
        }

        /***  加減速  ***/
        // Coordinate Mode 座標型態
        private List<string> _curveTypeList;
        public List<string> CurveTypeList
        {
            get { return _curveTypeList; }
            set { SetProperty(ref _curveTypeList, value); }
        }

        // 加減速型式
        private char[] _accCurveSelected = new char[6];
        public int AccCurveSelected
        {
            get { return (_accCurveSelected[axisSelected] == 'T') ? 0 : 1; }
            set { SetProperty(ref _accCurveSelected[axisSelected], (value == 0) ? 'T' : 'S'); }
        }

        private char[] _decCurveSelected = new char[6];
        public int DecCurveSelected
        {
            get { return (_decCurveSelected[axisSelected] == 'T') ? 0 : 1; }
            set { SetProperty(ref _decCurveSelected[axisSelected], (value == 0) ? 'T' : 'S'); }
        }

        // 加減速模式        
        private List<string> _accDecModeList;
        public List<string> AccDecModeList
        {
            get { return _accDecModeList; }
            set { SetProperty(ref _accDecModeList, value); }
        }

        private char[] _accDecModeSelected = new char[6];
        public int AccDecModeSelected
        {
            get { return (_accDecModeSelected[axisSelected] == 'A') ? 0 : 1; }
            set { SetProperty(ref _accDecModeSelected[axisSelected], (value == 0) ? 'A' : 'B'); }
        }

        // 加減速時間
        private double[] _accelerationTime = new double[6];
        public double AccelerationTime
        {
            get { return _accelerationTime[axisSelected]; }
            set { SetProperty(ref _accelerationTime[axisSelected], value); }
        }

        private double[] _decelerationTime = new double[6];
        public double DecelerationTime
        {
            get { return _decelerationTime[axisSelected]; }
            set { SetProperty(ref _decelerationTime[axisSelected], value); }
        }

        /********************
         * 復歸
         *******************/
        // 復歸模式
        private List<ushort> _goHomeModeList;
        public List<ushort> GoHomeModeList
        {
            get { return _goHomeModeList; }
            set { SetProperty(ref _goHomeModeList, value); }
        }

        private ushort[] _goHomeModeSelected = new ushort[6];
        public ushort GoHomeModeSelected
        {
            get { return _goHomeModeSelected[axisSelected]; }
            set { SetProperty(ref _goHomeModeSelected[axisSelected], value); }
        }

        // 復歸方向
        private List<string> _goHomeDirectList;
        public List<string> GoHomeDirectList
        {
            get { return _goHomeDirectList; }
            set { SetProperty(ref _goHomeDirectList, value); }
        }

        private ushort[] _goHomeDirectSelected = new ushort[6];
        public ushort GoHomeDirectSelected
        {
            get { return _goHomeDirectSelected[axisSelected]; }
            set { SetProperty(ref _goHomeDirectSelected[axisSelected], value); }
        }

        // 原點Sensor
        private List<string> _homeSensorList;
        public List<string> HomeSensorList
        {
            get { return _homeSensorList; }
            set { SetProperty(ref _homeSensorList, value); }
        }

        private ushort[] _homeSensorSelected = new ushort[6];
        public ushort HomeSensorSelected
        {
            get { return _homeSensorSelected[axisSelected]; }
            set { SetProperty(ref _homeSensorSelected[axisSelected], value); }
        }

        // HomeIndex
        private int[] _goHomeIndex = new int[6];
        public int GoHomeIndex
        {
            get { return _goHomeIndex[axisSelected]; }
            set { SetProperty(ref _goHomeIndex[axisSelected], value); }
        }

        // GoHomeHightSpeed
        private double[] _goHomeHightSpeed = new double[6];
        public double GoHomeHightSpeed
        {
            get { return _goHomeHightSpeed[axisSelected]; }
            set { SetProperty(ref _goHomeHightSpeed[axisSelected], value); }
        }

        // GoHomeLowSpeed
        private double[] _goHomeLowSpeed = new double[6];
        public double GoHomeLowSpeed
        {
            get { return _goHomeLowSpeed[axisSelected]; }
            set { SetProperty(ref _goHomeLowSpeed[axisSelected], value); }
        }

        // GoHomeAccTime
        private double[] _goHomeAccTime = new double[6];
        public double GoHomeAccTime
        {
            get { return _goHomeAccTime[axisSelected]; }
            set { SetProperty(ref _goHomeAccTime[axisSelected], value); }
        }

        // GoHomeDecTime
        private double[] _goHomeDecTime = new double[6];
        public double GoHomeDecTime
        {
            get { return _goHomeDecTime[axisSelected]; }
            set { SetProperty(ref _goHomeDecTime[axisSelected], value); }
        }

        // LogicHomeOffset
        private double[] _logicHomeOffset = new double[6];
        public double LogicHomeOffset
        {
            get { return _logicHomeOffset[axisSelected]; }
            set { SetProperty(ref _logicHomeOffset[axisSelected], value); }
        }

        /********************
         * Path Blending 連續運動功能
         *******************/
        private bool[] _pathBlending = new bool[6];
        public bool PathBlending
        {
            get { return _pathBlending[axisSelected]; }
            set { SetProperty(ref _pathBlending[axisSelected], value); }
        }

        /********************
         * In Position 定位確認功能
         *******************/
        private List<string> _inPositionModeList;
        public List<string> InPositionModeList
        {
            get { return _inPositionModeList; }
            set { SetProperty(ref _inPositionModeList, value); }
        }

        private bool[] _inPositionOnOff = new bool[6];
        public bool InPositionOnOff
        {
            get { return _inPositionOnOff[axisSelected]; }
            set { SetProperty(ref _inPositionOnOff[axisSelected], value); }
        }

        private ushort[] _inPosMode = new ushort[6];
        public ushort InPosMode
        {
            get { return _inPosMode[axisSelected]; }
            set { SetProperty(ref _inPosMode[axisSelected], value); }
        }

        private ushort[] _inPosMaxCheckTime = new ushort[6];
        public ushort InPosMaxCheckTime
        {
            get { return _inPosMaxCheckTime[axisSelected]; }
            set { SetProperty(ref _inPosMaxCheckTime[axisSelected], value); }
        }

        private ushort[] _inPosSettleTime = new ushort[6];
        public ushort InPosSettleTime
        {
            get { return _inPosSettleTime[axisSelected]; }
            set { SetProperty(ref _inPosSettleTime[axisSelected], value); }
        }

        private double[] _inPosTolerance = new double[6];
        public double InPosTolerance
        {
            get { return _inPosTolerance[axisSelected]; }
            set { SetProperty(ref _inPosTolerance[axisSelected], value); }
        }

        /********************
         * Tracking Error 跟隨誤差功能
         *******************/
        private bool[] _trackErrorOnOff = new bool[6];
        public bool TrackErrorOnOff
        {
            get { return _trackErrorOnOff[axisSelected]; }
            set { SetProperty(ref _trackErrorOnOff[axisSelected], value); }
        }

        private double[] _trackErrorLimit = new double[6];
        public double TrackErrorLimit
        {
            get { return _trackErrorLimit[axisSelected]; }
            set { SetProperty(ref _trackErrorLimit[axisSelected], value); }
        }

        /********************
         * 速度
         *******************/
        private double[] _baseSpeed = new double[6];
        public double BaseSpeed
        {
            get { return _baseSpeed[axisSelected]; }
            set { SetProperty(ref _baseSpeed[axisSelected], value); }
        }

        private int[] _ultraHighSpeedRate = new int[6];
        public int UltraHighSpeedRate
        {
            get { return _ultraHighSpeedRate[axisSelected]; }
            set { SetProperty(ref _ultraHighSpeedRate[axisSelected], value); }
        }

        private int[] _highSpeedRate = new int[6];
        public int HighSpeedRate
        {
            get { return _highSpeedRate[axisSelected]; }
            set { SetProperty(ref _highSpeedRate[axisSelected], value); }
        }

        private int[] _middleSpeedRate = new int[6];
        public int MiddleSpeedRate
        {
            get { return _middleSpeedRate[axisSelected]; }
            set { SetProperty(ref _middleSpeedRate[axisSelected], value); }
        }

        private int[] _lowSpeedRate = new int[6];
        public int LowSpeedRate
        {
            get { return _lowSpeedRate[axisSelected]; }
            set { SetProperty(ref _lowSpeedRate[axisSelected], value); }
        }

        private int[] _ultraLowSpeedRate = new int[6];
        public int UltraLowSpeedRate
        {
            get { return _ultraLowSpeedRate[axisSelected]; }
            set { SetProperty(ref _ultraLowSpeedRate[axisSelected], value); }
        }

        /********************
         * 按鍵文字
         *******************/
        private string _radioCaptionX;
        public string RadioCaptionX
        {
            get { return _radioCaptionX; }
            set { SetProperty(ref _radioCaptionX, value); }
        }

        private string _radioCaptionY;
        public string RadioCaptionY
        {
            get { return _radioCaptionY; }
            set { SetProperty(ref _radioCaptionY, value); }
        }

        private string _radioCaptionZ;
        public string RadioCaptionZ
        {
            get { return _radioCaptionZ; }
            set { SetProperty(ref _radioCaptionZ, value); }
        }

        private string _radioCaptionR;
        public string RadioCaptionR
        {
            get { return _radioCaptionR; }
            set { SetProperty(ref _radioCaptionR, value); }
        }

        private string _radioCaptionTray;
        public string RadioCaptionTray
        {
            get { return _radioCaptionTray; }
            set { SetProperty(ref _radioCaptionTray, value); }
        }

        private string _radioCaptionClamp;
        public string RadioCaptionClamp
        {
            get { return _radioCaptionClamp; }
            set { SetProperty(ref _radioCaptionClamp, value); }
        }
    }
}
