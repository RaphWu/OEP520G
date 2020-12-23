using EPCIO;
using EPCIO.IoSystem;
using OEP520G.Functions;
using OEP520G.Parameter;
using Prism.Commands;
using Prism.Mvvm;

namespace OEP520G.Manual.ViewModels
{
    public class ObjectMoveViewModel : BindableBase
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly Nozzle nozzle = new Nozzle();
        private readonly Clamp clamp = Clamp.Instance;
        private readonly ObjectMotion objectMotion = new ObjectMotion();

        // 狀態顯示Timer
        //private readonly Timer StatusUpdate = new Timer { Interval = 300 };

        /********** 顯示動作字串 **********/
        // 吸嘴動作
        private const string NOZZLE_OFF = "OFF";
        private const string NOZZLE_VACCUM = "VACCUM";
        private const string NOZZLE_BLOW = "BLOW";
        private const string NOZZLE_OFF_MESSAGE = "關閉";
        private const string NOZZLE_VACCUM_MESSAGE = "吸氣";
        private const string NOZZLE_BLOW_MESSAGE = "吐氣";

        // 氣缸動作
        private const string CYLINDER_UP = "UP";
        private const string CYLINDER_DOWN = "DOWN";
        private const string CYLINDER_UP_MESSAGE = "上";
        private const string CYLINDER_DOWN_MESSAGE = "下";

        // 夾爪動作
        private const string CLAMP_OPEN = "OPEN";
        private const string CLAMP_PINCH = "PINCH";
        private const string CLAMP_OPEN_MESSAGE = "打開";
        private const string CLAMP_PINCH_MESSAGE = "閉合";

        /********** 動作變數 **********/
        // 選擇目標 "N1"=>"N"+1
        private string targetCode;
        private int targetIndex;
        private ENozzleId targetId;

        // [0]
        private string[] nozzleStatus = new string[Nozzle.MAX_NOZZLE + 1];
        private string[] nozzleStatusMessage = new string[Nozzle.MAX_NOZZLE + 1];
        private string[] nozzleCylinderStatus = new string[Nozzle.MAX_NOZZLE + 1];
        private string[] nozzleCylinderStatusMessage = new string[Nozzle.MAX_NOZZLE + 1];
        private string[] clampStatus = new string[Clamp.MAX_CLAMP + 1];
        private string[] clampStatusMessage = new string[Clamp.MAX_CLAMP + 1];
        private string[] clampCylinderStatus = new string[Clamp.MAX_CLAMP + 1];
        private string[] clampCylinderStatusMessage = new string[Clamp.MAX_CLAMP + 1];

        /// <summary>
        /// 吸嘴動作: 0=>OFF 1=>吐氣 -1=>吸氣
        /// </summary>
        //private int nozzleAction = 0;

        // 按鍵
        public DelegateCommand AxisZUpCommand { get; private set; }
        public DelegateCommand AxisZDownCommand { get; private set; }
        public DelegateCommand MovingCameraToObjectCommand { get; private set; }
        public DelegateCommand ObjectToMovingCameraCommand { get; private set; }
        public DelegateCommand ObjectToFixCameraCommand { get; private set; }
        public DelegateCommand ObjectToStageCommand { get; private set; }
        public DelegateCommand CylinderUpCommand { get; private set; }
        public DelegateCommand CylinderDownCommand { get; private set; }
        public DelegateCommand NozzleInhaleCommand { get; private set; }
        public DelegateCommand NozzleExhaleCommand { get; private set; }
        public DelegateCommand NozzleVaccumOffCommand { get; private set; }
        public DelegateCommand ClampCloseCommand { get; private set; }
        public DelegateCommand ClampOpenCommand { get; private set; }

        // Window Loaded
        public DelegateCommand HandleLoadedCommand { get; private set; }

        // Window Closing
        public DelegateCommand HandleClosingCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public ObjectMoveViewModel()
        {
            ResoultSelected = "0.01";
            ObjectSelect = "N01";
            ObjectChanged();

            // 是否顯示Z軸座標
            AxisZEnabled = true;

            // 訊息初始顯示
            ShowAllStatus();

            // 按鍵
            AxisZUpCommand = new DelegateCommand(AxisZUp);
            AxisZDownCommand = new DelegateCommand(AxisZDown);
            MovingCameraToObjectCommand = new DelegateCommand(MovingCameraToObject);
            ObjectToMovingCameraCommand = new DelegateCommand(ObjectToMovingCamera);
            ObjectToFixCameraCommand = new DelegateCommand(ObjectToFixCamera);
            ObjectToStageCommand = new DelegateCommand(ObjectToStage);
            CylinderUpCommand = new DelegateCommand(CylinderUp);
            CylinderDownCommand = new DelegateCommand(CylinderDown);
            NozzleInhaleCommand = new DelegateCommand(NozzleVaccum);
            NozzleExhaleCommand = new DelegateCommand(NozzleBlow);
            NozzleVaccumOffCommand = new DelegateCommand(NozzleOff);
            ClampCloseCommand = new DelegateCommand(ClampClose);
            ClampOpenCommand = new DelegateCommand(ClampOpen);

            //// Window Loaded
            //HandleLoadedCommand = new DelegateCommand(HandleLoaded);

            //// Window Closing
            //HandleClosingCommand = new DelegateCommand(HandleClosing);

            // 狀態顯示Timer
            //StatusUpdate.Elapsed += new ElapsedEventHandler(StatusUpdateTimer);
        }

