using OEP520G.Core;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using System;

namespace OEP520G.Parameter.ViewModels
{
    public class FixCameraViewModel : BindableBase, IActiveAware
    {
        private Camera camera = Camera.Instance;

        // 視窗Active/Deactive
        public event EventHandler IsActiveChanged;
        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                if (value)
                    ReadData();
            }
        }

        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public FixCameraViewModel()
        {
            ReadData();

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
                camera.FixCamera.CameraId = CameraIdField;
                camera.FixCamera.OriginX = OriginXField;
                camera.FixCamera.OriginY = OriginYField;
                camera.FixCamera.OriginZ = OriginZField;

                camera.WriteParameter();
            }
        }

        /// <summary>
        /// 取得參數值
        /// </summary>
        public void ReadData()
        {
            CameraIdField = camera.FixCamera.CameraId;
            OriginXField = camera.FixCamera.OriginX;
            OriginYField = camera.FixCamera.OriginY;
            OriginZField = camera.FixCamera.OriginZ;
        }

        /********************
         * 繫結
         *******************/
        private int _cameraIdField;
        public int CameraIdField
        {
            get { return _cameraIdField; }
            set { SetProperty(ref _cameraIdField, value); }
        }

        private double _originXField;
        public double OriginXField
        {
            get { return _originXField; }
            set { SetProperty(ref _originXField, value); }
        }

        private double _originYField;
        public double OriginYField
        {
            get { return _originYField; }
            set { SetProperty(ref _originYField, value); }
        }

        private double _originZField;
        public double OriginZField
        {
            get { return _originZField; }
            set { SetProperty(ref _originZField, value); }
        }
    }
}
