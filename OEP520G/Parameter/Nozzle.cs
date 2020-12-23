/****************************
 * 吸嘴參數檔
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
    public class Nozzle
    {
        // Singleton單例模式
        private static readonly Lazy<Nozzle> lazy = new Lazy<Nozzle>(() => new Nozzle());
        public static Nozzle Instance => lazy.Value;

        private readonly Epcio epcio = Epcio.Instance;
        private readonly CameraData camera = Camera.Instance.MoveCamera;
        private readonly Machine machine = Machine.Instance;

        /// <summary>
        /// 最大吸嘴數
        /// </summary>
        public const int MAX_NOZZLE = 11;

        /// <summary>
        /// 基準吸嘴編號
        /// </summary>
        public ENozzleId DatumNozzleId { get; set; }

        public readonly List<string> NOZZLE_NAME_LIST = new List<string>
        {
            "吸嘴1", "吸嘴2", "吸嘴3", "吸嘴4", "吸嘴5", "吸嘴6",
            "吸嘴7", "吸嘴8", "吸嘴9", "吸嘴10", "吸嘴11"
        };

        /// <summary>
        /// 吸嘴資料表
        /// </summary>
        public List<NozzleObject> NozzleList { get; set; }

        /********************
         * .ini檔作業
         *******************/
        private readonly string FileName = FileList.INI_NOZZLE;
        private string sectionName;

        /// <summary>
        /// 建構函式
        /// </summary>
        public Nozzle()
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

            // 基準吸嘴
            sectionName = "DatumNozzle";
            iniFile.WriteIniFile(sectionName, "DatumNozzleId", DatumNozzleId);

            // 各吸嘴
            for (int nozzle = 1; nozzle <= MAX_NOZZLE; nozzle++)
            {
                sectionName = "Nozzle" + nozzle.ToString();
                int idx = nozzle - 1;

                iniFile.WriteIniFile(sectionName, "Name", NozzleList[idx].Name);

                iniFile.WriteIniFile(sectionName, "Position_X", NozzleList[idx].Position.X);
                iniFile.WriteIniFile(sectionName, "Position_Y", NozzleList[idx].Position.Y);
                iniFile.WriteIniFile(sectionName, "Position_Z", NozzleList[idx].Position.Z);

                iniFile.WriteIniFile(sectionName, "Pulse_X", NozzleList[idx].Pulse.X);
                iniFile.WriteIniFile(sectionName, "Pulse_Y", NozzleList[idx].Pulse.Y);
                iniFile.WriteIniFile(sectionName, "Pulse_Z", NozzleList[idx].Pulse.Z);

                iniFile.WriteIniFile(sectionName, "Encoder_X", NozzleList[idx].Encoder.X);
                iniFile.WriteIniFile(sectionName, "Encoder_Y", NozzleList[idx].Encoder.Y);
                iniFile.WriteIniFile(sectionName, "Encoder_Z", NozzleList[idx].Encoder.Z);

                iniFile.WriteIniFile(sectionName, "Distance_X", NozzleList[idx].DistanceToMoveCamera.X);
                iniFile.WriteIniFile(sectionName, "Distance_Y", NozzleList[idx].DistanceToMoveCamera.Y);

                iniFile.WriteIniFile(sectionName, "UltraHighEncMarker_X", NozzleList[idx].UltraHighEncMarker.X);
                iniFile.WriteIniFile(sectionName, "UltraHighEncMarker_Y", NozzleList[idx].UltraHighEncMarker.Y);
                iniFile.WriteIniFile(sectionName, "HighEncMarker_X", NozzleList[idx].HighEncMarker.X);
                iniFile.WriteIniFile(sectionName, "HighEncMarker_Y", NozzleList[idx].HighEncMarker.Y);
                iniFile.WriteIniFile(sectionName, "MiddleEncMarker_X", NozzleList[idx].MiddleEncMarker.X);
                iniFile.WriteIniFile(sectionName, "MiddleEncMarker_Y", NozzleList[idx].MiddleEncMarker.Y);

                iniFile.WriteIniFile(sectionName, "UltraHighTimeMarker_X", NozzleList[idx].UltraHighTimeMarker.X);
                iniFile.WriteIniFile(sectionName, "UltraHighTimeMarker_Y", NozzleList[idx].UltraHighTimeMarker.Y);
                iniFile.WriteIniFile(sectionName, "HighTimeMarker_X", NozzleList[idx].HighTimeMarker.X);
                iniFile.WriteIniFile(sectionName, "HighTimeMarker_Y", NozzleList[idx].HighTimeMarker.Y);
                iniFile.WriteIniFile(sectionName, "MiddleTimeMarker_X", NozzleList[idx].MiddleTimeMarker.X);
                iniFile.WriteIniFile(sectionName, "MiddleTimeMarker_Y", NozzleList[idx].MiddleTimeMarker.Y);

                iniFile.WriteIniFile(sectionName, "MeasureHeight", NozzleList[idx].MeasureHeight);
            }
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

            // 定義物件
            NozzleList = new List<NozzleObject>();

            // 基準吸嘴
            sectionName = "DatumNozzle";
            DatumNozzleId = Enum.Parse<ENozzleId>(iniFile.ReadIniFile(sectionName, "DatumNozzleId", "0"));

            // 各吸嘴
            for (int nozzle = 1; nozzle <= MAX_NOZZLE; nozzle++)
            {
                sectionName = "Nozzle" + nozzle.ToString();
                NozzleObject ni = new NozzleObject
                {
                    Name = iniFile.ReadIniFile(sectionName, "Name", $"吸嘴{nozzle}"),

                    Position = new PointXYZ
                    {
                        X = double.Parse(iniFile.ReadIniFile(sectionName, "Position_X", "0")),
                        Y = double.Parse(iniFile.ReadIniFile(sectionName, "Position_Y", "0")),
                        Z = double.Parse(iniFile.ReadIniFile(sectionName, "Position_Z", "0"))
                    },
                    Pulse = new LongPointXYZ
                    {
                        X = long.Parse(iniFile.ReadIniFile(sectionName, "Pulse_X", "0")),
                        Y = long.Parse(iniFile.ReadIniFile(sectionName, "Pulse_Y", "0")),
                        Z = long.Parse(iniFile.ReadIniFile(sectionName, "Pulse_Z", "0"))
                    },
                    Encoder = new IntPointXYZ
                    {
                        X = int.Parse(iniFile.ReadIniFile(sectionName, "Encoder_X", "0")),
                        Y = int.Parse(iniFile.ReadIniFile(sectionName, "Encoder_Y", "0")),
                        Z = int.Parse(iniFile.ReadIniFile(sectionName, "Encoder_Z", "0"))
                    },
                    DistanceToMoveCamera = new PointXY
                    {
                        X = double.Parse(iniFile.ReadIniFile(sectionName, "Distance_X", "0")),
                        Y = double.Parse(iniFile.ReadIniFile(sectionName, "Distance_Y", "0"))
                    },

                    UltraHighEncMarker = new IntPointXY
                    {
                        X = int.Parse(iniFile.ReadIniFile(sectionName, "UltraHighEncMarker_X", "0")),
                        Y = int.Parse(iniFile.ReadIniFile(sectionName, "UltraHighEncMarker_Y", "0"))
                    },
                    HighEncMarker = new IntPointXY
                    {
                        X = int.Parse(iniFile.ReadIniFile(sectionName, "HighEncMarker_X", "0")),
                        Y = int.Parse(iniFile.ReadIniFile(sectionName, "HighEncMarker_Y", "0"))
                    },
                    MiddleEncMarker = new IntPointXY
                    {
                        X = int.Parse(iniFile.ReadIniFile(sectionName, "MiddleEncMarker_X", "0")),
                        Y = int.Parse(iniFile.ReadIniFile(sectionName, "MiddleEncMarker_Y", "0"))
                    },

                    UltraHighTimeMarker = new PointXY
                    {
                        X = double.Parse(iniFile.ReadIniFile(sectionName, "UltraHighTimeMarker_X", "0")),
                        Y = double.Parse(iniFile.ReadIniFile(sectionName, "UltraHighTimeMarker_Y", "0"))
                    },
                    HighTimeMarker = new PointXY
                    {
                        X = double.Parse(iniFile.ReadIniFile(sectionName, "HighTimeMarker_X", "0")),
                        Y = double.Parse(iniFile.ReadIniFile(sectionName, "HighTimeMarker_Y", "0"))
                    },
                    MiddleTimeMarker = new PointXY
                    {
                        X = double.Parse(iniFile.ReadIniFile(sectionName, "MiddleTimeMarker_X", "0")),
                        Y = double.Parse(iniFile.ReadIniFile(sectionName, "MiddleTimeMarker_Y", "0"))
                    },

                    MeasureHeight = double.Parse(iniFile.ReadIniFile(sectionName, "MeasureHeight", "0"))
                };

                NozzleList.Add(ni);
            }
        }

        /********************
         * 參數作業
         *******************/
        /// <summary>
        /// 取得指定吸嘴與基準吸嘴間的距離
        /// </summary>
        /// <param name="nozzleId">指定吸嘴ID</param>
        /// <returns>距離(X,Y)</returns>
        public (double X, double Y) GetDistanceFromDatum(ENozzleId nozzleId)
        {
            var noz = NozzleList[(int)nozzleId];

            return (machine.DatumPoint1.Position.X + noz.Position.X,
                    machine.DatumPoint1.Position.Y + noz.Position.Y);
        }

        /********************
         * 吸嘴上升動作
         *******************/
        /// <summary>
        /// 全部吸嘴上升
        /// </summary>
        public void ALlNozzleUp()
        {
            epcio.RioOutput(epcio.Nozzle01_Cylinder, false);
            epcio.RioOutput(epcio.Nozzle02_Cylinder, false);
            epcio.RioOutput(epcio.Nozzle03_Cylinder, false);
            epcio.RioOutput(epcio.Nozzle04_Cylinder, false);
            epcio.RioOutput(epcio.Nozzle05_Cylinder, false);
            epcio.RioOutput(epcio.Nozzle06_Cylinder, false);
            epcio.RioOutput(epcio.Nozzle07_Cylinder, false);
            epcio.RioOutput(epcio.Nozzle08_Cylinder, false);
            epcio.RioOutput(epcio.Nozzle09_Cylinder, false);
            epcio.RioOutput(epcio.Nozzle10_Cylinder, false);
            epcio.RioOutput(epcio.Nozzle11_Cylinder, false);
        }

        /// <summary>
        /// 指定吸嘴的氣缸上升
        /// </summary>
        /// <param name="nozzleIdList">吸嘴ID列表</param>
        public void NozzleUp(ENozzleId[] nozzleIdList)
        {
            foreach (ENozzleId noz in nozzleIdList)
                NozzleUp(noz);
        }

        /// <summary>
        /// 指定吸嘴的氣缸上升
        /// </summary>
        /// <param name="nozzleId">吸嘴ID</param>
        public void NozzleUp(ENozzleId nozzleId)
        {
            switch (nozzleId)
            {
                case ENozzleId.Nozzle01:
                    epcio.RioOutput(epcio.Nozzle01_Cylinder, false);
                    break;
                case ENozzleId.Nozzle02:
                    epcio.RioOutput(epcio.Nozzle02_Cylinder, false);
                    break;
                case ENozzleId.Nozzle03:
                    epcio.RioOutput(epcio.Nozzle03_Cylinder, false);
                    break;
                case ENozzleId.Nozzle04:
                    epcio.RioOutput(epcio.Nozzle04_Cylinder, false);
                    break;
                case ENozzleId.Nozzle05:
                    epcio.RioOutput(epcio.Nozzle05_Cylinder, false);
                    break;
                case ENozzleId.Nozzle06:
                    epcio.RioOutput(epcio.Nozzle06_Cylinder, false);
                    break;
                case ENozzleId.Nozzle07:
                    epcio.RioOutput(epcio.Nozzle07_Cylinder, false);
                    break;
                case ENozzleId.Nozzle08:
                    epcio.RioOutput(epcio.Nozzle08_Cylinder, false);
                    break;
                case ENozzleId.Nozzle09:
                    epcio.RioOutput(epcio.Nozzle09_Cylinder, false);
                    break;
                case ENozzleId.Nozzle10:
                    epcio.RioOutput(epcio.Nozzle10_Cylinder, false);
                    break;
                case ENozzleId.Nozzle11:
                    epcio.RioOutput(epcio.Nozzle11_Cylinder, false);
                    break;
            }
        }

        /********************
         * 吸嘴下降動作
         *******************/
        /// <summary>
        /// 全部吸嘴下降
        /// </summary>
        public void ALlNozzleDown()
        {
            epcio.RioOutput(epcio.Nozzle01_Cylinder, true);
            epcio.RioOutput(epcio.Nozzle02_Cylinder, true);
            epcio.RioOutput(epcio.Nozzle03_Cylinder, true);
            epcio.RioOutput(epcio.Nozzle04_Cylinder, true);
            epcio.RioOutput(epcio.Nozzle05_Cylinder, true);
            epcio.RioOutput(epcio.Nozzle06_Cylinder, true);
            epcio.RioOutput(epcio.Nozzle07_Cylinder, true);
            epcio.RioOutput(epcio.Nozzle08_Cylinder, true);
            epcio.RioOutput(epcio.Nozzle09_Cylinder, true);
            epcio.RioOutput(epcio.Nozzle10_Cylinder, true);
            epcio.RioOutput(epcio.Nozzle11_Cylinder, true);
        }

        /// <summary>
        /// 指定吸嘴的氣缸下降
        /// </summary>
        /// <param name="nozzleIdList">吸嘴ID列表</param>
        public void NozzleDown(ENozzleId[] nozzleIdList)
        {
            foreach (ENozzleId noz in nozzleIdList)
                NozzleDown(noz);
        }

        /// <summary>
        /// 指定吸嘴的氣缸下降
        /// </summary>
        /// <param name="nozzleId">吸嘴ID</param>
        public void NozzleDown(ENozzleId nozzleId)
        {
            switch (nozzleId)
            {
                case ENozzleId.Nozzle01:
                    epcio.RioOutput(epcio.Nozzle01_Cylinder, true);
                    break;
                case ENozzleId.Nozzle02:
                    epcio.RioOutput(epcio.Nozzle02_Cylinder, true);
                    break;
                case ENozzleId.Nozzle03:
                    epcio.RioOutput(epcio.Nozzle03_Cylinder, true);
                    break;
                case ENozzleId.Nozzle04:
                    epcio.RioOutput(epcio.Nozzle04_Cylinder, true);
                    break;
                case ENozzleId.Nozzle05:
                    epcio.RioOutput(epcio.Nozzle05_Cylinder, true);
                    break;
                case ENozzleId.Nozzle06:
                    epcio.RioOutput(epcio.Nozzle06_Cylinder, true);
                    break;
                case ENozzleId.Nozzle07:
                    epcio.RioOutput(epcio.Nozzle07_Cylinder, true);
                    break;
                case ENozzleId.Nozzle08:
                    epcio.RioOutput(epcio.Nozzle08_Cylinder, true);
                    break;
                case ENozzleId.Nozzle09:
                    epcio.RioOutput(epcio.Nozzle09_Cylinder, true);
                    break;
                case ENozzleId.Nozzle10:
                    epcio.RioOutput(epcio.Nozzle10_Cylinder, true);
                    break;
                case ENozzleId.Nozzle11:
                    epcio.RioOutput(epcio.Nozzle11_Cylinder, true);
                    break;
            }
        }

        /********************
         * 吸嘴上升位置檢查
         *******************/
        /// <summary>
        /// 檢查全部吸嘴的氣缸是否在上升位置
        /// </summary>
        /// <returns>確認結果</returns>
        public bool IsAllNozzleUp()
        {
            if (!epcio.Nozzle01_UpLs.Value)
                return false;

            if (!epcio.Nozzle02_UpLs.Value)
                return false;

            if (!epcio.Nozzle03_UpLs.Value)
                return false;

            if (!epcio.Nozzle04_UpLs.Value)
                return false;

            if (!epcio.Nozzle05_UpLs.Value)
                return false;

            if (!epcio.Nozzle06_UpLs.Value)
                return false;

            if (!epcio.Nozzle07_UpLs.Value)
                return false;

            if (!epcio.Nozzle08_UpLs.Value)
                return false;

            if (!epcio.Nozzle09_UpLs.Value)
                return false;

            if (!epcio.Nozzle10_UpLs.Value)
                return false;

            if (!epcio.Nozzle11_UpLs.Value)
                return false;

            return true;
        }

        /// <summary>
        /// 檢查指定吸嘴的氣缸是否在上升位置
        /// </summary>
        /// <param name="nozzleIdList">吸嘴ID列表</param>
        /// <returns>確認結果</returns>
        public bool IsNozzleUp(ENozzleId[] nozzleIdList)
        {
            foreach (ENozzleId noz in nozzleIdList)
                if (!IsNozzleUp(noz))
                    return false;

            return true;
        }

        /// <summary>
        /// 檢查指定吸嘴的氣缸是否在上升位置
        /// </summary>
        /// <param name="nozzleId">吸嘴ID</param>
        /// <returns>確認結果</returns>
        public bool IsNozzleUp(ENozzleId nozzleId)
        {
            if (nozzleId == ENozzleId.Nozzle01)
                if (!epcio.Nozzle01_UpLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle02)
                if (!epcio.Nozzle02_UpLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle03)
                if (!epcio.Nozzle03_UpLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle04)
                if (!epcio.Nozzle04_UpLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle05)
                if (!epcio.Nozzle05_UpLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle06)
                if (!epcio.Nozzle06_UpLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle07)
                if (!epcio.Nozzle07_UpLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle08)
                if (!epcio.Nozzle08_UpLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle09)
                if (!epcio.Nozzle09_UpLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle10)
                if (!epcio.Nozzle10_UpLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle11)
                if (!epcio.Nozzle11_UpLs.Value)
                    return false;

            return true;
        }

        /********************
         * 吸嘴下降位置檢查
         *******************/
        /// <summary>
        /// 檢查全部吸嘴的氣缸是否在下降位置
        /// </summary>
        /// <returns>true: 確認結果</returns>
        public bool IsAllNozzleDown()
        {
            if (!epcio.Nozzle01_DownLs.Value)
                return false;

            if (!epcio.Nozzle02_DownLs.Value)
                return false;

            if (!epcio.Nozzle03_DownLs.Value)
                return false;

            if (!epcio.Nozzle04_DownLs.Value)
                return false;

            if (!epcio.Nozzle05_DownLs.Value)
                return false;

            if (!epcio.Nozzle06_DownLs.Value)
                return false;

            if (!epcio.Nozzle07_DownLs.Value)
                return false;

            if (!epcio.Nozzle08_DownLs.Value)
                return false;

            if (!epcio.Nozzle09_DownLs.Value)
                return false;

            if (!epcio.Nozzle10_DownLs.Value)
                return false;

            if (!epcio.Nozzle11_DownLs.Value)
                return false;

            return true;
        }

        /// <summary>
        /// 檢查指定吸嘴的氣缸是否在下降位置
        /// </summary>
        /// <param name="nozzleIdList">吸嘴ID列表</param>
        /// <returns>確認結果</returns>
        public bool IsNozzleDown(ENozzleId[] nozzleIdList)
        {
            foreach (ENozzleId noz in nozzleIdList)
                if (!IsNozzleUp(noz))
                    return false;

            return true;
        }

        /// <summary>
        /// 檢查指定吸嘴的氣缸是否在下降位置
        /// </summary>
        /// <param name="nozzleId">吸嘴ID</param>
        /// <returns>確認結果</returns>
        public bool IsNozzleDown(ENozzleId nozzleId)
        {
            if (nozzleId == ENozzleId.Nozzle01)
                if (!epcio.Nozzle01_DownLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle02)
                if (!epcio.Nozzle02_DownLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle03)
                if (!epcio.Nozzle03_DownLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle04)
                if (!epcio.Nozzle04_DownLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle05)
                if (!epcio.Nozzle05_DownLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle06)
                if (!epcio.Nozzle06_DownLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle07)
                if (!epcio.Nozzle07_DownLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle08)
                if (!epcio.Nozzle08_DownLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle09)
                if (!epcio.Nozzle09_DownLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle10)
                if (!epcio.Nozzle10_DownLs.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle11)
                if (!epcio.Nozzle11_DownLs.Value)
                    return false;

            return true;
        }

        /********************
         * 吸嘴位置等待
         *******************/
        /// <summary>
        /// 等待全部吸嘴下降到上位置Sensor
        /// </summary>
        public async Task WaitingForAllNozzleUp()
        {
            await WaitingForNozzleUp(ENozzleId.Nozzle01);
            await WaitingForNozzleUp(ENozzleId.Nozzle02);
            await WaitingForNozzleUp(ENozzleId.Nozzle03);
            await WaitingForNozzleUp(ENozzleId.Nozzle04);
            await WaitingForNozzleUp(ENozzleId.Nozzle05);
            await WaitingForNozzleUp(ENozzleId.Nozzle06);
            await WaitingForNozzleUp(ENozzleId.Nozzle07);
            await WaitingForNozzleUp(ENozzleId.Nozzle08);
            await WaitingForNozzleUp(ENozzleId.Nozzle09);
            await WaitingForNozzleUp(ENozzleId.Nozzle10);
            await WaitingForNozzleUp(ENozzleId.Nozzle11);
        }

        /// <summary>
        /// 等待指定吸嘴下降到上位置Sensor
        /// </summary>
        /// <param name="nozzleIdlist">吸嘴ID列表</param>
        public async Task WaitingForNozzleUp(ENozzleId[] nozzleIdlist)
        {
            foreach (ENozzleId noz in nozzleIdlist)
                await WaitingForNozzleUp(noz);
        }

        /// <summary>
        /// 等待指定吸嘴下降到上位置Sensor
        /// </summary>
        /// <param name="nozzleId">吸嘴ID</param>
        public async Task WaitingForNozzleUp(ENozzleId nozzleId)
        {
            await Task.Run(() =>
            {
                if (nozzleId == ENozzleId.Nozzle01)
                    while (!epcio.Nozzle01_UpLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle02)
                    while (!epcio.Nozzle02_UpLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle03)
                    while (!epcio.Nozzle03_UpLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle04)
                    while (!epcio.Nozzle04_UpLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle05)
                    while (!epcio.Nozzle05_UpLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle06)
                    while (!epcio.Nozzle06_UpLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle07)
                    while (!epcio.Nozzle07_UpLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle08)
                    while (!epcio.Nozzle08_UpLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle09)
                    while (!epcio.Nozzle09_UpLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle10)
                    while (!epcio.Nozzle10_UpLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle11)
                    while (!epcio.Nozzle11_UpLs.Value)
                    { }
            });
        }

        /********************
         * 吸嘴位置等待
         *******************/
        /// <summary>
        /// 等待全部吸嘴下降到下位置Sensor
        /// </summary>
        public async Task WaitingForAllNozzleDown()
        {
            await WaitingForNozzleDown(ENozzleId.Nozzle01);
            await WaitingForNozzleDown(ENozzleId.Nozzle02);
            await WaitingForNozzleDown(ENozzleId.Nozzle03);
            await WaitingForNozzleDown(ENozzleId.Nozzle04);
            await WaitingForNozzleDown(ENozzleId.Nozzle05);
            await WaitingForNozzleDown(ENozzleId.Nozzle06);
            await WaitingForNozzleDown(ENozzleId.Nozzle07);
            await WaitingForNozzleDown(ENozzleId.Nozzle08);
            await WaitingForNozzleDown(ENozzleId.Nozzle09);
            await WaitingForNozzleDown(ENozzleId.Nozzle10);
            await WaitingForNozzleDown(ENozzleId.Nozzle11);
        }

        /// <summary>
        /// 等待指定吸嘴下降到下位置Sensor
        /// </summary>
        /// <param name="nozzleIdlist">吸嘴ID列表</param>
        public async Task WaitingForNozzleDown(ENozzleId[] nozzleIdlist)
        {
            foreach (ENozzleId noz in nozzleIdlist)
                await WaitingForNozzleDown(noz);
        }

        /// <summary>
        /// 等待指定吸嘴下降到下位置Sensor
        /// </summary>
        /// <param name="nozzleId">吸嘴ID</param>
        public async Task WaitingForNozzleDown(ENozzleId nozzleId)
        {
            await Task.Run(() =>
            {
                if (nozzleId == ENozzleId.Nozzle01)
                    while (!epcio.Nozzle01_DownLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle02)
                    while (!epcio.Nozzle02_DownLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle03)
                    while (!epcio.Nozzle03_DownLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle04)
                    while (!epcio.Nozzle04_DownLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle05)
                    while (!epcio.Nozzle05_DownLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle06)
                    while (!epcio.Nozzle06_DownLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle07)
                    while (!epcio.Nozzle07_DownLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle08)
                    while (!epcio.Nozzle08_DownLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle09)
                    while (!epcio.Nozzle09_DownLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle10)
                    while (!epcio.Nozzle10_DownLs.Value)
                    { }

                if (nozzleId == ENozzleId.Nozzle11)
                    while (!epcio.Nozzle11_DownLs.Value)
                    { }
            });
        }

        /********************
         * 吸嘴真空動作
         *******************/
        /// <summary>
        /// 全部吸嘴真空全開
        /// </summary>
        public void AllNozzleVaccumOn()
        {
            NozzleVaccumOn(ENozzleId.Nozzle01);
            NozzleVaccumOn(ENozzleId.Nozzle02);
            NozzleVaccumOn(ENozzleId.Nozzle03);
            NozzleVaccumOn(ENozzleId.Nozzle04);
            NozzleVaccumOn(ENozzleId.Nozzle05);
            NozzleVaccumOn(ENozzleId.Nozzle06);
            NozzleVaccumOn(ENozzleId.Nozzle07);
            NozzleVaccumOn(ENozzleId.Nozzle08);
            NozzleVaccumOn(ENozzleId.Nozzle09);
            NozzleVaccumOn(ENozzleId.Nozzle10);
            NozzleVaccumOn(ENozzleId.Nozzle11);
        }

        /// <summary>
        /// 開啟指定吸嘴的真空吸氣、關閉吐氣
        /// </summary>
        /// <param name="nozzleIdList">吸嘴ID列表</param>
        public void NozzleVaccumOn(ENozzleId[] nozzleIdList)
        {
            foreach (ENozzleId noz in nozzleIdList)
                NozzleVaccumOn(noz);
        }

        /// <summary>
        /// 開啟指定吸嘴的真空吸氣、關閉吐氣
        /// </summary>
        /// <param name="nozzleId">吸嘴ID</param>
        public void NozzleVaccumOn(ENozzleId nozzleId)
        {
            switch (nozzleId)
            {
                case ENozzleId.Nozzle01:
                    epcio.RioOutput(epcio.Nozzle01_Vacuum, true);
                    epcio.RioOutput(epcio.Nozzle01_Blow, false);
                    break;
                case ENozzleId.Nozzle02:
                    epcio.RioOutput(epcio.Nozzle02_Vacuum, true);
                    epcio.RioOutput(epcio.Nozzle02_Blow, false);
                    break;
                case ENozzleId.Nozzle03:
                    epcio.RioOutput(epcio.Nozzle03_Vacuum, true);
                    epcio.RioOutput(epcio.Nozzle03_Blow, false);
                    break;
                case ENozzleId.Nozzle04:
                    epcio.RioOutput(epcio.Nozzle04_Vacuum, true);
                    epcio.RioOutput(epcio.Nozzle04_Blow, false);
                    break;
                case ENozzleId.Nozzle05:
                    epcio.RioOutput(epcio.Nozzle05_Vacuum, true);
                    epcio.RioOutput(epcio.Nozzle05_Blow, false);
                    break;
                case ENozzleId.Nozzle06:
                    epcio.RioOutput(epcio.Nozzle06_Vacuum, true);
                    epcio.RioOutput(epcio.Nozzle06_Blow, false);
                    break;
                case ENozzleId.Nozzle07:
                    epcio.RioOutput(epcio.Nozzle07_Vacuum, true);
                    epcio.RioOutput(epcio.Nozzle07_Blow, false);
                    break;
                case ENozzleId.Nozzle08:
                    epcio.RioOutput(epcio.Nozzle08_Vacuum, true);
                    epcio.RioOutput(epcio.Nozzle08_Blow, false);
                    break;
                case ENozzleId.Nozzle09:
                    epcio.RioOutput(epcio.Nozzle09_Vacuum, true);
                    epcio.RioOutput(epcio.Nozzle09_Blow, false);
                    break;
                case ENozzleId.Nozzle10:
                    epcio.RioOutput(epcio.Nozzle10_Vacuum, true);
                    epcio.RioOutput(epcio.Nozzle10_Blow, false);
                    break;
                case ENozzleId.Nozzle11:
                    epcio.RioOutput(epcio.Nozzle11_Vacuum, true);
                    epcio.RioOutput(epcio.Nozzle11_Blow, false);
                    break;
            }
        }

        /********************
         * 吸嘴吐氣動作
         *******************/
        /// <summary>
        /// 全部吸嘴吐氣全開
        /// </summary>
        public void AllNozzleBlowOn()
        {
            NozzleBlowOn(ENozzleId.Nozzle01);
            NozzleBlowOn(ENozzleId.Nozzle02);
            NozzleBlowOn(ENozzleId.Nozzle03);
            NozzleBlowOn(ENozzleId.Nozzle04);
            NozzleBlowOn(ENozzleId.Nozzle05);
            NozzleBlowOn(ENozzleId.Nozzle06);
            NozzleBlowOn(ENozzleId.Nozzle07);
            NozzleBlowOn(ENozzleId.Nozzle08);
            NozzleBlowOn(ENozzleId.Nozzle09);
            NozzleBlowOn(ENozzleId.Nozzle10);
            NozzleBlowOn(ENozzleId.Nozzle11);
        }

        /// <summary>
        /// 開啟指定吸嘴的吐氣、關閉真空吸氣
        /// </summary>
        /// <param name="nozzleIdList">吸嘴ID列表</param>
        public void NozzleBlowOn(ENozzleId[] nozzleIdList)
        {
            foreach (ENozzleId noz in nozzleIdList)
                NozzleBlowOn(noz);
        }

        /// <summary>
        /// 開啟指定吸嘴的吐氣、關閉真空吸氣
        /// </summary>
        /// <param name="nozzleId">吸嘴ID</param>
        public void NozzleBlowOn(ENozzleId nozzleId)
        {
            switch (nozzleId)
            {
                case ENozzleId.Nozzle01:
                    epcio.RioOutput(epcio.Nozzle01_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle01_Blow, true);
                    break;
                case ENozzleId.Nozzle02:
                    epcio.RioOutput(epcio.Nozzle02_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle02_Blow, true);
                    break;
                case ENozzleId.Nozzle03:
                    epcio.RioOutput(epcio.Nozzle03_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle03_Blow, true);
                    break;
                case ENozzleId.Nozzle04:
                    epcio.RioOutput(epcio.Nozzle04_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle04_Blow, true);
                    break;
                case ENozzleId.Nozzle05:
                    epcio.RioOutput(epcio.Nozzle05_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle05_Blow, true);
                    break;
                case ENozzleId.Nozzle06:
                    epcio.RioOutput(epcio.Nozzle06_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle06_Blow, true);
                    break;
                case ENozzleId.Nozzle07:
                    epcio.RioOutput(epcio.Nozzle07_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle07_Blow, true);
                    break;
                case ENozzleId.Nozzle08:
                    epcio.RioOutput(epcio.Nozzle08_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle08_Blow, true);
                    break;
                case ENozzleId.Nozzle09:
                    epcio.RioOutput(epcio.Nozzle09_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle09_Blow, true);
                    break;
                case ENozzleId.Nozzle10:
                    epcio.RioOutput(epcio.Nozzle10_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle10_Blow, true);
                    break;
                case ENozzleId.Nozzle11:
                    epcio.RioOutput(epcio.Nozzle11_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle11_Blow, true);
                    break;
            }
        }

        /********************
         * 真空、吐氣全關閉
         *******************/
        /// <summary>
        /// 全部吸嘴真空/吐氣全關
        /// </summary>
        public void AllNozzleOff()
        {
            NozzleOff(ENozzleId.Nozzle01);
            NozzleOff(ENozzleId.Nozzle02);
            NozzleOff(ENozzleId.Nozzle03);
            NozzleOff(ENozzleId.Nozzle04);
            NozzleOff(ENozzleId.Nozzle05);
            NozzleOff(ENozzleId.Nozzle06);
            NozzleOff(ENozzleId.Nozzle07);
            NozzleOff(ENozzleId.Nozzle08);
            NozzleOff(ENozzleId.Nozzle09);
            NozzleOff(ENozzleId.Nozzle10);
            NozzleOff(ENozzleId.Nozzle11);
        }

        /// <summary>
        /// 關閉指定吸嘴的真空吸氣及吐氣
        /// </summary>
        /// <param name="nozzleIdList">吸嘴ID列表</param>
        public void NozzleOff(ENozzleId[] nozzleIdList)
        {
            foreach (ENozzleId noz in nozzleIdList)
                NozzleOff(noz);
        }

        /// <summary>
        /// 關閉指定吸嘴的真空吸氣及吐氣
        /// </summary>
        /// <param name="nozzleId">吸嘴ID</param>
        public void NozzleOff(ENozzleId nozzleId)
        {
            switch (nozzleId)
            {
                case ENozzleId.Nozzle01:
                    epcio.RioOutput(epcio.Nozzle01_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle01_Blow, false);
                    break;
                case ENozzleId.Nozzle02:
                    epcio.RioOutput(epcio.Nozzle02_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle02_Blow, false);
                    break;
                case ENozzleId.Nozzle03:
                    epcio.RioOutput(epcio.Nozzle03_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle03_Blow, false);
                    break;
                case ENozzleId.Nozzle04:
                    epcio.RioOutput(epcio.Nozzle04_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle04_Blow, false);
                    break;
                case ENozzleId.Nozzle05:
                    epcio.RioOutput(epcio.Nozzle05_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle05_Blow, false);
                    break;
                case ENozzleId.Nozzle06:
                    epcio.RioOutput(epcio.Nozzle06_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle06_Blow, false);
                    break;
                case ENozzleId.Nozzle07:
                    epcio.RioOutput(epcio.Nozzle07_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle07_Blow, false);
                    break;
                case ENozzleId.Nozzle08:
                    epcio.RioOutput(epcio.Nozzle08_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle08_Blow, false);
                    break;
                case ENozzleId.Nozzle09:
                    epcio.RioOutput(epcio.Nozzle09_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle09_Blow, false);
                    break;
                case ENozzleId.Nozzle10:
                    epcio.RioOutput(epcio.Nozzle10_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle10_Blow, false);
                    break;
                case ENozzleId.Nozzle11:
                    epcio.RioOutput(epcio.Nozzle11_Vacuum, false);
                    epcio.RioOutput(epcio.Nozzle11_Blow, false);
                    break;
            }
        }

        /********************
         * 吸嘴真空檢查
         *******************/
        /// <summary>
        /// 檢查指定吸嘴是否真空有開啟
        /// </summary>
        /// <returns>檢查結果</returns>
        public bool IsAllNozzleVaccumOn()
        {
            if (!epcio.Nozzle01_Vacuum.Value)
                return false;

            if (!epcio.Nozzle02_Vacuum.Value)
                return false;

            if (!epcio.Nozzle03_Vacuum.Value)
                return false;

            if (!epcio.Nozzle04_Vacuum.Value)
                return false;

            if (!epcio.Nozzle05_Vacuum.Value)
                return false;

            if (!epcio.Nozzle06_Vacuum.Value)
                return false;

            if (!epcio.Nozzle07_Vacuum.Value)
                return false;

            if (!epcio.Nozzle08_Vacuum.Value)
                return false;

            if (!epcio.Nozzle09_Vacuum.Value)
                return false;

            if (!epcio.Nozzle10_Vacuum.Value)
                return false;

            if (!epcio.Nozzle11_Vacuum.Value)
                return false;

            return true;
        }

        /// <summary>
        /// 檢查指定吸嘴是否真空有開啟
        /// </summary>
        /// <param name="nozzleIdList">吸嘴ID列表</param>
        /// <returns>檢查結果</returns>
        public bool IsNozzleVaccumOn(ENozzleId[] nozzleIdList)
        {
            foreach (ENozzleId noz in nozzleIdList)
                if (!IsNozzleVaccumOn(noz))
                    return false;

            return true;
        }

        /// <summary>
        /// 檢查指定吸嘴是否真空有開啟
        /// </summary>
        /// <param name="nozzleId">吸嘴ID</param>
        /// <returns>檢查結果</returns>
        public bool IsNozzleVaccumOn(ENozzleId nozzleId)
        {
            if (nozzleId == ENozzleId.Nozzle01)
                if (!epcio.Nozzle01_Vacuum.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle02)
                if (!epcio.Nozzle02_Vacuum.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle03)
                if (!epcio.Nozzle03_Vacuum.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle04)
                if (!epcio.Nozzle04_Vacuum.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle05)
                if (!epcio.Nozzle05_Vacuum.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle06)
                if (!epcio.Nozzle06_Vacuum.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle07)
                if (!epcio.Nozzle07_Vacuum.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle08)
                if (!epcio.Nozzle08_Vacuum.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle09)
                if (!epcio.Nozzle09_Vacuum.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle10)
                if (!epcio.Nozzle10_Vacuum.Value)
                    return false;

            if (nozzleId == ENozzleId.Nozzle11)
                if (!epcio.Nozzle11_Vacuum.Value)
                    return false;

            return true;
        }
    }
}