        ///// <summary>
        ///// Window Loaded
        ///// </summary>
        //private void HandleLoaded()
        //{
        //    //io.RioInputChanged += StatusUpdate;
        //}

        ///// <summary>
        ///// Window Closing
        ///// </summary>
        //private void HandleClosing()
        //{
        //    //io.RioInputChanged -= StatusUpdate;
        //}

        /********************
         * 選擇物件
         *******************/
        /// <summary>
        /// 選擇物件變更
        /// </summary>
        private void ObjectChanged()
        {
            // Moving Camera
            if (ObjectSelect == "MC")
                targetCode = "MC";
            else
            {
                targetCode = ObjectSelect.Substring(0, 1);
                targetIndex = int.Parse(ObjectSelect.Substring(1)) - 1;
                targetId = (ENozzleId)targetIndex;
            }

            // Nozzle
            if (targetCode == "N")
            {
                NozzleActionEnable = true;
                ClinderActionEnable = true;
                ClampActionEnable = false;
                AxisZActionEnable = true;
                MoveCameraToObjectEnable = true;
                ObjectToMoveCameraEnable = true;
                ObjectToFixCameraEnable = true;
                ObjectToStageEnable = true;

                NozzleAction = nozzleStatusMessage[targetIndex];
                CylinderAction = nozzleCylinderStatusMessage[targetIndex];
            }
            else if (ObjectSelect == "MC")
            {
                // Moving Camera
                NozzleActionEnable = false;
                ClinderActionEnable = false;
                ClampActionEnable = false;
                AxisZActionEnable = false;
                MoveCameraToObjectEnable = false;
                ObjectToMoveCameraEnable = false;
                ObjectToFixCameraEnable = false;
                ObjectToStageEnable = true;
            }
            else if (targetCode == "C")
            {
                // Clamp
                NozzleActionEnable = false;
                ClinderActionEnable = true;
                ClampActionEnable = true;
                AxisZActionEnable = true;
                MoveCameraToObjectEnable = true;
                ObjectToMoveCameraEnable = true;
                ObjectToFixCameraEnable = false;
                ObjectToStageEnable = true;

                ClampAction = clampStatusMessage[targetIndex];
                CylinderAction = clampCylinderStatusMessage[targetIndex];
            }

            UpdateStatus();
        }

        /********************
         * 狀態顯示
         *******************/
        /// <summary>
        /// 狀態顯示Timer
        /// </summary>
        private void StatusUpdate(object sender, IoEventArgs e) => ShowAllStatus();

        /// <summary>
        /// 狀態顯示Timer
        /// </summary>
        //private void StatusUpdateEvent(object sender, EventArgs e)
        //    => AxisZCoordinate = epcio.Servos[Motion.AXIS_ID_Z].CurrentPosition;

        private void ShowAllStatus()
        {
            ShowStatus("N01");
            ShowStatus("N02");
            ShowStatus("N03");
            ShowStatus("N04");
            ShowStatus("N05");
            ShowStatus("N06");
            ShowStatus("N07");
            ShowStatus("N08");
            ShowStatus("N09");
            ShowStatus("N10");
            ShowStatus("N11");
            ShowStatus("C01");
            ShowStatus("C02");

            AxisZCoordinate = epcio.ServoZ.GetCurrentPosition().ToString("F3");
        }

