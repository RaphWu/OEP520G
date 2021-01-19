using EPCIO;
using Imageproject.Constants;
using Imageproject.Contracts;
using OEP520G.Core;
using OEP520G.Parameter;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Text;

namespace OEP520G.Teaching.ViewModels
{
    public class RotationCenterViewModel : BindableBase, IActiveAware
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly Stage stage = Stage.Instance;

        /// <summary>
        /// 顯示本文
        /// </summary>
        StringBuilder doc = new StringBuilder();

        // 按鍵
        public DelegateCommand MoveHereCommand { get; private set; }
        public DelegateCommand GetCoorCommand { get; private set; }
        public DelegateCommand StartCorrectCommand { get; private set; }
        public DelegateCommand UpdateCoorCommand { get; private set; }

        // 視窗Active/Deactive
        public event EventHandler IsActiveChanged;
        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        private readonly IImage _image;

        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public RotationCenterViewModel(IImage image)
        {
            _image = image;

            MoveHereCommand = new DelegateCommand(MoveHere);
            GetCoorCommand = new DelegateCommand(GetCoor);
            StartCorrectCommand = new DelegateCommand(StartCorrect);
            UpdateCoorCommand = new DelegateCommand(UpdateCoor);

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
            RotateCenterCoorX = stage.RotateCenter.X;
            RotateCenterCoorY = stage.RotateCenter.Y;
        }

        /// <summary>
        /// 存回參數值
        /// </summary>
        public void WriteData()
        {
            if (IsActive)
            {
                stage.RotateCenter.X = RotateCenterCoorX;
                stage.RotateCenter.Y = RotateCenterCoorY;

                stage.WriteParameter();
            }
        }

        /********************
         * 按鍵
         *******************/
        /// <summary>
        /// 移動旋轉中心座標
        /// </summary>
        private void MoveHere()
            => epcio.MoveTo(positionX: RotateCenterCoorX,
                            positionY: RotateCenterCoorY);

        /// <summary>
        /// 取得軸座標
        /// </summary>
        private void GetCoor()
        {
            VisionCoorX = epcio.ServoX.GetCurrentPosition();
            VisionCoorY = epcio.ServoY.GetCurrentPosition();
        }

        /// <summary>
        /// 開始執行
        /// </summary>
        private void StartCorrect()
        {
            //_image.TakePictureStart();
            //_image.TakePictures(EImageTargetId.Stage);
            //_image.TakePictureFinish();

            //doc.AppendLine("asdfasdfascxzcvwed");
            //DocumentDisplay = doc.ToString();
        }

        /// <summary>
        /// 將畫像座標更新至旋轉中心座標
        /// </summary>
        private void UpdateCoor()
        {
            RotateCenterCoorX = VisionCoorX;
            RotateCenterCoorY = VisionCoorY;
        }

        /********************
         * 繫結
         *******************/
        private string _documentDisplay;
        public string DocumentDisplay
        {
            get { return _documentDisplay; }
            set { SetProperty(ref _documentDisplay, value); }
        }

        private double _rotateCenterCoorX;
        public double RotateCenterCoorX
        {
            get { return _rotateCenterCoorX; }
            set { SetProperty(ref _rotateCenterCoorX, value); }
        }

        private double _rotateCenterCoorY;
        public double RotateCenterCoorY
        {
            get { return _rotateCenterCoorY; }
            set { SetProperty(ref _rotateCenterCoorY, value); }
        }

        private double _visionCoorX;
        public double VisionCoorX
        {
            get { return _visionCoorX; }
            set { SetProperty(ref _visionCoorX, value); }
        }

        private double _visionCoorY;
        public double VisionCoorY
        {
            get { return _visionCoorY; }
            set { SetProperty(ref _visionCoorY, value); }
        }
    }
}
