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
using System.Threading.Tasks;
using System.Windows;

namespace OEP520G.Teaching.ViewModels
{
    public class NozzleCorrectViewModel : BindableBase, IActiveAware
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly Machine machine = Machine.Instance;
        private readonly Nozzle nozzle = Nozzle.Instance;
        private readonly Dispenser dispenser = Dispenser.Instance;
        private readonly ObjectMotion objectMoving = new ObjectMotion();

        // 選擇物件
        private readonly List<string> FuncList = new List<string> { "固定相機", "黏土壓印", "視覺校正" };
        private readonly List<string> ConfirmContent = new List<string> { "取得座標", "黏土壓印置中確認", "吸嘴置中確認" };
        private const int FUNC_FIXCAMERA = 0;
        private const int FUNC_CLAY = 1;
        private const int FUNC_VISUAL = 2;

        private RemoteIo[] activeObject;

        /// <summary>
        /// 物件ID 0:膠針 1~11:吸嘴
        /// </summary>
        private int selectObjectId;

        // 座標暫存區
        private PointXYZ[] buffPosition = new PointXYZ[Nozzle.MAX_NOZZLE + 1];
        private LongPointXYZ[] buffPulse = new LongPointXYZ[Nozzle.MAX_NOZZLE + 1];
        private IntPointXYZ[] buffEncoder = new IntPointXYZ[Nozzle.MAX_NOZZLE + 1];
        private PointXY[] buffDistance = new PointXY[Nozzle.MAX_NOZZLE + 1];

