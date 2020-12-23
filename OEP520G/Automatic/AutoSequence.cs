using CSharpCore;
using CSharpCore.Database;
using CSharpCore.FileSystem;
using Dapper;
using OEP520G.Core;
using OEP520G.Parameter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace OEP520G.Automatic
{
    /// <summary>
    /// 生產步驟動作規劃
    /// </summary>
    public class SequenceData
    {
        public int Sequence { get; set; }           // 順序
        public string SelectedHead { get; set; }     // 動作
        public string SelectedAction { get; set; } // 動作頭
        public string SelectedTarget { get; set; } // 台車/托盤
        public string SelectedPart { get; set; }       // 零件編號
        public string SelectedTray { get; set; }       // 托盤編號
        public float Angle { get; set; }            // 組裝角度

        public bool Effective { get; set; }                     // S:有效的
        public bool ImageProcessing { get; set; }               // 1:畫像處理(飛行，步驟後執行)
        public bool LaunchStageAfterProcedure { get; set; }     // 2:推出小車(步驟後執行)
        public bool SkipAlignmentBeforeCarry { get; set; }      // 3:略過搭載(Carry)前對位處理
        public bool OpenClampWhenAssembly { get; set; }         // 4:置件(Assembly)時張開夾爪
        public bool StageReturn0AfterCarry { get; set; }        // 5:搭載完成後，將小車轉回0度
        public bool SkipPositionCheckWhenAssembly { get; set; } // 6:置件時不檢查下壓到位
        public bool OpenClampWhenSingleProcedure { get; set; }  // 7:於台車取置件時，不使用夾片
        public bool MeasureHighAfterAssembly { get; set; }      // 8:組裝完成後測高
        public bool GetCenterAfterStageRotate { get; set; }     // 9:絕對組裝前畫像對位處理

        ///// <summary>
        ///// 比對兩個SequenceData是否一模一樣
        ///// </summary>
        ///// <returns>比對結果</returns>
        //public override bool Equals(object obj)
        //{
        //    return obj is SequenceData data &&
        //        Sequence == data.Sequence &&
        //        SelectedHead == data.SelectedHead &&
        //        SelectedAction == data.SelectedAction &&
        //        SelectedTarget == data.SelectedTarget &&
        //        SelectedPart == data.SelectedPart &&
        //        SelectedTray == data.SelectedTray &&
        //        Effective == data.Effective &&
        //        ImageProcessing == data.ImageProcessing &&
        //        LaunchStageAfterProcedure == data.LaunchStageAfterProcedure &&
        //        SkipAlignmentBeforeCarry == data.SkipAlignmentBeforeCarry &&
        //        OpenClampWhenAssembly == data.OpenClampWhenAssembly &&
        //        StageReturn0AfterCarry == data.StageReturn0AfterCarry &&
        //        SkipPositionCheckWhenAssembly == data.SkipPositionCheckWhenAssembly &&
        //        OpenClampWhenSingleProcedure == data.OpenClampWhenSingleProcedure &&
        //        MeasureHighAfterAssembly == data.MeasureHighAfterAssembly &&
        //        GetCenterAfterStageRotate == data.GetCenterAfterStageRotate;
        //}

        //public override string ToString() => base.ToString();

        //public override int GetHashCode() => base.GetHashCode();
    }

    ///// <summary>
    ///// 生產動作規劃(供DataGrid記錄是否被變更)
    ///// </summary>
    //public class SequenceData : SequenceData
    //{
    //    public int OriginSequence { get; set; } // 原動作順序編號
    //    public char Edited { get; set; }        // 此筆資料是否有被修改(僅顯示用)
    //}

    /// <summary>
    /// 生產步驟資料，設定動作步驟的順序及內容
    /// </summary>
    public class AutoSequence
    {
        // Singleton單例模式
        private static readonly Lazy<AutoSequence> lazy = new Lazy<AutoSequence>(() => new AutoSequence());
        public static AutoSequence Instance => lazy.Value;

        private readonly SQLiteService sqlite = new SQLiteService();

        //
        public int BarrelTrayNo { get; set; }
        public int ProductTrayNo { get; set; }

        // 自動作業步驟數
        public int TotalSequence { get; private set; }

        // 自動作業步驟順序編號
        public List<SequenceData> SequenceDataList = new List<SequenceData>();

        // 動作順序編號列表(在未使用DB前，記錄所有編號用)
        private List<int> _sequenceList = new List<int>();

        /// <summary>
        /// 建構函式
        /// </summary>
        private AutoSequence()
        {
            ReadParameter();
        }

        /********************
         * 檔案作業
         *******************/
        // .ini存檔時檔案名稱及Section名稱
        private readonly string _fileName = FileList.INI_AUTO_SEQUENCE;
        private string _sectionName;

        /// <summary>
        /// 先更新AutoSequenceList，再寫入參數檔
        /// </summary>
        /// <param name="sde">更新資料來源</param>
        /// <remarks>尚未實現DB，先土法鍊鋼</remarks>
        public void WriteParameter(ObservableCollection<SequenceData> sde)
        {
            SequenceDataList.Clear();
            foreach (var data in sde)
                SequenceDataList.Add(data);

            WriteParameter();

            //SequenceDataList.Clear();
            //_sequenceList.Clear();

            //foreach (var sd in sde)
            //{
            //    SequenceDataList.Add(new SequenceData()
            //    {
            //        Sequence = sd.Sequence,
            //        HeadSelecter = sd.HeadSelecter,
            //        ActionSelecter = sd.ActionSelecter,
            //        TargetSelecter = sd.TargetSelecter,
            //        PartIdSelecter = sd.PartIdSelecter,
            //        TrayIdSelecter = sd.TrayIdSelecter,
            //        Effective = sd.Effective,
            //        ImageProcessing = sd.ImageProcessing,
            //        LaunchStageAfterProcedure = sd.LaunchStageAfterProcedure,
            //        SkipAlignmentBeforeCarry = sd.SkipAlignmentBeforeCarry,
            //        OpenClampWhenAssembly = sd.OpenClampWhenAssembly,
            //        StageReturn0AfterCarry = sd.StageReturn0AfterCarry,
            //        SkipPositionCheckWhenAssembly = sd.SkipPositionCheckWhenAssembly,
            //        OpenClampWhenSingleProcedure = sd.OpenClampWhenSingleProcedure,
            //        MeasureHighAfterAssembly = sd.MeasureHighAfterAssembly,
            //        GetCenterAfterStageRotate = sd.GetCenterAfterStageRotate
            //    });

            //    _sequenceList.Add(sd.Sequence);
            //}

            //WriteParameter();
        }

        /// <summary>
        /// 將AutoSequenceList寫入參數檔
        /// iniFile.WriteIniFile(SectionName, "[屬性名稱]", [屬性值]));
        /// </summary>
        public void WriteParameter()
        {
            //// 連線資料庫
            //var conn = sqlite.OpenConnection(SQLite.DB_FILE_NAME_PRODUCT);

            //if (conn != null)
            //{
            //    //// 確認資料表
            //    //sqlite.CreateTable(conn, SQLite.TABLE_CREATE_SQL_AUTO_SEQUENCE);

            //    // TODO: 只更新有變動資料
            //    // 刪除資料表資料
            //    conn.Query($"DELETE FROM {SQLite.TABLE_NAME_AUTO_SEQUENCE}");

            //    // 更新資料表
            //    foreach (var val in SequenceDataList)
            //    {
            //        var sqlCommand = $"INSERT INTO {SQLite.TABLE_NAME_AUTO_SEQUENCE} VALUES("
            //                       + $"{val.Sequence},"
            //                       + $"{val.SelectedHead},"
            //                       + $"{val.SelectedAction},"
            //                       + $"{val.SelectedTarget},"
            //                       + $"{val.SelectedPart},"
            //                       + $"{val.SelectedTray},"
            //                       + $"{(float)val.Angle},"
            //                       + $"{val.Effective},"
            //                       + $"{val.ImageProcessing},"
            //                       + $"{val.LaunchStageAfterProcedure},"
            //                       + $"{val.SkipAlignmentBeforeCarry},"
            //                       + $"{val.OpenClampWhenAssembly},"
            //                       + $"{val.StageReturn0AfterCarry},"
            //                       + $"{val.SkipPositionCheckWhenAssembly},"
            //                       + $"{val.OpenClampWhenSingleProcedure},"
            //                       + $"{val.MeasureHighAfterAssembly},"
            //                       + $"{val.GetCenterAfterStageRotate}"
            //                       + ");";

            //        conn.Query(sqlCommand);
            //    }

            //    // 關閉資料庫
            //    sqlite.Close(conn);

            //    SortData();
            //}
            //else
            //{
            //    SequenceDataList.Clear();
            //}

            //TotalSequence = SequenceDataList.Count;


            //// 若步驟編號有變更，則.INI檔無法刪除造成錯誤步驟
            //// 改用DB之前以刪除.INI再重建處理
            //File.Delete(_fileName);

            //IniFile iniFile = new IniFile(_fileName);

            //SortData();

            //_sectionName = "Information";
            //iniFile.WriteIniFile(_sectionName, "SequenceList", string.Join(',', _sequenceList));

            //foreach (SequenceData sd in SequenceDataList)
            //{
            //    _sectionName = sd.Sequence.ToString();

            //    iniFile.WriteIniFile(_sectionName, "Sequence", sd.Sequence);
            //    iniFile.WriteIniFile(_sectionName, "HeadSelecter", sd.HeadSelecter);
            //    iniFile.WriteIniFile(_sectionName, "ActionSelecter", sd.ActionSelecter);
            //    iniFile.WriteIniFile(_sectionName, "TargetSelecter", sd.TargetSelecter);
            //    iniFile.WriteIniFile(_sectionName, "PartIdSelecter", sd.PartIdSelecter);
            //    iniFile.WriteIniFile(_sectionName, "TrayIdSelecter", sd.TrayIdSelecter);

            //    iniFile.WriteIniFile(_sectionName, "Effective", sd.Effective);
            //    iniFile.WriteIniFile(_sectionName, "ImageProcessing", sd.ImageProcessing);
            //    iniFile.WriteIniFile(_sectionName, "LaunchStageAfterProcedure", sd.LaunchStageAfterProcedure);
            //    iniFile.WriteIniFile(_sectionName, "SkipAlignmentBeforeCarry", sd.SkipAlignmentBeforeCarry);
            //    iniFile.WriteIniFile(_sectionName, "OpenClampWhenAssembly", sd.OpenClampWhenAssembly);
            //    iniFile.WriteIniFile(_sectionName, "StageReturn0AfterCarry", sd.StageReturn0AfterCarry);
            //    iniFile.WriteIniFile(_sectionName, "SkipPositionCheckWhenAssembly", sd.SkipPositionCheckWhenAssembly);
            //    iniFile.WriteIniFile(_sectionName, "OpenClampWhenSingleProcedure", sd.OpenClampWhenSingleProcedure);
            //    iniFile.WriteIniFile(_sectionName, "MeasureHighAfterAssembly", sd.MeasureHighAfterAssembly);
            //    iniFile.WriteIniFile(_sectionName, "GetCenterAfterStageRotate", sd.GetCenterAfterStageRotate);
            //}
        }

        /// <summary>
        /// 從參數檔讀取參數
        /// [屬性名稱] = [Type].Parse(iniFile.ReadIniFile(SectionName, "[屬性名稱]", "[預設值]"));
        /// </summary>
        public void ReadParameter()
        {
            // 連線資料庫
            using var conn = sqlite.OpenConnection(SQLite.DB_FILE_NAME_PRODUCT);
            if (conn != null)
            {
                using var trans = conn.BeginTransaction();

                // 確認資料表
                sqlite.CreateTable(conn, SQLite.TABLE_CREATE_SQL_AUTO_SEQUENCE);

                // 讀取資料表
                SequenceDataList = conn.Query<SequenceData>(
                    $"SELECT * FROM {SQLite.TABLE_NAME_AUTO_SEQUENCE}"
                    ).ToList();

                trans.Commit();

                // 關閉資料庫
                sqlite.Close(conn);

                SortData();
            }
            else
            {
                SequenceDataList.Clear();
            }

            TotalSequence = SequenceDataList.Count;

            //string bufString;

            //IniFile iniFile = new IniFile(_fileName);

            //_sectionName = "Information";
            //bufString = iniFile.ReadIniFile(_sectionName, "SequenceList", "");
            //_sequenceList = (bufString == "") ? new List<int>() : bufString.ToIntList();

            //SequenceDataList.Clear();
            //for (int idx = 0; idx < _sequenceList.Count; idx++)
            //{
            //    int sequence = _sequenceList[idx];
            //    _sectionName = sequence.ToString();

            //    SequenceData sd = new SequenceData()
            //    {
            //        Sequence = sequence,
            //        HeadSelecter = Enum.Parse<EHead>(iniFile.ReadIniFile(_sectionName, "HeadSelecter", "")),
            //        ActionSelecter = Enum.Parse<EAction>(iniFile.ReadIniFile(_sectionName, "ActionSelecter", "")),
            //        TargetSelecter = Enum.Parse<ETarget>(iniFile.ReadIniFile(_sectionName, "TargetSelecter", "")),
            //        PartIdSelecter = iniFile.ReadIniFile(_sectionName, "PartIdSelecter", ""),
            //        TrayIdSelecter = iniFile.ReadIniFile(_sectionName, "TrayIdSelecter", ""),

            //        Effective = bool.Parse(iniFile.ReadIniFile(_sectionName, "Effective", "false")),
            //        ImageProcessing = bool.Parse(iniFile.ReadIniFile(_sectionName, "ImageProcessing", "false")),
            //        LaunchStageAfterProcedure = bool.Parse(iniFile.ReadIniFile(_sectionName, "LaunchStageAfterProcedure", "false")),
            //        SkipAlignmentBeforeCarry = bool.Parse(iniFile.ReadIniFile(_sectionName, "SkipAlignmentBeforeCarry", "false")),
            //        OpenClampWhenAssembly = bool.Parse(iniFile.ReadIniFile(_sectionName, "OpenClampWhenAssembly", "false")),
            //        StageReturn0AfterCarry = bool.Parse(iniFile.ReadIniFile(_sectionName, "StageReturn0AfterAssembly", "false")),
            //        SkipPositionCheckWhenAssembly = bool.Parse(iniFile.ReadIniFile(_sectionName, "SkipPositionCheckWhenAssembly", "false")),
            //        OpenClampWhenSingleProcedure = bool.Parse(iniFile.ReadIniFile(_sectionName, "OpenClampWhenSingleProcedure", "false")),
            //        MeasureHighAfterAssembly = bool.Parse(iniFile.ReadIniFile(_sectionName, "MeasureHighAfterAssembly", "false")),
            //        GetCenterAfterStageRotate = bool.Parse(iniFile.ReadIniFile(_sectionName, "GetCenterAfterStageRotate", "false"))
            //    };
            //    SequenceDataList.Add(sd);
            //}

            //SortData();
            //TotalSequence = SequenceDataList.Count;
        }

        /********************
         * Utility
         *******************/
        /// <summary>
        /// 排序
        /// </summary>
        private void SortData()
        {
            SequenceDataList.Sort((i, j) => { return i.Sequence - j.Sequence; });
            _sequenceList.Sort();
        }
    }
}
