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
using Imageproject.Converters;

namespace Imageproject.Services
{
    public class ImageService : IImage
    {
        //private readonly Logger _log = Logger.Instance;

        //private EImageTargetId _objId;          // 拍照需求的物件ID
        //private bool _hwTriggerMode = false;    // 是否在硬體觸發狀態
        //private bool _liveMode = false;         // 是否在Live狀態

        private IFrameQueueBuffer _fixCameraFrame;
        private IFrameQueueBuffer _moveCameraFrame;

        //private FrameSnapSink _frameSnapSink = new FrameSnapSink(MediaSubtypes.Y800);
        //private FrameQueueSink _frameQueueSink;

        private TimeSpan _timeOut = TimeSpan.FromMilliseconds(500);

        // TODO: _receiveImage須修改成偖存全部影像的物件，
        // 全部影像列表要用畫像ID，圖片存檔的檔名也要改成畫像ID
        // 拍照物件列表只需ID就好

        private readonly IEventAggregator _ea;
        private readonly ITcpipServer _tcpipServer;

        /// <summary>
        /// 建構函式
        /// </summary>
        public ImageService(IEventAggregator ea, ITcpipServer tcpipServer)
        {
            _ea = ea;
            _tcpipServer = tcpipServer;

            if (ImageParameters.FixCamera == null)
            {
                ImageParameters.FixCamera = new ICImagingControl
                {
                    Name = CameraId.FixCamera.ToString()
                };
                LoadFixCamera();
            }

            if (ImageParameters.MoveCamera == null)
            {
                ImageParameters.MoveCamera = new ICImagingControl
                {
                    Name = CameraId.MoveCamera.ToString()
                };
                LoadMoveCamera();
            }

            //if (ImageParameters.ImageList == null)
            //{
            //    ImageParameters.ImageList = new List<ObjectImage>();
            //    foreach (var obj in (EImageTargetId[])Enum.GetValues(typeof(EImageTargetId)))
            //    {
            //        ImageParameters.ImageList.Add(new ObjectImage()
            //        {
            //            ObjectId = obj,
            //            Title = obj.ToString(),
            //            Image = null,
            //            X = 0.0,
            //            Y = 0.0,
            //            A = 0.0
            //        });
            //    }
            //}

            //if (ImageParameters.ActiveCamera == null)
            //{
            //    //SwitchCamera(CameraId.FixCamera);
            //    LiveOff();
            //}

            if (!Directory.Exists(FileString.ImageDirectoty))
                Directory.CreateDirectory(FileString.ImageDirectoty);

            _ea.GetEvent<PublishSolve>().Subscribe(ReceiveSolve);
        }

        public IFrameQueueBuffer FixCameraFrame => _fixCameraFrame;
        public IFrameQueueBuffer MoveCameraFrame => _moveCameraFrame;

        /// <inheritdoc/>
        public void FixCameraOn() => LoadFixCamera();

        /// <inheritdoc/>
        public void FixCameraOff()
        {
            ImageParameters.FixCamera = null;
            _fixCameraFrame = null;
        }

        /// <inheritdoc/>
        public void MoveCameraOn() => LoadMoveCamera();

        /// <inheritdoc/>
        public void MoveCameraOff()
        {
            ImageParameters.MoveCamera = null;
            _moveCameraFrame = null;
        }

        /// <summary>
        /// 依CameraId
        /// </summary>
        /// <param name="cameraId"></param>
        private void LoadCamera(CameraId cameraId)
        {
            if (cameraId == CameraId.FixCamera)
                LoadFixCamera();
            else if (cameraId == CameraId.MoveCamera)
                LoadMoveCamera();
        }