        // 按鍵
        public DelegateCommand StartCorrectCommand { get; private set; }
        public DelegateCommand UpdateDataCommand { get; private set; }
        public DelegateCommand HeightMeasureCommand { get; private set; }
        public DelegateCommand CancelCorrectCommand { get; private set; }
        public DelegateCommand RestoreCoordinateCommand { get; private set; }
        public DelegateCommand ResetCoordinateCommand { get; private set; }
        public DelegateCommand NozzleUpCommand { get; private set; }
        public DelegateCommand NozzleToCcdCommand { get; private set; }
        public DelegateCommand CcdToNozzleCommand { get; private set; }

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
                ReadData();
            }
        }

        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public NozzleCorrectViewModel()
        {
            TargetObjectSelect = "N1";

            for (int idx = 0; idx <= Nozzle.MAX_NOZZLE; idx++)
            {
                buffPosition[idx] = new PointXYZ();
                buffPulse[idx] = new LongPointXYZ();
                buffEncoder[idx] = new IntPointXYZ();
                buffDistance[idx] = new PointXY();
            }

            activeObject = new RemoteIo[12]
            {
                epcio.DispenserCylinder,
                epcio.Nozzle01_Cylinder,
                epcio.Nozzle02_Cylinder,
                epcio.Nozzle03_Cylinder,
                epcio.Nozzle04_Cylinder,
                epcio.Nozzle05_Cylinder,
                epcio.Nozzle06_Cylinder,
                epcio.Nozzle07_Cylinder,
                epcio.Nozzle08_Cylinder,
                epcio.Nozzle09_Cylinder,
                epcio.Nozzle10_Cylinder,
                epcio.Nozzle11_Cylinder
            };

            ToolSelectSource = FuncList;
            ToolSelectItem = FuncList[FUNC_FIXCAMERA];
            ToolEnabled = true;

            ReadData();

            // 按鍵
            StartCorrectCommand = new DelegateCommand(StartCorrect);
            UpdateDataCommand = new DelegateCommand(UpdateData);
            HeightMeasureCommand = new DelegateCommand(HeightMeasure);
            RestoreCoordinateCommand = new DelegateCommand(RestoreCoordinate);
            ResetCoordinateCommand = new DelegateCommand(ResetCoordinate);
            NozzleUpCommand = new DelegateCommand(NozzleUp);
            NozzleToCcdCommand = new DelegateCommand(NozzleToCcd);
            CcdToNozzleCommand = new DelegateCommand(CcdToNozzle);

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
                // 點膠針
                dispenser.Position = buffPosition[0];
                dispenser.Pulse = buffPulse[0];
                dispenser.Encoder = buffEncoder[0];
                dispenser.Distance = buffDistance[0];
                dispenser.WriteParameter();

                // 吸嘴
                for (int noz = 1; noz <= Nozzle.MAX_NOZZLE; noz++)
                {
                    nozzle.NozzleList[noz - 1].Position = buffPosition[noz];
                    nozzle.NozzleList[noz - 1].Pulse = buffPulse[noz];
                    nozzle.NozzleList[noz - 1].Encoder = buffEncoder[noz];
                    nozzle.NozzleList[noz - 1].DistanceToMoveCamera = buffDistance[noz];
                }
                nozzle.WriteParameter();
                machine.WriteParameter();
            }
        }

        /// <summary>
        /// 讀取值
        /// </summary>
        private void ReadData()
        {
            // 點膠針
            buffPosition[0] = dispenser.Position;
            buffPulse[0] = dispenser.Pulse;
            buffEncoder[0] = dispenser.Encoder;
            buffDistance[0] = dispenser.Distance;

            DispenserNeedleX = buffDistance[0].X;
            DispenserNeedleZ = buffDistance[0].Y;

            // 吸嘴
            for (int noz = 1; noz <= Nozzle.MAX_NOZZLE; noz++)
            {
                buffPosition[noz] = nozzle.NozzleList[noz - 1].Position;
                buffPulse[noz] = nozzle.NozzleList[noz - 1].Pulse;
                buffEncoder[noz] = nozzle.NozzleList[noz - 1].Encoder;
                buffDistance[noz] = nozzle.NozzleList[noz - 1].DistanceToMoveCamera;
            }

            Nozzle1X = buffDistance[1].X;
            Nozzle1Y = buffDistance[1].Y;
            Nozzle1Z = buffPosition[1].Z;

            Nozzle2X = buffDistance[2].X;
            Nozzle2Y = buffDistance[2].Y;
            Nozzle2Z = buffPosition[2].Z;

            Nozzle3X = buffDistance[3].X;
            Nozzle3Y = buffDistance[3].Y;
            Nozzle3Z = buffPosition[3].Z;

            Nozzle4X = buffDistance[4].X;
            Nozzle4Y = buffDistance[4].Y;
            Nozzle4Z = buffPosition[4].Z;

            Nozzle5X = buffDistance[5].X;
            Nozzle5Y = buffDistance[5].Y;
            Nozzle5Z = buffPosition[5].Z;

            Nozzle6X = buffDistance[6].X;
            Nozzle6Y = buffDistance[6].Y;
            Nozzle6Z = buffPosition[6].Z;

            Nozzle7X = buffDistance[7].X;
            Nozzle7Y = buffDistance[7].Y;
            Nozzle7Z = buffPosition[7].Z;

            Nozzle8X = buffDistance[8].X;
            Nozzle8Y = buffDistance[8].Y;
            Nozzle8Z = buffPosition[8].Z;

            Nozzle9X = buffDistance[9].X;
            Nozzle9Y = buffDistance[9].Y;
            Nozzle9Z = buffPosition[9].Z;

            Nozzle10X = buffDistance[10].X;
            Nozzle10Y = buffDistance[10].Y;
            Nozzle10Z = buffPosition[10].Z;

            Nozzle11X = buffDistance[11].X;
            Nozzle11Y = buffDistance[11].Y;
            Nozzle11Z = buffPosition[11].Z;
        }

        /********************
         * 按鍵
         *******************/
        /// <summary>
        /// 開始校正
        /// </summary>
        private async void StartCorrect()
        {
            // 吸嘴上昇
            NozzleUp();

            // 物件ID錯誤則離開
            if ((selectObjectId < 0) || (selectObjectId > Nozzle.MAX_NOZZLE))
                return;

            // 取得buffCoor
            GetCoordinate();

            await epcio.MoveServoZToSafety();

            // 校正對象分支: "固定相機", "黏土壓印", "視覺校正"
            ToolEnabled = false;
            switch (ToolSelectIndex)
            {
                case FUNC_FIXCAMERA:
                    //if (targetObject == 0)
                    //    FixCamera_Dispenser_Correct();
                    //else
                    await FixCamera_Nozzle_Correct();
                    break;

                case FUNC_CLAY:
                    await epcio.WaitingForMotionStop(waitingServoZ: true);
                    await MoveCamera_Clay_Nozzle_Correct();
                    break;

                case FUNC_VISUAL:
                    //epcio.WaitForAxisStopCallback(Motion.AXIS_ID_Z, FixCamera_Visual_Correct);
                    break;
            }
        }

        /// <summary>
        /// 固定相機-吸嘴校正
        /// </summary>
        private async Task FixCamera_Nozzle_Correct()
        {
            await epcio.WaitingForMotionStop(waitingServoZ: true);
            epcio.SetSpeed(EServoSpeed.High);

            // X軸定位
            if (selectObjectId == 0)
                await objectMoving.DispenserToFixCamera(buffPosition[0].Z);
            else
                await objectMoving.NozzleToFixCamera((ENozzleId)selectObjectId - 1, buffPosition[selectObjectId].Z);

            await epcio.WaitingForMotionStop(waitingServoX: true);

            // Z軸下降、等待校正完成
            epcio.MoveTo(positionZ: buffPosition[selectObjectId].Z);
            epcio.RioOutput(activeObject[selectObjectId], true);

            // 按鍵提示
            Task<MessageBoxResult> actionSelect = new Task<MessageBoxResult>(() =>
            {
                MessageBoxResult result = MessageBox.Show("將吸嘴在相機中置中，按下【確定】後取得座標值。", "動作提示",
                    MessageBoxButton.OKCancel, MessageBoxImage.Information, MessageBoxResult.OK,
                    MessageBoxOptions.DefaultDesktopOnly);

                ToolEnabled = true;
                return result;
            });
            actionSelect.Start();
            await actionSelect.ContinueWith(x =>
            {
                if (x.Result == MessageBoxResult.OK)
                {
                    var servoX = epcio.ServoX;
                    var servoY = epcio.ServoY;
                    var servoZ = epcio.ServoZ;

                    // Position
                    buffPosition[selectObjectId].X = servoX.GetCurrentPosition();
                    buffPosition[selectObjectId].Y = servoY.GetCurrentPosition();
                    buffPosition[selectObjectId].Z = servoZ.GetCurrentPosition();

                    // Pulse
                    buffPulse[selectObjectId].X = servoX.GetCurrentPulse();
                    buffPulse[selectObjectId].Y = servoY.GetCurrentPulse();
                    buffPulse[selectObjectId].Z = servoZ.GetCurrentPulse();

                    // Encoder
                    buffEncoder[selectObjectId].X = servoX.GetCurrentEncoder();
                    buffEncoder[selectObjectId].Y = servoY.GetCurrentEncoder();
                    buffEncoder[selectObjectId].Z = servoZ.GetCurrentEncoder();

                    // Distance
                    int baseId = (int)nozzle.DatumNozzleId;
                    int nozzleId = selectObjectId - 1;

                    // 吸嘴與DP相對位置
                    buffDistance[selectObjectId].X
                        = buffPosition[selectObjectId].X - machine.DatumPoint1.Position.X; // TODO: 拍照

                    buffDistance[selectObjectId].Y
                        = buffPosition[selectObjectId].Y - machine.DatumPoint1.Position.Y; // TODO: 拍照
                    
                    UpdateCoordinate();
                }

                ToolEnabled = true;
            });
        }

        /// <summary>
        /// 移動相機-黏土壓印
        /// </summary>
        private async Task MoveCamera_Clay_Nozzle_Correct()
        {
            ToolEnabled = false;

            PointXY posClay = new PointXY(); // 黏土座標
            PointXY posCamera = new PointXY(); // 相機座標

            // 按鍵提示
            Task<MessageBoxResult> actionSelect = new Task<MessageBoxResult>(() =>
            {
                return MessageBox.Show("將基準吸嘴在黏土上壓印後，按下【確定】繼續。", "動作提示",
                    MessageBoxButton.OKCancel, MessageBoxImage.Information, MessageBoxResult.OK,
                    MessageBoxOptions.DefaultDesktopOnly);
            });
            actionSelect.Start();
            await actionSelect.ContinueWith(async x =>
            {
                if (x.Result == MessageBoxResult.OK)
                {
                    // 吸嘴壓印座標
                    posClay.X = epcio.ServoX.GetCurrentPosition();
                    posClay.Y = epcio.ServoY.GetCurrentPosition();

                    // 移動相機移到黏土上
                    epcio.MoveToRelative(distanceX: machine.DatumPoint1.Position.X - posClay.X);

                    // 按鍵提示
                    Task<MessageBoxResult> actionSelect = new Task<MessageBoxResult>(() =>
                    {
                        return MessageBox.Show("將相機移至壓印中心點後，按下【確定】繼續。", "動作提示",
                            MessageBoxButton.OKCancel, MessageBoxImage.Information, MessageBoxResult.OK,
                            MessageBoxOptions.DefaultDesktopOnly);
                    });
                    actionSelect.Start();
                    await actionSelect.ContinueWith(x =>
                    {
                        if (x.Result == MessageBoxResult.OK)
                        {
                            var servoX = epcio.ServoX;
                            var servoY = epcio.ServoY;
                            var servoZ = epcio.ServoZ;

                            // Position
                            buffPosition[selectObjectId].X = servoX.GetCurrentPosition();
                            buffPosition[selectObjectId].Y = servoY.GetCurrentPosition();
                            buffPosition[selectObjectId].Z = servoZ.GetCurrentPosition();

                            // Pulse
                            buffPulse[selectObjectId].X = servoX.GetCurrentPulse();
                            buffPulse[selectObjectId].Y = servoY.GetCurrentPulse();
                            buffPulse[selectObjectId].Z = servoZ.GetCurrentPulse();

                            // Encoder
                            buffEncoder[selectObjectId].X = servoX.GetCurrentEncoder();
                            buffEncoder[selectObjectId].Y = servoY.GetCurrentEncoder();
                            buffEncoder[selectObjectId].Z = servoZ.GetCurrentEncoder();

                            posCamera.X = servoX.GetCurrentPosition();
                            posCamera.Y = servoY.GetCurrentPosition();

                            // 基準吸嘴與移動相機距離
                            buffDistance[selectObjectId].X = posCamera.X - posClay.X;
                            buffDistance[selectObjectId].Y = posCamera.Y - posClay.Y;

                            UpdateCoordinate();
                        }
                    });
                }
            });

            ToolEnabled = true;
        }

        /********************
         * 固定相機-視覺校正
         *******************/
        private void FixCamera_Visual_Correct()
        {

        }

        private void FixCamera_Visual_Confirm()
        {

        }

        /********************
         * 畫面顯示處理
         *******************/
        /// <summary>
        /// 使用buffCoor更新畫面
        /// </summary>
        private void UpdateCoordinate()
        {
            switch (selectObjectId)
            {
                case 0:
                    DispenserNeedleX = buffDistance[0].X;
                    DispenserNeedleY = buffDistance[0].Y;
                    DispenserNeedleZ = buffPosition[0].Z;
                    break;
                case 1:
                    Nozzle1X = buffDistance[1].X;
                    Nozzle1Y = buffDistance[1].Y;
                    Nozzle1Z = buffPosition[1].Z;
                    break;
                case 2:
                    Nozzle2X = buffDistance[2].X;
                    Nozzle2Y = buffDistance[2].Y;
                    Nozzle2Z = buffPosition[2].Z;

                    break;
                case 3:
                    Nozzle3X = buffDistance[3].X;
                    Nozzle3Y = buffDistance[3].Y;
                    Nozzle3Z = buffPosition[3].Z;
                    break;
                case 4:
                    Nozzle4X = buffDistance[4].X;
                    Nozzle4Y = buffDistance[4].Y;
                    Nozzle4Z = buffPosition[4].Z;
                    break;
                case 5:
                    Nozzle5X = buffDistance[5].X;
                    Nozzle5Y = buffDistance[5].Y;
                    Nozzle5Z = buffPosition[5].Z;
                    break;
                case 6:
                    Nozzle6X = buffDistance[6].X;
                    Nozzle6Y = buffDistance[6].Y;
                    Nozzle6Z = buffPosition[6].Z;
                    break;
                case 7:
                    Nozzle7X = buffDistance[7].X;
                    Nozzle7Y = buffDistance[7].Y;
                    Nozzle7Z = buffPosition[7].Z;
                    break;
                case 8:
                    Nozzle8X = buffDistance[8].X;
                    Nozzle8Y = buffDistance[8].Y;
                    Nozzle8Z = buffPosition[8].Z;
                    break;
                case 9:
                    Nozzle9X = buffDistance[9].X;
                    Nozzle9Y = buffDistance[9].Y;
                    Nozzle9Z = buffPosition[9].Z;
                    break;
                case 10:
                    Nozzle10X = buffDistance[10].X;
                    Nozzle10Y = buffDistance[10].Y;
                    Nozzle10Z = buffPosition[10].Z;
                    break;
                case 11:
                    Nozzle11X = buffDistance[11].X;
                    Nozzle11Y = buffDistance[11].Y;
                    Nozzle11Z = buffPosition[11].Z;
                    break;
            }
        }

        /// <summary>
        /// 取得畫面選取物件的座標設定值到buffCoor
        /// </summary>
        private void GetCoordinate()
        {
            switch (selectObjectId)
            {
                case 0:
                    buffDistance[0].X = DispenserNeedleX;
                    buffDistance[0].Y = DispenserNeedleY;
                    buffPosition[0].Z = DispenserNeedleZ;
                    break;
                case 1:
                    buffDistance[1].X = Nozzle1X;
                    buffDistance[1].Y = Nozzle1Y;
                    buffPosition[1].Z = Nozzle1Z;
                    break;
                case 2:
                    buffDistance[2].X = Nozzle2X;
                    buffDistance[2].Y = Nozzle2Y;
                    buffPosition[2].Z = Nozzle2Z;
                    break;
                case 3:
                    buffDistance[3].X = Nozzle3X;
                    buffDistance[3].Y = Nozzle3Y;
                    buffPosition[3].Z = Nozzle3Z;
                    break;
                case 4:
                    buffDistance[4].X = Nozzle4X;
                    buffDistance[4].Y = Nozzle4Y;
                    buffPosition[4].Z = Nozzle4Z;
                    break;
                case 5:
                    buffDistance[5].X = Nozzle5X;
                    buffDistance[5].Y = Nozzle5Y;
                    buffPosition[5].Z = Nozzle5Z;
                    break;
                case 6:
                    buffDistance[6].X = Nozzle6X;
                    buffDistance[6].Y = Nozzle6Y;
                    buffPosition[6].Z = Nozzle6Z;
                    break;
                case 7:
                    buffDistance[7].X = Nozzle7X;
                    buffDistance[7].Y = Nozzle7Y;
                    buffPosition[7].Z = Nozzle7Z;
                    break;
                case 8:
                    buffDistance[8].X = Nozzle8X;
                    buffDistance[8].Y = Nozzle8Y;
                    buffPosition[8].Z = Nozzle8Z;
                    break;
                case 9:
                    buffDistance[9].X = Nozzle9X;
                    buffDistance[9].Y = Nozzle9Y;
                    buffPosition[9].Z = Nozzle9Z;
                    break;
                case 10:
                    buffDistance[10].X = Nozzle10X;
                    buffDistance[10].Y = Nozzle10Y;
                    buffPosition[10].Z = Nozzle10Z;
                    break;
                case 11:
                    buffDistance[11].X = Nozzle11X;
                    buffDistance[11].Y = Nozzle11Y;
                    buffPosition[11].Z = Nozzle11Z;
                    break;
            }
        }

        /********************
         * 共用Function
         *******************/
        /// <summary>
        /// 更新位移資料
        /// </summary>
        private void UpdateData() => WriteData();

        private void RestoreCoordinate()
        {
            if (MessageBox.Show("確定讀取原座標資料？\n注意：尚未更新的座標會被覆蓋丟失。", "資料讀取",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                ReadData();
        }

        /// <summary>
        /// 全部座標歸零
        /// </summary>
        private void ResetCoordinate()
        {
            if (MessageBox.Show("確定將全部座標歸零？\n注意：尚未更新的座標會被覆蓋丟失。\n注意：資料庫內仍為舊座標。", "資料歸零",
                MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                for (int obj = 0; obj <= Nozzle.MAX_NOZZLE; obj++)
                {
                    buffDistance[obj].X = 0;
                    buffDistance[obj].Y = 0;
                    buffPosition[obj].Z = 0;
                }
                UpdateCoordinate();
            }
        }

        /// <summary>
        /// 測高
        /// </summary>
        private void HeightMeasure()
        {

        }

        /// <summary>
        /// 吸嘴上昇
        /// </summary>
        private void NozzleUp() => epcio.SafetyPosition();

        /// <summary>
        /// Nozzle => CCD
        /// </summary>
        private void NozzleToCcd()
        {
        }

        /// <summary>
        /// CCD => Nozzle
        /// </summary>
        private void CcdToNozzle()
        {
        }

        /********************
         * 繫結
         *******************/
        // 選擇物件
        private string _targetObjectSelect;
        public string TargetObjectSelect
        {
            get { return _targetObjectSelect; }
            set
            {
                SetProperty(ref _targetObjectSelect, value);

                if (value == "NEEDLE")
                    selectObjectId = 0;
                else
                    selectObjectId = int.Parse(value.Substring(1));
            }
        }

        // 校正工具
        private List<string> _toolSelectSource;
        public List<string> ToolSelectSource
        {
            get { return _toolSelectSource; }
            set { SetProperty(ref _toolSelectSource, value); }
        }

        private int _toolSelectIndex;
        public int ToolSelectIndex
        {
            get { return _toolSelectIndex; }
            set { SetProperty(ref _toolSelectIndex, value); }
        }

        private bool _toolEnabled;
        public bool ToolEnabled
        {
            get { return _toolEnabled; }
            set { SetProperty(ref _toolEnabled, value); }
        }

        private string _toolSelectItem;
        public string ToolSelectItem
        {
            get { return _toolSelectItem; }
            set
            {
                SetProperty(ref _toolSelectItem, value);
                NozzleImageVisibility = FuncList[FUNC_VISUAL] == value;
            }
        }

        // 座標表
        private double _dispenserNeedleX;
        public double DispenserNeedleX
        {
            get { return _dispenserNeedleX; }
            set { SetProperty(ref _dispenserNeedleX, value); }
        }

        private double _dispenserNeedleY;
        public double DispenserNeedleY
        {
            get { return _dispenserNeedleY; }
            set { SetProperty(ref _dispenserNeedleY, value); }
        }

        private double _dispenserNeedleZ;
        public double DispenserNeedleZ
        {
            get { return _dispenserNeedleZ; }
            set { SetProperty(ref _dispenserNeedleZ, value); }
        }

        private double _zozzle1X;
        public double Nozzle1X
        {
            get { return _zozzle1X; }
            set { SetProperty(ref _zozzle1X, value); }
        }

        private double _zozzle1Y;
        public double Nozzle1Y
        {
            get { return _zozzle1Y; }
            set { SetProperty(ref _zozzle1Y, value); }
        }

        private double _zozzle1Z;
        public double Nozzle1Z
        {
            get { return _zozzle1Z; }
            set { SetProperty(ref _zozzle1Z, value); }
        }

        private double _zozzle2X;
        public double Nozzle2X
        {
            get { return _zozzle2X; }
            set { SetProperty(ref _zozzle2X, value); }
        }

        private double _zozzle2Y;
        public double Nozzle2Y
        {
            get { return _zozzle2Y; }
            set { SetProperty(ref _zozzle2Y, value); }
        }

        private double _zozzle2Z;
        public double Nozzle2Z
        {
            get { return _zozzle2Z; }
            set { SetProperty(ref _zozzle2Z, value); }
        }

        private double _zozzle3X;
        public double Nozzle3X
        {
            get { return _zozzle3X; }
            set { SetProperty(ref _zozzle3X, value); }
        }

        private double _zozzle3Y;
        public double Nozzle3Y
        {
            get { return _zozzle3Y; }
            set { SetProperty(ref _zozzle3Y, value); }
        }

        private double _zozzle3Z;
        public double Nozzle3Z
        {
            get { return _zozzle3Z; }
            set { SetProperty(ref _zozzle3Z, value); }
        }

        private double _zozzle4X;
        public double Nozzle4X
        {
            get { return _zozzle4X; }
            set { SetProperty(ref _zozzle4X, value); }
        }

        private double _zozzle4Y;
        public double Nozzle4Y
        {
            get { return _zozzle4Y; }
            set { SetProperty(ref _zozzle4Y, value); }
        }

        private double _zozzle4Z;
        public double Nozzle4Z
        {
            get { return _zozzle4Z; }
            set { SetProperty(ref _zozzle4Z, value); }
        }

        private double _zozzle5X;
        public double Nozzle5X
        {
            get { return _zozzle5X; }
            set { SetProperty(ref _zozzle5X, value); }
        }

        private double _zozzle5Y;
        public double Nozzle5Y
        {
            get { return _zozzle5Y; }
            set { SetProperty(ref _zozzle5Y, value); }
        }

        private double _zozzle5Z;
        public double Nozzle5Z
        {
            get { return _zozzle5Z; }
            set { SetProperty(ref _zozzle5Z, value); }
        }

        private double _zozzle6X;
        public double Nozzle6X
        {
            get { return _zozzle6X; }
            set { SetProperty(ref _zozzle6X, value); }
        }

        private double _zozzle6Y;
        public double Nozzle6Y
        {
            get { return _zozzle6Y; }
            set { SetProperty(ref _zozzle6Y, value); }
        }

        private double _zozzle6Z;
        public double Nozzle6Z
        {
            get { return _zozzle6Z; }
            set { SetProperty(ref _zozzle6Z, value); }
        }

        private double _zozzle7X;
        public double Nozzle7X
        {
            get { return _zozzle7X; }
            set { SetProperty(ref _zozzle7X, value); }
        }

        private double _zozzle7Y;
        public double Nozzle7Y
        {
            get { return _zozzle7Y; }
            set { SetProperty(ref _zozzle7Y, value); }
        }

        private double _zozzle7Z;
        public double Nozzle7Z
        {
            get { return _zozzle7Z; }
            set { SetProperty(ref _zozzle7Z, value); }
        }

        private double _zozzle8X;
        public double Nozzle8X
        {
            get { return _zozzle8X; }
            set { SetProperty(ref _zozzle8X, value); }
        }

        private double _zozzle8Y;
        public double Nozzle8Y
        {
            get { return _zozzle8Y; }
            set { SetProperty(ref _zozzle8Y, value); }
        }

        private double _zozzle8Z;
        public double Nozzle8Z
        {
            get { return _zozzle8Z; }
            set { SetProperty(ref _zozzle8Z, value); }
        }

        private double _zozzle9X;
        public double Nozzle9X
        {
            get { return _zozzle9X; }
            set { SetProperty(ref _zozzle9X, value); }
        }

        private double _zozzle9Y;
        public double Nozzle9Y
        {
            get { return _zozzle9Y; }
            set { SetProperty(ref _zozzle9Y, value); }
        }

        private double _zozzle9Z;
        public double Nozzle9Z
        {
            get { return _zozzle9Z; }
            set { SetProperty(ref _zozzle9Z, value); }
        }

        private double _zozzle10X;
        public double Nozzle10X
        {
            get { return _zozzle10X; }
            set { SetProperty(ref _zozzle10X, value); }
        }

        private double _zozzle10Y;
        public double Nozzle10Y
        {
            get { return _zozzle10Y; }
            set { SetProperty(ref _zozzle10Y, value); }
        }

        private double _zozzle10Z;
        public double Nozzle10Z
        {
            get { return _zozzle10Z; }
            set { SetProperty(ref _zozzle10Z, value); }
        }

        private double _zozzle11X;
        public double Nozzle11X
        {
            get { return _zozzle11X; }
            set { SetProperty(ref _zozzle11X, value); }
        }

        private double _zozzle11Y;
        public double Nozzle11Y
        {
            get { return _zozzle11Y; }
            set { SetProperty(ref _zozzle11Y, value); }
        }

        private double _zozzle11Z;
        public double Nozzle11Z
        {
            get { return _zozzle11Z; }
            set { SetProperty(ref _zozzle11Z, value); }
        }

        // 畫像
        private List<string> _nozzleImageSource;
        public List<string> NozzleImageSource
        {
            get { return _nozzleImageSource; }
            set { SetProperty(ref _nozzleImageSource, value); }
        }

        private bool _nozzleImageVisibility;
        public bool NozzleImageVisibility
        {
            get { return _nozzleImageVisibility; }
            set { SetProperty(ref _nozzleImageVisibility, value); }
        }

        private string _nozzleImageItem1;
        public string NozzleImageItem1
        {
            get { return _nozzleImageItem1; }
            set { SetProperty(ref _nozzleImageItem1, value); }
        }

        private string _nozzleImageItem2;
        public string NozzleImageItem2
        {
            get { return _nozzleImageItem2; }
            set { SetProperty(ref _nozzleImageItem2, value); }
        }

        private string _nozzleImageItem3;
        public string NozzleImageItem3
        {
            get { return _nozzleImageItem3; }
            set { SetProperty(ref _nozzleImageItem3, value); }
        }

        private string _nozzleImageItem4;
        public string NozzleImageItem4
        {
            get { return _nozzleImageItem4; }
            set { SetProperty(ref _nozzleImageItem4, value); }
        }

        private string _nozzleImageItem5;
        public string NozzleImageItem5
        {
            get { return _nozzleImageItem5; }
            set { SetProperty(ref _nozzleImageItem5, value); }
        }

        private string _nozzleImageItem6;
        public string NozzleImageItem6
        {
            get { return _nozzleImageItem6; }
            set { SetProperty(ref _nozzleImageItem6, value); }
        }

        private string _nozzleImageItem7;
        public string NozzleImageItem7
        {
            get { return _nozzleImageItem7; }
            set { SetProperty(ref _nozzleImageItem7, value); }
        }

        private string _nozzleImageItem8;
        public string NozzleImageItem8
        {
            get { return _nozzleImageItem8; }
            set { SetProperty(ref _nozzleImageItem8, value); }
        }

        private string _nozzleImageItem9;
        public string NozzleImageItem9
        {
            get { return _nozzleImageItem9; }
            set { SetProperty(ref _nozzleImageItem9, value); }
        }

        private string _nozzleImageItem10;
        public string NozzleImageItem10
        {
            get { return _nozzleImageItem10; }
            set { SetProperty(ref _nozzleImageItem10, value); }
        }

        private string _nozzleImageItem11;
        public string NozzleImageItem11
        {
            get { return _nozzleImageItem11; }
            set { SetProperty(ref _nozzleImageItem11, value); }
        }
    }
}
