using EPCIO;
using OEP520G.Core;
using OEP520G.Functions;
using OEP520G.Parameter;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace OEP520G.Teaching.ViewModels
{
    public class CameraCorrectViewModel : BindableBase
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly ObjectMotion objectMoving = new ObjectMotion();
        private readonly Machine machine = Machine.Instance;
        private readonly Camera camera = Camera.Instance;
        private readonly Nozzle nozzle = Nozzle.Instance;

        DatumPoint datumPoint1;
        NozzleObject nozzleItem;

        // 基準吸嘴ID
        private ENozzleId datumNozzleId;

        public DelegateCommand StartCorrectCommand { get; private set; }
        public DelegateCommand UpdateDataCommand { get; private set; }
        public DelegateCommand FixCameraCorrectCommand { get; private set; }
        public DelegateCommand CylinderUpCommand { get; private set; }
        public DelegateCommand CylinderDownCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public CameraCorrectViewModel()
        {
            datumPoint1 = machine.DatumPoint1;
            //nozzleItem = nozzle.NozzleList[baseNozzleId - 1];

            // 相機
            CameraSelectSource = camera.CAMERA_NAME_LIST;
            CameraSelectItem = camera.CAMERA_NAME_LIST[camera.MOVE_CAMERA_ID];

            // 基準吸嘴
            BaseNozzleSelector = nozzle.DatumNozzleId.ToString();

            StartCorrectCommand = new DelegateCommand(StartCorrect);
            UpdateDataCommand = new DelegateCommand(UpdateData);
            FixCameraCorrectCommand = new DelegateCommand(FixCameraCorrect);
            CylinderUpCommand = new DelegateCommand(CylinderUp);
            CylinderDownCommand = new DelegateCommand(CylinderDown);
        }

        /********************
         * 校正開始
         *******************/

        private void StartCorrect()
        {

        }

        private void UpdateData()
        {

        }

        /********************
         * 固定相機座標校正
         * 
         *******************/
        private void FixCameraCorrect()
        {
            string nozId;
            PointXY nozPosition = new PointXY();

            switch (datumNozzleId)
            {
                case ENozzleId.Nozzle01:
                    nozId = ObjectMotion.NOZZLE_01;
                    nozPosition.X = nozzle.NozzleList[0].Position.X;
                    nozPosition.Y = nozzle.NozzleList[0].Position.Y;
                    break;
                case ENozzleId.Nozzle02:
                    nozId = ObjectMotion.NOZZLE_02;
                    nozPosition.X = nozzle.NozzleList[1].Position.X;
                    nozPosition.Y = nozzle.NozzleList[1].Position.Y;
                    break;
                case ENozzleId.Nozzle03:
                    nozId = ObjectMotion.NOZZLE_03;
                    nozPosition.X = nozzle.NozzleList[2].Position.X;
                    nozPosition.Y = nozzle.NozzleList[2].Position.Y;
                    break;
                case ENozzleId.Nozzle04:
                    nozId = ObjectMotion.NOZZLE_04;
                    nozPosition.X = nozzle.NozzleList[3].Position.X;
                    nozPosition.Y = nozzle.NozzleList[3].Position.Y;
                    break;
                case ENozzleId.Nozzle05:
                    nozId = ObjectMotion.NOZZLE_05;
                    nozPosition.X = nozzle.NozzleList[4].Position.X;
                    nozPosition.Y = nozzle.NozzleList[4].Position.Y;
                    break;
                case ENozzleId.Nozzle06:
                    nozId = ObjectMotion.NOZZLE_06;
                    nozPosition.X = nozzle.NozzleList[5].Position.X;
                    nozPosition.Y = nozzle.NozzleList[5].Position.Y;
                    break;
                case ENozzleId.Nozzle07:
                    nozId = ObjectMotion.NOZZLE_07;
                    nozPosition.X = nozzle.NozzleList[6].Position.X;
                    nozPosition.Y = nozzle.NozzleList[6].Position.Y;
                    break;
                case ENozzleId.Nozzle08:
                    nozId = ObjectMotion.NOZZLE_08;
                    nozPosition.X = nozzle.NozzleList[7].Position.X;
                    nozPosition.Y = nozzle.NozzleList[7].Position.Y;
                    break;
                case ENozzleId.Nozzle09:
                    nozId = ObjectMotion.NOZZLE_09;
                    nozPosition.X = nozzle.NozzleList[8].Position.X;
                    nozPosition.Y = nozzle.NozzleList[8].Position.Y;
                    break;
                case ENozzleId.Nozzle10:
                    nozId = ObjectMotion.NOZZLE_10;
                    nozPosition.X = nozzle.NozzleList[9].Position.X;
                    nozPosition.Y = nozzle.NozzleList[9].Position.Y;
                    break;
                case ENozzleId.Nozzle11:
                    nozId = ObjectMotion.NOZZLE_11;
                    nozPosition.X = nozzle.NozzleList[10].Position.X;
                    nozPosition.Y = nozzle.NozzleList[10].Position.Y;
                    break;
                default:
                    nozId = "";
                    break;
            }

            if (nozId == "")
            {
                MessageBox.Show("基準吸嘴選擇錯誤。", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Task<MessageBoxResult> actionSelect = new Task<MessageBoxResult>(() =>
             {
                 return MessageBox.Show("將吸嘴在相機中置中，按下【確定】後取得座標值。", "動作提示",
                     MessageBoxButton.OKCancel, MessageBoxImage.Information, MessageBoxResult.OK,
                     MessageBoxOptions.DefaultDesktopOnly);
             });
            actionSelect.Start();
            actionSelect.ContinueWith(x =>
            {
                if (x.Result == MessageBoxResult.OK)
                {
                    var servoX = epcio.ServoX;
                    var servoY = epcio.ServoY;
                    var servoZ = epcio.ServoZ;

                    // 吸嘴
                    nozzle.DatumNozzleId = datumNozzleId;
                    var noz = nozzle.NozzleList[(int)datumNozzleId];

                    noz.Position.X = servoX.GetCurrentPosition();
                    noz.Position.Y = servoY.GetCurrentPosition();
                    noz.Position.Z = servoZ.GetCurrentPosition();

                    noz.Pulse.X = servoX.GetCurrentPulse();
                    noz.Pulse.Y = servoY.GetCurrentPulse();
                    noz.Pulse.Z = servoZ.GetCurrentPulse();

                    noz.Encoder.X = servoX.GetCurrentEncoder();
                    noz.Encoder.Y = servoY.GetCurrentEncoder();
                    noz.Encoder.Z = servoZ.GetCurrentEncoder();

                    // (Xf,Yf)
                    camera.FixCamera.OriginX = servoX.GetCurrentPosition();
                    camera.FixCamera.OriginY = servoY.GetCurrentPosition();
                    camera.FixCamera.OriginZ = servoZ.GetCurrentPosition();

                    // 固定相機與基準點相對位置
                    machine.DatumPoint1.DistanceToFixCamera.X = noz.Position.X - machine.DatumPoint1.Position.X;
                    machine.DatumPoint1.DistanceToFixCamera.Y = noz.Position.Y - machine.DatumPoint1.Position.Y;

                    nozzle.WriteParameter();
                    camera.WriteParameter();
                    machine.WriteParameter();
                }
            });
        }

        /********************
         * 其他動作
         *******************/
        private void CylinderUp()
        {
            switch (datumNozzleId)
            {
                case ENozzleId.Nozzle01:
                    epcio.Nozzle01_Cylinder.Value = false;
                    break;
                case ENozzleId.Nozzle02:
                    epcio.Nozzle01_Cylinder.Value = false;
                    break;
                case ENozzleId.Nozzle03:
                    epcio.Nozzle01_Cylinder.Value = false;
                    break;
                case ENozzleId.Nozzle04:
                    epcio.Nozzle01_Cylinder.Value = false;
                    break;
                case ENozzleId.Nozzle05:
                    epcio.Nozzle01_Cylinder.Value = false;
                    break;
                case ENozzleId.Nozzle06:
                    epcio.Nozzle01_Cylinder.Value = false;
                    break;
                case ENozzleId.Nozzle07:
                    epcio.Nozzle01_Cylinder.Value = false;
                    break;
                case ENozzleId.Nozzle08:
                    epcio.Nozzle01_Cylinder.Value = false;
                    break;
                case ENozzleId.Nozzle09:
                    epcio.Nozzle01_Cylinder.Value = false;
                    break;
                case ENozzleId.Nozzle10:
                    epcio.Nozzle01_Cylinder.Value = false;
                    break;
                case ENozzleId.Nozzle11:
                    epcio.Nozzle01_Cylinder.Value = false;
                    break;
            }
        }

        private void CylinderDown()
        {
            switch (datumNozzleId)
            {
                case ENozzleId.Nozzle01:
                    epcio.Nozzle01_Cylinder.Value = true;
                    break;
                case ENozzleId.Nozzle02:
                    epcio.Nozzle01_Cylinder.Value = true;
                    break;
                case ENozzleId.Nozzle03:
                    epcio.Nozzle01_Cylinder.Value = true;
                    break;
                case ENozzleId.Nozzle04:
                    epcio.Nozzle01_Cylinder.Value = true;
                    break;
                case ENozzleId.Nozzle05:
                    epcio.Nozzle01_Cylinder.Value = true;
                    break;
                case ENozzleId.Nozzle06:
                    epcio.Nozzle01_Cylinder.Value = true;
                    break;
                case ENozzleId.Nozzle07:
                    epcio.Nozzle01_Cylinder.Value = true;
                    break;
                case ENozzleId.Nozzle08:
                    epcio.Nozzle01_Cylinder.Value = true;
                    break;
                case ENozzleId.Nozzle09:
                    epcio.Nozzle01_Cylinder.Value = true;
                    break;
                case ENozzleId.Nozzle10:
                    epcio.Nozzle01_Cylinder.Value = true;
                    break;
                case ENozzleId.Nozzle11:
                    epcio.Nozzle01_Cylinder.Value = true;
                    break;
            }
        }

        /********************
         * 繫結
         *******************/
        private List<string> _cameraSelectSource;
        public List<string> CameraSelectSource
        {
            get { return _cameraSelectSource; }
            set { SetProperty(ref _cameraSelectSource, value); }
        }

        private int _cameraSelectIndex;
        public int CameraSelectIndex
        {
            get { return _cameraSelectIndex; }
            set { SetProperty(ref _cameraSelectIndex, value); }
        }

        private string _cameraSelectItem;
        public string CameraSelectItem
        {
            get { return _cameraSelectItem; }
            set
            {
                SetProperty(ref _cameraSelectItem, value);
                OnMoveCameraCorrectVisibility = value == camera.CAMERA_NAME_LIST[camera.FIX_CAMERA_ID];
            }
        }

        private string _baseNozzleSelector;
        public string BaseNozzleSelector
        {
            get { return _baseNozzleSelector; }
            set
            {
                SetProperty(ref _baseNozzleSelector, value);
                datumNozzleId = Enum.Parse<ENozzleId>(value);
                nozzle.WriteParameter(); // TODO: 只更新datumNozzleId
            }
        }

        // OnFly
        private bool _onMoveCameraCorrectVisibility;
        public bool OnMoveCameraCorrectVisibility
        {
            get { return _onMoveCameraCorrectVisibility; }
            set { SetProperty(ref _onMoveCameraCorrectVisibility, value); }
        }
    }
}
