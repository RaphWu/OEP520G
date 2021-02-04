using CSharpCore.Database;
using OEP520G.Parameter;
using OEP520G.Product;
using System;
using System.Collections.Generic;
using System.Text;

namespace OEP520G.Core
{
    /// <summary>
    /// 資料庫
    /// </summary>
    public class SQLite : IProductManager
    {
        private readonly SQLiteService sqlite = new SQLiteService();

        /// <summary>
        /// 建構函式
        /// </summary>
        public SQLite()
        {
            // 訂閱ProductSwitch事件，並設定呼叫SwProduct函式
            Common.EA.GetEvent<OnSwitchProduct>().Subscribe(onProductChangeover, true);
            Common.EA.GetEvent<AfterSwitchProduct>().Subscribe(afterProductChangeover, true);
        }

        /// <summary>
        /// 資料庫檔案的副檔名
        /// </summary>
        public const string DB_FILE_EXT_NAME = ".db";

        /// <summary>
        /// DB Name: 機台參數檔案名稱
        /// </summary>
        public const string DB_FILE_NAME_SYSTEM = @".\System" + DB_FILE_EXT_NAME;

        /// <summary>
        /// DB Name: 產品參數檔案名稱
        /// 此檔案名稱由品種切換功能設定
        /// </summary>
        public static string DB_FILE_NAME_PRODUCT { get; set; } = "";

        /// <inheritdoc/>
        /// <param name="productId">切換後的品種ID</param>
        public void onProductChangeover(string productId)
        {
            if (productId != "")
                DB_FILE_NAME_PRODUCT = $@".\{FileList.DIRECTORY_PRODUCT}\{productId}{DB_FILE_EXT_NAME}";
            else
                DB_FILE_NAME_PRODUCT = "";
        }

        /// <inheritdoc/>
        /// <param name="productId">切換後的品種ID</param>
        public void afterProductChangeover(string productId)
        {
        }

        /********************
        * SQL資料表名稱
        * 共同
        *******************/
        // Table Name: DB資訊表
        public const string TABLE_NAME_INFORMATION = "Information";

        public const string TABLE_CREATE_SQL_SYSTEM_INFORMATION = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_INFORMATION + @"](
[MachineId] TEXT,
[MachineName] TEXT);";

        public const string TABLE_CREATE_SQL_PRODUCT_INFORMATION = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_INFORMATION + @"](
[ProductId] TEXT NOT NULL UNIQUE,
[MachineId] TEXT,
[Memo] TEXT);";

        /********************
        * SQL資料表名稱
        * System檔
        *******************/
        // Table Name: 基準點
        public const string TABLE_NAME_DATUM_POINT = "DatumPoint";
        public const string TABLE_CREATE_SQL_DATUM_POINT = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_DATUM_POINT + @"](
[DatumPointId] INTEGER PRIMARY KEY ASC NOT NULL UNIQUE,
[X] DOUBLE DEFAULT(0.0),
[Y] DOUBLE DEFAULT(0.0),
[Z] DOUBLE DEFAULT(0.0),
[Frequency] INTEGER DEFAULT 0,
[Tolerance] DOUBLE DEFAULT(0.0)) WITHOUT ROWID;";

        /// Table Name: 相機
        public const string TABLE_NAME_CAMERA = "Camera";
        public const string TABLE_CREATE_SQL_CAMERA = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_CAMERA + @"](
[CameraName] TEXT NOT NULL UNIQUE,
[DatumX] DOUBLE NOT NULL DEFAULT (0.0),
[DatumY] DOUBLE NOT NULL DEFAULT (0.0),
[DatumZ] DOUBLE NOT NULL DEFAULT (0.0));";

        // Table Name: 夾爪
        public const string TABLE_NAME_CLAMP = "Clamp";
        public const string TABLE_CREATE_SQL_CLAMP = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_CLAMP + @"](
[ClampId] INTEGER PRIMARY KEY ASC NOT NULL UNIQUE,
[CorrectX] DOUBLE DEFAULT (0.0),
[CorrectY] DOUBLE DEFAULT (0.0),
[StageX] DOUBLE DEFAULT (0.0),
[StageY] DOUBLE DEFAULT (0.0),
[StageZ] DOUBLE DEFAULT (0.0),
[Timer1] INTEGER DEFAULT 300,
[Timer2] INTEGER DEFAULT 300) WITHOUT ROWID;";

        // Table Name: 平台(測高平台)
        public const string TABLE_NAME_PLATFORM = "Platform";
        public const string TABLE_CREATE_SQL_PLATFORM = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_PLATFORM + @"](
