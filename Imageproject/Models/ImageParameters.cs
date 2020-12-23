using System;
using System.Collections.Generic;
using System.Text;
using TcpipServer.Models;
using TIS.Imaging;

namespace Imageproject.Models
{
    public class ImageParameters
    {
        public static ICImagingControl FixCamera;
        public static FrameSnapSink FixCameraSnapSink;
        public static ICImagingControl MoveCamera;
        public static FrameSnapSink MoveCameraSnapSink;

        // 接收到的所有影像
        internal static List<ObjectImage> ImageList = null;

        // 拍照取得影像的Buff
        internal static List<FrameBuff> ReceiveFrame = new List<FrameBuff>();
    }
}
