/****************************
 * 機台參數檔
 ***************************/
using CSharpCore;
using CSharpCore.FileSystem;
using OEP520G.Core;
using OEP520G.Functions;
using System;

namespace OEP520G.Parameter
{
    public class Machine
    {
        // Singleton單例模式
        private static readonly Lazy<Machine> lazy = new Lazy<Machine>(() => new Machine());
        public static Machine Instance => lazy.Value;

        // 機台資料
        public static string MachineId { get; set; }
        public static string MachineName { get; set; }

        // 基準點
        public DatumPoint DatumPoint1 { get; set; }
        public DatumPoint DatumPoint2 { get; set; }

        // 組裝側(後側)
        public PointXYZ AssembleDiscardBox { get; set; }
        public int AssembleDiscardExhaleTime { get; set; }
        public int AssembleDiscardExhaleNumbers { get; set; }
        public int AssembleDiscardGapTime { get; set; }
        public PointXYZ AssembleMeasureHeightPlatform { get; set; }
        public PointXYZ AssembleClay { get; set; }

        // 夾爪側(前側)
        public PointXYZ SemiFinishedDiscardBox { get; set; }
        public int SemiFinishedDiscardOpenCloseTime { get; set; }
        public int SemiFinishedDiscardOpenCloseNumbers { get; set; }
        public PointXYZ SemiFinishedMeasureHeightPlatform { get; set; }
        public PointXYZ FrontClay { get; set; }

        // .ini存檔時檔案名稱及Section名稱
        private readonly string machineFileName = FileList.INI_MACHINE;
        private readonly string systemFileName = FileList.INI_SYSTEM;
        private string sectionName;

        /// <summary>
        /// 建構函式
        /// </summary>
        public Machine()
        {
            DatumPoint1 = new DatumPoint();
            DatumPoint2 = new DatumPoint();
            AssembleDiscardBox = new PointXYZ();
            AssembleMeasureHeightPlatform = new PointXYZ();
            AssembleClay = new PointXYZ();
            SemiFinishedDiscardBox = new PointXYZ();
            SemiFinishedMeasureHeightPlatform = new PointXYZ();
            FrontClay = new PointXYZ();

            // 標題
            ShowMachineId();

            ReadParameter();
        }

