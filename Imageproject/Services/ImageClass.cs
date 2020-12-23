using CSharpCore.Logger;
using Imageproject.Constants;
using Imageproject.Contracts;
using Imageproject.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using TcpipServer.Contracts;
using TcpipServer.Models;
using TIS.Imaging;
using Prism.Events;
using TcpipServer.Services;
using Newtonsoft.Json;

namespace Imageproject.Services
{
    public class ImageClass : IImage
    {
        //private readonly Logger _log = Logger.Instance;

        private EImageTargetId _objId;          // 拍照需求的物件ID
        private bool isCameraActived = false;   // 是否允許拍照

        // TODO: _receiveImage須修改成偖存全部影像的物件，
        // 全部影像列表要用畫像ID，圖片存檔的檔名也要改成畫像ID
        // 拍照物件列表只需ID就好

        private readonly IEventAggregator _ea;
        private readonly ITcpipServer _tcpipServer;

        /// <summary>
        /// 建構函式
        /// </summary>
        public ImageClass(IEventAggregator ea, ITcpipServer tcpipServer)
        {
            _ea = ea;
            _tcpipServer = tcpipServer;

            if (ImageParameters.FixCamera == null)
            {
                ImageParameters.FixCamera = new ICImagingControl();
                ImageParameters.FixCamera.Name = "FixCamera";

                ImageParameters.FixCameraSnapSink = new FrameSnapSink(MediaSubtypes.Y800);
                InitFixCamera();
            }

            //if (CameraParameters.MoveCamera == null)
            //{
            //    CameraParameters.MoveCamera = new ICImagingControl();
            //    CameraParameters.MoveCamera.Name = "MoveCamera";

            //    CameraParameters.MoveCameraSnapSink = new FrameSnapSink(MediaSubtypes.Y800);
            //    InitMoveCamera();
            //}

            if (ImageParameters.ImageList == null)
            {
                ImageParameters.ImageList = new List<ObjectImage>();
                foreach (var obj in (EImageTargetId[])Enum.GetValues(typeof(EImageTargetId)))
                {
                    ImageParameters.ImageList.Add(new ObjectImage()
                    {
                        ObjectId = obj,
                        Title = obj.ToString(),
                        Image = null,
                        X = 0.0,
                        Y = 0.0,
                        A = 0.0
                    });
                }
            }

            if (!Directory.Exists(FileString.ImageDirectoty))
                Directory.CreateDirectory(FileString.ImageDirectoty);

            _ea.GetEvent<PublishSolve>().Subscribe(ReceiveSolve);
        }

        /// <inheritdoc/>
        public void InitCamera(CameraId cameraId)
        {
            if (cameraId == CameraId.FixCamera || cameraId == CameraId.All)
                InitFixCamera();

            //if (cameraId == CameraId.MoveCamera || cameraId == CameraId.All)
            //    InitMoveCamera();
        }

        /// <summary>
        /// 初始化固定相機
        /// </summary>
        private void InitFixCamera()
        {
            if (ImageParameters.FixCamera.LiveVideoRunning)
                ImageParameters.FixCamera.LiveStop();

            try
            {
                ImageParameters.FixCamera.LoadDeviceStateFromFile(CameraCfgFile.FixCameraXmlFile, true);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Load device failed. with {e}");
            }

            if (ImageParameters.FixCamera.DeviceValid)
            {
                ImageParameters.FixCamera.Sink = ImageParameters.FixCameraSnapSink;
                ImageParameters.FixCamera.LiveStart();
            }
        }

        ///// <summary>
        ///// 初始化移動相機
        ///// </summary>
        //private void InitMoveCamera()
        //{
        //    if (CameraParameters.MoveCamera.LiveVideoRunning)
        //        CameraParameters.MoveCamera.LiveStop();

        //    try
        //    {
        //        CameraParameters.MoveCamera.LoadDeviceStateFromFile(CameraCfgFile.MoveCameraXmlFile, true);
        //    }
        //    catch (Exception e)
        //    {
        //        MessageBox.Show($"Load device failed. with {e}");
        //    }

        //    if (CameraParameters.MoveCamera.DeviceValid)
        //    {
        //        CameraParameters.MoveCamera.Sink = CameraParameters.MoveCameraSnapSink;
        //        CameraParameters.MoveCamera.LiveStart();
        //    }
        //}

        /********************
         * 狀態
         ********************/
        /// <inheritdoc/>
        public void TakePictureStart()
        {
            ImageParameters.ReceiveFrame = new List<FrameBuff>();

            // 開始接受拍照
            isCameraActived = true;
        }

        /// <inheritdoc/>
        public void TakePictureFinish()
        {
            // 停止接受拍照
            isCameraActived = false;

            SaveBufferToImageList();
            //SendImageToTcpipServer();
            //SaveImageToFile();
        }

        /********************
         * 相機拍照處理
         ********************/
        /// <inheritdoc/>
        public void TakePicture(EImageTargetId objectId)
        {
            // 截圖的相機
            FrameSnapSink cameraToUse;

            if (!isCameraActived)
                return;

            //_log.WriteLine($"\t\tCapture Image: {objectId}");

            _objId = objectId;

            // 判斷採用的相機
            if (_objId < EImageTargetId.Nozzle01 || _objId == EImageTargetId.MovingCamera)
                cameraToUse = ImageParameters.MoveCameraSnapSink;
            else if (_objId <= EImageTargetId.Needle || _objId == EImageTargetId.FixCamera)
                cameraToUse = ImageParameters.FixCameraSnapSink;
            else
                return;

            try
            {
                // 截取影像
                IFrameQueueBuffer frm = cameraToUse.SnapSingle(TimeSpan.FromMilliseconds(500));

                // 判斷是否須求解
                bool needSolve = (_objId == EImageTargetId.MovingCamera || _objId == EImageTargetId.FixCamera)
                    ? false : true;

                // 影像存入Buffer
                ImageParameters.ReceiveFrame.Add(new FrameBuff()
                {
                    ObjectId = _objId,
                    Frame = frm,
                    NeedSolve = needSolve
                });
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to snap image: {e.Message}");
            }
        }