        /// <summary>
        /// 載入固定相機參數
        /// 設定Sink
        /// </summary>
        private void LoadFixCamera()
        {
            if (ImageParameters.FixCamera != null)
                if (ImageParameters.FixCamera.LiveVideoRunning)
                    ImageParameters.FixCamera.LiveStop();

            try
            {
                ImageParameters.FixCamera.LoadDeviceStateFromFile(CameraCfgFile.FixCameraXmlFile, true);

                ImageParameters.FixCameraSink = new FrameQueueSink(frame =>
                {
                    _fixCameraFrame = frame;
                    _ea.GetEvent<FixCameraQueued>().Publish(CameraId.FixCamera);
                    return FrameQueuedResult.ReQueue;
                }, MediaSubtypes.Y800, ImageParameters.MAX_IMAGE_COUNT);
                ImageParameters.FixCamera.Sink = ImageParameters.FixCameraSink;
            }
            catch (Exception e)
            {
                ImageParameters.FixCamera = null;
                _fixCameraFrame = null;
                MessageBox.Show($"Load device failed. with {e}");
            }
        }

        /// <summary>
        /// 載入移動相機參數
        /// 設定Sink
        /// </summary>
        private void LoadMoveCamera()
        {
            if (ImageParameters.MoveCamera != null)
                if (ImageParameters.MoveCamera.LiveVideoRunning)
                    ImageParameters.MoveCamera.LiveStop();

            try
            {
                ImageParameters.MoveCamera.LoadDeviceStateFromFile(CameraCfgFile.MoveCameraXmlFile, true);

                ImageParameters.MoveCameraSink = new FrameQueueSink(frame =>
                {
                    _moveCameraFrame = frame;
                    _ea.GetEvent<MoveCameraQueued>().Publish(CameraId.MoveCamera);
                    return FrameQueuedResult.ReQueue;
                }, MediaSubtypes.Y800, ImageParameters.MAX_IMAGE_COUNT);
            }
            catch (Exception e)
            {
                ImageParameters.MoveCamera = null;
                _moveCameraFrame = null;
                MessageBox.Show($"Load device failed. with {e}");
            }
        }

        ///********************
        // * 相機切換及狀態設定
        // ********************/
        ///// <inheritdoc/>
        //public void SwitchCamera(CameraId cameraId)
        //{
        //    if (cameraId == CameraId.FixCamera)
        //    {
        //        ImageParameters.ActiveCamera = ImageParameters.FixCamera;
        //        ImageParameters.ActiveCameraId = CameraId.FixCamera;
        //    }
        //    else if (cameraId == CameraId.MoveCamera)
        //    {
        //        ImageParameters.ActiveCamera = ImageParameters.MoveCamera;
        //        ImageParameters.ActiveCameraId = CameraId.MoveCamera;
        //    }
        //    else
        //    {
        //        ImageParameters.ActiveCamera = null;
        //        ImageParameters.ActiveCameraId = CameraId.None;
        //    }
        //}

        ///// <inheritdoc/>
        //public void HwTriggerOn()
        //{
        //    if (ImageParameters.ActiveCamera == null)
        //        return;

        //    if (!ImageParameters.ActiveCamera.DeviceValid)
        //        return;

        //    if (_hwTriggerMode)
        //        return;

        //    // 暫不強制切換至移動相機，保留將來若移動相機也要硬體觸發功能
        //    if (ImageParameters.ActiveCamera.Name != CameraId.FixCamera.ToString())
        //        return;

        //    _frameQueueSink = new FrameQueueSink(HwTriggerPicture,
        //                                         MediaSubtypes.Y800,
        //                                         ImageParameters.MAX_IMAGE_COUNT);
        //    ImageParameters.ActiveCamera.Sink = _frameQueueSink;
        //    ImageParameters.ActiveSinkType = SinkType.QueueSink;

        //    if (ImageParameters.ActiveCamera.LiveVideoRunning)
        //        ImageParameters.ActiveCamera.LiveStop();

        //    ImageParameters.ActiveCamera.DeviceTrigger = true;
        //    ImageParameters.ActiveCamera.LiveStart();
        //    _hwTriggerMode = true;
        //}

        ///// <inheritdoc/>
        //public void HwTriggerOff()
        //{
        //    if (ImageParameters.ActiveCamera == null)
        //        return;

        //    if (!ImageParameters.ActiveCamera.DeviceValid)
        //        return;

        //    if (!_hwTriggerMode)
        //    {
        //        LiveOff();
        //        return;
        //    }

