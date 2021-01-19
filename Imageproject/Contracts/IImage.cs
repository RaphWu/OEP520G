using Imageproject.Constants;
using Imageproject.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TIS.Imaging;

namespace Imageproject.Contracts
{
    public interface IImage
    {
        IFrameQueueBuffer FixCameraFrame { get; }
        IFrameQueueBuffer MoveCameraFrame { get; }

        /// <summary>
        /// 啟動固定相機
        /// </summary>
        void FixCameraOn();

        /// <summary>
        /// 關閉固定相機
        /// </summary>
        void FixCameraOff();

        /// <summary>
        /// 啟動移動相機
        /// </summary>
        void MoveCameraOn();

        /// <summary>
        /// 關閉移動相機
        /// </summary>
        void MoveCameraOff();

        /********************
         * 單張拍照
         ********************/
        /// <summary>
        /// 取得固定相機當前照片
        /// </summary>
        /// <returns>取得的照片。<br/>null: 相機無效或無照片。</returns>
        IFrameQueueBuffer TakePictureFromFixCamera();

        /// <summary>
        /// 取得移動相機當前照片
        /// </summary>
        /// <returns>取得的照片。<br/>null: 相機無效或無照片。</returns>
        IFrameQueueBuffer TakePictureFromMoveCamera();

        /********************
         * 相機內建設定程式
         ********************/
        /// <summary>
        /// 呼叫相機驅動器設定程式
        /// </summary>
        /// <param name="cameraId">那支相機</param>
        void DeviceSetting(CameraId cameraId);

        /// <summary>
        /// 呼叫相機參數設定程式
        /// </summary>
        /// <param name="cameraId">那支相機</param>
        void PropertSetting(CameraId cameraId);
    }
}