        /********************
         * 求解
         ********************/
        /// <summary>
        /// 將影像List傳給TCP/IP Server
        /// </summary>
        /// <returns>傳送是否成功</returns>
        /// <remarks>工研院UI專用</remarks>
        private bool SendImageToTcpipServer()
        {
            List<ImageInfo> toSendImages = new List<ImageInfo>();

            foreach (var item in ImageParameters.ReceiveFrame)
            {
                if (item.NeedSolve)
                    toSendImages.Add(new ImageInfo()
                    {
                        ObjectId = (int)item.ObjectId,
                        Title = item.ObjectId.ToString(),
                        imgByte = BitmapToByteArray(item.Frame.CreateBitmapWrap())
                    });
            }

            toSendImages.Sort((x, y) => { return x.ObjectId.CompareTo(y.ObjectId); });
            return _tcpipServer.SendImage(toSendImages);
        }

        /// <summary>
        /// 接收工研院UI -> TCP/IP傳回的演算結果
        /// </summary>
        private void ReceiveSolve(string result)
        {
            var jsonstrResult = JsonConvert.DeserializeObject<List<ResultInfo>>(result);

            foreach (var item in jsonstrResult)
            {
                var imageItem = ImageParameters.ImageList.Find(x => (int)x.ObjectId == item.Id);
                imageItem.X = item.X;
                imageItem.Y = item.Y;
                imageItem.A = item.A;
            }

            _ea.GetEvent<RequestUpdateDemo>().Publish("");
        }

        /********************
         * 影像格式轉換
         ********************/
        /// <summary>
        /// Bitmap轉byte[]
        /// </summary>
        private byte[] BitmapToByteArray(Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] bytes = ms.GetBuffer();
            ms.Close();

            return bytes;
        }

        /// <summary>
        /// Bitmap轉BitmapImage
        /// </summary>
        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Bmp);

            stream.Position = 0;

            BitmapImage result = new BitmapImage();
            result.BeginInit();
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();

            return result;
        }

        /********************
         * 影像儲存
         ********************/
        /// <summary>
        /// 將相機的Frame存至影像List
        /// </summary>
        private void SaveBufferToImageList()
        {
            foreach (var item in ImageParameters.ReceiveFrame)
            {
                if (item.NeedSolve)
                {
                    var imageItem = ImageParameters.ImageList.Find(x => x.ObjectId == item.ObjectId);

                    imageItem.ObjectId = item.ObjectId;
                    imageItem.Title = item.ObjectId.ToString();
                    imageItem.Image = BitmapToBitmapImage(item.Frame.CreateBitmapWrap());
                    imageItem.X = 0.0;
                    imageItem.Y = 0.0;
                    imageItem.A = 0.0;
                }
            }
        }

        /// <summary>
        /// 影像存檔
        /// </summary>
        private void SaveImageToFile()
        {
            //log.WriteLine($"ImageSave: ");

            string saveTime = DateTime.Now.ToString("yyyMMdd hhmmss");

            foreach (var item in ImageParameters.ReceiveFrame)
            {
                string dir = $@"{FileString.ImageDirectoty}\{item.ObjectId}";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                if (item.NeedSolve)
                {
                    // TODO: 檔名要改成畫像ID
                    string fullFileName = $@"{dir}\{item.ObjectId} {saveTime}.bmp";
                    item.Frame.SaveAsBitmap(fullFileName);
                }
            }
        }

        /********************
         * 相機內建設定程式
         ********************/
        /// <inheritdoc/>
        public void DeviceSetting(CameraId cameraId)
        {
            var camera = (cameraId == CameraId.FixCamera)
                ? ImageParameters.FixCamera
                : ImageParameters.MoveCamera;

            string cameraXml = (cameraId == CameraId.FixCamera)
                ? CameraCfgFile.FixCameraXmlFile
                : CameraCfgFile.MoveCameraXmlFile;

            if (camera.LiveVideoRunning)
                camera.LiveStop();

            camera.ShowDeviceSettingsDialog();

            if (camera.DeviceValid)
            {
                camera.SaveDeviceStateToFile(cameraXml);
                InitCamera(cameraId);
            }
        }

        /// <inheritdoc/>
        public void PropertSetting(CameraId cameraId)
        {
            var camera = (cameraId == CameraId.FixCamera)
                ? ImageParameters.FixCamera
                : ImageParameters.MoveCamera;

            string cameraXml = (cameraId == CameraId.FixCamera)
                ? CameraCfgFile.FixCameraXmlFile
                : CameraCfgFile.MoveCameraXmlFile;

            if (camera.LiveVideoRunning)
                camera.LiveStop();

            camera.ShowPropertyDialog();

            if (camera.DeviceValid)
            {
                camera.SaveDeviceStateToFile(cameraXml);
                InitCamera(cameraId);
            }
        }
    }
}