        //    // PS:Sink切換及LiveStart()交由LiveOff()處理
        //    if (ImageParameters.ActiveCamera.LiveVideoRunning)
        //        ImageParameters.ActiveCamera.LiveStop();
        //    ImageParameters.ActiveCamera.DeviceTrigger = false;

        //    _hwTriggerMode = false;
        //    LiveOff();
        //}

        ///// <inheritdoc/>
        //public void LiveOn(Func<IFrameQueueBuffer, FrameQueuedResult> frameQueued)
        //    => Live(true, frameQueued);

        ///// <inheritdoc/>
        //public void LiveOff()
        //    => Live(false, null);

        ///// <summary>
        ///// 切換Live狀態及Sink設定
        ///// </summary>
        ///// <param name="liveMode">true: Live ON<br/>false:Live OFF</param>
        ///// <param name="frameQueued">
        ///// Live ON時，負責接收截取圖片的函式。<br/>
        ///// 函式規格參考官方文件：<see cref="https://www.theimagingsource.com/support/documentation/ic-imaging-control-net/meth_descFrameQueueSink_FrameQueueSink.htm"/>
        ///// </param>
        //private void Live(bool liveMode, Func<IFrameQueueBuffer, FrameQueuedResult> frameQueued)
        //{
        //    if (ImageParameters.ActiveCamera == null)
        //        return;

        //    if (!ImageParameters.ActiveCamera.DeviceValid)
        //        return;

        //    // 硬體觸發模式使用LiveOn
        //    if (_hwTriggerMode)
        //        return;

        //    // LiveOn => OFF -> FrameQueueSink+更換frameQueuedFunc
        //    // LiveOff && LiveVideoRunning => OFF -> FrameSnapSink
        //    // LiveOff && !LiveVideoRunning => do nothing
        //    if (!_liveMode && !ImageParameters.ActiveCamera.LiveVideoRunning)
        //        return;
        //    else
        //        ImageParameters.ActiveCamera.LiveStop();

        //    if (liveMode)
        //    {
        //        ImageParameters.ActiveCamera.Sink
        //           = new FrameQueueSink(frameQueued, MediaSubtypes.Y800, ImageParameters.MAX_IMAGE_COUNT);
        //        ImageParameters.ActiveSinkType = SinkType.QueueSink;
        //    }
        //    else
        //    {
        //        ImageParameters.ActiveCamera.Sink
        //            = new FrameSnapSink(MediaSubtypes.Y800);
        //        ImageParameters.ActiveSinkType = SinkType.SnapSink;
        //    }

        //    _liveMode = liveMode;
        //    ImageParameters.ActiveCamera.LiveStart();
        //}

        /********************
         * 軟體指令拍照
         ********************/
        /// <inheritdoc/>
        public IFrameQueueBuffer TakePictureFromFixCamera() => _fixCameraFrame;

        /// <inheritdoc/>
        public IFrameQueueBuffer TakePictureFromMoveCamera() => _moveCameraFrame;

        ///// <inheritdoc/>
        //public void TakePictureStart()
        //{
        //    ImageParameters.ReceiveFrame = new List<FrameBuff>();
        //    _liveMode = true;
        //}

        ///// <inheritdoc/>
        //public void TakePictureFinish()
        //{
        //    // 停止接受拍照
        //    _liveMode = false;
        //    SaveBufferToImageList();
        //    //SendImageToTcpipServer();
        //    SaveImageToFile();
        //}

        ///// <inheritdoc/>
        //public void TakePictures()
        //{
        //    //// 截圖的相機
        //    //FrameSnapSink sinkToUse;
        //    //_objId = objectId;

        //    //_log.WriteLine($"\t\tCapture Image: {objectId}");

        //    try
        //    {
        //        // 判斷採用的相機
        //        //if (_objId < EImageTargetId.Nozzle01 || _objId == EImageTargetId.MovingCamera)
        //        //    sinkToUse = ImageParameters.MoveCameraSnapSink;
        //        //els
        //        //if (_objId <= EImageTargetId.Needle || _objId == EImageTargetId.FixCamera)
        //        //    sinkToUse = ImageParameters.FixCameraSnapSink;
        //        //else
        //        //    return;