        /********************
         * 檔案作業
         *******************/
        /// <summary>
        /// 將參數寫入參數檔
        /// iniFile.WriteIniFile(sectionName, "[屬性名稱]", [屬性值]));
        /// </summary>
        internal void WriteParameter()
        {
            IniFile iniFile = new IniFile(machineFileName);

            // 機台資訊
            sectionName = "Machine";
            if (MachineId != null)
            {
                iniFile.WriteIniFile(sectionName, "MachineId", MachineId);
                iniFile.WriteIniFile(sectionName, "MachineName", MachineName);
            }

            iniFile = new IniFile(systemFileName);

            // 基準點
            sectionName = "DatumPoint1";
            iniFile.WriteIniFile(sectionName, "X", DatumPoint1.Position.X);
            iniFile.WriteIniFile(sectionName, "Y", DatumPoint1.Position.Y);
            iniFile.WriteIniFile(sectionName, "Z", DatumPoint1.Position.Z);
            iniFile.WriteIniFile(sectionName, "Frequency", DatumPoint1.Frequency);
            iniFile.WriteIniFile(sectionName, "Tolerance", DatumPoint1.Tolerance);
            iniFile.WriteIniFile(sectionName, "DistanceToFixCamera.X", DatumPoint1.DistanceToFixCamera.X);
            iniFile.WriteIniFile(sectionName, "DistanceToFixCamera.Y", DatumPoint1.DistanceToFixCamera.Y);

            sectionName = "DatumPoint2";
            iniFile.WriteIniFile(sectionName, "X", DatumPoint2.Position.X);
            iniFile.WriteIniFile(sectionName, "Y", DatumPoint2.Position.Y);
            iniFile.WriteIniFile(sectionName, "Z", DatumPoint2.Position.Z);
            iniFile.WriteIniFile(sectionName, "Frequency", DatumPoint2.Frequency);
            iniFile.WriteIniFile(sectionName, "Tolerance", DatumPoint2.Tolerance);
            iniFile.WriteIniFile(sectionName, "DistanceToFixCamera.X", DatumPoint2.DistanceToFixCamera.X);
            iniFile.WriteIniFile(sectionName, "DistanceToFixCamera.Y", DatumPoint2.DistanceToFixCamera.Y);

            // 後方
            sectionName = "AssembleDiscardBox";
            iniFile.WriteIniFile(sectionName, "X", AssembleDiscardBox.X);
            iniFile.WriteIniFile(sectionName, "Y", AssembleDiscardBox.Y);
            iniFile.WriteIniFile(sectionName, "Z", AssembleDiscardBox.Z);
            iniFile.WriteIniFile(sectionName, "AssembleDiscardExhaleTime", AssembleDiscardExhaleTime);
            iniFile.WriteIniFile(sectionName, "AssembleDiscardExhaleNumbers", AssembleDiscardExhaleNumbers);
            iniFile.WriteIniFile(sectionName, "AssembleDiscardGapTime", AssembleDiscardGapTime);

            sectionName = "AssembleMeasureHeightPlatform";
            iniFile.WriteIniFile(sectionName, "X", AssembleMeasureHeightPlatform.X);
            iniFile.WriteIniFile(sectionName, "Y", AssembleMeasureHeightPlatform.Y);
            iniFile.WriteIniFile(sectionName, "Z", AssembleMeasureHeightPlatform.Z);

            sectionName = "AssembleClay";
            iniFile.WriteIniFile(sectionName, "X", AssembleClay.X);
            iniFile.WriteIniFile(sectionName, "Y", AssembleClay.Y);
            iniFile.WriteIniFile(sectionName, "Z", AssembleClay.Z);

            // 前方
            sectionName = "SemiFinishedDiscardBox";
            iniFile.WriteIniFile(sectionName, "X", SemiFinishedDiscardBox.X);
            iniFile.WriteIniFile(sectionName, "Y", SemiFinishedDiscardBox.Y);
            iniFile.WriteIniFile(sectionName, "Z", SemiFinishedDiscardBox.Z);
            iniFile.WriteIniFile(sectionName, "SemiFinishedDiscardOpenCloseTime", SemiFinishedDiscardOpenCloseTime);
            iniFile.WriteIniFile(sectionName, "SemiFinishedDiscardOpenCloseNumbers", SemiFinishedDiscardOpenCloseNumbers);

            sectionName = "SemiFinishedMeasureHeightPlatform";
            iniFile.WriteIniFile(sectionName, "X", SemiFinishedMeasureHeightPlatform.X);
            iniFile.WriteIniFile(sectionName, "Y", SemiFinishedMeasureHeightPlatform.Y);
            iniFile.WriteIniFile(sectionName, "Z", SemiFinishedMeasureHeightPlatform.Z);

            sectionName = "SemiFinishedClay";
            iniFile.WriteIniFile(sectionName, "X", FrontClay.X);
            iniFile.WriteIniFile(sectionName, "Y", FrontClay.Y);
            iniFile.WriteIniFile(sectionName, "Z", FrontClay.Z);
        }