        /// <summary>
        /// 讀取狀態
        /// </summary>
        private void ShowStatus(string objName)
        {
            bool vaccum = default;
            bool blow = default;
            bool cylinder = default;

            bool UpDown = default;
            bool OpenClose = default;

            switch (objName)
            {
                case "N01":
                    if (epcio.Nozzle01_Vacuum.Value)
                        NozzleStatusN01 = NOZZLE_VACCUM_MESSAGE;
                    else if (epcio.Nozzle01_Blow.Value)
                        NozzleStatusN01 = NOZZLE_BLOW_MESSAGE;
                    else
                        NozzleStatusN01 = NOZZLE_OFF_MESSAGE;

                    NozzleCylinderStatusN01 = epcio.Nozzle01_Cylinder.Value
                        ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;
                    break;

                case "N02":
                    if (epcio.Nozzle02_Vacuum.Value)
                        NozzleStatusN02 = NOZZLE_VACCUM_MESSAGE;
                    else if (epcio.Nozzle02_Blow.Value)
                        NozzleStatusN02 = NOZZLE_BLOW_MESSAGE;
                    else
                        NozzleStatusN02 = NOZZLE_OFF_MESSAGE;

                    NozzleCylinderStatusN02 = epcio.Nozzle02_Cylinder.Value
                        ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;
                    break;

                case "N03":
                    if (epcio.Nozzle03_Vacuum.Value)
                        NozzleStatusN03 = NOZZLE_VACCUM_MESSAGE;
                    else if (epcio.Nozzle03_Blow.Value)
                        NozzleStatusN03 = NOZZLE_BLOW_MESSAGE;
                    else
                        NozzleStatusN03 = NOZZLE_OFF_MESSAGE;

                    NozzleCylinderStatusN03 = epcio.Nozzle03_Cylinder.Value
                        ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;
                    break;

                case "N04":
                    if (epcio.Nozzle04_Vacuum.Value)
                        NozzleStatusN04 = NOZZLE_VACCUM_MESSAGE;
                    else if (epcio.Nozzle04_Blow.Value)
                        NozzleStatusN04 = NOZZLE_BLOW_MESSAGE;
                    else
                        NozzleStatusN04 = NOZZLE_OFF_MESSAGE;

                    NozzleCylinderStatusN04 = epcio.Nozzle04_Cylinder.Value
                        ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;
                    break;

                case "N05":
                    if (epcio.Nozzle05_Vacuum.Value)
                        NozzleStatusN05 = NOZZLE_VACCUM_MESSAGE;
                    else if (epcio.Nozzle05_Blow.Value)
                        NozzleStatusN05 = NOZZLE_BLOW_MESSAGE;
                    else
                        NozzleStatusN05 = NOZZLE_OFF_MESSAGE;

                    NozzleCylinderStatusN05 = epcio.Nozzle05_Cylinder.Value
                        ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;
                    break;

                case "N06":
                    if (epcio.Nozzle06_Vacuum.Value)
                        NozzleStatusN06 = NOZZLE_VACCUM_MESSAGE;
                    else if (epcio.Nozzle06_Blow.Value)
                        NozzleStatusN06 = NOZZLE_BLOW_MESSAGE;
                    else
                        NozzleStatusN06 = NOZZLE_OFF_MESSAGE;

                    NozzleCylinderStatusN06 = epcio.Nozzle06_Cylinder.Value
                        ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;
                    break;

                case "N07":
                    if (epcio.Nozzle07_Vacuum.Value)
                        NozzleStatusN07 = NOZZLE_VACCUM_MESSAGE;
                    else if (epcio.Nozzle07_Blow.Value)
                        NozzleStatusN07 = NOZZLE_BLOW_MESSAGE;
                    else
                        NozzleStatusN07 = NOZZLE_OFF_MESSAGE;

                    NozzleCylinderStatusN07 = epcio.Nozzle07_Cylinder.Value
                        ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;
                    break;

                case "N08":
                    if (epcio.Nozzle08_Vacuum.Value)
                        NozzleStatusN08 = NOZZLE_VACCUM_MESSAGE;
                    else if (epcio.Nozzle08_Blow.Value)
                        NozzleStatusN08 = NOZZLE_BLOW_MESSAGE;
                    else
                        NozzleStatusN08 = NOZZLE_OFF_MESSAGE;

                    NozzleCylinderStatusN08 = epcio.Nozzle08_Cylinder.Value
                        ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;
                    break;

                case "N09":
                    if (epcio.Nozzle09_Vacuum.Value)
                        NozzleStatusN09 = NOZZLE_VACCUM_MESSAGE;
                    else if (epcio.Nozzle09_Blow.Value)
                        NozzleStatusN09 = NOZZLE_BLOW_MESSAGE;
                    else
                        NozzleStatusN09 = NOZZLE_OFF_MESSAGE;

                    NozzleCylinderStatusN09 = epcio.Nozzle09_Cylinder.Value
                        ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;
                    break;

                case "N10":
                    if (epcio.Nozzle10_Vacuum.Value)
                        NozzleStatusN10 = NOZZLE_VACCUM_MESSAGE;
                    else if (epcio.Nozzle10_Blow.Value)
                        NozzleStatusN10 = NOZZLE_BLOW_MESSAGE;
                    else
                        NozzleStatusN10 = NOZZLE_OFF_MESSAGE;

                    NozzleCylinderStatusN10 = epcio.Nozzle10_Cylinder.Value
                        ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;
                    break;

                case "N11":
                    if (epcio.Nozzle11_Vacuum.Value)
                        NozzleStatusN11 = NOZZLE_VACCUM_MESSAGE;
                    else if (epcio.Nozzle11_Blow.Value)
                        NozzleStatusN11 = NOZZLE_BLOW_MESSAGE;
                    else
                        NozzleStatusN11 = NOZZLE_OFF_MESSAGE;

                    NozzleCylinderStatusN11 = epcio.Nozzle11_Cylinder.Value
                        ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;
                    break;

                case "C01":
                    ClampStatusC01 = epcio.Clamp1UpDown.Value
                        ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;
                    ClampCylinderStatusC01 = epcio.Clamp1OpenClose.Value
                        ? CLAMP_PINCH_MESSAGE : CLAMP_OPEN_MESSAGE;
                    break;

                case "C02":
                    ClampStatusC02 = epcio.Clamp2UpDown.Value
                        ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;
                    ClampCylinderStatusC02 = epcio.Clamp2OpenClose.Value
                        ? CLAMP_PINCH_MESSAGE : CLAMP_OPEN_MESSAGE;
                    break;
            }

            if (targetCode == "N")
            {
                if (vaccum)
                    nozzleStatus[targetIndex] = NOZZLE_VACCUM;
                else if (blow)
                    nozzleStatus[targetIndex] = NOZZLE_BLOW;
                else
                    nozzleStatus[targetIndex] = NOZZLE_OFF;

                nozzleCylinderStatus[targetIndex] = cylinder ? CYLINDER_DOWN : CYLINDER_UP;
            }
            else if (targetCode == "C")
            {
                clampStatus[targetIndex] = UpDown ? CYLINDER_DOWN : CYLINDER_UP;
                clampCylinderStatus[targetIndex] = OpenClose ? CLAMP_PINCH : CYLINDER_UP;
            }
        }

