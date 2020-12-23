using EPCIO;
using OEP520G.Core;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace OEP520G.Parameter.ViewModels
{
    public class SystemReferPointViewModel : BindableBase, IActiveAware
    {
        private readonly Machine machine = Machine.Instance;
        private readonly Epcio epcio = Epcio.Instance;

        // 指令繫結
        public DelegateCommand<string> GetCoorCommand { get; set; }
        public DelegateCommand<string> MovtToHereCommand { get; set; }

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
            }
        }

        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public SystemReferPointViewModel()
        {
            // 指令繫結
            GetCoorCommand = new DelegateCommand<string>(GetCoor);
            MovtToHereCommand = new DelegateCommand<string>(MovtToHere);

            // 全域Save事件
            WriteDataCommand = new DelegateCommand(WriteData);
            ApplicationCommands.WriteCommand.RegisterCommand(WriteDataCommand);

            GetParameter();
        }

        /********************
         * 參數作業
         *******************/
        /// <summary>
        /// 取得參數值
        /// </summary>
        public void GetParameter()
        {
            DatumPoint1X = machine.DatumPoint1.Position.X;
            DatumPoint1Y = machine.DatumPoint1.Position.Y;
            DatumPoint1Z = machine.DatumPoint1.Position.Z;

            Frequency1 = machine.DatumPoint1.Frequency;
            Tolerance1 = machine.DatumPoint1.Tolerance;

            DatumPoint2X = machine.DatumPoint2.Position.X;
            DatumPoint2Y = machine.DatumPoint2.Position.Y;
            DatumPoint2Z = machine.DatumPoint2.Position.Z;
            Frequency2 = machine.DatumPoint2.Frequency;
            Tolerance2 = machine.DatumPoint2.Tolerance;

            AssembleDiscardBoxX = machine.AssembleDiscardBox.X;
            AssembleDiscardBoxY = machine.AssembleDiscardBox.Y;
            AssembleDiscardBoxZ = machine.AssembleDiscardBox.Z;
            AssembleMeasureHeightPlatformX = machine.AssembleMeasureHeightPlatform.X;
            AssembleMeasureHeightPlatformY = machine.AssembleMeasureHeightPlatform.Y;
            AssembleMeasureHeightPlatformZ = machine.AssembleMeasureHeightPlatform.Z;
            AssembleClayX = machine.AssembleClay.X;
            AssembleClayY = machine.AssembleClay.Y;
            AssembleClayZ = machine.AssembleClay.Z;

            SemiFinishedDiscardBoxX = machine.SemiFinishedDiscardBox.X;
            SemiFinishedDiscardBoxY = machine.SemiFinishedDiscardBox.Y;
            SemiFinishedDiscardBoxZ = machine.SemiFinishedDiscardBox.Z;
            SemiFinishedMeasureHeightPlatformX = machine.SemiFinishedMeasureHeightPlatform.X;
            SemiFinishedMeasureHeightPlatformY = machine.SemiFinishedMeasureHeightPlatform.Y;
            SemiFinishedMeasureHeightPlatformZ = machine.SemiFinishedMeasureHeightPlatform.Z;
            SemiFinishedClayX = machine.FrontClay.X;
            SemiFinishedClayY = machine.FrontClay.Y;
            SemiFinishedClayZ = machine.FrontClay.Z;
        }

        /// <summary>
        /// 存回參數值
        /// </summary>
        public void WriteData()
        {
            if (IsActive)
            {
                machine.DatumPoint1.Position.X = DatumPoint1X;
                machine.DatumPoint1.Position.Y = DatumPoint1Y;
                machine.DatumPoint1.Position.Z = DatumPoint1Z;
                machine.DatumPoint1.Frequency = Frequency1;
                machine.DatumPoint1.Tolerance = Tolerance1;

                machine.DatumPoint2.Position.X = DatumPoint2X;
                machine.DatumPoint2.Position.Y = DatumPoint2Y;
                machine.DatumPoint2.Position.Z = DatumPoint2Z;
                machine.DatumPoint2.Frequency = Frequency2;
                machine.DatumPoint2.Tolerance = Tolerance2;

                machine.AssembleDiscardBox.X = AssembleDiscardBoxX;
                machine.AssembleDiscardBox.Y = AssembleDiscardBoxY;
                machine.AssembleDiscardBox.Z = AssembleDiscardBoxZ;
                machine.AssembleMeasureHeightPlatform.X = AssembleMeasureHeightPlatformX;
                machine.AssembleMeasureHeightPlatform.Y = AssembleMeasureHeightPlatformY;
                machine.AssembleMeasureHeightPlatform.Z = AssembleMeasureHeightPlatformZ;
                machine.AssembleClay.X = AssembleClayX;
                machine.AssembleClay.Y = AssembleClayY;
                machine.AssembleClay.Z = AssembleClayZ;

                machine.SemiFinishedDiscardBox.X = SemiFinishedDiscardBoxX;
                machine.SemiFinishedDiscardBox.Y = SemiFinishedDiscardBoxY;
                machine.SemiFinishedDiscardBox.Z = SemiFinishedDiscardBoxZ;
                machine.SemiFinishedMeasureHeightPlatform.X = SemiFinishedMeasureHeightPlatformX;
                machine.SemiFinishedMeasureHeightPlatform.Y = SemiFinishedMeasureHeightPlatformY;
                machine.SemiFinishedMeasureHeightPlatform.Z = SemiFinishedMeasureHeightPlatformZ;
                machine.FrontClay.X = SemiFinishedClayX;
                machine.FrontClay.Y = SemiFinishedClayY;
                machine.FrontClay.Z = SemiFinishedClayZ;

                machine.WriteParameter();
            }
        }

        /********************
         * 按鍵
         *******************/
        /// <summary>
        /// 取得座標
        /// </summary>
        /// <param name="para">儲存目標</param>
        private void GetCoor(string para)
        {
            switch (para)
            {
                case "DATUM_POINT_1":
                    DatumPoint1X = epcio.ServoX.GetCurrentPosition();
                    DatumPoint1Y = epcio.ServoY.GetCurrentPosition();
                    break;

                case "DATUM_POINT_2":
                    DatumPoint2X = epcio.ServoX.GetCurrentPosition();
                    DatumPoint2Y = epcio.ServoY.GetCurrentPosition();
                    break;

                case "ASSEMBLE_DISCARD_BOX":
                    AssembleDiscardBoxX = epcio.ServoX.GetCurrentPosition();
                    //AssembleDiscardBoxY = epcio.ServoY.GetCurrentPosition();
                    AssembleDiscardBoxZ = epcio.ServoZ.GetCurrentPosition();
                    break;

                case "ASSEMBLE_MEASURE_HEIGHT_PLATFORM":
                    AssembleMeasureHeightPlatformX = epcio.ServoX.GetCurrentPosition();
                    AssembleMeasureHeightPlatformY = epcio.ServoY.GetCurrentPosition();
                    AssembleMeasureHeightPlatformZ = epcio.ServoZ.GetCurrentPosition();
                    break;

                case "ASSEMBLE_CLAY":
                    AssembleClayX = epcio.ServoX.GetCurrentPosition();
                    AssembleClayY = epcio.ServoY.GetCurrentPosition();
                    AssembleClayZ = epcio.ServoZ.GetCurrentPosition();
                    break;

                case "SEMI_FINISHED_DISCARD_BOX":
                    SemiFinishedDiscardBoxX = epcio.ServoClamp.GetCurrentPosition();
                    SemiFinishedDiscardBoxY = epcio.ServoY.GetCurrentPosition();
                    //SemiFinishedDiscardBoxZ = epcio.ServoZ.GetCurrentPosition();
                    break;

                    //case "SEMI_FINISHED_MEASURE_HEIGHT_PLATFORM":
                    //    SemiFinishedMeasureHeightPlatformX = epcio.ServoX.GetCurrentPosition();
                    //    SemiFinishedMeasureHeightPlatformY = epcio.ServoY.GetCurrentPosition();
                    //    SemiFinishedMeasureHeightPlatformZ = epcio.ServoZ.GetCurrentPosition();
                    //    break;

                    //case "SEMI_FINISHED_CLAY":
                    //    SemiFinishedClayX = epcio.ServoClamp.GetCurrentPosition();
                    //    SemiFinishedClayY = servos[Motion.AXIS_ID_TRAY].CurrentPosition;
                    //    SemiFinishedClayZ = epcio.ServoZ.GetCurrentPosition();
                    //    break;
            }
        }

        /// <summary>
        /// 移到此處
        /// </summary>
        /// <param name="para">移動目標</param>
        private void MovtToHere(string para)
        {
            epcio.SetSpeed(EServoSpeed.Middle);

            // 確認安全位置
            double FinalZ;
            if (epcio.IsServoZSafety())
                FinalZ = epcio.ServoZ.GetCurrentPosition();
            else
            {
                FinalZ = epcio.ServoZ.MinPosition;
                epcio.MoveTo(positionZ: FinalZ);
            }

            switch (para)
            {
                case "DATUM_POINT_1":
                    epcio.MoveTo(positionX: DatumPoint1X,
                                 positionY: DatumPoint1Y);
                    break;

                case "DATUM_POINT_2":
                    epcio.MoveTo(positionX: DatumPoint2X,
                                 positionY: DatumPoint2Y);
                    break;

                case "ASSEMBLE_DISCARD_BOX":
                    epcio.MoveTo(positionX: AssembleDiscardBoxX);
                    FinalZ = AssembleDiscardBoxZ;
                    break;

                case "ASSEMBLE_MEASURE_HEIGHT_PLATFORM":
                    epcio.MoveTo(positionX: AssembleMeasureHeightPlatformX,
                                 positionY: AssembleMeasureHeightPlatformY);
                    FinalZ = AssembleMeasureHeightPlatformZ;
                    break;

                //case "ASSEMBLE_CLAY":
                //    break;

                case "SEMI_FINISHED_DISCARD_BOX":
                    epcio.MoveTo(positionClamp: SemiFinishedDiscardBoxX,
                                 positionY: SemiFinishedDiscardBoxY);
                    break;

                    //case "SEMI_FINISHED_MEASURE_HEIGHT_PLATFORM":
                    //    break;

                    //case "SEMI_FINISHED_CLAY":
                    //    break;
            }

            epcio.MoveTo(positionZ: FinalZ);
        }

        /********************
         * 繫結
         *******************/
        // 第1基準點
        private double _datumPoint1X;
        public double DatumPoint1X
        {
            get { return _datumPoint1X; }
            set { SetProperty(ref _datumPoint1X, value); }
        }

        private double _datumPoint1Y;
        public double DatumPoint1Y
        {
            get { return _datumPoint1Y; }
            set { SetProperty(ref _datumPoint1Y, value); }
        }

        private double _datumPoint1Z;
        public double DatumPoint1Z
        {
            get { return _datumPoint1Z; }
            set { SetProperty(ref _datumPoint1Z, value); }
        }

        private int _frequency1;
        public int Frequency1
        {
            get { return _frequency1; }
            set { SetProperty(ref _frequency1, value); }
        }

        private double _tolerance1;
        public double Tolerance1
        {
            get { return _tolerance1; }
            set { SetProperty(ref _tolerance1, value); }
        }

        // 第2基準點
        private double _datumPoint2X;
        public double DatumPoint2X
        {
            get { return _datumPoint2X; }
            set { SetProperty(ref _datumPoint2X, value); }
        }

        private double _datumPoint2Y;
        public double DatumPoint2Y
        {
            get { return _datumPoint2Y; }
            set { SetProperty(ref _datumPoint2Y, value); }
        }

        private double _datumPoint2Z;
        public double DatumPoint2Z
        {
            get { return _datumPoint2Z; }
            set { SetProperty(ref _datumPoint2Z, value); }
        }

        private int _frequency2;
        public int Frequency2
        {
            get { return _frequency2; }
            set { SetProperty(ref _frequency2, value); }
        }

        private double _tolerance2;
        public double Tolerance2
        {
            get { return _tolerance2; }
            set { SetProperty(ref _tolerance2, value); }
        }

        // 後方抛料盒
        private double _assembleDiscardBoxX;
        public double AssembleDiscardBoxX
        {
            get { return _assembleDiscardBoxX; }
            set { SetProperty(ref _assembleDiscardBoxX, value); }
        }

        private double _assembleDiscardBoxY;
        public double AssembleDiscardBoxY
        {
            get { return _assembleDiscardBoxY; }
            set { SetProperty(ref _assembleDiscardBoxY, value); }
        }

        private double _assembleDiscardBoxZ;
        public double AssembleDiscardBoxZ
        {
            get { return _assembleDiscardBoxZ; }
            set { SetProperty(ref _assembleDiscardBoxZ, value); }
        }

        // 後方測高平台
        private double _assembleMeasureHeightPlatformX;
        public double AssembleMeasureHeightPlatformX
        {
            get { return _assembleMeasureHeightPlatformX; }
            set { SetProperty(ref _assembleMeasureHeightPlatformX, value); }
        }

        private double _assembleMeasureHeightPlatformY;
        public double AssembleMeasureHeightPlatformY
        {
            get { return _assembleMeasureHeightPlatformY; }
            set { SetProperty(ref _assembleMeasureHeightPlatformY, value); }
        }

        private double _assembleMeasureHeightPlatformZ;
        public double AssembleMeasureHeightPlatformZ
        {
            get { return _assembleMeasureHeightPlatformZ; }
            set { SetProperty(ref _assembleMeasureHeightPlatformZ, value); }
        }

        // 後方黏土
        private double _assembleClayX;
        public double AssembleClayX
        {
            get { return _assembleClayX; }
            set { SetProperty(ref _assembleClayX, value); }
        }

        private double _assembleClayY;
        public double AssembleClayY
        {
            get { return _assembleClayY; }
            set { SetProperty(ref _assembleClayY, value); }
        }

        private double _assembleClayZ;
        public double AssembleClayZ
        {
            get { return _assembleClayZ; }
            set { SetProperty(ref _assembleClayZ, value); }
        }

        // 前方抛料盒
        private double _semiFinishedDiscardBoxX;
        public double SemiFinishedDiscardBoxX
        {
            get { return _semiFinishedDiscardBoxX; }
            set { SetProperty(ref _semiFinishedDiscardBoxX, value); }
        }

        private double _semiFinishedDiscardBoxY;
        public double SemiFinishedDiscardBoxY
        {
            get { return _semiFinishedDiscardBoxY; }
            set { SetProperty(ref _semiFinishedDiscardBoxY, value); }
        }

        private double _semiFinishedDiscardBoxZ;
        public double SemiFinishedDiscardBoxZ
        {
            get { return _semiFinishedDiscardBoxZ; }
            set { SetProperty(ref _semiFinishedDiscardBoxZ, value); }
        }

        // 前方測高平台
        private double _semiFinishedMeasureHeightPlatformX;
        public double SemiFinishedMeasureHeightPlatformX
        {
            get { return _semiFinishedMeasureHeightPlatformX; }
            set { SetProperty(ref _semiFinishedMeasureHeightPlatformX, value); }
        }

        private double _semiFinishedMeasureHeightPlatformY;
        public double SemiFinishedMeasureHeightPlatformY
        {
            get { return _semiFinishedMeasureHeightPlatformY; }
            set { SetProperty(ref _semiFinishedMeasureHeightPlatformY, value); }
        }

        private double _semiFinishedMeasureHeightPlatformZ;
        public double SemiFinishedMeasureHeightPlatformZ
        {
            get { return _semiFinishedMeasureHeightPlatformZ; }
            set { SetProperty(ref _semiFinishedMeasureHeightPlatformZ, value); }
        }

        // 前方黏土
        private double _semiFinishedClayX;
        public double SemiFinishedClayX
        {
            get { return _semiFinishedClayX; }
            set { SetProperty(ref _semiFinishedClayX, value); }
        }

        private double _semiFinishedClayY;
        public double SemiFinishedClayY
        {
            get { return _semiFinishedClayY; }
            set { SetProperty(ref _semiFinishedClayY, value); }
        }

        private double _semiFinishedClayZ;
        public double SemiFinishedClayZ
        {
            get { return _semiFinishedClayZ; }
            set { SetProperty(ref _semiFinishedClayZ, value); }
        }
    }
}