        //        // 截取影像
        //        if (ImageParameters.ActiveSinkType == SinkType.SnapSink)
        //        {
        //            FrameSnapSink sink = _frameSnapSink;
        //            IFrameQueueBuffer frm = sink.SnapSingle(_timeOut);

        //            // 影像存入Buffer
        //            ImageParameters.ReceiveFrame.Add(new FrameBuff()
        //            {
        //                ObjectId = _objId,
        //                Frame = frm
        //            });
        //        }
        //        else
        //        {

        //        }

        //        //// 判斷是否須求解
        //        //bool needSolve = (_objId == EImageTargetId.MovingCamera || _objId == EImageTargetId.FixCamera)
        //        //    ? false : true;

        //        // 影像存入Buffer
        //        ImageParameters.ReceiveFrame.Add(new FrameBuff()
        //        {
        //            ObjectId = _objId,
        //            Frame = frm
        //        });
        //    }
        //    catch (Exception e)
        //    {
        //        MessageBox.Show($"Failed to snap image: {e.Message}");
        //    }
        //}

        ///********************
        // * 硬體觸發拍照
        // ********************/
        ///// <inheritdoc/>
        //public void HwTriggerPictureStart()
        //{
        //    if (!_hwTriggerMode)
        //        return;

        //    //flyObjectId = startObjectId;
        //    ImageParameters.ReceiveFrame = new List<FrameBuff>();

        //    ImageParameters.FixCamera.Sink = ImageParameters.FixCameraQueueSink;
        //    //ImageParameters.MoveCamera.Sink = ImageParameters.MoveCameraQueueSink;

        //    // 開始接受拍照
        //    isQueueActived = true;
        //    if (ImageParameters.FixCamera.LiveVideoRunning)
        //        ImageParameters.FixCamera.LiveStop();
        //    //if (ImageParameters.MoveCamera.LiveVideoRunning)
        //    //    ImageParameters.MoveCamera.LiveStop();

        //    ImageParameters.FixCamera.DeviceTrigger = true;
        //    //ImageParameters.MoveCamera.DeviceTrigger = true;

        //    ImageParameters.FixCamera.LiveStart();
        //    //ImageParameters.MoveCamera.LiveStart();

        //    //_log.WriteLine($"\tTakeQueuePictureStart(): {startObjectId}, {flyObjectId}");
        //}

        ///// <inheritdoc/>
        //public void HwTriggerPictureFinish()
        //{
        //    if (!_hwTriggerMode)
        //        return;

        //    // 停止接受拍照
        //    ImageParameters.FixCamera.LiveStop();
        //    //ImageParameters.MoveCamera.LiveStop();
        //    ImageParameters.FixCamera.DeviceTrigger = false;
        //    //ImageParameters.MoveCamera.DeviceTrigger = false;
        //    isQueueActived = false;

        //    SaveBufferToImageList();
        //    //SendImageToTcpipServer();
        //    SaveImageToFile();

        //    //_log.WriteLine("\tTakeQueuePictureFinish()");
        //}

        ///// <summary>
        ///// Queue拍照
        ///// </summary>
        //private FrameQueuedResult HwTriggerPicture(IFrameQueueBuffer img)
        //{
        //    if (!_hwTriggerMode)
        //        return FrameQueuedResult.SkipReQueue;

        //    // 截圖的相機
        //    FrameQueueSink sinkToUse;

        //    //_log.WriteLine($"\tTakeQueuePicture: {flyObjectId}");

        //    // 判斷採用的相機
        //    //if (flyObjectId < EImageTargetId.Nozzle01 || flyObjectId == EImageTargetId.MovingCamera)
        //    //    sinkToUse = ImageParameters.MoveCameraQueueSink;
        //    //else
        //    if (flyObjectId <= EImageTargetId.Needle || flyObjectId == EImageTargetId.FixCamera)
        //        sinkToUse = ImageParameters.FixCameraQueueSink;
        //    else
        //        return FrameQueuedResult.SkipReQueue;

