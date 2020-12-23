/****************************
 * 相機參數檔
 ***************************/
using CSharpCore;
using CSharpCore.FileSystem;
using OEP520G.Core;
using System;
using System.Collections.Generic;

namespace OEP520G.Parameter
{
    public class Camera
    {
        // Singleton單例模式
        private static readonly Lazy<Camera> lazy = new Lazy<Camera>(() => new Camera());
        public static Camera Instance => lazy.Value;

        /// <summary>
        /// 相機名稱
        /// </summary>
        public readonly List<string> CAMERA_NAME_LIST = new List<string> { "移動相機", "固定相機" };

        // 相機編號
        public readonly int MOVE_CAMERA_ID = 0;
        public readonly int FIX_CAMERA_ID = 1;

        /// <summary>
        /// 移動相機參數
        /// </summary>
        /// <remarks>Origin值為移動相機拍照座標偏移值</remarks>
        public CameraData MoveCamera { get; private set; }

        /// <summary>
        /// 固定相機參數
        /// </summary>
        /// <remarks>Origin值為固定相機軸座標</remarks>
        public CameraData FixCamera { get; private set; }

        /// <summary>
        /// 固定相機軸座標
        /// </summary>
        //public PointXY FixCameraReferenceCoor = new PointXY();

        /********************
         * .ini檔作業
         *******************/
        private readonly string FileName = FileList.INI_CAMERA;
        private string sectionName;

        /// <summary>
        /// 建構函式
        /// </summary>
        public Camera()
        {
            MoveCamera = new CameraData() { CameraId = MOVE_CAMERA_ID };
            FixCamera = new CameraData() { CameraId = FIX_CAMERA_ID };
            ReadParameter();
        }

        /********************
         * 檔案作業
         *******************/
        /// <summary>
        /// 將參數寫入參數檔
        /// </summary>
        /// <remarks>
        /// iniFile.WriteIniFile(SectionName, "[屬性名稱]", [屬性值]));
        /// </remarks>
        public void WriteParameter()
        {
            // 參數檔檔案名稱
            IniFile iniFile = new IniFile(FileName);

            sectionName = "MoveCamera";
            iniFile.WriteIniFile(sectionName, "OriginX", MoveCamera.OriginX);
            iniFile.WriteIniFile(sectionName, "OriginY", MoveCamera.OriginY);
            iniFile.WriteIniFile(sectionName, "OriginZ", MoveCamera.OriginZ);

            sectionName = "FixCamera";
            iniFile.WriteIniFile(sectionName, "OriginX", FixCamera.OriginX);
            iniFile.WriteIniFile(sectionName, "OriginY", FixCamera.OriginY);
            iniFile.WriteIniFile(sectionName, "OriginZ", FixCamera.OriginZ);
            //iniFile.WriteIniFile(sectionName, "FixCameraReferenceCoor.X", FixCameraReferenceCoor.X);
            //iniFile.WriteIniFile(sectionName, "FixCameraReferenceCoor.Y", FixCameraReferenceCoor.Y);
        }

        /// <summary>
        /// 從參數檔讀取參數
        /// </summary>
        /// <remarks>
        /// [屬性名稱] = [Type].Parse(iniFile.ReadIniFile(SectionName, "[屬性名稱]", "[預設值]"));
        /// </remarks>
        public void ReadParameter()
        {
            // 參數檔檔案名稱
            IniFile iniFile = new IniFile(FileName);

            sectionName = "MoveCamera";
            MoveCamera.OriginX = double.Parse(iniFile.ReadIniFile(sectionName, "OriginX", "0"));
            MoveCamera.OriginY = double.Parse(iniFile.ReadIniFile(sectionName, "OriginY", "0"));
            MoveCamera.OriginZ = double.Parse(iniFile.ReadIniFile(sectionName, "OriginZ", "0"));

            sectionName = "FixCamera";
            FixCamera.OriginX = double.Parse(iniFile.ReadIniFile(sectionName, "OriginX", "0"));
            FixCamera.OriginY = double.Parse(iniFile.ReadIniFile(sectionName, "OriginY", "0"));
            FixCamera.OriginZ = double.Parse(iniFile.ReadIniFile(sectionName, "OriginZ", "0"));
            //FixCameraReferenceCoor.X = double.Parse(iniFile.ReadIniFile(sectionName, "FixCameraReferenceCoor.X", "0"));
            //FixCameraReferenceCoor.Y = double.Parse(iniFile.ReadIniFile(sectionName, "FixCameraReferenceCoor.Y", "0"));
        }
    }
}