        /// <summary>
        /// 更新畫面狀態顯示
        /// </summary>
        private void UpdateStatus()
        {
            // N01
            if (epcio.Nozzle01_Vacuum.Value)
                NozzleStatusN01 = NOZZLE_VACCUM_MESSAGE;
            else if (epcio.Nozzle01_Blow.Value)
                NozzleStatusN01 = NOZZLE_BLOW_MESSAGE;
            else
                NozzleStatusN01 = NOZZLE_OFF_MESSAGE;

            NozzleCylinderStatusN01 = epcio.Nozzle01_Cylinder.Value
                ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;

            // N02
            if (epcio.Nozzle02_Vacuum.Value)
                NozzleStatusN02 = NOZZLE_VACCUM_MESSAGE;
            else if (epcio.Nozzle02_Blow.Value)
                NozzleStatusN02 = NOZZLE_BLOW_MESSAGE;
            else
                NozzleStatusN02 = NOZZLE_OFF_MESSAGE;

            NozzleCylinderStatusN02 = epcio.Nozzle02_Cylinder.Value
                ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;

            // N03
            if (epcio.Nozzle03_Vacuum.Value)
                NozzleStatusN03 = NOZZLE_VACCUM_MESSAGE;
            else if (epcio.Nozzle03_Blow.Value)
                NozzleStatusN03 = NOZZLE_BLOW_MESSAGE;
            else
                NozzleStatusN03 = NOZZLE_OFF_MESSAGE;

            NozzleCylinderStatusN03 = epcio.Nozzle03_Cylinder.Value
                ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;

            // N04
            if (epcio.Nozzle04_Vacuum.Value)
                NozzleStatusN04 = NOZZLE_VACCUM_MESSAGE;
            else if (epcio.Nozzle04_Blow.Value)
                NozzleStatusN04 = NOZZLE_BLOW_MESSAGE;
            else
                NozzleStatusN04 = NOZZLE_OFF_MESSAGE;

            NozzleCylinderStatusN04 = epcio.Nozzle04_Cylinder.Value
                ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;

            // N05
            if (epcio.Nozzle05_Vacuum.Value)
                NozzleStatusN05 = NOZZLE_VACCUM_MESSAGE;
            else if (epcio.Nozzle05_Blow.Value)
                NozzleStatusN05 = NOZZLE_BLOW_MESSAGE;
            else
                NozzleStatusN05 = NOZZLE_OFF_MESSAGE;

            NozzleCylinderStatusN05 = epcio.Nozzle05_Cylinder.Value
                ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;

            // N06
            if (epcio.Nozzle06_Vacuum.Value)
                NozzleStatusN06 = NOZZLE_VACCUM_MESSAGE;
            else if (epcio.Nozzle06_Blow.Value)
                NozzleStatusN06 = NOZZLE_BLOW_MESSAGE;
            else
                NozzleStatusN06 = NOZZLE_OFF_MESSAGE;

            NozzleCylinderStatusN06 = epcio.Nozzle06_Cylinder.Value
                ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;

            // N07
            if (epcio.Nozzle07_Vacuum.Value)
                NozzleStatusN07 = NOZZLE_VACCUM_MESSAGE;
            else if (epcio.Nozzle07_Blow.Value)
                NozzleStatusN07 = NOZZLE_BLOW_MESSAGE;
            else
                NozzleStatusN07 = NOZZLE_OFF_MESSAGE;

            NozzleCylinderStatusN07 = epcio.Nozzle07_Cylinder.Value
                ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;

            // N08
            if (epcio.Nozzle08_Vacuum.Value)
                NozzleStatusN08 = NOZZLE_VACCUM_MESSAGE;
            else if (epcio.Nozzle08_Blow.Value)
                NozzleStatusN08 = NOZZLE_BLOW_MESSAGE;
            else
                NozzleStatusN08 = NOZZLE_OFF_MESSAGE;

            NozzleCylinderStatusN08 = epcio.Nozzle08_Cylinder.Value
                ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;

            // N09
            if (epcio.Nozzle09_Vacuum.Value)
                NozzleStatusN09 = NOZZLE_VACCUM_MESSAGE;
            else if (epcio.Nozzle09_Blow.Value)
                NozzleStatusN09 = NOZZLE_BLOW_MESSAGE;
            else
                NozzleStatusN09 = NOZZLE_OFF_MESSAGE;

            NozzleCylinderStatusN09 = epcio.Nozzle09_Cylinder.Value
                ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;

            // N10
            if (epcio.Nozzle10_Vacuum.Value)
                NozzleStatusN10 = NOZZLE_VACCUM_MESSAGE;
            else if (epcio.Nozzle10_Blow.Value)
                NozzleStatusN10 = NOZZLE_BLOW_MESSAGE;
            else
                NozzleStatusN10 = NOZZLE_OFF_MESSAGE;

            NozzleCylinderStatusN10 = epcio.Nozzle10_Cylinder.Value
                ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;

            // N11
            if (epcio.Nozzle11_Vacuum.Value)
                NozzleStatusN11 = NOZZLE_VACCUM_MESSAGE;
            else if (epcio.Nozzle11_Blow.Value)
                NozzleStatusN11 = NOZZLE_BLOW_MESSAGE;
            else
                NozzleStatusN11 = NOZZLE_OFF_MESSAGE;

            NozzleCylinderStatusN11 = epcio.Nozzle11_Cylinder.Value
                ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;


            // 顯示夾爪狀態
            ClampStatusC01 = epcio.Clamp1UpDown.Value
                ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;
            ClampCylinderStatusC01 = epcio.Clamp1OpenClose.Value
                ? CLAMP_PINCH_MESSAGE : CLAMP_OPEN_MESSAGE;

            ClampStatusC02 = epcio.Clamp2UpDown.Value
                ? CYLINDER_DOWN_MESSAGE : CYLINDER_UP_MESSAGE;
            ClampCylinderStatusC02 = epcio.Clamp2OpenClose.Value
                ? CLAMP_PINCH_MESSAGE : CLAMP_OPEN_MESSAGE;
        }

