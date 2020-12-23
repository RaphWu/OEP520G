using EPCIO;
using OEP520G.Core;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using System;

namespace OEP520G.Parameter.ViewModels
{
    public class DiscardBoxViewModel : BindableBase, IActiveAware
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly Machine machine = Machine.Instance;
        private readonly Clamp clamp = Clamp.Instance;

        // 視窗Active/Deactive
        public event EventHandler IsActiveChanged;
        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                epcio.SafetyPosition();

                if (!value)
                    ReadData();
            }
        }

        public DelegateCommand AssembleDiscardMovtToHereCommand { get; private set; }
        public DelegateCommand AssembleDiscardGetCoorCommand { get; private set; }
        public DelegateCommand SemiFinishedDiscardMovtToHereCommand { get; private set; }
        public DelegateCommand SemiFinishedDiscardGetCoorCommand { get; private set; }
        public DelegateCommand ClampUpCommand { get; private set; }
        public DelegateCommand ClampDownCommand { get; private set; }

        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public DiscardBoxViewModel()
        {
            ReadData();

            AssembleDiscardMovtToHereCommand = new DelegateCommand(AssembleDiscardMovtToHere);
            AssembleDiscardGetCoorCommand = new DelegateCommand(AssembleDiscardGetCoor);
            SemiFinishedDiscardMovtToHereCommand = new DelegateCommand(SemiFinishedDiscardMovtToHere);
            SemiFinishedDiscardGetCoorCommand = new DelegateCommand(SemiFinishedDiscardGetCoor);
            ClampUpCommand = new DelegateCommand(ClampUp);
            ClampDownCommand = new DelegateCommand(ClampDown);

            // 全域Save事件
            WriteDataCommand = new DelegateCommand(WriteData);
            ApplicationCommands.WriteCommand.RegisterCommand(WriteDataCommand);
        }

        /// <summary>
        /// 全域Save事件
        /// </summary>
        private void WriteData()
        {
            if (IsActive)
            {
                machine.AssembleDiscardBox.X = AssembleDiscardBoxPositionX;
                machine.AssembleDiscardBox.Z = AssembleDiscardBoxPositionZ;
                machine.AssembleDiscardExhaleTime = AssembleDiscardExhaleTime;
                machine.AssembleDiscardExhaleNumbers = AssembleDiscardExhaleNumbers;
                machine.AssembleDiscardGapTime = AssembleDiscardGapTime;

                machine.SemiFinishedDiscardBox.X = SemiFinishedDiscardBoxPositionX;
                machine.SemiFinishedDiscardBox.Y = SemiFinishedDiscardBoxPositionY;
                machine.SemiFinishedDiscardOpenCloseTime = SemiFinishedDiscardOpenCloseTime;
                machine.SemiFinishedDiscardOpenCloseNumbers = SemiFinishedDiscardOpenCloseNumbers;

                machine.WriteParameter();
            }
        }

        /// <summary>
        /// 取得參數值
        /// </summary>
        public void ReadData()
        {
            AssembleDiscardBoxPositionX = machine.AssembleDiscardBox.X;
            AssembleDiscardBoxPositionZ = machine.AssembleDiscardBox.Z;
            AssembleDiscardExhaleTime = machine.AssembleDiscardExhaleTime;
            AssembleDiscardExhaleNumbers = machine.AssembleDiscardExhaleNumbers;
            AssembleDiscardGapTime = machine.AssembleDiscardGapTime;

            SemiFinishedDiscardBoxPositionX = machine.SemiFinishedDiscardBox.X;
            SemiFinishedDiscardBoxPositionY = machine.SemiFinishedDiscardBox.Y;
            SemiFinishedDiscardOpenCloseTime = machine.SemiFinishedDiscardOpenCloseTime;
            SemiFinishedDiscardOpenCloseNumbers = machine.SemiFinishedDiscardOpenCloseNumbers;
        }

        /********************
         * 按鈕_組裝側
         *******************/
        private void AssembleDiscardMovtToHere()
        {
            epcio.SafetyPosition();

            epcio.MoveTo(positionX: AssembleDiscardBoxPositionX);
            epcio.WaitingForMotionStop(waitingServoX: true);

            epcio.MoveTo(positionZ: AssembleDiscardBoxPositionZ);
        }

        private void AssembleDiscardGetCoor()
        {
            AssembleDiscardBoxPositionX = epcio.ServoX.GetCurrentPosition();
            AssembleDiscardBoxPositionZ = epcio.ServoZ.GetCurrentPosition();
        }

        /********************
         * 按鈕_夾爪側
         *******************/
        private void SemiFinishedDiscardMovtToHere()
        {
            epcio.MoveTo(
                positionClamp: SemiFinishedDiscardBoxPositionX,
                positionY: SemiFinishedDiscardBoxPositionY);
        }

        private void SemiFinishedDiscardGetCoor()
        {
            SemiFinishedDiscardBoxPositionX = epcio.ServoClamp.GetCurrentPosition();
            SemiFinishedDiscardBoxPositionY = epcio.ServoY.GetCurrentPosition();
        }

        /// <summary>
        /// 夾爪上升
        /// </summary>
        private void ClampUp()
        {
            clamp.ClampSlideCylinderUp();
            clamp.ClampUp(clamp1: true, clamp2: true);
        }

        /// <summary>
        /// 夾爪下降
        /// </summary>
        private void ClampDown()
        {
            clamp.ClampSlideCylinderDown();
            clamp.ClampDown(clamp1: true, clamp2: true);
        }

        /********************
         * 繫結
         *******************/
        // 組裝側吸嘴
        private double _assembleDiscardBoxPositionX;
        public double AssembleDiscardBoxPositionX
        {
            get { return _assembleDiscardBoxPositionX; }
            set { SetProperty(ref _assembleDiscardBoxPositionX, value); }
        }

        private double _assembleDiscardBoxPositionZ;
        public double AssembleDiscardBoxPositionZ
        {
            get { return _assembleDiscardBoxPositionZ; }
            set { SetProperty(ref _assembleDiscardBoxPositionZ, value); }
        }

        private int _assembleDiscardExhaleTime;
        public int AssembleDiscardExhaleTime
        {
            get { return _assembleDiscardExhaleTime; }
            set { SetProperty(ref _assembleDiscardExhaleTime, value); }
        }

        private int _assembleDiscardExhaleNumbers;
        public int AssembleDiscardExhaleNumbers
        {
            get { return _assembleDiscardExhaleNumbers; }
            set
            {
                SetProperty(ref _assembleDiscardExhaleNumbers, value);
                AssembleDiscardGapTimeEnabled = value > 1;
            }
        }

        private int _assembleDiscardGapTime;
        public int AssembleDiscardGapTime
        {
            get { return _assembleDiscardGapTime; }
            set { SetProperty(ref _assembleDiscardGapTime, value); }
        }

        private bool _assembleDiscardGapTimeEnabled;
        public bool AssembleDiscardGapTimeEnabled
        {
            get { return _assembleDiscardGapTimeEnabled; }
            set { SetProperty(ref _assembleDiscardGapTimeEnabled, value); }
        }

        // 夾爪側
        private double _semiFinishedDiscardBoxPositionX;
        public double SemiFinishedDiscardBoxPositionX
        {
            get { return _semiFinishedDiscardBoxPositionX; }
            set { SetProperty(ref _semiFinishedDiscardBoxPositionX, value); }
        }

        private double _semiFinishedDiscardBoxPositionY;
        public double SemiFinishedDiscardBoxPositionY
        {
            get { return _semiFinishedDiscardBoxPositionY; }
            set { SetProperty(ref _semiFinishedDiscardBoxPositionY, value); }
        }

        private int _semiFinishedDiscardOpenCloseTime;
        public int SemiFinishedDiscardOpenCloseTime
        {
            get { return _semiFinishedDiscardOpenCloseTime; }
            set { SetProperty(ref _semiFinishedDiscardOpenCloseTime, value); }
        }

        private int _semiFinishedDiscardOpenCloseNumbers;
        public int SemiFinishedDiscardOpenCloseNumbers
        {
            get { return _semiFinishedDiscardOpenCloseNumbers; }
            set { SetProperty(ref _semiFinishedDiscardOpenCloseNumbers, value); }
        }
    }
}