[PlatformId] INTEGER PRIMARY KEY ASC NOT NULL UNIQUE,
[PlatformName] TEXT,
[X] DOUBLE DEFAULT (0.0),
[Y] DOUBLE DEFAULT (0.0),
[Z] DOUBLE DEFAULT (0.0)) WITHOUT ROWID;";

        // Table Name: 抛料點
        public const string TABLE_NAME_DISCARD = "Discard";
        public const string TABLE_CREATE_SQL_DISCARD = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_DISCARD + @"](
[DiscardName] TEXT NOT NULL UNIQUE,
[X] DOUBLE DEFAULT (0.0),
[Y] DOUBLE DEFAULT (0.0),
[Times] INTEGER DEFAULT 1,
[Timer1] INTEGER DEFAULT 500,
[Timer2] INTEGER DEFAULT 500);";

        /********************
        * SQL資料表名稱
        * 品種資料
        * 1.檔案名稱 = 品種代號.DB_FILE_EXT_NAME
        *******************/
        /*===== 台車 =====*/
        // Table Name: 台車
        public const string TABLE_NAME_STAGE = "Stage";
        public const string TABLE_CREATE_SQL_STAGE = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_STAGE + @"](
[StageId] INTEGER NOT NULL UNIQUE,
[RotateCenterX] DOUBLE NOT NULL DEFAULT (0.0),
[RotateCenterY] DOUBLE NOT NULL DEFAULT (0.0),
[Height] DOUBLE DEFAULT (0.0),
[StartAngle] REAL NOT NULL DEFAULT (0.0),
[EndAngle] REAL NOT NULL DEFAULT (360.0),
[IntervalAngle] REAL NOT NULL DEFAULT (5.0));";

        // Table Name: 台車旋轉位移
        public const string TABLE_NAME_STAGE_ROTATE = "StageRotate";
        public const string TABLE_CREATE_SQL_STAGE_ROTATE = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_STAGE_ROTATE + @"](
[Angle] REAL PRIMARY KEY ASC NOT NULL,
[ShiftX] DOUBLE DEFAULT (0.0),
[ShiftY] DOUBLE DEFAULT (0.0)) WITHOUT ROWID;";

        /*===== 吸嘴 =====*/
        // Table Name: 不分吸嘴的參數(基準吸嘴)
        public const string TABLE_NAME_NOZZLE = "Nozzle";
        public const string TABLE_CREATE_SQL_NOZZLE = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_NOZZLE + @"](
[DatumNozzle] INTEGER NOT NULL);";

        // Table Name: 各支吸嘴參數
        public const string TABLE_NAME_NOZZLES = "Nozzles";
        public const string TABLE_CREATE_SQL_NOZZLES = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_STAGE_ROTATE + @"](
[NozzleId] INTEGER PRIMARY KEY ASC NOT NULL UNIQUE,
[MeasureHeight] DOUBLE NOT NULL DEFAULT (0.0),
[PositionX] DOUBLE NOT NULL DEFAULT (0.0),
[PositionY] DOUBLE NOT NULL DEFAULT (0.0),
[PositionZ] DOUBLE NOT NULL DEFAULT (0.0),
[PulseX] BIGINT NOT NULL DEFAULT 0,
[PulseY] BIGINT NOT NULL DEFAULT 0,
[PulseZ] BIGINT NOT NULL DEFAULT 0,
[EncoderX] INTEGER NOT NULL DEFAULT 0,
[EncoderY] INTEGER NOT NULL DEFAULT 0,
[EncoderZ] INTEGER NOT NULL DEFAULT 0,
[DistanceX] DOUBLE NOT NULL DEFAULT (0.0),
[DistanceY] DOUBLE NOT NULL DEFAULT (0.0),
[EncMarkerXUltraHigh] INTEGER NOT NULL DEFAULT 0,
[EncMarkerYUltraHigh] INTEGER NOT NULL DEFAULT 0,
[EncMarkerXHigh] INTEGER NOT NULL DEFAULT 0,
[EncMarkerYHigh] INTEGER NOT NULL DEFAULT 0,
[EncMarkerXMiddle] INTEGER NOT NULL DEFAULT 0,
[EncMarkerYMiddle] INTEGER NOT NULL DEFAULT 0,
[TimeMarkerXUltraHigh] INTEGER NOT NULL DEFAULT 0,
[TimeMarkerYUltraHigh] INTEGER NOT NULL DEFAULT 0,
[TimeMarkerXHigh] INTEGER NOT NULL DEFAULT 0,
[TimeMarkerYHigh] INTEGER NOT NULL DEFAULT 0,
[TimeMarkerXMiddle] INTEGER NOT NULL DEFAULT 0,
[TimeMarkerYMiddle] INTEGER NOT NULL DEFAULT 0) WITHOUT ROWID;";

        /*===== 托盤 =====*/
        // Table Name: Tray主檔(供料器描述、長寬、排列方向、位移)
        public const string TABLE_NAME_TRAY = "Tray";
        public const string TABLE_CREATE_SQL_TRAY = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_TRAY + @"](
