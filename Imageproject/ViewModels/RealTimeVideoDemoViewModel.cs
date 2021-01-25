using Imageproject.Converters;
using Imageproject.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Imaging;
using TIS.Imaging;

namespace Imageproject.ViewModels
{
    public class RealTimeVideoDemoViewModel : BindableBase
    {
        FrameQueueSink VideoDemoSink;

        public RealTimeVideoDemoViewModel()
        {
            ImageParameters.FixCamera = new ICImagingControl();
            ImageParameters.FixCamera.Name = "FixCamera";
            ImageParameters.FixCamera.LoadDeviceStateFromFile(CameraCfgFile.FixCameraXmlFile, true);

             VideoDemoSink = new FrameQueueSink(VideoDemoFunc, MediaSubtypes.Y800, 5);
            ImageParameters.FixCamera.Sink = VideoDemoSink;

            if (ImageParameters.FixCamera.LiveVideoRunning)
                ImageParameters.FixCamera.LiveStop();
            ImageParameters.FixCamera.DeviceTrigger = false;
            ImageParameters.FixCamera.LiveDisplay = true;
            ImageParameters.FixCamera.LiveStart();
        }

        private FrameQueuedResult VideoDemoFunc(IFrameQueueBuffer img)
        {
            VideoSource = ImageFormatter.BitmapToBitmapImage(img.CreateBitmapWrap());

            Copied = VideoDemoSink.CountOfFramesCopied.ToString();
            Dropped = VideoDemoSink.CountOfFramesDropped.ToString();

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

        private DelegateCommand _SnapCommand;
        public DelegateCommand SnapCommand =>
            _SnapCommand ?? (_SnapCommand = new DelegateCommand(ExecuteSnapCommand));
                void ExecuteSnapCommand()
        {
            VideoSnap = VideoSource;
        }

        private BitmapImage _VideoSource;
        public BitmapImage VideoSource
        {
            get { return _VideoSource; }
            set { SetProperty(ref _VideoSource, value); }
        }

        private BitmapImage _VideoSnap;
        public BitmapImage VideoSnap
        {
            get { return _VideoSnap; }
            set { SetProperty(ref _VideoSnap, value); }
        }

        private string _Copied;
        public string Copied
        {
            get { return _Copied; }
            set { SetProperty(ref _Copied, value); }
        }

        private string _Dropped;
        public string Dropped
        {
            get { return _Dropped; }
            set { SetProperty(ref _Dropped, value); }
        }
    }
}
