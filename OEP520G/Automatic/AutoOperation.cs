using CSharpCore;
using OEP520G.Parameter;
using System;
using OEP520G.Functions;
using CSharpCore.FileSystem;

namespace OEP520G.Automatic
{
    /// <summary>
    /// 自動作業
    /// </summary>
    public class AutoOperation
    {
        // 生產數量設定
        public int Quantity { get; set; }

        // 生產計數
        public long EndProductThisRun { get; set; }
        public long DiscardThisRun { get; set; }
        public long PickUpMaterialThisRun { get; set; }
        public long EndProductTotal { get; set; }
        public long DiscardTotal { get; set; }
        public long PickUpMaterialTotal { get; set; }
        public DateTime RecountTime { get; set; }

        // 工作時間
        public long LastTimeCt { get; set; }
        public long ThisTimeCt { get; set; }
        public DateTime StandbyTime { get; set; }
        public DateTime TotalStandbyTime { get; set; }
        public DateTime RunningTime { get; set; }
        public DateTime TotalRunningTime { get; set; }

        // 選項
        public bool DiscardPartWhenPhotoFailed { get; set; }
        public bool NoStopWhenPhotoFailed { get; set; }
        public int TrayTimesWhenPhotoFailed { get; set; }
        public bool ResetWhenNoTray { get; set; }
        public bool MeasureHeightAfterAssembly { get; set; }
        public bool ProgressInfo { get; set; }

        // .ini存檔時檔案名稱及Section名稱
        private readonly string FileName = FileList.INI_AUTO_OPERATION;
        private string SectionName;

        /// <summary>
        /// 建構函式
        /// </summary>
        public AutoOperation()
        {
            ReadParameter();
        }

        /// <summary>
        /// 將參數寫入參數檔
        /// iniFile.WriteIniFile(SectionName, "[屬性名稱]", [屬性值]));
        /// </summary>
        public void WriteParameter()
        {
            IniFile iniFile = new IniFile(FileName);

            SectionName = "AutoOperation";
            iniFile.WriteIniFile(SectionName, "ProductionQuantity", Quantity);
            iniFile.WriteIniFile(SectionName, "EndProductThisRun", EndProductThisRun);
            iniFile.WriteIniFile(SectionName, "DiscardThisRun", DiscardThisRun);
            iniFile.WriteIniFile(SectionName, "PickUpMaterialThisRun", PickUpMaterialThisRun);
            iniFile.WriteIniFile(SectionName, "EndProductTotal", EndProductTotal);
            iniFile.WriteIniFile(SectionName, "DiscardTotal", DiscardTotal);
            iniFile.WriteIniFile(SectionName, "RecountTime", RecountTime);

            iniFile.WriteIniFile(SectionName, "DiscardPartWhenPhotoFailed", DiscardPartWhenPhotoFailed);
            iniFile.WriteIniFile(SectionName, "NoStopWhenPhotoFailed", NoStopWhenPhotoFailed);
            iniFile.WriteIniFile(SectionName, "TrayTimesWhenPhotoFailed", TrayTimesWhenPhotoFailed);
            iniFile.WriteIniFile(SectionName, "ResetWhenNoTray", ResetWhenNoTray);
            iniFile.WriteIniFile(SectionName, "MeasureHeightAfterAssembly", MeasureHeightAfterAssembly);
            iniFile.WriteIniFile(SectionName, "ProgressInfo", ProgressInfo);
        }

        /// <summary>
        /// 從參數檔讀取參數
        /// [屬性名稱] = [Type].Parse(iniFile.ReadIniFile(SectionName, "[屬性名稱]", "[預設值]"));
        /// </summary>
        public void ReadParameter()
        {
            IniFile iniFile = new IniFile(FileName);

            SectionName = "AutoOperation";
            Quantity = int.Parse(iniFile.ReadIniFile(SectionName, "ProductionQuantity", "0"));
            EndProductThisRun = int.Parse(iniFile.ReadIniFile(SectionName, "EndProductThisRun", "0"));
            DiscardThisRun = int.Parse(iniFile.ReadIniFile(SectionName, "DiscardThisRun", "0"));
            PickUpMaterialThisRun = int.Parse(iniFile.ReadIniFile(SectionName, "PickUpMaterialThisRun", "0"));
            EndProductTotal = int.Parse(iniFile.ReadIniFile(SectionName, "EndProductTotal", "0"));
            DiscardTotal = int.Parse(iniFile.ReadIniFile(SectionName, "DiscardTotal", "0"));
            PickUpMaterialTotal = int.Parse(iniFile.ReadIniFile(SectionName, "PickUpMaterialTotal", "0"));
            RecountTime = DateTime.Parse(iniFile.ReadIniFile(SectionName, "PickUpMaterialTotal", DateTime.Now.ToString()));

            DiscardPartWhenPhotoFailed = bool.Parse(iniFile.ReadIniFile(SectionName, "DiscardPartWhenPhotoFailed", "false"));
            NoStopWhenPhotoFailed = bool.Parse(iniFile.ReadIniFile(SectionName, "NoStopWhenPhotoFailed", "false"));
            NoStopWhenPhotoFailed = bool.Parse(iniFile.ReadIniFile(SectionName, "NoStopWhenPhotoFailed", "false"));
            TrayTimesWhenPhotoFailed = int.Parse(iniFile.ReadIniFile(SectionName, "TrayTimesWhenPhotoFailed", "3"));
            ResetWhenNoTray = bool.Parse(iniFile.ReadIniFile(SectionName, "ResetWhenNoTray", "false"));
            MeasureHeightAfterAssembly = bool.Parse(iniFile.ReadIniFile(SectionName, "MeasureHeightAfterAssembly", "false"));
            ProgressInfo = bool.Parse(iniFile.ReadIniFile(SectionName, "ProgressInfo", "false"));
        }
    }
}
