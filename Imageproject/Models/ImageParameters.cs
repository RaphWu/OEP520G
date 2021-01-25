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
        public const int MAX_IMAGE_COUNT = 11;

        // 固定相機
        public static ICImagingControl FixCamera = null;
        public static FrameSnapSink FixSnapSink;
        public static CameraStatus FixCameraStatus = CameraStatus.Disable;

        // 移動相機
        public static ICImagingControl MoveCamera = null;
        public static FrameSnapSink MoveSnapSink;
        public static CameraStatus MoveCameraStatus = CameraStatus.Disable;

        // SnapSink拍照存放區
        public static IFrameQueueBuffer SnapShot;

        // 連續拍照存放區(飛拍)
        public static IFrameQueueBuffer[] FrameList = new IFrameQueueBuffer[MAX_IMAGE_COUNT];
    }
}