        /********************
         * Z軸
         *******************/
        /// <summary>
        /// Z軸上昇
        /// </summary>
        private void AxisZUp()
        {
            if (ResoultSelected != null)
                epcio.MoveToRelative(distanceZ: double.Parse(ResoultSelected) * -1);
        }

        /// <summary>
        /// Z軸下降
        /// </summary>
        private void AxisZDown()
        {
            if (ResoultSelected != null)
                epcio.MoveToRelative(distanceZ: double.Parse(ResoultSelected));
        }

        /********************
         * 物件移動
         *******************/
        /// <summary>
        /// 移動相機 -> 物件
        /// </summary>
        private async void MovingCameraToObject()
        {
            switch (targetCode)
            {
                case "N":
                    await objectMotion.MoveCameraToNozzle(targetId);
                    break;
                case "C":
                    break;
            }
        }

        /// <summary>
        /// 物件 -> 移動相機
        /// </summary>
        private async void ObjectToMovingCamera()
        {
            switch (targetCode)
            {
                case "N":
                    await objectMotion.NozzleToMoveCamera(targetId);
                    break;
                case "C":
                    break;
                    //case "MC":
                    //    break;
            }
        }

        /// <summary>
        /// 物件 -> 固定相機
        /// </summary>
        private async void ObjectToFixCamera()
        {
            switch (targetCode)
            {
                case "N":
                    await objectMotion.NozzleToFixCamera(targetId);
                    break;
                case "C":
                    break;
                case "MC":
                    break;
            }
        }

        /// <summary>
        /// 物件 -> 台車
        /// </summary>
        private async void ObjectToStage()
        {
            switch (targetCode)
            {
                case "N":
                    await objectMotion.NozzleToStage(targetId);
                    break;
                case "C":
                    break;
                case "MC":
                    await objectMotion.MoveCameraToStage();
                    break;
            }
        }

        /********************
         * 吸嘴
         *******************/
        /// <summary>
        /// 吸嘴動作
        /// </summary>
        private void DoNozzleAction()
        {
            RemoteIo vaccum = default;
            RemoteIo blow = default;
            RemoteIo cylinder = default;

            switch (ObjectSelect)
            {
                case "N01":
                    vaccum = epcio.Nozzle01_Vacuum;
                    blow = epcio.Nozzle01_Blow;
                    cylinder = epcio.Nozzle01_Cylinder;
                    break;
                case "N02":
                    vaccum = epcio.Nozzle02_Vacuum;
                    blow = epcio.Nozzle02_Blow;
                    cylinder = epcio.Nozzle02_Cylinder;
                    break;
                case "N03":
                    vaccum = epcio.Nozzle03_Vacuum;
                    blow = epcio.Nozzle03_Blow;
                    cylinder = epcio.Nozzle03_Cylinder;
                    break;
                case "N04":
                    vaccum = epcio.Nozzle04_Vacuum;
                    blow = epcio.Nozzle04_Blow;
                    cylinder = epcio.Nozzle04_Cylinder;
                    break;
                case "N05":
                    vaccum = epcio.Nozzle05_Vacuum;
                    blow = epcio.Nozzle05_Blow;
                    cylinder = epcio.Nozzle05_Cylinder;
                    break;
                case "N06":
                    vaccum = epcio.Nozzle06_Vacuum;
                    blow = epcio.Nozzle06_Blow;
                    cylinder = epcio.Nozzle06_Cylinder;
                    break;
                case "N07":
                    vaccum = epcio.Nozzle07_Vacuum;
                    blow = epcio.Nozzle07_Blow;
                    cylinder = epcio.Nozzle07_Cylinder;
                    break;
                case "N08":
                    vaccum = epcio.Nozzle08_Vacuum;
                    blow = epcio.Nozzle08_Blow;
                    cylinder = epcio.Nozzle08_Cylinder;
                    break;
                case "N09":
                    vaccum = epcio.Nozzle09_Vacuum;
                    blow = epcio.Nozzle09_Blow;
                    cylinder = epcio.Nozzle09_Cylinder;
                    break;
                case "N10":
                    vaccum = epcio.Nozzle10_Vacuum;
                    blow = epcio.Nozzle10_Blow;
                    cylinder = epcio.Nozzle10_Cylinder;
                    break;
                case "N11":
                    vaccum = epcio.Nozzle11_Vacuum;
                    blow = epcio.Nozzle11_Blow;
                    cylinder = epcio.Nozzle11_Cylinder;
                    break;
            }

            switch (NozzleAction)
            {
                case NOZZLE_OFF:
                    epcio.RioOutput(vaccum, false);
                    epcio.RioOutput(blow, false);
                    break;

                case NOZZLE_VACCUM:
                    epcio.RioOutput(vaccum, true);
                    epcio.RioOutput(blow, false);
                    break;

                case NOZZLE_BLOW:
                    epcio.RioOutput(vaccum, false);
                    epcio.RioOutput(blow, true);
                    break;
            }
        }