[Name] TEXT PRIMARY KEY NOT NULL, 
[Memo] TEXT, 
[TotalPoints] INTEGER, 
[Direction] INTEGER NOT NULL DEFAULT 0, 
[Arrangement] INTEGER NOT NULL DEFAULT 0, 
[DatumX] DOUBLE NOT NULL DEFAULT 0, 
[DatumY] DOUBLE NOT NULL DEFAULT 0, 
[OffsetX] DOUBLE DEFAULT (0.0), 
[OffsetY] DOUBLE DEFAULT (0.0), 
[CurrentPoint] INTEGER DEFAULT 1, 
[NextTray] INTEGER) WITHOUT ROWID;";

        // Table Name: 托盤各組排列組合
        public const string TABLE_NAME_TRAY_LAYOUT = "TrayLayout";
        public const string TABLE_CREATE_SQL_TRAY_LAYOUT = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_TRAY_LAYOUT + @"](
[Name] TEXT, 
[TrayName] TEXT NOT NULL, 
[Effective] BOOLEAN NOT NULL DEFAULT 1, 
[TotalLines] INTEGER NOT NULL, 
[PointsInLine] INTEGER NOT NULL, 
[OriginX] DOUBLE NOT NULL DEFAULT (0.0), 
[OriginY] DOUBLE NOT NULL DEFAULT (0.0), 
[DiagonalX] DOUBLE NOT NULL DEFAULT (0.0), 
[DiagonalY] DOUBLE NOT NULL DEFAULT (0.0), 
[GapHorizontal] DOUBLE DEFAULT (0.0), 
[GapVertical] DOUBLE DEFAULT (0.0));";

        // Table Name: 托盤點位表
        public const string TABLE_NAME_TRAY_POINT_MATRIX = "TrayPointMatrix";
        public const string TABLE_CREATE_SQL_TRAY_POINT_MATRIX = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_TRAY_POINT_MATRIX + @"](
[PointNo] INTEGER NOT NULL UNIQUE, 
[TrayName] TEXT NOT NULL, 
[PointMatrixX] DOUBLE NOT NULL DEFAULT (0.0), 
[PointMatrixY] DOUBLE NOT NULL DEFAULT (0.0));";

        // Table Name: 托盤無效點位
        public const string TABLE_NAME_TRAY_MASK = "TrayMask";
        public const string TABLE_CREATE_SQL_TRAY_MASK = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_TRAY_MASK + @"](
[PointNo] INTEGER, 
[TrayName] TEXT NOT NULL);";

        // Table Name: 托盤進供料器
        public const string TABLE_NAME_TRAY_FEEDER = "TrayFeeder";
        public const string TABLE_CREATE_SQL_TRAY_FEEDER = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_TRAY_FEEDER + @"](
[FeederId] INTEGER PRIMARY KEY ASC NOT NULL, 
[Effective] BOOLEAN DEFAULT 0, 
[Part] TEXT, 
[PartEnable] BOOLEAN DEFAULT 0, 
[ImageBeforePickup] TEXT, 
[ImageBeforePickupEnable] BOOLEAN DEFAULT 0, 
[ImageBeforeCarry] TEXT, 
[ImageBeforeCarryEnable] BOOLEAN DEFAULT 0) WITHOUT ROWID;";

        public const string SELECT_TRAY = @"
SELECT Tray.*, TrayFeeder.*, TrayArrangement.*, TrayPointMatrix.*, TrayMask.*
FROM (((Tray LEFT JOIN TrayFeeder ON Tray.TrayId = TrayFeeder.TrayId)
LEFT JOIN TrayArrangement ON Tray.TrayId = TrayArrangement.TrayId)
LEFT JOIN TrayMask ON Tray.TrayId = TrayMask.TrayId)
LEFT JOIN TrayPointMatrix ON Tray.TrayId = TrayPointMatrix.TrayId;";

        /*===== 點膠 =====*/
        // Table Name: 點膠基本座標
        public const string TABLE_NAME_DISPENSER = "Dispenser";
        public const string TABLE_CREATE_SQL_DISPENSER = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_DISPENSER + @"](
