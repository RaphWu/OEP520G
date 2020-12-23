using EPCIO;
using OEP520G.Core;
using OEP520G.Parameter;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace OEP520G.Teaching.ViewModels
{
    public class RotateShiftData : StageRotateShiftData
    {
        public double MeasureX { get; set; }
        public double MeasureY { get; set; }
    }

    public class EccentricCorrectViewModel : BindableBase, IActiveAware
    {
        private readonly Epcio epcio = Epcio.Instance;

        private Stage stage = Stage.Instance;
        private ObservableCollection<RotateShiftData> rotateShift = new ObservableCollection<RotateShiftData>();

        // 按鍵
        public DelegateCommand MoveToVisionCenterCommand { get; private set; }
        public DelegateCommand AxisRZeroingCommand { get; private set; }
        public DelegateCommand StartCorrectCommand { get; private set; }
        public DelegateCommand CrooectPauseCommand { get; private set; }
        public DelegateCommand StopCorrectCommand { get; private set; }
        public DelegateCommand DataResetCommand { get; private set; }

        // 視窗Active/Deactive
        public event EventHandler IsActiveChanged;
        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public EccentricCorrectViewModel()
        {
            InZeroEnabled = true;
            btnEnabled_InCorrect(false);

            IntervalList = new int[4] { 1, 2, 5, 10 };
            GetData();

            MoveToVisionCenterCommand = new DelegateCommand(MoveToVisionCenter);
            AxisRZeroingCommand = new DelegateCommand(AxisRZeroing);
            StartCorrectCommand = new DelegateCommand(StartCorrect);
            CrooectPauseCommand = new DelegateCommand(CrooectPause);
            StopCorrectCommand = new DelegateCommand(StopCorrect);
            DataResetCommand = new DelegateCommand(DataReset);

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
                stage.StartAngle = StartAngle;
                stage.EndAngle = EndAngle;
                stage.IntervalAngle = IntervalAngle;

                stage.StageRotateShift.Clear();
                foreach (RotateShiftData rst in rotateShift)
                {
                    stage.StageRotateShift.Add(new StageRotateShiftData
                    {
                        Angle = rst.Angle,
                        ShiftX = rst.ShiftX,
                        ShiftY = rst.ShiftY
                    });
                }

                stage.WriteParameter();
            }
        }

        /// <summary>
        /// 取得參數值
        /// </summary>
        private void GetData()
        {
            StartAngle = stage.StartAngle;
            EndAngle = stage.EndAngle;
            IntervalAngle = stage.IntervalAngle;

            rotateShift.Clear();
            foreach (StageRotateShiftData srst in stage.StageRotateShift)
            {
                rotateShift.Add(new RotateShiftData
                {
                    Angle = srst.Angle,
                    ShiftX = srst.ShiftX,
                    ShiftY = srst.ShiftY
                });
            }
            RefreshDataGrid();
        }

        /********************
         * DataGrid
         *******************/
        private void RefreshDataGrid()
        {
            CorrectTable = null;
            CorrectTable = rotateShift;
        }

        /********************
         * 按鍵
         *******************/
        /// <summary>
        /// 移至畫像中心
        /// </summary>
        private void MoveToVisionCenter()
        {
        }

        /// <summary>
        /// R軸復歸
        /// </summary>
        private void AxisRZeroing()
        {
            InZeroEnabled = false;
            epcio.GoHome(servoRGoHome: true);
            //epcio.WaitingForMotionStop(waitingServoR: true);
            InZeroEnabled = true;
        }

        /********************
         * 校正程序
         *******************/
        private float angle;
        private bool correctContinue;

        /// <summary>
        /// 校正開始
        /// </summary>
        private void StartCorrect()
        {
            Common.log.WriteLine("旋轉位移校正開始.");
            btnEnabled_InCorrect(true);

            // TODO: 不需要每次都重建
            rotateShift.Clear();

            angle = StartAngle;
            correctContinue = true;

            // 移動至旋轉中心
            epcio.SetSpeed(EServoSpeed.High);
            epcio.MoveTo(positionX: stage.RotateCenter.X,
                         positionY: stage.RotateCenter.Y);

            epcio.WaitingForAllServoMotionStop();

            Task.Run(async () =>
            {
                // 角度判斷
                if (correctContinue && (angle < EndAngle))
                {
                    // 轉角度
                    epcio.MoveTo(degreeR: angle);
                    await epcio.WaitingForMotionStop(waitingServoR: true);
                }
                else
                {
                    epcio.MoveTo(degreeR: 0);
                    RefreshDataGrid();

                    correctContinue = false;
                    btnEnabled_InCorrect(false);
                    Common.log.WriteLine("旋轉位移校正結束。");
                }

                // TODO: 拍照

                Random rnd = new Random(Guid.NewGuid().GetHashCode());

                rotateShift.Add(new RotateShiftData()
                {
                    Angle = angle,
                    ShiftX = (double)rnd.Next(-10, 10) / 1000,
                    ShiftY = (double)rnd.Next(-10, 10) / 1000,
                    MeasureX = (double)rnd.Next(-10, 10) / 1000,
                    MeasureY = (double)rnd.Next(-10, 10) / 1000
                });

                angle += IntervalAngle;
            });
        }

        /// <summary>
        /// 校正暫停
        /// </summary>
        private void CrooectPause()
        {
            // TODO: 暫停功能
        }

        /// <summary>
        /// 強制停止
        /// </summary>
        private void StopCorrect() => correctContinue = false;

        /// <summary>
        /// 數據重置
        /// </summary>
        private void DataReset()
        {
            rotateShift.Clear();
        }

        private void btnEnabled_InCorrect(bool inCorrect)
        {
            if (inCorrect)
            {
                VisionEnabled = false;
                AngleEnabled = false;
                StartCorrectEnabled = false;
                PauseCorrectEnabled = true;
                StopCorrectEnabled = true;
                DataResetEnabled = false;
            }
            else
            {
                VisionEnabled = true;
                AngleEnabled = true;
                StartCorrectEnabled = true;
                PauseCorrectEnabled = false;
                StopCorrectEnabled = false;
                DataResetEnabled = true;
            }
        }

        /********************
         * 繫結
         *******************/
        private ObservableCollection<RotateShiftData> _correctTable;
        public ObservableCollection<RotateShiftData> CorrectTable
        {
            get { return _correctTable; }
            set { SetProperty(ref _correctTable, value); }
        }

        private RotateShiftData _correctTableSelected;
        public RotateShiftData CorrectTableSelected
        {
            get { return _correctTableSelected; }
            set { SetProperty(ref _correctTableSelected, value); }
        }

        private float _startAngle;
        public float StartAngle
        {
            get { return _startAngle; }
            set { SetProperty(ref _startAngle, value); }
        }

        private float _endAngle;
        public float EndAngle
        {
            get { return _endAngle; }
            set { SetProperty(ref _endAngle, value); }
        }

        private int[] _intervalList;
        public int[] IntervalList
        {
            get { return _intervalList; }
            set { SetProperty(ref _intervalList, value); }
        }

        private float _intervalAngle;
        public float IntervalAngle
        {
            get { return _intervalAngle; }
            set { SetProperty(ref _intervalAngle, value); }
        }

        private bool _inZeroEnabled;
        public bool InZeroEnabled
        {
            get { return _inZeroEnabled; }
            set { SetProperty(ref _inZeroEnabled, value); }
        }

        private bool _visionEnabled;
        public bool VisionEnabled
        {
            get { return _visionEnabled; }
            set { SetProperty(ref _visionEnabled, value); }
        }

        private bool _angleEnabled;
        public bool AngleEnabled
        {
            get { return _angleEnabled; }
            set { SetProperty(ref _angleEnabled, value); }
        }

        private bool _startCorrectEnabled;
        public bool StartCorrectEnabled
        {
            get { return _startCorrectEnabled; }
            set { SetProperty(ref _startCorrectEnabled, value); }
        }

        private bool _pauseCorrectEnabled;
        public bool PauseCorrectEnabled
        {
            get { return _pauseCorrectEnabled; }
            set { SetProperty(ref _pauseCorrectEnabled, value); }
        }

        private bool _stopCorrectEnabled;
        public bool StopCorrectEnabled
        {
            get { return _stopCorrectEnabled; }
            set { SetProperty(ref _stopCorrectEnabled, value); }
        }

        private bool _dataResetEnabled;
        public bool DataResetEnabled
        {
            get { return _dataResetEnabled; }
            set { SetProperty(ref _dataResetEnabled, value); }
        }
    }
}