        /// <summary>
        /// 氣缸
        /// </summary>
        private void CylinderUp()
        {
            if (targetCode == "N")
                nozzle.NozzleUp((ENozzleId)targetIndex);
            else if (targetCode == "C")
                clamp.ClampUp((EClampId)targetIndex);

            ShowStatus(ObjectSelect);
        }

        private void CylinderDown()
        {
            if (targetCode == "N")
                nozzle.NozzleDown((ENozzleId)targetIndex);
            else if (targetCode == "C")
                clamp.ClampDown((EClampId)targetIndex);

            ShowStatus(ObjectSelect);
        }

        /// <summary>
        /// 吸嘴吸氣
        /// </summary>
        private void NozzleVaccum()
        {
            if (targetCode == "N")
            {
                switch (targetIndex)
                {
                    case 0:
                        epcio.RioOutput(epcio.Nozzle01_Vacuum, true);
                        epcio.RioOutput(epcio.Nozzle01_Blow, false);
                        break;
                    case 1:
                        epcio.RioOutput(epcio.Nozzle02_Vacuum, true);
                        epcio.RioOutput(epcio.Nozzle02_Blow, false);
                        break;
                    case 2:
                        epcio.RioOutput(epcio.Nozzle03_Vacuum, true);
                        epcio.RioOutput(epcio.Nozzle03_Blow, false);
                        break;
                    case 3:
                        epcio.RioOutput(epcio.Nozzle04_Vacuum, true);
                        epcio.RioOutput(epcio.Nozzle04_Blow, false);
                        break;
                    case 4:
                        epcio.RioOutput(epcio.Nozzle05_Vacuum, true);
                        epcio.RioOutput(epcio.Nozzle05_Blow, false);
                        break;
                    case 5:
                        epcio.RioOutput(epcio.Nozzle06_Vacuum, true);
                        epcio.RioOutput(epcio.Nozzle06_Blow, false);
                        break;
                    case 6:
                        epcio.RioOutput(epcio.Nozzle07_Vacuum, true);
                        epcio.RioOutput(epcio.Nozzle07_Blow, false);
                        break;
                    case 7:
                        epcio.RioOutput(epcio.Nozzle08_Vacuum, true);
                        epcio.RioOutput(epcio.Nozzle08_Blow, false);
                        break;
                    case 8:
                        epcio.RioOutput(epcio.Nozzle09_Vacuum, true);
                        epcio.RioOutput(epcio.Nozzle09_Blow, false);
                        break;
                    case 9:
                        epcio.RioOutput(epcio.Nozzle10_Vacuum, true);
                        epcio.RioOutput(epcio.Nozzle10_Blow, false);
                        break;
                    case 10:
                        epcio.RioOutput(epcio.Nozzle11_Vacuum, true);
                        epcio.RioOutput(epcio.Nozzle11_Blow, false);
                        break;
                }

                ShowStatus(ObjectSelect);
            }
        }

        /// <summary>
        /// 吸嘴吐氣
        /// </summary>
        private void NozzleBlow()
        {
            if (targetCode == "N")
            {
                switch (targetIndex)
                {
                    case 0:
                        epcio.RioOutput(epcio.Nozzle01_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle01_Blow, true);
                        break;
                    case 1:
                        epcio.RioOutput(epcio.Nozzle02_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle02_Blow, true);
                        break;
                    case 2:
                        epcio.RioOutput(epcio.Nozzle03_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle03_Blow, true);
                        break;
                    case 3:
                        epcio.RioOutput(epcio.Nozzle04_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle04_Blow, true);
                        break;
                    case 4:
                        epcio.RioOutput(epcio.Nozzle05_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle05_Blow, true);
                        break;
                    case 5:
                        epcio.RioOutput(epcio.Nozzle06_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle06_Blow, true);
                        break;
                    case 6:
                        epcio.RioOutput(epcio.Nozzle07_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle07_Blow, true);
                        break;
                    case 7:
                        epcio.RioOutput(epcio.Nozzle08_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle08_Blow, true);
                        break;
                    case 8:
                        epcio.RioOutput(epcio.Nozzle09_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle09_Blow, true);
                        break;
                    case 9:
                        epcio.RioOutput(epcio.Nozzle10_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle10_Blow, true);
                        break;
                    case 10:
                        epcio.RioOutput(epcio.Nozzle11_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle11_Blow, true);
                        break;
                }

                ShowStatus(ObjectSelect);
            }
        }

