using Imageproject.Constants;
using Imageproject.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TIS.Imaging;

namespace Imageproject.Contracts
{
    public interface IImage
    {
        /// <summary>
        /// 連續拍照的存放區
        /// </summary>
        IFrameQueueBuffer[] FrameList { get; }

        /// <summary>
        /// 啟動固定相機，使用FrameSnapSink
        /// </summary>
        void FixCameraOn();

        /// <summary>
        /// 啟動固定相機
        /// </summary>
        /// <param name="frameQueuedFunc">
        /// <list type="number">
        /// <item>null: 使用FrameSnapSink</item>
        /// <item>FrameQueueSink的接收函式: 格式及用法請參閱官方網站：<br/>
        /// <see cref="https://www.theimagingsource.com/support/documentation/ic-imaging-control-net/meth_descFrameQueueSink_FrameQueueSink.htm"/></item>
        /// </list>
        /// </param>
        void FixCameraOn(Func<IFrameQueueBuffer, FrameQueuedResult> frameQueuedFunc);

        /// <summary>
        /// 關閉固定相機
        /// </summary>
        void FixCameraOff();

        /// <summary>
        /// 啟動移動相機，使用FrameSnapSink
        /// </summary>
        void MoveCameraOn();

        /// <summary>
        /// 啟動移動相機
        /// </summary>
        /// <param name="frameQueuedFunc">
        /// <list type="number">
        /// <item>null: 使用FrameSnapSink</item>
        /// <item>FrameQueueSink的接收函式: 格式及用法請參閱官方網站：<br/>
        /// <see cref="https://www.theimagingsource.com/support/documentation/ic-imaging-control-net/meth_descFrameQueueSink_FrameQueueSink.htm"/></item>
        /// </list>
        /// </param>
        void MoveCameraOn(Func<IFrameQueueBuffer, FrameQueuedResult> frameQueuedFunc);

        /// <summary>
        /// 關閉移動相機
        /// </summary>
        void MoveCameraOff();

        /********************
         * 單張拍照
         ********************/
        /// <summary>
        /// 從固定相機拍攝一張照片
        /// </summary>
        /// <returns>取得的照片。<br/>null: 相機無效或無照片。</returns>
        /// <remarks>僅在使用SnapSink時會拍照</remarks>
        IFrameQueueBuffer TakePictureWithFixCamera();

        /// <summary>
        /// 從移動相機拍攝一張照片
        /// </summary>
        /// <returns>取得的照片。<br/>null: 相機無效或無照片。</returns>
        /// <remarks>僅在使用SnapSink時會拍照</remarks>
        IFrameQueueBuffer TakePictureWithMoveCamera();

        /********************
         * 硬體觸發 (目前只有固定相機)
         ********************/
        /// <summary>
        /// 開啟固定相機的硬體觸發
        /// </summary>
        void FixCameraHwTriggerOn();

        /// <summary>
        /// 關閉固定相機的硬體觸發，且切換成SnapSink
        /// </summary>
        void FixCameraHwTriggerOff();

        /********************
         * 影像儲存
         ********************/
        /// <summary>
        /// 影像存檔
        /// </summary>
        void SaveImageToFile(string imgName, IFrameQueueBuffer frame);

        /********************
         * 相機內建設定程式
         ********************/
        /// <summary>
        /// 呼叫固定相機驅動器設定程式
        /// </summary>
        void FixCameraDeviceSetting();

        /// <summary>
        /// 呼叫固定相機參數設定程式
        /// </summary>
        void FixCameraPropertSetting();

        /// <summary>
        /// 呼叫移動相機驅動器設定程式
        /// </summary>
        void MoveCameraDeviceSetting();

        /// <summary>
        /// 呼叫移動相機參數設定程式
        /// </summary>
        void MoveCameraPropertSetting();
    }
}
