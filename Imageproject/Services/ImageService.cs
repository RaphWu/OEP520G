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
using System.Windows.Forms;

namespace Imageproject.Services
{
    public class ImageService : IImage
    {
        //private readonly Logger _log = Logger.Instance;

        //private EImageTargetId _objId;          // 拍照需求的物件ID
        //private bool _hwTriggerMode = false;    // 是否在硬體觸發狀態
        //private bool _liveMode = false;         // 是否在Live狀態

        //private FrameSnapSink _frameSnapSink = new FrameSnapSink(MediaSubtypes.Y800);
        //private FrameQueueSink _frameQueueSink;

        private int _countFrameList;

        private TimeSpan _timeOut = TimeSpan.FromMilliseconds(500);

        // TODO: _receiveImage須修改成偖存全部影像的物件，
        // 全部影像列表要用畫像ID，圖片存檔的檔名也要改成畫像ID
        // 拍照物件列表只需ID就好

        private readonly IEventAggregator _ea;
        private readonly ITcpipServer _tcpipServer;

        // ctor
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
                ImageParameters.FixSnapSink = new FrameSnapSink();
                LoadFixCamera();
            }

            if (ImageParameters.MoveCamera == null)
            {
                ImageParameters.MoveCamera = new ICImagingControl
                {
                    Name = CameraId.MoveCamera.ToString()
                };
                ImageParameters.MoveSnapSink = new FrameSnapSink();
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

            ImageParameters.FrameList = new IFrameQueueBuffer[ImageParameters.MAX_IMAGE_COUNT];

            if (!Directory.Exists(FileString.ImageDirectoty))
                Directory.CreateDirectory(FileString.ImageDirectoty);

            _ea.GetEvent<PublishSolve>().Subscribe(ReceiveSolve);
        }

        public IFrameQueueBuffer[] FrameList => ImageParameters.FrameList;

        /// <summary>
        /// 載入固定相機參數
        /// 設定Sink
        /// </summary>
        private void LoadFixCamera()
        {
            FixCameraOff();
            try
            {
                ImageParameters.FixCamera.LoadDeviceStateFromFile(CameraCfgFile.FixCameraXmlFile, true);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Load device failed. with {e}");
            }
        }

        /// <summary>
        /// 載入移動相機參數
        /// 設定Sink
        /// </summary>
        private void LoadMoveCamera()
        {
            MoveCameraOff();
            try
            {
                ImageParameters.MoveCamera.LoadDeviceStateFromFile(CameraCfgFile.MoveCameraXmlFile, true);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Load device failed. with {e}");
            }
        }

        /// <inheritdoc/>
        public void FixCameraOn() => FixCameraOn(null);

        /// <inheritdoc/>
        public void FixCameraOn(Func<IFrameQueueBuffer, FrameQueuedResult> frameQueuedFunc)
        {
            var fc = ImageParameters.FixCamera;
            var fcs = ImageParameters.FixCameraStatus;

            if (fc.DeviceValid)
            {
                // 硬體觸發優先
                if (fcs == CameraStatus.HardwareTrigger)
                    return;

                // 已經是SnapSink
                if (fcs == CameraStatus.SnapSink && frameQueuedFunc == null)
                    return;

                // 已經是QueueSink
                if (fcs == CameraStatus.QueueSink && frameQueuedFunc != null)
                    return;

                if (fc.LiveVideoRunning)
                    fc.LiveStop();

                if (frameQueuedFunc == null)
                {
                    // SnapSink
                    fc.Sink = ImageParameters.FixSnapSink;
                    fcs = CameraStatus.SnapSink;
                }
                else
                {
                    // QueueSink
                    fc.Sink = new FrameQueueSink(frameQueuedFunc,
                                                 MediaSubtypes.Y800,
                                                 ImageParameters.MAX_IMAGE_COUNT);
                    fcs = CameraStatus.QueueSink;
                }

                fc.LiveStart();
            }
            else
            {
                if (fcs != CameraStatus.Disable)
                    fcs = CameraStatus.Disable;
            }
        }

