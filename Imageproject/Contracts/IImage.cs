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
        /// <summary>
        /// 相機初始化
        /// </summary>
        /// <param name="cameraId">要初始化那台相機</param>
        void InitCamera(CameraId cameraId);

        /// <summary>
        /// 拍攝一張照片
        /// </summary>
        /// <param name="objectId">被拍照物件ID</param>
        void TakePicture(EImageTargetId objectId);

        /// <summary>
        /// 相機開始可拍照狀態
        /// </summary>
        void TakePictureStart();

        /// <summary>
        /// 相機結束可拍照狀態，並開始數值演算
        /// </summary>
        void TakePictureFinish();

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
