using CSharpCore;
using CSharpCore.FileSystem;
using OEP520G.Parameter;
using System.Linq;
using OEP520G.Product;
using System;
using System.Collections.Generic;
using System.IO;
using CSharpCore.Database;
using OEP520G.Core;
using Dapper;
using OEP520G.Product.Views;
using System.Transactions;

namespace OEP520G.Parameter
{
    /// <summary>
    /// Tray資料及演算法
    /// </summary>
    public class Tray
    {
        // Singleton單例模式
        private static readonly Lazy<Tray> lazy = new Lazy<Tray>(() => new Tray());
        public static Tray Instance => lazy.Value;

        private readonly Machine machine = Machine.Instance;
        private readonly SQLiteService sqlite = new SQLiteService();
        private readonly Nozzle nozzle = Nozzle.Instance;
        private readonly Clamp clamp = Clamp.Instance;

        /// <summary>
        /// Tray盤列表
        /// </summary>
        public List<TrayData> TrayList { get; private set; } = new List<TrayData>();

        /// <summary>
        /// Tray Feeder
        /// </summary>
        public List<TrayFeeder> FeederList { get; private set; } = new List<TrayFeeder>();

        // Tray盤資料是否變更?
        internal bool TrayHasChanged;

        /// <summary>
        /// 建構函式
        /// </summary>
        private Tray()
        {
            ReadParameter();
        }

        /********************
        * 參數作業
        *******************/
        /// <summary>
        /// 寫入參數檔
        /// </summary>
        public void WriteParameter()
        {
            if (ProductManager.HasProductActived)
            {
                // 連線資料庫
                using var conn = sqlite.OpenConnection(SQLite.DB_FILE_NAME_PRODUCT);
                if (conn != null)
                {
                    using var trans = conn.BeginTransaction();

                    string sqlString;
                    DynamicParameters parameters;

                    // TODO: UPDATE
                    conn.Query($"DELETE FROM {SQLite.TABLE_NAME_TRAY};");
                    conn.Query($"DELETE FROM {SQLite.TABLE_NAME_TRAY_LAYOUT};");
                    conn.Query($"DELETE FROM {SQLite.TABLE_NAME_TRAY_POINT_MATRIX};");
                    conn.Query($"DELETE FROM {SQLite.TABLE_NAME_TRAY_MASK};");

                    foreach (var tray in TrayList)
                    {
                        // 主Key
                        string trayName = tray.Name;

                        // 設定關聯
                        parameters = new DynamicParameters();
                        parameters.Add("Name", trayName);

                        // 儲存主資料表
                        sqlString = $@"INSERT INTO {SQLite.TABLE_NAME_TRAY} (
Name, Memo, TotalPoints, Direction, Arrangement,
DatumX, DatumY, OffsetX, OffsetY, CurrentPoint, NextTray
) VALUES (
'{trayName}',
'{tray.Memo}',
{tray.TotalPoints},
{(int)tray.Direction},
{(int)tray.Arrangement},
{tray.DatumX},
{tray.DatumY},
{tray.OffsetX},
{tray.OffsetY},
{tray.CurrentPoint},
{tray.NextTray}
);";
                        conn.Execute(sqlString);

                        // 儲存Tray排列
                        foreach (var layout in tray.Layout)
                        {
                            sqlString = $@"INSERT INTO {SQLite.TABLE_NAME_TRAY_LAYOUT} (
Name, TrayName, Effective, TotalLines, PointsInLine,
OriginX, OriginY, DiagonalX, DiagonalY, GapHorizontal, GapVertical
) VALUES (
'{layout.Name}',
'{trayName}',
{layout.Effective},
{layout.TotalLines},
{layout.PointsInLine},
{layout.OriginX},
{layout.OriginY},
{layout.DiagonalX},
{layout.DiagonalY},
{layout.GapHorizontal},
{layout.GapVertical}
);";
                            conn.Execute(sqlString);
                        }

                        // 儲存點位矩陣
                        foreach (var pointMatrix in tray.PointMatrix)
                        {
                            sqlString = $@"INSERT INTO {SQLite.TABLE_NAME_TRAY_POINT_MATRIX} (
PointNo, TrayName, PointMatrixX, PointMatrixY
) VALUES (
{pointMatrix.PointNo},
'{trayName}',
{pointMatrix.PointMatrixX},
{pointMatrix.PointMatrixY}
);";
                            conn.Execute(sqlString);
                        }

                        // 儲存Mask
                        foreach (var mask in tray.Mask)
                        {
                            sqlString = $@"INSERT INTO {SQLite.TABLE_NAME_TRAY_MASK} (
PointNo, TrayName
) VALUES (
{mask},
'{trayName}'
);";
                            conn.Execute(sqlString);
                        }
                    }