        /// <inheritdoc/>
        public void FixCameraOff()
        {
            if (ImageParameters.FixCamera.DeviceValid)
                if (ImageParameters.FixCamera.LiveVideoRunning)
                    ImageParameters.FixCamera.LiveStop();

            if (ImageParameters.FixCameraStatus != CameraStatus.Disable)
                ImageParameters.FixCameraStatus = CameraStatus.Disable;
        }

        /// <inheritdoc/>
        public void MoveCameraOn() => MoveCameraOn(null);

        /// <inheritdoc/>
        public void MoveCameraOn(Func<IFrameQueueBuffer, FrameQueuedResult> frameQueuedFunc)
        {
            var mc = ImageParameters.MoveCamera;
            var mcs = ImageParameters.MoveCameraStatus;

            if (mc.DeviceValid)
            {
                // 硬體觸發優先
                if (mcs == CameraStatus.HardwareTrigger)
                    return;

                // 已經是SnapSink
                if (mcs == CameraStatus.SnapSink && frameQueuedFunc == null)
                    return;

                // 已經是QueueSink
                if (mcs == CameraStatus.QueueSink && frameQueuedFunc != null)
                    return;

                if (mc.LiveVideoRunning)
                    mc.LiveStop();

                if (frameQueuedFunc == null)
                {
                    // SnapSink
                    mc.Sink = ImageParameters.MoveSnapSink;
                    mcs = CameraStatus.SnapSink;
                }
                else
                {
                    // QueueSink
                    mc.Sink = new FrameQueueSink(frameQueuedFunc,
                                                  MediaSubtypes.Y800,
                                                  ImageParameters.MAX_IMAGE_COUNT);
                    mcs = CameraStatus.QueueSink;
                }

                mc.LiveStart();
            }
            else
            {
                if (mcs != CameraStatus.Disable)
                    mcs = CameraStatus.Disable;
            }
        }

        /// <inheritdoc/>
        public void MoveCameraOff()
        {
            if (ImageParameters.MoveCamera.DeviceValid)
                if (ImageParameters.MoveCamera.LiveVideoRunning)
                    ImageParameters.MoveCamera.LiveStop();

            if (ImageParameters.MoveCameraStatus != CameraStatus.Disable)
                ImageParameters.MoveCameraStatus = CameraStatus.Disable;
        }

        ///// <summary>
        ///// 依CameraId
        ///// </summary>
        ///// <param name="cameraId"></param>
        //private void LoadCamera(CameraId cameraId)
        //{
        //    if (cameraId == CameraId.FixCamera)
        //        LoadFixCamera();
        //    else if (cameraId == CameraId.MoveCamera)
        //        LoadMoveCamera();
        //}

        /********************
         * 軟體指令拍照
         ********************/
        /// <inheritdoc/>
        public IFrameQueueBuffer TakePictureWithFixCamera()
        {
            if (ImageParameters.FixCameraStatus == CameraStatus.SnapSink)
                return ImageParameters.FixSnapSink.SnapSingle(_timeOut);
            else
                return null;
        }

        /// <inheritdoc/>
        public IFrameQueueBuffer TakePictureWithMoveCamera()
        {
            if (ImageParameters.MoveCameraStatus == CameraStatus.SnapSink)
                return ImageParameters.MoveSnapSink.SnapSingle(_timeOut);
            else
                return null;
        }