        //    // 判斷是否須求解
        //    bool needSolve = (flyObjectId == EImageTargetId.MovingCamera || flyObjectId == EImageTargetId.FixCamera)
        //        ? false : true;

        //    // 影像存入Buffer
        //    ImageParameters.ReceiveFrame.Add(new FrameBuff()
        //    {
        //        ObjectId = flyObjectId,
        //        Frame = img,
        //        NeedSolve = needSolve
        //    });
        //    flyObjectId++;

        //    return FrameQueuedResult.ReQueue;
        //}

        ///********************
        // * 求解
        // ********************/
        ///// <summary>
        ///// 將影像List傳給TCP/IP Server
        ///// </summary>
        ///// <returns>傳送是否成功</returns>
        ///// <remarks>工研院UI專用</remarks>
        //private bool SendImageToTcpipServer()
        //{
        //    List<ImageInfo> toSendImages = new List<ImageInfo>();

        //    foreach (var item in ImageParameters.ReceiveFrame)
        //    {
        //        if (item.NeedSolve)
        //            toSendImages.Add(new ImageInfo()
        //            {
        //                ObjectId = (int)item.ObjectId,
        //                Title = item.ObjectId.ToString(),
        //                imgByte = ImageFormatter.BitmapToByteArray(item.Frame.CreateBitmapWrap())
        //            });
        //    }

        //    toSendImages.Sort((x, y) => { return x.ObjectId.CompareTo(y.ObjectId); });
        //    return _tcpipServer.SendImage(toSendImages);
        //}

        /// <summary>
        /// 接收工研院UI -> TCP/IP傳回的演算結果
        /// </summary>
        private void ReceiveSolve(string result)
        {
            var jsonstrResult = JsonConvert.DeserializeObject<List<ResultInfo>>(result);

            //foreach (var item in jsonstrResult)
            //{
            //    var imageItem = ImageParameters.ImageList.Find(x => (int)x.ObjectId == item.Id);
            //    imageItem.X = item.X;
            //    imageItem.Y = item.Y;
            //    imageItem.A = item.A;
            //}

            //_ea.GetEvent<RequestUpdateDemo>().Publish("");
        }

        ///********************
        // * 影像儲存
        // ********************/
        ///// <summary>
        ///// 將相機的Frame存至影像List
        ///// </summary>
        //private void SaveBufferToImageList()
        //{
        //    foreach (var item in ImageParameters.ReceiveFrame)
        //    {
        //        if (item.NeedSolve)
        //        {
        //            var imageItem = ImageParameters.ImageList.Find(x => x.ObjectId == item.ObjectId);

        //            imageItem.ObjectId = item.ObjectId;
        //            imageItem.Title = item.ObjectId.ToString();
        //            imageItem.Image = ImageFormatter.BitmapToBitmapImage(item.Frame.CreateBitmapWrap());
        //            imageItem.X = 0.0;
        //            imageItem.Y = 0.0;
        //            imageItem.A = 0.0;
        //        }
        //    }
        //}

        ///// <summary>
        ///// 影像存檔
        ///// </summary>
        //private void SaveImageToFile()
        //{
        //    //log.WriteLine($"ImageSave: ");

        //    string saveTime = DateTime.Now.ToString("yyyMMdd hhmmss");

        //    foreach (var item in ImageParameters.ReceiveFrame)
        //    {
        //        string productName = item.ObjectId.ToString();
        //        string dir = $@"{FileString.ImageDirectoty}\{productName}";
        //        if (!Directory.Exists(dir))
        //            Directory.CreateDirectory(dir);

        //        if (item.NeedSolve)
        //        {
        //            // TODO: 檔名要改成畫像ID
        //            string fullFileName = $@"{dir}\{productName} {saveTime}.bmp";
        //            item.Frame.SaveAsBitmap(fullFileName);
        //        }
        //    }
        //}

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
                LoadCamera(cameraId);
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
                LoadCamera(cameraId);
            }
        }
    }
}