[DispenserId] INTEGER PRIMARY KEY NOT NULL UNIQUE,
[DatumX] DOUBLE DEFAULT (0.0),
[DatumY] DOUBLE DEFAULT (0.0),
[PositionX] DOUBLE DEFAULT (0.0),
[PositionY] DOUBLE DEFAULT (0.0),
[PositionZ] DOUBLE DEFAULT (0.0)) WITHOUT ROWID;";

        // Table Name: 點膠參數列表
        public const string TABLE_NAME_DISPENSER_GROUP = "DispenserGroup";
        public const string TABLE_CREATE_SQL_DISPENSER_GROUP = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_DISPENSER_GROUP + @"](
[GroupNo] INTEGER PRIMARY KEY ASC NOT NULL UNIQUE,
[DspSpeed] REAL DEFAULT (100.0),
[SpeedA] REAL DEFAULT 150,
[SWait] INTEGER DEFAULT 0,
[EShot] INTEGER DEFAULT 0,
[PreStop] REAL DEFAULT (0.0),
[EWait] INTEGER DEFAULT 0,
[UpXY] REAL DEFAULT (0.0),
[UpZ] REAL DEFAULT (0.0),
[UpSpeed] REAL DEFAULT (0.0),
[UpDelay] INTEGER DEFAULT 0,
[UpWay] INTEGER DEFAULT 0) WITHOUT ROWID;";

        // Table Name: 點膠動作列表
        public const string TABLE_NAME_DISPENSER_ACTION = "DispenserAction";
        public const string TABLE_CREATE_SQL_DISPENSER_ACTION = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_DISPENSER_ACTION + @"](
[ActionNo] INTEGER PRIMARY KEY ASC NOT NULL UNIQUE,
[SequenceId] INTEGER NOT NULL,
[ActionName] INTEGER,
[OffsetX] DOUBLE DEFAULT (0.0),
[OffsetY] DOUBLE DEFAULT (0.0),
[OffsetZ] DOUBLE DEFAULT (0.0),
[OffsetA] DOUBLE DEFAULT (0.0),
[Radius] DOUBLE DEFAULT (0.0),
[GroupId] INTEGER DEFAULT 1) WITHOUT ROWID;";

        // Table Name: 點膠動作順序
        public const string TABLE_NAME_DISPENSER_SEQUENCE = "DispenserSequence";
        public const string TABLE_CREATE_SQL_DISPENSER_SEQUENCE = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_DISPENSER_SEQUENCE + @"](
[SequenceId] INTEGER PRIMARY KEY NOT NULL UNIQUE,
[SequenceTitle] TEXT NOT NULL) WITHOUT ROWID;";

        /*===== 自動作業 =====*/
        // Table Name: 自動作業生產順序設定
        public const string TABLE_NAME_AUTO_SEQUENCE = "AutoSequence";
        public const string TABLE_CREATE_SQL_AUTO_SEQUENCE = @"CREATE TABLE IF NOT EXISTS [" +
        TABLE_NAME_AUTO_SEQUENCE + @"](
[Sequence] INTEGER PRIMARY KEY ASC NOT NULL,
[SelectedHead] INTEGER,
[SelectedAction] INTEGER,
[SelectedTarget] INTEGER,
[SelectedPart] INTEGER,
[SelectedTray] INTEGER,
[Angle] REAL NOT NULL DEFAULT (0.0),
[Effective] BOOLEAN NOT NULL DEFAULT 0,
[ImageProcessing] BOOLEAN NOT NULL DEFAULT 0,
[LaunchStageAfterProcedure] BOOLEAN NOT NULL DEFAULT 0,
[SkipAlignmentBeforeCarry] BOOLEAN NOT NULL DEFAULT 0,
[OpenClampWhenAssembly] BOOLEAN NOT NULL DEFAULT 0,
[StageReturn0AfterCarry] BOOLEAN NOT NULL DEFAULT 0,
[SkipPositionCheckWhenAssembly] BOOLEAN NOT NULL DEFAULT 0,
[OpenClampWhenSingleProcedure] BOOLEAN NOT NULL DEFAULT 0,
[AbsoluteZeroDegreeAssembly] BOOLEAN NOT NULL DEFAULT 0,
[GetCenterAfterStageRotate] BOOLEAN NOT NULL DEFAULT 0) WITHOUT ROWID;";

        /********************
        *
        *******************/

    }
}