        /// <summary>
        /// 從參數檔讀取參數
        /// [屬性名稱] = [Type].Parse(iniFile.ReadIniFile(sectionName, "[屬性名稱]", "[預設值]"));
        /// </summary>
        internal void ReadParameter()
        {
            IniFile iniFile = new IniFile(machineFileName);

            // 機台資訊
            sectionName = "Machine";
            MachineId = iniFile.ReadIniFile(sectionName, "MachineId", "LINE-x");
            MachineName = iniFile.ReadIniFile(sectionName, "MachineName", "520Gxxx");

            iniFile = new IniFile(systemFileName);

            // 基準點
            sectionName = "DatumPoint1";
            DatumPoint1.Position.X = double.Parse(iniFile.ReadIniFile(sectionName, "X", "0.0"));
            DatumPoint1.Position.Y = double.Parse(iniFile.ReadIniFile(sectionName, "Y", "0.0"));
            DatumPoint1.Position.Z = double.Parse(iniFile.ReadIniFile(sectionName, "Z", "0.0"));
            DatumPoint1.Frequency = int.Parse(iniFile.ReadIniFile(sectionName, "Frequency", "0"));
            DatumPoint1.Tolerance = double.Parse(iniFile.ReadIniFile(sectionName, "Tolerance", "0.0"));
            DatumPoint1.DistanceToFixCamera.X = double.Parse(iniFile.ReadIniFile(sectionName, "DistanceToFixCamera.X", "0.0"));
            DatumPoint1.DistanceToFixCamera.Y = double.Parse(iniFile.ReadIniFile(sectionName, "DistanceToFixCamera.Y", "0.0"));

            sectionName = "DatumPoint2";
            DatumPoint2.Position.X = double.Parse(iniFile.ReadIniFile(sectionName, "X", "0.0"));
            DatumPoint2.Position.Y = double.Parse(iniFile.ReadIniFile(sectionName, "Y", "0.0"));
            DatumPoint2.Position.Z = double.Parse(iniFile.ReadIniFile(sectionName, "Z", "0.0"));
            DatumPoint2.Frequency = int.Parse(iniFile.ReadIniFile(sectionName, "Frequency", "0"));
            DatumPoint2.Tolerance = double.Parse(iniFile.ReadIniFile(sectionName, "Tolerance", "0.0"));
            DatumPoint2.DistanceToFixCamera.X = double.Parse(iniFile.ReadIniFile(sectionName, "DistanceToFixCamera.X", "0.0"));
            DatumPoint2.DistanceToFixCamera.Y = double.Parse(iniFile.ReadIniFile(sectionName, "DistanceToFixCamera.Y", "0.0"));

            // 組裝側(後側)
            sectionName = "AssembleDiscardBox";
            AssembleDiscardBox.X = double.Parse(iniFile.ReadIniFile(sectionName, "X", "0.0"));
            AssembleDiscardBox.Y = double.Parse(iniFile.ReadIniFile(sectionName, "Y", "0.0"));
            AssembleDiscardBox.Z = double.Parse(iniFile.ReadIniFile(sectionName, "Z", "0.0"));
            AssembleDiscardExhaleTime = int.Parse(iniFile.ReadIniFile(sectionName, "AssembleDiscardExhaleTime", "500"));
            AssembleDiscardExhaleNumbers = int.Parse(iniFile.ReadIniFile(sectionName, "AssembleDiscardExhaleNumbers", "1"));
            AssembleDiscardGapTime = int.Parse(iniFile.ReadIniFile(sectionName, "AssembleDiscardGapTime", "500"));

            sectionName = "AssembleMeasureHeightPlatform";
            AssembleMeasureHeightPlatform.X = double.Parse(iniFile.ReadIniFile(sectionName, "X", "0.0"));
            AssembleMeasureHeightPlatform.Y = double.Parse(iniFile.ReadIniFile(sectionName, "Y", "0.0"));
            AssembleMeasureHeightPlatform.Z = double.Parse(iniFile.ReadIniFile(sectionName, "Z", "0.0"));

            sectionName = "AssembleClay";
            AssembleClay.X = double.Parse(iniFile.ReadIniFile(sectionName, "X", "0.0"));
            AssembleClay.Y = double.Parse(iniFile.ReadIniFile(sectionName, "Y", "0.0"));
            AssembleClay.Z = double.Parse(iniFile.ReadIniFile(sectionName, "Z", "0.0"));

            // 夾爪側(前側)
            sectionName = "SemiFinishedDiscardBox";
            SemiFinishedDiscardBox.X = double.Parse(iniFile.ReadIniFile(sectionName, "X", "0.0"));
            SemiFinishedDiscardBox.Y = double.Parse(iniFile.ReadIniFile(sectionName, "Y", "0.0"));
            SemiFinishedDiscardBox.Z = double.Parse(iniFile.ReadIniFile(sectionName, "Z", "0.0"));
            SemiFinishedDiscardOpenCloseTime = int.Parse(iniFile.ReadIniFile(sectionName, "SemiFinishedDiscardOpenCloseTime", "500"));
            SemiFinishedDiscardOpenCloseNumbers = int.Parse(iniFile.ReadIniFile(sectionName, "SemiFinishedDiscardOpenCloseNumbers", "2"));

            sectionName = "SemiFinishedMeasureHeightPlatform";
            SemiFinishedMeasureHeightPlatform.X = double.Parse(iniFile.ReadIniFile(sectionName, "X", "0.0"));
            SemiFinishedMeasureHeightPlatform.Y = double.Parse(iniFile.ReadIniFile(sectionName, "Y", "0.0"));
            SemiFinishedMeasureHeightPlatform.Z = double.Parse(iniFile.ReadIniFile(sectionName, "Z", "0.0"));

            sectionName = "SemiFinishedClay";
            FrontClay.X = double.Parse(iniFile.ReadIniFile(sectionName, "X", "0.0"));
            FrontClay.Y = double.Parse(iniFile.ReadIniFile(sectionName, "Y", "0.0"));
            FrontClay.Z = double.Parse(iniFile.ReadIniFile(sectionName, "Z", "0.0"));
        }

        /// <summary>
        /// 在標題列設定顯示機台ID
        /// </summary>
        public void ShowMachineId()
        {
            Common.EA.GetEvent<WindowTitleSetter>().Publish(new WindowTitleData()
            {
                Key = "11 MachineId",
                Title = "機台ID [" + (MachineId ?? "") + "]"
            });
        }
    }
}
