using Imageproject.Constants;
using System;
using System.Collections.Generic;
using System.Text;
using TcpipServer.Models;
using TIS.Imaging;

namespace Imageproject.Models
{
    public class ImageParameters
    {
        //public static CameraId ActiveCameraId;
        //public static ICImagingControl ActiveCamera = null;
        //public static SinkType ActiveSinkType;

        public static ICImagingControl FixCamera = null;
        public static FrameQueueSink FixCameraSink;

        public static ICImagingControl MoveCamera = null;
        public static FrameQueueSink MoveCameraSink;

        // 接收到的所有影像
        public const int MAX_IMAGE_COUNT = 11;
        //public static List<IFrameQueueBuffer> ReceiveFrame = new List<IFrameQueueBuffer>();
        //public static List<ObjectImage> ImageList = null;
    }
}