        /********************
         * 硬體觸發 (目前只有固定相機)
         ********************/
        /// <inheritdoc/>
        public void FixCameraHwTriggerOn()
        {
            var fc = ImageParameters.FixCamera;
            var fcs = ImageParameters.FixCameraStatus;

            if (!fc.DeviceValid)
                return;

            // 已啟動硬體觸發
            if (fcs == CameraStatus.HardwareTrigger)
                return;

            // 使用Sink時不可啟動硬體觸發，須先將Camera OFF
            if (fcs != CameraStatus.Disable)
                return;

            if (fc.LiveVideoRunning)
                fc.LiveStop();

            fc.Sink = new FrameQueueSink(FixCameraHwTriggerFunc,
                                         MediaSubtypes.Y800,
                                         ImageParameters.MAX_IMAGE_COUNT);
            _countFrameList = 0;
            fc.DeviceTrigger = true;
            fc.LiveStart();
            fcs = CameraStatus.HardwareTrigger;
        }

        /// <inheritdoc/>
        public void FixCameraHwTriggerOff()
        {
            var fc = ImageParameters.FixCamera;
            var fcs = ImageParameters.FixCameraStatus;

            if (!fc.DeviceValid)
                return;

            if (fcs != CameraStatus.HardwareTrigger)
                return;

            if (fc.LiveVideoRunning)
                fc.LiveStop();
            fc.DeviceTrigger = false;
            fcs = CameraStatus.Disable;

            // Sink切換及LiveStart()交由FixCameraOn()處理
            FixCameraOn();
        }

        /// <summary>
        /// 固定相機硬體觸發連拍處理
        /// </summary>
        /// <see cref="https://www.theimagingsource.com/support/documentation/ic-imaging-control-net/meth_descFrameQueueSink_FrameQueueSink.htm"/>
        private FrameQueuedResult FixCameraHwTriggerFunc(IFrameQueueBuffer frame)
        {
            if (_countFrameList >= ImageParameters.MAX_IMAGE_COUNT)
                return FrameQueuedResult.SkipReQueue;

            ImageParameters.FrameList[_countFrameList++] = frame;
            return FrameQueuedResult.ReQueue;
        }

        /********************
         * 相機內建設定程式
         ********************/
        /// <inheritdoc/>
        public void FixCameraDeviceSetting()
        {
            if (ImageParameters.FixCamera.ShowDeviceSettingsDialog() == DialogResult.OK)
                if (ImageParameters.FixCamera.DeviceValid)
                    ImageParameters.FixCamera.SaveDeviceStateToFile(CameraCfgFile.FixCameraXmlFile);
        }

        /// <inheritdoc/>
        public void FixCameraPropertSetting()
        {
            ImageParameters.FixCamera.ShowPropertyDialog();
            if (ImageParameters.FixCamera.DeviceValid)
                ImageParameters.FixCamera.SaveDeviceStateToFile(CameraCfgFile.FixCameraXmlFile);
        }

        /// <inheritdoc/>
        public void MoveCameraDeviceSetting()
        {
            if (ImageParameters.MoveCamera.ShowDeviceSettingsDialog() == DialogResult.OK)
                if (ImageParameters.MoveCamera.DeviceValid)
                    ImageParameters.MoveCamera.SaveDeviceStateToFile(CameraCfgFile.MoveCameraXmlFile);
        }

        /// <inheritdoc/>
        public void MoveCameraPropertSetting()
        {
            ImageParameters.MoveCamera.ShowPropertyDialog();
            if (ImageParameters.MoveCamera.DeviceValid)
                ImageParameters.MoveCamera.SaveDeviceStateToFile(CameraCfgFile.MoveCameraXmlFile);
        }

        /********************
         * 影像儲存
         ********************/
        /// <inheritdoc/>
        public void SaveImageToFile(string imgName, IFrameQueueBuffer frame)
        {
            string dir = $@"{FileString.ImageDirectoty}\{imgName}";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // TODO: 檔名要改成畫像ID
            string saveTime = DateTime.Now.ToString("yyyMMdd hhmmss");
            string fullFileName = $@"{dir}\{imgName} {saveTime}.bmp";
            frame.SaveAsBitmap(fullFileName);
        }

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

        /********************
         * 影像儲存
         ********************/
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

    }
}
