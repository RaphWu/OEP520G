using Imageproject.Converters;
using Imageproject.Models;
using Prism.Commands;
using Prism.Mvvm;
using System.IO.Ports;
using System;

using System.Windows.Media.Imaging;
using TIS.Imaging;
using Imageproject.Contracts;

namespace Imageproject.ViewModels
{
    public class ImagedisplayViewModel : BindableBase
    {


        //ICImagingControl ic = new ICImagingControl();
        public DelegateCommand FixedCommand { get; private set; }
        public DelegateCommand MobileCommand { get; private set; }
        public DelegateCommand lockingCommand { get; private set; }
        public DelegateCommand widthlessCommand { get; private set; }
        public DelegateCommand ResetCommand { get; private set; }
        private double camera;

        private readonly ICamera _camera;
        private readonly ILighting _lighting;

        // ctor
        public ImagedisplayViewModel(ICamera camera, ILighting lighting)
        {
            FixedCommand = new DelegateCommand(Fixed);
            MobileCommand = new DelegateCommand(Mobile);
            lockingCommand = new DelegateCommand(locking);
            widthlessCommand = new DelegateCommand(widthless);
            ResetCommand = new DelegateCommand(Reset);
            this.camera = 0;

            _camera = camera;
            _lighting = lighting;
        }


        public void Fixed()
        {
            camera = 1;
            //ImageParameters.FixCamera = new ICImagingControl();
            //ImageParameters.FixCamera.LoadDeviceStateFromFile(CameraCfgFile.FixCameraXmlFile, true);
            //ImageParameters.FixCamera.Name = "FixCamera";
            //var VideoDemoSink = new FrameQueueSink(VideoDemoFunc, MediaSubtypes.Y800, 5);
            //ImageParameters.FixCamera.Sink = VideoDemoSink;
            //if (ImageParameters.FixCamera.LiveVideoRunning)
            //    ImageParameters.FixCamera.LiveStop();
            //ImageParameters.FixCamera.DeviceTrigger = false;
            //ImageParameters.FixCamera.LiveDisplay = true;
            //ImageParameters.FixCamera.LiveStart();

            _camera.MoveCameraOff();
            _camera.FixCameraOn(VideoDemoFunc);
        }

        public void Mobile()
        {
            camera = 2;
            //ImageParameters.MoveCamera = new ICImagingControl();
            //ImageParameters.MoveCamera.LoadDeviceStateFromFile(CameraCfgFile.MoveCameraXmlFile, true);
            //ImageParameters.MoveCamera.Name = "FixCamera";
            //var VideoDemoSink = new FrameQueueSink(VideoDemoFunc, MediaSubtypes.Y800, 5);
            //ImageParameters.MoveCamera.Sink = VideoDemoSink;
            //if (ImageParameters.MoveCamera.LiveVideoRunning)
            //    ImageParameters.MoveCamera.LiveStop();
            //ImageParameters.MoveCamera.DeviceTrigger = false;
            //ImageParameters.MoveCamera.LiveDisplay = true;
            //ImageParameters.MoveCamera.LiveStart();

            _camera.FixCameraOff();
            _camera.MoveCameraOn(VideoDemoFunc);
        }

        public void locking()
        {



        }

        public void widthless()
        {
            _width = _width - 1;
            width = _width;
        }

        public void Reset()
        {
            _width = 0;
            _height = 0;
            _left = 0;
            _top = 0;
        }

        public void coordinate()
        {
            if (camera == 1)
            {
                width1 = _width / 3;
                height1 = _height / 3;
                left1 = _left - (_width1 / 2);
                top1 = _top - (_height1 / 2);
                widthmm = width1 / 1270 * 25.4;
                heightmm = height1 / 1270 * 25.4;
            }

            if (camera == 2)
            {
                width1 = _width / 3 / 1.25;
                height1 = _height / 3 / 1.25;
                left1 = 225 - (_width1 / 2);
                top1 = 225 - (_height1 / 2);
                widthmm = width1 / 1270 * 25.4;
                heightmm = height1 / 1270 * 25.4;

                //width1 = _width / 3 / 1.25;
                //height1 = _height / 3 / 1.25;
                //left1 = _left - (_width1 / 2);
                //top1 = _top - (_height1 / 2);
                //widthmm = width1 / 1270 * 25.4;
                //heightmm = height1 / 1270 * 25.4;
            }
        }


        private FrameQueuedResult VideoDemoFunc(IFrameQueueBuffer img)
        {
            VideoSource = ImageFormatter.BitmapToBitmapImage(img.CreateBitmapWrap());

            return FrameQueuedResult.ReQueue;
        }

        private DelegateCommand _Unloaded;
        public DelegateCommand Unloaded
            => _Unloaded ??= new DelegateCommand(ExecuteUnloaded);
        void ExecuteUnloaded()
        {
            CameraParameters.FixCamera.LiveStop();
            CameraParameters.FixCamera.DeviceTrigger = false;
        }

        private BitmapImage _VideoSource;
        public BitmapImage VideoSource
        {
            get { return _VideoSource; }
            set { SetProperty(ref _VideoSource, value); }
        }



        /// <summary>
        /// 繫結
        /// </summary>

        private double _width;
        public double width
        {
            get
            {
                return _width;

            }
            set
            {
                SetProperty(ref _width, value);
                coordinate();
            }
        }
        private double _height;
        public double height
        {
            get
            {
                return _height;

            }
            set
            {
                SetProperty(ref _height, value);
                coordinate();
            }
        }
        private double _left;
        public double left
        {
            get
            {
                return _left;
            }
            set
            {
                SetProperty(ref _left, value);
                coordinate();
            }

        }
        private double _top;
        public double top
        {
            get
            {
                return _top;
            }
            set
            {
                SetProperty(ref _top, value);
                coordinate();
            }
        }
        private double _left1;
        public double left1
        {
            get { return _left1; }
            set { SetProperty(ref _left1, value); }

        }
        private double _top1;
        public double top1
        {
            get { return _top1; }
            set { SetProperty(ref _top1, value); }
        }

        public double pxdata { get; private set; }
        private double _width1;
        public double width1
        {
            get { return _width1; }
            set { SetProperty(ref _width1, value); }
        }
        private double _height1;
        public double height1
        {
            get { return _height1; }
            set { SetProperty(ref _height1, value); }
        }
        private double _widthmm;
        public double widthmm
        {
            get { return _widthmm; }
            set { SetProperty(ref _widthmm, value); }
        }
        private double _heightmm;
        public double heightmm
        {
            get { return _heightmm; }
            set { SetProperty(ref _heightmm, value); }
        }

        private double _Upperlightsource;
        public double Upperlightsource
        {
            get { return _Upperlightsource; }
            set { SetProperty(ref _Upperlightsource, value); }
        }

    }

}