        /// <summary>
        /// 吸嘴真空關閉
        /// </summary>
        private void NozzleOff()
        {
            if (targetCode == "N")
            {
                switch (targetIndex)
                {
                    case 0:
                        epcio.RioOutput(epcio.Nozzle01_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle01_Blow, false);
                        break;
                    case 1:
                        epcio.RioOutput(epcio.Nozzle02_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle02_Blow, false);
                        break;
                    case 2:
                        epcio.RioOutput(epcio.Nozzle03_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle03_Blow, false);
                        break;
                    case 3:
                        epcio.RioOutput(epcio.Nozzle04_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle04_Blow, false);
                        break;
                    case 4:
                        epcio.RioOutput(epcio.Nozzle05_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle05_Blow, false);
                        break;
                    case 5:
                        epcio.RioOutput(epcio.Nozzle06_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle06_Blow, false);
                        break;
                    case 6:
                        epcio.RioOutput(epcio.Nozzle07_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle07_Blow, false);
                        break;
                    case 7:
                        epcio.RioOutput(epcio.Nozzle08_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle08_Blow, false);
                        break;
                    case 8:
                        epcio.RioOutput(epcio.Nozzle09_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle09_Blow, false);
                        break;
                    case 9:
                        epcio.RioOutput(epcio.Nozzle10_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle10_Blow, false);
                        break;
                    case 10:
                        epcio.RioOutput(epcio.Nozzle11_Vacuum, false);
                        epcio.RioOutput(epcio.Nozzle11_Blow, false);
                        break;
                }

                ShowStatus(ObjectSelect);
            }
        }

        /********************
         * 夾爪
         *******************/
        /// <summary>
        /// 夾爪閉合
        /// </summary>
        private void ClampClose()
        {
            if (targetCode == "C")
                clamp.ClampClose((EClampId)targetIndex);
        }

        /// <summary>
        /// 夾爪打開
        /// </summary>
        private void ClampOpen()
        {
            if (targetCode == "C")
                clamp.ClampOpen((EClampId)targetIndex);
        }

        /********************
         * 繫結
         *******************/
        // 物件選擇
        private string _objectSelect;
        public string ObjectSelect
        {
            get { return _objectSelect; }
            set
            {
                SetProperty(ref _objectSelect, value);
                ObjectChanged();
            }
        }

        // Z軸座標
        private string _axisZCoordinate;
        public string AxisZCoordinate
        {
            get { return _axisZCoordinate; }
            set { SetProperty(ref _axisZCoordinate, value); }
        }


        private bool _axisZEnabled;
        public bool AxisZEnabled
        {
            get { return _axisZEnabled; }
            set { SetProperty(ref _axisZEnabled, value); }
        }

        // 動作類
        private string _nozzleAction;
        public string NozzleAction
        {
            get { return _nozzleAction; }
            set
            {
                SetProperty(ref _nozzleAction, value);
                DoNozzleAction();
            }
        }

        private string _cylinderAction;
        public string CylinderAction
        {
            get { return _cylinderAction; }
            set { SetProperty(ref _cylinderAction, value); }
        }

        private string _clampAction;
        public string ClampAction
        {
            get { return _clampAction; }
            set { SetProperty(ref _clampAction, value); }
        }

        private string _resoultSelected;
        public string ResoultSelected
        {
            get { return _resoultSelected; }
            set { SetProperty(ref _resoultSelected, value); }
        }

        // Enable類
        private bool _nozzleActionEnable;
        public bool NozzleActionEnable
        {
            get { return _nozzleActionEnable; }
            set { SetProperty(ref _nozzleActionEnable, value); }
        }

        private bool _clinderActionEnable;
        public bool ClinderActionEnable
        {
            get { return _clinderActionEnable; }
            set { SetProperty(ref _clinderActionEnable, value); }
        }

        private bool _clampActionEnable;
        public bool ClampActionEnable
        {
            get { return _clampActionEnable; }
            set { SetProperty(ref _clampActionEnable, value); }
        }

        private bool _axisZActionEnable;
        public bool AxisZActionEnable
        {
            get { return _axisZActionEnable; }
            set { SetProperty(ref _axisZActionEnable, value); }
        }

        private bool _flyCameraToObjectEnable;
        public bool MoveCameraToObjectEnable
        {
            get { return _flyCameraToObjectEnable; }
            set { SetProperty(ref _flyCameraToObjectEnable, value); }
        }

        private bool _objectToMoveCameraEnable;
        public bool ObjectToMoveCameraEnable
        {
            get { return _objectToMoveCameraEnable; }
            set { SetProperty(ref _objectToMoveCameraEnable, value); }
        }

        private bool _objectToFixCameraEnable;
        public bool ObjectToFixCameraEnable
        {
            get { return _objectToFixCameraEnable; }
            set { SetProperty(ref _objectToFixCameraEnable, value); }
        }

        private bool _objectToStageEnable;
        public bool ObjectToStageEnable
        {
            get { return _objectToStageEnable; }
            set { SetProperty(ref _objectToStageEnable, value); }
        }

        // Status類
        private string _nozzleStatusN01;
        public string NozzleStatusN01
        {
            get { return _nozzleStatusN01; }
            set { SetProperty(ref _nozzleStatusN01, value); }
        }

