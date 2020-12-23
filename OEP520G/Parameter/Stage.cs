/****************************
 * 台車參數檔
 ***************************/
using CSharpCore;
using CSharpCore.FileSystem;
using EPCIO;
using OEP520G.Core;
using OEP520G.Functions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OEP520G.Parameter
{
    public class StageRotateShiftData
    {
        public float Angle { get; set; }
        public double ShiftX { get; set; }
        public double ShiftY { get; set; }
    }

    public class Stage
    {
        // Singleton單例模式
        private static readonly Lazy<Stage> lazy = new Lazy<Stage>(() => new Stage());
        public static Stage Instance => lazy.Value;

        private readonly Epcio epcio = Epcio.Instance;

        // 台車ID,名稱
        public int Id { get; set; }
        public string Name { get; set; }

        // 旋轉中心
        public PointXY RotateCenter { get; set; }

        // 旋轉位移
        public float StartAngle { get; set; }
        public float EndAngle { get; set; }
        public float IntervalAngle { get; set; }
        public List<StageRotateShiftData> StageRotateShift { get; set; }

        // 測高
        public double HeightZ { get; set; }

        /********************
         * .ini檔作業
         *******************/
        private readonly string FileName = FileList.INI_STAGE;
        private string sectionName;

        /// <summary>
        /// 建構函式
        /// </summary>
        public Stage()
        {
            RotateCenter = new PointXY();
            StageRotateShift = new List<StageRotateShiftData>();

            ReadParameter();
        }

        /********************
         * 檔案作業
         *******************/
        /// <summary>
        /// 將參數寫入參數檔
        /// </summary>
        /// <example>
        /// iniFile.WriteIniFile(sectionName,"[屬性名稱]", [屬性值]);
        /// </example>
        public void WriteParameter()
        {
            // 參數檔檔案名稱
            IniFile iniFile = new IniFile(FileName);

            sectionName = "Stage";
            // 台車ID,名稱
            iniFile.WriteIniFile(sectionName, "Id", Id);
            iniFile.WriteIniFile(sectionName, "Name", Name);

            // 旋轉中心
            sectionName = "StageRotateCenter";
            iniFile.WriteIniFile(sectionName, "RotateCenterX", RotateCenter.X);
            iniFile.WriteIniFile(sectionName, "RotateCenterY", RotateCenter.Y);

            // 旋轉位移
            sectionName = "StageRotateShift";
            iniFile.WriteIniFile(sectionName, "StartAngle", StartAngle);
            iniFile.WriteIniFile(sectionName, "EndAngle", EndAngle);
            iniFile.WriteIniFile(sectionName, "IntervalAngle", IntervalAngle);

            for (int index = 0; index < StageRotateShift.Count; index++)
            {
                StageRotateShiftData stsd = StageRotateShift[index];

                sectionName = $"Angle_{index}";
                iniFile.WriteIniFile(sectionName, "Angle", stsd.Angle);
                iniFile.WriteIniFile(sectionName, "RotateShiftX", stsd.ShiftX);
                iniFile.WriteIniFile(sectionName, "RotateShiftY", stsd.ShiftY);
            }

            // 測高
            iniFile.WriteIniFile(sectionName, "HeightZ", HeightZ);
        }

        /// <summary>
        /// 從參數檔讀取參數
        /// </summary>
        /// <example>
        /// [屬性名稱] = [Type].Parse(iniFile.ReadIniFile(sectionName,"[屬性名稱]","[預設值]"));
        /// </example>
        public void ReadParameter()
        {
            // 參數檔檔案名稱
            IniFile iniFile = new IniFile(FileName);

            sectionName = "Stage";
            // 台車ID,名稱
            Id = int.Parse(iniFile.ReadIniFile(sectionName, "Id", "1"));
            Name = iniFile.ReadIniFile(sectionName, "Name", "台車1");

            // 旋轉中心
            sectionName = "StageRotateCenter";
            RotateCenter.X = double.Parse(iniFile.ReadIniFile(sectionName, "RotateCenterX", "0"));
            RotateCenter.Y = double.Parse(iniFile.ReadIniFile(sectionName, "RotateCenterY", "0"));

            // 旋轉位移
            sectionName = "StageRotateShift";
            StartAngle = int.Parse(iniFile.ReadIniFile(sectionName, "StartAngle", "0"));
            EndAngle = int.Parse(iniFile.ReadIniFile(sectionName, "EndAngle", "360"));
            IntervalAngle = int.Parse(iniFile.ReadIniFile(sectionName, "IntervalAngle", "5"));

            StageRotateShift.Clear();
            int index = 0;
            for (float angle = StartAngle; angle < EndAngle; angle += IntervalAngle)
            {
                sectionName = $"Angle_{++index}";
                StageRotateShift.Add(new StageRotateShiftData()
                {
                    Angle = angle,
                    ShiftX = double.Parse(iniFile.ReadIniFile(sectionName, "RotateShiftX", "0")),
                    ShiftY = double.Parse(iniFile.ReadIniFile(sectionName, "RotateShiftY", "0"))
                });
            }

            // 測高
            HeightZ = double.Parse(iniFile.ReadIniFile(sectionName, "HeightZ", "0"));
        }

        /********************
         * 台車夾片打開
         *******************/
        /// <summary>
        /// 台車夾片打開
        /// </summary>
        public void StageClampOpen()
            => epcio.RioOutput(epcio.StageClampOpenClose, true);

        /// <summary>
        /// 檢查台車夾片是否在打開狀態
        /// </summary>
        /// <returns>檢查結果</returns>
        public bool IsStageClampOpen()
            => epcio.StageClampOpenLs.Value;

        /// <summary>
        /// 等待台車夾片到打開狀態
        /// </summary>
        public async Task WaitingForClampOpen()
        {
            await Task.Run(() =>
            {
                while (!IsStageClampOpen()) { }
            });
        }

        /********************
         * 台車夾片閉合
         *******************/
        /// <summary>
        /// 台車夾片閉合
        /// </summary>
        public void StageClampClose()
            => epcio.RioOutput(epcio.StageClampOpenClose, false);

        /// <summary>
        /// 檢查台車夾片是否在閉合狀態
        /// </summary>
        /// <returns>檢查結果</returns>
        public bool IsStageClampClose()
            => epcio.StageClampCloseLs.Value;

        /// <summary>
        /// 等待台車夾片到閉合狀態
        /// </summary>
        public async Task WaitingForClampClose()
        {
            await Task.Run(() =>
            {
                while (!IsStageClampClose()) { }
            });
        }

        /********************
         * 台車真空關閉
         *******************/
        /// <summary>
        /// 台車真空關閉
        /// </summary>
        public void StageVaccumOff()
            => epcio.RioOutput(epcio.StageVaccumOnOff, false);

        /// <summary>
        /// 檢查台車真空是否在關閉狀態
        /// </summary>
        /// <returns>檢查結果</returns>
        public bool IsStageVaccumOff()
            => !epcio.StageVacuumPressure.Value;

        /// <summary>
        /// 等待台車真空關閉
        /// </summary>
        public async Task WaitingForVaccumOff()
        {
            await Task.Run(() =>
            {
                while (!IsStageVaccumOff()) { }
            });
        }

        /********************
         * 台車真空開啟
         *******************/
        /// <summary>
        /// 台車真空開啟
        /// </summary>
        public void StageVaccumOn()
            => epcio.RioOutput(epcio.StageVaccumOnOff, true);

        /// <summary>
        /// 檢查台車真空是否在開啟狀態
        /// </summary>
        /// <returns>檢查結果</returns>
        public bool IsStageVaccumOn()
            => epcio.StageVacuumPressure.Value;

        /// <summary>
        /// 等待台車真空開啟
        /// </summary>
        public async Task WaitingForVaccumOn()
        {
            await Task.Run(() =>
            {
                while (!IsStageVaccumOn()) { }
            });
        }
    }
}
