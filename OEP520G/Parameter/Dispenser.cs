/****************************
 * 點膠參數檔
 ***************************/
using CSharpCore;
using CSharpCore.FileSystem;
using OEP520G.Core;
using OEP520G.Functions;
using System;

namespace OEP520G.Parameter
{
    public class Dispenser
    {
        // Singleton單例模式
        private static readonly Lazy<Dispenser> lazy = new Lazy<Dispenser>(() => new Dispenser());
        public static Dispenser Instance => lazy.Value;

        /********************
         * 屬性
         *******************/
        // 膠針校正座標
        public PointXYZ Position { get; set; }
        public LongPointXYZ Pulse { get; set; }
        public IntPointXYZ Encoder { get; set; }

        // 膠針與移動相機距離
        public PointXY Distance { get; set; }

        /********************
         * .ini檔作業
         *******************/
        private readonly string FileName = FileList.INI_DISPENSER;
        private string sectionName;

        /// <summary>
        /// 建構函式
        /// </summary>
        public Dispenser()
        {
            ReadParameter();
        }

        /********************
         * 檔案作業
         *******************/
        /// <summary>
        /// 將參數寫入參數檔
        /// </summary>
        /// <example>
        /// iniFile.WriteIniFile(sectionName,"[屬性名稱]", [屬性值]));
        /// </example>
        public void WriteParameter()
        {
            // 參數檔檔案名稱
            IniFile iniFile = new IniFile(FileName);

            sectionName = "NeedleCorrect";
            iniFile.WriteIniFile(sectionName, "Position_X", Position.X);
            iniFile.WriteIniFile(sectionName, "Position_Y", Position.Y);
            iniFile.WriteIniFile(sectionName, "Position_Z", Position.Z);

            iniFile.WriteIniFile(sectionName, "Pulse_X", Pulse.X);
            iniFile.WriteIniFile(sectionName, "Pulse_Y", Pulse.Y);
            iniFile.WriteIniFile(sectionName, "Pulse_Z", Pulse.Z);

            iniFile.WriteIniFile(sectionName, "Encoder_X", Encoder.X);
            iniFile.WriteIniFile(sectionName, "Encoder_Y", Encoder.Y);
            iniFile.WriteIniFile(sectionName, "Encoder_Z", Encoder.Z);

            iniFile.WriteIniFile(sectionName, "Distance_X", Distance.X);
            iniFile.WriteIniFile(sectionName, "Distance_Y", Distance.Y);
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

            sectionName = "NeedleCorrect";
            Position = new PointXYZ
            {
                X = double.Parse(iniFile.ReadIniFile(sectionName, "Position_X", "0")),
                Y = double.Parse(iniFile.ReadIniFile(sectionName, "Position_Y", "0")),
                Z = double.Parse(iniFile.ReadIniFile(sectionName, "Position_Z", "0"))
            };
            Pulse = new LongPointXYZ
            {
                X = long.Parse(iniFile.ReadIniFile(sectionName, "Pulse_X", "0")),
                Y = long.Parse(iniFile.ReadIniFile(sectionName, "Pulse_Y", "0")),
                Z = long.Parse(iniFile.ReadIniFile(sectionName, "Pulse_Z", "0"))
            };
            Encoder = new IntPointXYZ
            {
                X = int.Parse(iniFile.ReadIniFile(sectionName, "Encoder_X", "0")),
                Y = int.Parse(iniFile.ReadIniFile(sectionName, "Encoder_Y", "0")),
                Z = int.Parse(iniFile.ReadIniFile(sectionName, "Encoder_Z", "0"))
            };
            Distance = new PointXY
            {
                X = double.Parse(iniFile.ReadIniFile(sectionName, "Distance_X", "0")),
                Y = double.Parse(iniFile.ReadIniFile(sectionName, "Distance_Y", "0"))
            };
        }
    }
}