        private string _nozzleCylinderStatusN01;
        public string NozzleCylinderStatusN01
        {
            get { return _nozzleCylinderStatusN01; }
            set { SetProperty(ref _nozzleCylinderStatusN01, value); }
        }

        private string _nozzleStatusN02;
        public string NozzleStatusN02
        {
            get { return _nozzleStatusN02; }
            set { SetProperty(ref _nozzleStatusN02, value); }
        }

        private string _nozzleCylinderStatusN02;
        public string NozzleCylinderStatusN02
        {
            get { return _nozzleCylinderStatusN02; }
            set { SetProperty(ref _nozzleCylinderStatusN02, value); }
        }

        private string _nozzleStatusN03;
        public string NozzleStatusN03
        {
            get { return _nozzleStatusN03; }
            set { SetProperty(ref _nozzleStatusN03, value); }
        }

        private string _nozzleCylinderStatusN03;
        public string NozzleCylinderStatusN03
        {
            get { return _nozzleCylinderStatusN03; }
            set { SetProperty(ref _nozzleCylinderStatusN03, value); }
        }

        private string _nozzleStatusN04;
        public string NozzleStatusN04
        {
            get { return _nozzleStatusN04; }
            set { SetProperty(ref _nozzleStatusN04, value); }
        }

        private string _nozzleCylinderStatusN04;
        public string NozzleCylinderStatusN04
        {
            get { return _nozzleCylinderStatusN04; }
            set { SetProperty(ref _nozzleCylinderStatusN04, value); }
        }

        private string _nozzleStatusN05;
        public string NozzleStatusN05
        {
            get { return _nozzleStatusN05; }
            set { SetProperty(ref _nozzleStatusN05, value); }
        }

        private string _nozzleCylinderStatusN05;
        public string NozzleCylinderStatusN05
        {
            get { return _nozzleCylinderStatusN05; }
            set { SetProperty(ref _nozzleCylinderStatusN05, value); }
        }

        private string _nozzleStatusN06;
        public string NozzleStatusN06
        {
            get { return _nozzleStatusN06; }
            set { SetProperty(ref _nozzleStatusN06, value); }
        }

        private string _nozzleCylinderStatusN06;
        public string NozzleCylinderStatusN06
        {
            get { return _nozzleCylinderStatusN06; }
            set { SetProperty(ref _nozzleCylinderStatusN06, value); }
        }

        private string _nozzleStatusN07;
        public string NozzleStatusN07
        {
            get { return _nozzleStatusN07; }
            set { SetProperty(ref _nozzleStatusN07, value); }
        }

        private string _nozzleCylinderStatusN07;
        public string NozzleCylinderStatusN07
        {
            get { return _nozzleCylinderStatusN07; }
            set { SetProperty(ref _nozzleCylinderStatusN07, value); }
        }

        private string _nozzleStatusN08;
        public string NozzleStatusN08
        {
            get { return _nozzleStatusN08; }
            set { SetProperty(ref _nozzleStatusN08, value); }
        }

        private string _nozzleCylinderStatusN08;
        public string NozzleCylinderStatusN08
        {
            get { return _nozzleCylinderStatusN08; }
            set { SetProperty(ref _nozzleCylinderStatusN08, value); }
        }

        private string _nozzleStatusN09;
        public string NozzleStatusN09
        {
            get { return _nozzleStatusN09; }
            set { SetProperty(ref _nozzleStatusN09, value); }
        }

        private string _nozzleCylinderStatusN09;
        public string NozzleCylinderStatusN09
        {
            get { return _nozzleCylinderStatusN09; }
            set { SetProperty(ref _nozzleCylinderStatusN09, value); }
        }

        private string _nozzleStatusN10;
        public string NozzleStatusN10
        {
            get { return _nozzleStatusN10; }
            set { SetProperty(ref _nozzleStatusN10, value); }
        }

        private string _nozzleCylinderStatusN10;
        public string NozzleCylinderStatusN10
        {
            get { return _nozzleCylinderStatusN10; }
            set { SetProperty(ref _nozzleCylinderStatusN10, value); }
        }

        private string _nozzleStatusN11;
        public string NozzleStatusN11
        {
            get { return _nozzleStatusN11; }
            set { SetProperty(ref _nozzleStatusN11, value); }
        }

        private string _nozzleCylinderStatusN11;
        public string NozzleCylinderStatusN11
        {
            get { return _nozzleCylinderStatusN11; }
            set { SetProperty(ref _nozzleCylinderStatusN11, value); }
        }


        private string _clampStatusC01;
        public string ClampStatusC01
        {
            get { return _clampStatusC01; }
            set { SetProperty(ref _clampStatusC01, value); }
        }

        private string _clampCylinderStatusC01;
        public string ClampCylinderStatusC01
        {
            get { return _clampCylinderStatusC01; }
            set { SetProperty(ref _clampCylinderStatusC01, value); }
        }

        private string _clampStatusC02;
        public string ClampStatusC02
        {
            get { return _clampStatusC02; }
            set { SetProperty(ref _clampStatusC02, value); }
        }

        private string _clampCylinderStatusC02;
        public string ClampCylinderStatusC02
        {
            get { return _clampCylinderStatusC02; }
            set { SetProperty(ref _clampCylinderStatusC02, value); }
        }
    }
}