                    // 儲存Feeder
                    conn.Query($"DELETE FROM {SQLite.TABLE_NAME_TRAY_FEEDER};");

                    FeederList.Sort((x, y) => x.FeederId.CompareTo(y.FeederId));
                    foreach (var fl in FeederList)
                    {
                        sqlString = $@"INSERT INTO {SQLite.TABLE_NAME_TRAY_FEEDER} (
FeederId, Effective, Part, PartEnable,
ImageBeforePickup, ImageBeforePickupEnable, ImageBeforeCarry, ImageBeforeCarryEnable
) VALUES (
{fl.FeederId},
{fl.Effective},
'{fl.Part}',
{fl.PartEnable},
'{fl.ImageBeforePickup}',
{fl.ImageBeforePickupEnable},
'{fl.ImageBeforeCarry}',
{fl.ImageBeforeCarryEnable}
);";
                        conn.Execute(sqlString);
                    }

                    trans.Commit();
                }
            }
        }

        /// <summary>
        /// 從參數檔讀取全部Tray參數
        /// </summary>
        /// <remarks>
        /// [屬性名稱] = [Type].Parse(iniFile.ReadIniFile(SectionName, "[屬性名稱]", "[預設值]"));
        /// </remarks>
        public void ReadParameter()
        {
            // 連線資料庫
            using var conn = sqlite.OpenConnection(SQLite.DB_FILE_NAME_PRODUCT);
            if (conn != null)
            {
                using var trans = conn.BeginTransaction();

                // 確認資料表
                sqlite.CreateTable(conn, SQLite.TABLE_CREATE_SQL_TRAY);
                sqlite.CreateTable(conn, SQLite.TABLE_CREATE_SQL_TRAY_LAYOUT);
                sqlite.CreateTable(conn, SQLite.TABLE_CREATE_SQL_TRAY_POINT_MATRIX);
                sqlite.CreateTable(conn, SQLite.TABLE_CREATE_SQL_TRAY_MASK);
                sqlite.CreateTable(conn, SQLite.TABLE_CREATE_SQL_TRAY_FEEDER);

                string sqlString;
                DynamicParameters parameters;

                // 讀取主資料表
                sqlString = $"SELECT * FROM {SQLite.TABLE_NAME_TRAY}";
                //TrayList.Clear();
                TrayList = conn.Query<TrayData>(sqlString).ToList();

                foreach (var tray in TrayList)
                {
                    // 設定關聯
                    parameters = new DynamicParameters();
                    parameters.Add("Name", tray.Name);

                    // 讀取Tray排列
                    sqlString = $@"SELECT * FROM {SQLite.TABLE_NAME_TRAY_LAYOUT} WHERE TrayName=@Name";
                    tray.Layout = new List<TrayLayout>();
                    tray.Layout.AddRange(conn.Query<TrayLayout>(sqlString, parameters));

                    // 讀取點位矩陣
                    sqlString = $@"SELECT * FROM {SQLite.TABLE_NAME_TRAY_POINT_MATRIX} WHERE TrayName=@Name";
                    tray.PointMatrix = new List<TrayPointMatrix>();
                    tray.PointMatrix.AddRange(conn.Query<TrayPointMatrix>(sqlString, parameters));

                    // 讀取Mask
                    sqlString = $@"SELECT * FROM {SQLite.TABLE_NAME_TRAY_MASK} WHERE TrayName=@Name";
                    tray.Mask = new List<int>();
                    tray.Mask = conn.Query<int>(sqlString, parameters).ToList();
                }

                // 讀取TrayFeeder
                sqlString = $"SELECT * FROM {SQLite.TABLE_NAME_TRAY_FEEDER} WHERE Effective";
                FeederList = conn.Query<TrayFeeder>(sqlString).ToList();

                trans.Commit();
            }
            else
            {
                TrayList.Clear();
            }


            //TrayList.Clear();
            //string trayPath = $"{FileList.DIRECTORY_ACTIVE_PRODUCT}\\{FileList.DIRECTORY_TRAY}";

            //string sectionName = "Tray";

            //// 若TRAY資料夾不存在則離開
            //if (!Directory.Exists(trayPath))
            // return;

            //string[] fileLists = Directory.GetFiles(trayPath, "*.ini");

            //for (int fileIdx = 0; fileIdx < fileLists.Length; fileIdx++)
            //{
            // string bufString;

            // string filePath = fileLists[fileIdx];
            // //string[] fileSplit = filePath.Split('\\');
            // //string fileName = fileSplit[fileSplit.Length - 1];
            // //string TrayId = fileName.Substring(0, fileName.Length - 4);
            // string trayName = filePath.Split('\\')[^1][0..^4];

            // IniFile iniFile = new IniFile(filePath);

            // TrayObject ti = new TrayObject();

            // ti.Name = iniFile.ReadIniFile(sectionName, "Name", "");
            // ti.Memo = iniFile.ReadIniFile(sectionName, "Memo", "");
            // ti.TotalX = int.Parse(iniFile.ReadIniFile(sectionName, "TotalX", "1"));
            // ti.TotalY = int.Parse(iniFile.ReadIniFile(sectionName, "TotalY", "1"));

            // ti.Direction = iniFile.ReadIniFile(sectionName, "Direction", "X");

            // ti.OriginX = double.Parse(iniFile.ReadIniFile(sectionName, "OriginX", "0"));
            // ti.OriginY = double.Parse(iniFile.ReadIniFile(sectionName, "OriginY", "0"));
            // ti.OriginR = double.Parse(iniFile.ReadIniFile(sectionName, "OriginR", "0"));
            // ti.DiagonalX = double.Parse(iniFile.ReadIniFile(sectionName, "DiagonalX", "0"));
            // ti.DiagonalY = double.Parse(iniFile.ReadIniFile(sectionName, "DiagonalY", "0"));
            // ti.DiagonalR = double.Parse(iniFile.ReadIniFile(sectionName, "DiagonalR", "0"));

            // ti.GapX = double.Parse(iniFile.ReadIniFile(sectionName, "GapX", "0"));
            // ti.GapY = double.Parse(iniFile.ReadIniFile(sectionName, "GapY", "0"));
            // ti.OffsetX = double.Parse(iniFile.ReadIniFile(sectionName, "OffsetX", "0"));
            // ti.OffsetY = double.Parse(iniFile.ReadIniFile(sectionName, "OffsetY", "0"));

            // ti.CurrentPoint = int.Parse(iniFile.ReadIniFile(sectionName, "CurrentPoint", "1"));

            // bufString = iniFile.ReadIniFile(sectionName, "PositionNo", "");
            // ti.PositionNo = (bufString == "") ? new List<int>() : bufString.ToIntList();

            // bufString = iniFile.ReadIniFile(sectionName, "PositionX", "");
            // ti.PositionX = (bufString == "") ? new List<int>() : bufString.ToIntList();

            // bufString = iniFile.ReadIniFile(sectionName, "PositionY", "");
            // ti.PositionY = (bufString == "") ? new List<int>() : bufString.ToIntList();

            // bufString = iniFile.ReadIniFile(sectionName, "PointMatrixX", "");
            // ti.PointMatrixX = (bufString == "") ? new List<double>() : bufString.ToDoubleList();

            // bufString = iniFile.ReadIniFile(sectionName, "PointMatrixY", "");
            // ti.PointMatrixY = (bufString == "") ? new List<double>() : bufString.ToDoubleList();

            // bufString = iniFile.ReadIniFile(sectionName, "PointMatrixR", "");
            // ti.PointMatrixR = (bufString == "") ? new List<double>() : bufString.ToDoubleList();

            // bufString = iniFile.ReadIniFile(sectionName, "Mask", "");
            // ti.Mask = (bufString == "") ? new List<int>() : bufString.ToIntList();

            // ti.TotalPoints = ti.TotalX * ti.TotalY - ti.Mask.Count;

            // SetTrayMinimumDemand(ti);

            // TrayList.Add(ti);
            //}
        }

        ///// <summary>
        ///// 將參數設定為最低需求狀態(預設值)
        ///// </summary>
        //private void SetTrayMinimumDemand(TrayData ti)
        //{
        // if ((ti.Direction != "X") && (ti.Direction != "Y"))
        // ti.Direction = "X";

        // if (ti.PositionNo == null)
        // ti.PositionNo = new List<int>();
        // if (ti.PositionX == null)
        // ti.PositionX = new List<int>();
        // if (ti.PositionY == null)
        // ti.PositionY = new List<int>();
        // if (ti.PointMatrixX == null)
        // ti.PointMatrixX = new List<double>();
        // if (ti.PointMatrixY == null)
        // ti.PointMatrixY = new List<double>();
        // if (ti.PointMatrixR == null)
        // ti.PointMatrixR = new List<double>();

        // if (ti.Mask == null)
        // ti.Mask = new List<int>();
        //}

        /********************
        * CRUD
        *******************/
        /// <summary>
        /// 新建Tray盤
        /// </summary>
        /// <param name="te">TrayExchange</param>
        /// <returns>是否新建成功</returns>
        public bool NewTray(TrayExchange te)
        {
            if (IsTrayExist(te.NewTrayName))
                return false;

            TrayData ti = new TrayData
            {
                Name = te.NewTrayName,
                Memo = te.Memo
            };

            TrayList.Add(ti);
            //SetTrayMinimumDemand(ti);
            WriteParameter();

            return true;
        }

        /// <summary>
        /// 複製Tray盤
        /// </summary>
        /// <param name="te">TrayExchange</param>
        /// <returns>是否複製成功</returns>
        public bool CopyTray(TrayExchange te)
        {
            if (IsTrayExist(te.NewTrayName))
                return false;

            string trayPath = $"{FileList.DIRECTORY_ACTIVE_PRODUCT}\\{FileList.DIRECTORY_TRAY}";
            string oldFileName = $"{trayPath}\\{te.TrayName}.ini";
            string destFileName = $"{trayPath}\\{te.NewTrayName}.ini";

            File.Copy(oldFileName, destFileName, true);
            ReadParameter();

            // 複製完會有兩個內容一樣的檔案，故TrayList中有兩筆一樣的Name
            // 抓一個變更Name，寫入檔時會依檔案覆蓋正確的檔
            int idx = TrayList.FindLastIndex(x => x.Name == te.TrayName);
            if (idx >= 0)
            {
                TrayList[idx].Name = te.NewTrayName;
                TrayList[idx].Memo = te.Memo;
                WriteParameter();

                // 更新整個TrayInfo
                ReadParameter();
                return true;
            }
            else
            {
                File.Delete(destFileName);
                return false;
            }
        }

        /// <summary>
        /// Tray更名
        /// </summary>
        /// <param name="te">TrayExchange</param>
        /// <returns>是否更名成功</returns>
        public bool RenameTray(TrayExchange te)
        {
            if (IsTrayExist(te.NewTrayName))
                return false;

            string trayPath = $"{FileList.DIRECTORY_ACTIVE_PRODUCT}\\{FileList.DIRECTORY_TRAY}";
            string oldFileName = $"{trayPath}\\{te.TrayName}.ini";
            string destFileName = $"{trayPath}\\{te.NewTrayName}.ini";

            // 找到要更名的Tray位置
            int idx = TrayList.FindIndex(x => x.Name == te.TrayName);
            if (idx >= 0)
            {
                // 檔案更名
                File.Move(oldFileName, destFileName);

                // TrayList更名
                TrayList[idx].Name = te.NewTrayName;
                TrayList[idx].Memo = te.Memo;

                // 寫回
                WriteParameter();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 刪除Tray盤
        /// </summary>
        /// <param name="ti"></param>
        /// <returns>是否刪除成功</returns>
        public bool DeleteTray(TrayData ti)
        {
            if (IsTrayExist(ti.Name))
            {
                // TrayList刪除
                TrayList.Remove(ti);

                // 檔案刪除
                string fileName = $"{FileList.DIRECTORY_ACTIVE_PRODUCT}\\{FileList.DIRECTORY_TRAY}\\{ti.Name}.ini";
                if (File.Exists(fileName))
                    File.Delete(fileName);

                return true;
            }

            return false;
        }

        /********************
         * 轉換
         ********************/
        /// <summary>
        /// 檢查Tray盤名稱是否已存在
        /// </summary>
        /// <param name="trayName">Tray盤名稱</param>
        /// <returns>True:名稱重覆 False:名稱不存在</returns>
        public bool IsTrayExist(string trayName)
            => TrayList.Exists(x => x.Name == trayName);

        /// <summary>
        /// 取得Tray資料
        /// </summary>
        /// <param name="trayName">Tray名稱</param>
        /// <param name="trayData">返回Tray</param>
        public TrayData GetTrayData(string trayName)
            => TrayList.Find(x => x.Name == trayName);

        /// <summary>
        /// 由Feeder Id取得Tray Name
        /// </summary>
        /// <param name="feederId">Feeder Id</param>
        /// <returns>Tray Name</returns>
        public string GetTrayNameFromFeederId(int feederId)
        {
            TrayData t = null;

            int feederIndex = FeederList.FindIndex(x => x.FeederId == feederId);
            if (feederIndex >= 0)
                t = GetTrayData(FeederList[feederIndex].Part);

            return (feederIndex >= 0 && t != null) ? t.Name : string.Empty;
        }

        /********************
         * PointMatrix
         ********************/
        /// <summary>
        /// PointMatrix重新排列
        /// </summary>
        /// <param name="trayData">要重新排列的Tray</param>
        public void RearrangePointMatrix(TrayData trayData)
        {
            if (trayData == null)
                return;

            // 資料合法性確認
            if (!Enum.IsDefined(typeof(EDirection), trayData.Direction)
                || !Enum.IsDefined(typeof(EArrangement), trayData.Arrangement)
                )
                return;

            var trayCalcList = new List<TrayCalculation>();

            /********************
             * Tray基準點 => 第一組的第1點 絕對座標
             ********************/
            double datumX = trayData.Layout[0].OriginX;
            double datumY = trayData.Layout[0].OriginY;

            /********************
             * Tray基準點 => 第一組的第1點 與機台基準點的相對位置
             ********************/
            trayData.DatumX = datumX - machine.DatumPoint1.Position.X;
            trayData.DatumY = datumY - machine.DatumPoint1.Position.Y;

            /********************
             * 各分組演算
             ********************/
            int selectedIndex = 0;
            foreach (var layout in trayData.Layout)
            {
                // 資料合法性確認
                if (layout.Effective
                    && layout.PointsInLine > 0
                    && layout.TotalLines > 0
                    )
                {
                    var calc = new TrayCalculation()
                    {
                        SelectedIndex = selectedIndex,
                        TotalLines = layout.TotalLines,
                        PointsInLine = layout.PointsInLine,
                        OriginX = layout.OriginX,
                        OriginY = layout.OriginY,
                        OffsetX = new double[layout.TotalLines, layout.PointsInLine],
                        OffsetY = new double[layout.TotalLines, layout.PointsInLine]
                    };

                    /**********
                     * 數據設定
                     **********/
                    // 長寬 點位數
                    int totalHorizontal;
                    int totalVertical;

                    if (trayData.Direction == EDirection.Horizontal)
                    {
                        totalHorizontal = layout.PointsInLine;
                        totalVertical = layout.TotalLines;
                    }
                    else
                    {
                        totalHorizontal = layout.TotalLines;
                        totalVertical = layout.PointsInLine;
                    }

                    // 計算長寬
                    double sizeH = Math.Abs(layout.OriginX - layout.DiagonalX);
                    double sizeV = Math.Abs(layout.OriginY - layout.DiagonalY);

                    // 排列向量
                    double dirH = layout.OriginX < layout.DiagonalX ? 1 : -1;
                    double dirV = layout.OriginY < layout.DiagonalY ? 1 : -1;

                    // Gap值
                    layout.GapHorizontal = sizeH / (totalHorizontal - 1);
                    layout.GapVertical = sizeV / (totalVertical - 1);

                    // Gap向量
                    double gapHorizontal = layout.GapHorizontal * dirH;
                    double gapVertical = layout.GapVertical * dirV;

                    // 排列演算
                    if (trayData.Direction == EDirection.Horizontal)
                    {
                        double offsetX;
                        double offsetY;

                        for (int loopLine = 0; loopLine < totalVertical; loopLine++)
                        {
                            offsetY = gapVertical * loopLine;

                            for (int loopPoint = 0; loopPoint < totalHorizontal; loopPoint++)
                            {
                                offsetX = gapHorizontal * loopPoint;

                                calc.OffsetX[loopLine, loopPoint] = calc.OriginX - datumX + offsetX;
                                calc.OffsetY[loopLine, loopPoint] = calc.OriginY - datumY + offsetY;
                            }
                        }
                    }
                    else
                    {
                        double offsetX;
                        double offsetY;

                        for (int loopLine = 0; loopLine < totalHorizontal; loopLine++)
                        {
                            //if (dirH == 1)
                            //    offsetX = loopLine * gapHorizontal;
                            //else
                            //    offsetX = (totalHorizontal - 1 - loopLine) * gapHorizontal;

                            offsetX = gapHorizontal * loopLine;

                            for (int loopPoint = 0; loopPoint < totalVertical; loopPoint++)
                            {
                                //if (dirV == 1)
                                //    offsetY = loopPoint * gapVertical;
                                //else
                                //    offsetY = (totalVertical - 1 - loopPoint) * gapVertical;

                                offsetY = gapVertical * loopPoint;

                                calc.OffsetX[loopLine, loopPoint] = calc.OriginX - datumX + offsetX;
                                calc.OffsetY[loopLine, loopPoint] = calc.OriginY - datumY + offsetY;
                            }
                        }
                    }

                    trayCalcList.Add(calc);
                }

                selectedIndex++;
            }

            /********************
             * 合併前處理
             ********************/
            // 數據設定
            int totalLayout = trayCalcList.Count;        // 有幾組要合併
            bool[] finishedFlag = new bool[totalLayout]; // 完成旗標
            int[] finishedLine = new int[totalLayout];   // 完成行數

            for (int idx = 0; idx < totalLayout; idx++)
            {
                finishedFlag[idx] = false;
                finishedLine[idx] = 0;
            }

            /********************
             * 合併作業
             ********************/
            int layoutSelected = 0;  // 目前合併的是那組
            int finishedCounter = 0; // 已合併完成組數

            trayData.PointMatrix.Clear();
            trayData.TotalPoints = 0;

            // 點位編號
            int pointNo = 1;

            while (finishedCounter < totalLayout)
            {
                // 此組合併未完成
                if (!finishedFlag[layoutSelected])
                {
                    // 合併一行
                    var layout = trayCalcList[layoutSelected];
                    int lineNo = finishedLine[layoutSelected];

                    for (int count = 0; count < layout.PointsInLine; count++)
                    {
                        trayData.PointMatrix.Add(new TrayPointMatrix()
                        {
                            PointNo = pointNo++,
                            PointMatrixX = layout.OffsetX[lineNo, count],
                            PointMatrixY = layout.OffsetY[lineNo, count]
                        });
                    }

                    // 此行合併完成
                    finishedLine[layoutSelected]++;
                    trayData.TotalPoints += layout.PointsInLine;

                    // 判斷是否合併完成
                    if (finishedLine[layoutSelected] >= layout.TotalLines)
                    {
                        finishedFlag[layoutSelected] = true;
                        finishedCounter++;
                    }
                }

                // 是否換組
                if (trayData.Arrangement == EArrangement.Staggered)
                {
                    layoutSelected++;
                    if (layoutSelected >= totalLayout)
                        layoutSelected = 0;
                }
            }

            TrayHasChanged = true;
        }

        /********************
        * Tray座標計算
        *******************/
        ///// <summary>
        ///// 取得Tray點位至機台基準點的相對座標
        ///// </summary>
        ///// <param name="feederId">Fedder ID</param>
        ///// <returns>CurrentPoint至機台基準點的相對座標(X,Y)</returns>
        //public (double X, double Y) GetDistanceToDatumPoint(int feederId)
        //{
        //    return GetDistanceToDatumPoint(FeederList.Find(x => x.FeederId == feederId).Part);
        //}

        /// <summary>
        /// 取得Tray點位至機台基準點的相對座標
        /// </summary>
        /// <param name="trayName">Tray盤Name</param>
        /// <returns>CurrentPoint至機台基準點的相對座標(X,Y)</returns>
        public (double X, double Y) GetDistanceToDatumPoint(string trayName)
        {
            var t = GetTrayData(trayName);
            if (t != null)
            {
                var pMatrix = t.PointMatrix[t.CurrentPoint - 1];

                double pmX = t.DatumX + pMatrix.PointMatrixX + t.OffsetX;
                double pmY = t.DatumY + pMatrix.PointMatrixY + t.OffsetY;

                return (pmX, pmY);
            }
            else
                return (0, 0);
        }

        ///// <summary>
        ///// 取得Tray點位絕對座標
        ///// </summary>
        ///// <param name="feederId">Fedder ID</param>
        ///// <returns>絕對座標(X,Y)</returns>
        //public (double X, double Y) GetPosition(int feederId)
        //    => GetPosition(FeederList.Find(x => x.FeederId == feederId).Part);

        ///// <summary>
        ///// 取得Tray點位絕對座標
        ///// </summary>
        ///// <param name="trayName">Tray盤Name</param>
        ///// <returns>絕對座標(X,Y)</returns>
        //public (double X, double Y) GetPosition(string trayName)
        //{
        //    var trayData = TrayList.Find(x => x.Name == trayName);
        //    var pMatrix = trayData.PointMatrix[trayData.CurrentPoint];

        //    double posX = pmX + machine.DatumPoint1.Position.X;
        //    double posY = pmY + machine.DatumPoint1.Position.Y;

        //    return (posX, posY);
        //}

        /********************
        * 取料位址
        *******************/
        /// <summary>
        /// 移至下一個取料位址
        /// </summary>
        /// <param name="trayName">指定Tray盤ID</param>
        public void MoveNext(string trayName)
        {
            var t = GetTrayData(trayName);
            if (t != null)
            {
                do
                {
                    t.CurrentPoint++;
                } while (t.Mask.Contains(t.CurrentPoint));
            }
        }

        /// <summary>
        /// 將目前取料位址歸零
        /// </summary>
        /// <param name="trayName">指定Tray盤ID</param>
        public void ResetPosition(string trayName)
        {
            var t = GetTrayData(trayName);
            if (t != null)
                t.CurrentPoint = 0;
        }
    }
}
