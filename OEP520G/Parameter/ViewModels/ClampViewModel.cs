using EPCIO;
using OEP520G.Core;
using OEP520G.Parameter;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OEP520G.Parameter.ViewModels
{
    public class ClampViewModel : BindableBase, IActiveAware
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly Stage stage = Stage.Instance;
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
                //epcio.SafetyPosition();
                if (_isActive)
                    ReadData();
            }
        }

        // 按鍵
        public DelegateCommand<string> MovtToClampCommand { get; private set; }
        public DelegateCommand<string> GetCoordinationCommand { get; private set; }
        public DelegateCommand<string> ClampUpCommand { get; private set; }
        public DelegateCommand<string> ClampDownCommand { get; private set; }


        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public ClampViewModel()
        {
            ReadData();

            // 按鍵
            MovtToClampCommand = new DelegateCommand<string>(MovtToClamp);
            GetCoordinationCommand = new DelegateCommand<string>(GetCoordination);
            ClampUpCommand = new DelegateCommand<string>(ClampUp);
            ClampDownCommand = new DelegateCommand<string>(ClampDown);

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
                clamp.Clamp1.StageCoordination.X = Clamp1StageCoordinationX;
                clamp.Clamp1.StageCoordination.Y = Clamp1StageCoordinationY;
                clamp.Clamp1.DelayTime1 = Clamp1DelayTime1;
                clamp.Clamp1.DelayTime2 = Clamp1DelayTime2;

                clamp.Clamp2.StageCoordination.X = Clamp2StageCoordinationX;
                clamp.Clamp2.StageCoordination.Y = Clamp2StageCoordinationY;
                clamp.Clamp2.DelayTime1 = Clamp2DelayTime1;
                clamp.Clamp2.DelayTime2 = Clamp2DelayTime2;

                clamp.WriteParameter();
            }
        }

        /// <summary>
        /// 取得參數值
        /// </summary>
        public void ReadData()
        {
            Clamp1StageCoordinationX = clamp.Clamp1.StageCoordination.X;
            Clamp1StageCoordinationY = clamp.Clamp1.StageCoordination.Y;
            Clamp1DelayTime1 = clamp.Clamp1.DelayTime1;
            Clamp1DelayTime2 = clamp.Clamp1.DelayTime2;

            Clamp2StageCoordinationX = clamp.Clamp2.StageCoordination.X;
            Clamp2StageCoordinationY = clamp.Clamp2.StageCoordination.Y;
            Clamp2DelayTime1 = clamp.Clamp2.DelayTime1;
            Clamp2DelayTime2 = clamp.Clamp2.DelayTime2;
        }

        /********************
         * 按鍵
         *******************/
        /// <summary>
        /// 移到此處
        /// </summary>
        /// <param name="clampId">夾爪編號</param>
        private void MovtToClamp(string clampId)
        {
            if (clampId == "1")
                epcio.MoveTo(positionClamp: Clamp1StageCoordinationX, positionY: Clamp1StageCoordinationY);
            else if (clampId == "2")
                epcio.MoveTo(positionClamp: Clamp2StageCoordinationX, positionY: Clamp2StageCoordinationY);
        }

        /// <summary>
        /// 取得座標
        /// </summary>
        /// <param name="clampId">夾爪編號</param>
        private void GetCoordination(string clampId)
        {
            if (clampId == "1")
            {
                Clamp1StageCoordinationX = epcio.ServoClamp.GetCurrentPosition();
                Clamp1StageCoordinationY = epcio.ServoY.GetCurrentPosition();
            }
            else if (clampId == "2")
            {
                Clamp2StageCoordinationX = epcio.ServoClamp.GetCurrentPosition();
                Clamp2StageCoordinationY = epcio.ServoY.GetCurrentPosition();
            }
        }

        /// <summary>
        /// 夾爪上升
        /// </summary>
        /// <param name="clampId">夾爪編號</param>
        private void ClampUp(string clampId)
        {
            if (clampId == "1")
            {
                clamp.ClampSlideCylinderUp();
                clamp.ClampUp(clamp1: true);
            }
            else if (clampId == "2")
            {
                clamp.ClampSlideCylinderUp();
                clamp.ClampUp(clamp2: true);
            }
        }

        /// <summary>
        /// 夾爪下降
        /// </summary>
        /// <param name="clampId">夾爪編號</param>
        private void ClampDown(string clampId)
        {
            if (clampId == "1")
            {
                clamp.ClampSlideCylinderDown();
                clamp.ClampDown(clamp1: true);
                clamp.ClampUp(clamp2: true);
            }
            else if (clampId == "2")
            {
                clamp.ClampSlideCylinderDown();
                clamp.ClampDown(clamp2: true);
                clamp.ClampUp(clamp1: true);
            }
        }

        /********************
         * 繫結
         *******************/
        // Clamp1
        public double Clamp1StageCoordinationX
        {
            get { return _clamp1StageCoordinationX; }
            set { SetProperty(ref _clamp1StageCoordinationX, value); }
        }
        private double _clamp1StageCoordinationX;

        public double Clamp1StageCoordinationY
        {
            get { return _clamp1AccessY; }
            set { SetProperty(ref _clamp1AccessY, value); }
        }
        private double _clamp1AccessY;

        public double Clamp1StageCoordinationZ
        {
            get { return _clamp1AccessZ; }
            set { SetProperty(ref _clamp1AccessZ, value); }
        }
        private double _clamp1AccessZ;

        public int Clamp1DelayTime1
        {
            get { return _clamp1DelayTime1; }
            set { SetProperty(ref _clamp1DelayTime1, value); }
        }
        private int _clamp1DelayTime1;

        public int Clamp1DelayTime2
        {
            get { return _clamp1DelayTime2; }
            set { SetProperty(ref _clamp1DelayTime2, value); }
        }
        private int _clamp1DelayTime2;

        // Clamp2
        public double Clamp2StageCoordinationX
        {
            get { return _clamp2AccessX; }
            set { SetProperty(ref _clamp2AccessX, value); }
        }
        private double _clamp2AccessX;

        public double Clamp2StageCoordinationY
        {
            get { return _clamp2AccessY; }
            set { SetProperty(ref _clamp2AccessY, value); }
        }
        private double _clamp2AccessY;

        public double Clamp2StageCoordinationZ
        {
            get { return _clamp2AccessZ; }
            set { SetProperty(ref _clamp2AccessZ, value); }
        }
        private double _clamp2AccessZ;

        public int Clamp2DelayTime1
        {
            get { return _clamp2DelayTime1; }
            set { SetProperty(ref _clamp2DelayTime1, value); }
        }
        private int _clamp2DelayTime1;

        public int Clamp2DelayTime2
        {
            get { return _clamp2DelayTime2; }
            set { SetProperty(ref _clamp2DelayTime2, value); }
        }
        private int _clamp2DelayTime2;
    }
}
