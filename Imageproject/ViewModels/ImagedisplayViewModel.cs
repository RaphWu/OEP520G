using Imageproject.Contracts;
using Imageproject.Converters;
using Imageproject.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Events;
using System;

using System.Windows.Media.Imaging;
using TIS.Imaging;
using Prism;
using TcpipServer.Services;

namespace Imageproject.ViewModels
{
    public class ImagedisplayViewModel : BindableBase, IActiveAware
    {


        //ICImagingControl ic = new ICImagingControl();
        public DelegateCommand FixedCommand { get; private set; }
        public DelegateCommand MobileCommand { get; private set; }
        public DelegateCommand lockingCommand { get; private set; }
        public DelegateCommand widthlessCommand { get; private set; }
        public DelegateCommand ResetCommand { get; private set; }
        private double width1;
        private double width2;

        // View Active/Deactive & ApplicationCommands
        private bool _IsActive = false;
        public bool IsActive
        {
            get { return _IsActive; }
            set
            {
                _IsActive = value;

                if (!value)
                {
                    _image.FixCameraOff();
                    _image.MoveCameraOff();
                }
            }
        }
        public event EventHandler IsActiveChanged;

        private readonly IEventAggregator _ea;
        private readonly IImage _image;

        // ctor5
        public ImagedisplayViewModel(IEventAggregator ea, IImage image)
        {
            _ea = ea;
            _image = image;

            FixedCommand = new DelegateCommand(Fixed);
            MobileCommand = new DelegateCommand(Mobile);
            lockingCommand = new DelegateCommand(locking);
            widthlessCommand = new DelegateCommand(widthless);
            ResetCommand = new DelegateCommand(Reset);
        }

        private void Fixed()
        {

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

            _image.MoveCameraOff();
            _image.FixCameraOn(VideoDemoFunc);
        }

        private void Mobile()
        {
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

            _image.FixCameraOff();
            _image.MoveCameraOn(VideoDemoFunc);
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
            left1 = _left - (_width / 2);
            top1 = _top - (_height / 2);
        }


        //public void cameraopen()
        //{   /*
        //    ImageParameters.FixCamera.LoadDeviceStateFromFile(CameraCfgFile.FixCameraXmlFile, true);
        //    //ic.LoadDeviceStateFromFile("device.xml", true);

        //    ImageParameters.FixCamera = new ICImagingControl();
        //    ImageParameters.FixCamera.Name = "FixCamera";
        //    var VideoDemoSink = new FrameQueueSink(VideoDemoFunc, MediaSubtypes.Y800, 5);
        //    ImageParameters.FixCamera.Sink = VideoDemoSink;
        //    if (ImageParameters.FixCamera.LiveVideoRunning)
        //        ImageParameters.FixCamera.LiveStop();
        //    ImageParameters.FixCamera.DeviceTrigger = false;
        //    ImageParameters.FixCamera.LiveDisplay = true;
        //    ImageParameters.FixCamera.LiveStart();
        //    */

        //    try
        //    {
        //        ic.LoadDeviceStateFromFile("device.xml", true);
        //    }
        //    catch
        //    {
        //        ic.ShowDeviceSettingsDialog();
        //        if (ic.DeviceValid)
        //        {
        //            // Save the currently used device into a file in order to be able to open it
        //            // automatically at the next program start.
        //            ic.SaveDeviceStateToFile("device.xml");
        //        }

        //    }

        //    //ic = new ICImagingControl();
        //    ic.Name = "FixCamera";
        //    var VideoDemoSink = new FrameQueueSink(VideoDemoFunc, MediaSubtypes.Y800, 5);
        //    ic.Sink = VideoDemoSink;
        //    if (ic.LiveVideoRunning)
        //        ic.LiveStop();
        //    ic.DeviceTrigger = false;
        //    ic.LiveDisplay = true;
        //    ic.LiveStart();

        //}

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
            ImageParameters.FixCamera.LiveStop();
            ImageParameters.FixCamera.DeviceTrigger = false;
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
            get
            {
                return _left1;
            }
            set
            {
                SetProperty(ref _left1, value);

            }

        }
        private double _top1;
        public double top1
        {
            get
            {
                return _top1;
            }
            set
            {
                SetProperty(ref _top1, value);

            }
        }
    }





}

