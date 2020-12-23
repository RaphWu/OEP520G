/****************************
 * 夾爪參數檔
 ***************************/
using CSharpCore;
using CSharpCore.FileSystem;
using EPCIO;
using OEP520G.Automatic;
using OEP520G.Core;
using OEP520G.Functions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OEP520G.Parameter
{
    public class Clamp
    {
        // Singleton單例模式
        private static readonly Lazy<Clamp> lazy = new Lazy<Clamp>(() => new Clamp());
        public static Clamp Instance => lazy.Value;

        private readonly Epcio epcio = Epcio.Instance;

        // 常數
        public const int MAX_CLAMP = 2;

        /// <summary>
        /// 夾爪1
        /// </summary>
        public ClampObject Clamp1 { get; set; }

        /// <summary>
        /// 夾爪2
        /// </summary>
        public ClampObject Clamp2 { get; set; }

        /// <summary>
        /// 夾爪列表
        /// </summary>
        public List<ClampObject> ClampList { get; set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        private Clamp()
        {
            Clamp1 = new ClampObject(EClampId.Clamp1, "");
            Clamp2 = new ClampObject(EClampId.Clamp2, "");

            ClampList = new List<ClampObject>
            {
                Clamp1,
                Clamp2
            };

            ReadParameter();
        }

        /********************
         * 檔案作業
         *******************/
        private readonly string FileName = FileList.INI_CLAMP;
        private string sectionName;

        /// <summary>
        /// 將參數寫入參數檔
        /// </summary>
        /// <example>
        /// iniFile.WriteIniFile(sectionName,"[屬性名稱]", [屬性值]));
        /// </example>
        public void WriteParameter()
        {
            IniFile iniFile = new IniFile(FileName);

            // 各夾爪
            foreach (ClampObject clamp in ClampList)
            {
                string clampSecName = $"{clamp.Id}_";

                //sectionName = $"{clampSecName}BasicData";
                //iniFile.WriteIniFile(sectionName, "Id", clamp.Id);
                //iniFile.WriteIniFile(sectionName, "Title", clamp.Title);

                // 與X軸轉換比例
                sectionName = $"{clampSecName}ConvertToMoveCamera";
                iniFile.WriteIniFile(sectionName, "X", clamp.ConvertToMoveCamera.X);
                iniFile.WriteIniFile(sectionName, "Y", clamp.ConvertToMoveCamera.Y);

                // Stage取放料點
                sectionName = $"{clampSecName}StageCoordination";
                iniFile.WriteIniFile(sectionName, "X", clamp.StageCoordination.X);
                iniFile.WriteIniFile(sectionName, "Y", clamp.StageCoordination.Y);

                // 等待時間
                sectionName = $"{clampSecName}DelayTime";
                iniFile.WriteIniFile(sectionName, "DelayTime1", clamp.DelayTime1);
                iniFile.WriteIniFile(sectionName, "DelayTime2", clamp.DelayTime2);
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

            // 各夾爪
            foreach (ClampObject clamp in ClampList)
            {
                string clampSecName = $"{clamp.Id}_";

                //sectionName = $"{clampSecName}BasicData";
                //clamp.Id = Enum.Parse<EClampId>(iniFile.ReadIniFile(sectionName, "Id", clamp.Id.ToString()));
                //clamp.Title = iniFile.ReadIniFile(sectionName, "Title", "夾爪");

                // 相對第一基準點座標
                sectionName = $"{clampSecName}ConvertToMoveCamera";
                clamp.ConvertToMoveCamera.X = double.Parse(iniFile.ReadIniFile(sectionName, "X", "0"));
                clamp.ConvertToMoveCamera.Y = double.Parse(iniFile.ReadIniFile(sectionName, "Y", "0"));

                // Stage取放料點
                sectionName = $"{clampSecName}StageCoordination";
                clamp.StageCoordination.X = double.Parse(iniFile.ReadIniFile(sectionName, "X", "0"));
                clamp.StageCoordination.Y = double.Parse(iniFile.ReadIniFile(sectionName, "Y", "0"));

                // 等待時間
                sectionName = $"{clampSecName}DelayTime";
                clamp.DelayTime1 = int.Parse(iniFile.ReadIniFile(sectionName, "DelayTime1", "300"));
                clamp.DelayTime2 = int.Parse(iniFile.ReadIniFile(sectionName, "DelayTime2", "300"));
            }
        }

        /********************
         * 滑台氣缸上升
         *******************/
        /// <summary>
        /// 夾爪滑台氣缸上升
        /// </summary>
        public void ClampSlideCylinderUp()
            => epcio.RioOutput(epcio.ClampSlideCylinder, false);

        /// <summary>
        /// 檢查夾爪滑台氣缸是否在上升位置
        /// </summary>
        /// <returns>檢查結果</returns>
        public bool IsClampSlideCylinderUp()
            => epcio.ClampSlideCylinderUpLs.Value;

        /// <summary>
        /// 等待夾爪滑台氣缸到上升位置
        /// </summary>
        /// <returns></returns>
        public async Task WaitingForSlideCylinderUp()
        {
            await Task.Run(() =>
            {
                while (!IsClampSlideCylinderUp())
                { }
            });
        }

        /********************
         * 滑台氣缸下降
         *******************/
        /// <summary>
        /// 夾爪滑台氣缸下降
        /// </summary>
        public void ClampSlideCylinderDown()
            => epcio.RioOutput(epcio.ClampSlideCylinder, true);

        /// <summary>
        /// 檢查夾爪滑台氣缸是否在下降位置
        /// </summary>
        /// <returns>檢查結果</returns>
        public bool IsClampSlideCylinderDown()
            => epcio.ClampSlideCylinderDownLs.Value;

        /// <summary>
        /// 等待夾爪滑台氣缸到下降位置
        /// </summary>
        /// <returns></returns>
        public async Task WaitingForSlideCylinderDown()
        {
            await Task.Run(() =>
            {
                while (!IsClampSlideCylinderDown())
                { }
            });
        }

        /********************
         * 夾爪上升
         *******************/
        /// <summary>
        /// 全部夾爪上升
        /// </summary>
        public void AllClampUp()
        {
            epcio.RioOutput(epcio.Clamp1UpDown, false);
            epcio.RioOutput(epcio.Clamp2UpDown, false);
        }

        /// <summary>
        /// 夾爪上升動作
        /// </summary>
        /// <remarks>clamp1/clamp2: 選擇要上升的夾爪</remarks>
        public void ClampUp(bool clamp1 = false, bool clamp2 = false)
        {
            if (clamp1)
                epcio.RioOutput(epcio.Clamp1UpDown, false);
            if (clamp2)
                epcio.RioOutput(epcio.Clamp2UpDown, false);
        }

        /// <summary>
        /// 夾爪上升動作
        /// </summary>
        /// <param name="clampId">選擇要上升的夾爪</param>
        public void ClampUp(EClampId clampId)
        {
            if (clampId == EClampId.Clamp1)
                epcio.RioOutput(epcio.Clamp1UpDown, false);
            else if (clampId == EClampId.Clamp2)
                epcio.RioOutput(epcio.Clamp2UpDown, false);
        }

        /// <summary>
        /// 檢查夾爪是否在上升位置
        /// </summary>
        /// <remarks>clamp1/clamp2: 選擇要檢查的夾爪</remarks>
        /// <returns>檢查結果</returns>
        public bool IsClampUp(bool clamp1 = false, bool clamp2 = false)
        {
            if (clamp1)
                if (!epcio.Clamp1UpLs.Value)
                    return false;

            if (clamp2)
                if (!epcio.Clamp2UpLs.Value)
                    return false;

            return true;
        }

        /// <summary>
        /// 等待夾爪到上升位置
        /// </summary>
        /// <remarks>clamp1/clamp2: 選擇要等待的夾爪</remarks>
        public async Task WaitingForClampUp(bool clamp1 = false, bool clamp2 = false)
        {
            await Task.Run(() =>
            {
                while (!IsClampUp(clamp1, clamp2))
                { }
            });
        }

        /********************
         * 夾爪下降
         *******************/
        /// <summary>
        /// 全部夾爪下降
        /// </summary>
        public void AllClampDown()
        {
            epcio.RioOutput(epcio.Clamp1UpDown, true);
            epcio.RioOutput(epcio.Clamp2UpDown, true);
        }

        /// <summary>
        /// 夾爪下降動作
        /// </summary>
        /// <remarks>clamp1/clamp2: 選擇要下降的夾爪</remarks>
        public void ClampDown(bool clamp1 = false, bool clamp2 = false)
        {
            if (clamp1)
                epcio.RioOutput(epcio.Clamp1UpDown, true);
            if (clamp2)
                epcio.RioOutput(epcio.Clamp2UpDown, true);
        }

        /// <summary>
        /// 夾爪下降動作
        /// </summary>
        /// <param name="clampId">選擇要下降的夾爪</param>
        public void ClampDown(EClampId clampId)
        {
            if (clampId == EClampId.Clamp1)
                epcio.RioOutput(epcio.Clamp1UpDown, true);
            else if (clampId == EClampId.Clamp2)
                epcio.RioOutput(epcio.Clamp2UpDown, true);
        }

        /// <summary>
        /// 檢查夾爪是否在下降位置
        /// </summary>
        /// <remarks>clamp1/clamp2: 選擇要檢查的夾爪</remarks>
        /// <returns>檢查結果</returns>
        public bool IsClampDown(bool clamp1 = false, bool clamp2 = false)
        {
            if (clamp1)
                if (!epcio.Clamp1DownLs.Value)
                    return false;

            if (clamp2)
                if (!epcio.Clamp2DownLs.Value)
                    return false;

            return true;
        }

        /// <summary>
        /// 等待夾爪到上升位置
        /// </summary>
        /// <remarks>clamp1/clamp2: 選擇要等待的夾爪</remarks>
        public async Task WaitingForClampDown(bool clamp1 = false, bool clamp2 = false)
        {
            await Task.Run(() =>
            {
                while (!IsClampDown(clamp1, clamp2))
                { }
            });
        }

        /********************
         * 夾爪打開
         *******************/
        /// <summary>
        /// 全部夾爪打開
        /// </summary>
        public void AllClampOpen()
        {
            epcio.RioOutput(epcio.Clamp1OpenClose, false);
            epcio.RioOutput(epcio.Clamp2OpenClose, false);
        }

        /// <summary>
        /// 夾爪打開動作
        /// </summary>
        /// <remarks>clamp1/clamp2: 選擇要等待的夾爪</remarks>
        public void ClampOpen(bool clamp1 = false, bool clamp2 = false)
        {
            if (clamp1)
                epcio.RioOutput(epcio.Clamp1OpenClose, false);
            if (clamp2)
                epcio.RioOutput(epcio.Clamp2OpenClose, false);
        }

        /// <summary>
        /// 夾爪打開動作
        /// </summary>
        /// <remarks>clamp1/clamp2: 選擇要打開的夾爪</remarks>
        public void ClampOpen(EClampId clampId)
        {
            if (clampId == EClampId.Clamp1)
                epcio.RioOutput(epcio.Clamp1OpenClose, false);
            else if (clampId == EClampId.Clamp2)
                epcio.RioOutput(epcio.Clamp2OpenClose, false);
        }

        /// <summary>
        /// 檢查指定夾爪是否在打開狀態
        /// </summary>
        /// <remarks>clamp1/clamp2: 選擇要檢查的夾爪</remarks>
        /// <returns>檢查結果</returns>
        public bool IsClampOpen(bool clamp1 = false, bool clamp2 = false)
        {
            if (clamp1)
                if (!epcio.Clamp1OpenLs.Value)
                    return false;

            if (clamp2)
                if (!epcio.Clamp2OpenLs.Value)
                    return false;

            return true;
        }

        /// <summary>
        /// 等待夾爪到打開位置
        /// </summary>
        /// <remarks>clamp1/clamp2: 選擇要等待的夾爪</remarks>
        public async Task WaitingForClampOpen(bool clamp1 = false, bool clamp2 = false)
        {
            await Task.Run(() =>
            {
                while (!IsClampOpen(clamp1, clamp2))
                { }
            });
        }

        /********************
         * 夾爪閉合
         *******************/
        /// <summary>
        /// 全部夾爪閉合
        /// </summary>
        public void AllClampClose()
        {
            epcio.RioOutput(epcio.Clamp1OpenClose, true);
            epcio.RioOutput(epcio.Clamp2OpenClose, true);
        }

        /// <summary>
        /// 夾爪閉合動作
        /// </summary>
        /// <remarks>clamp1/clamp2: 選擇要等待的夾爪</remarks>
        public void ClampClose(bool clamp1 = false, bool clamp2 = false)
        {
            if (clamp1)
                epcio.RioOutput(epcio.Clamp1OpenClose, true);
            if (clamp2)
                epcio.RioOutput(epcio.Clamp2OpenClose, true);
        }

        /// <summary>
        /// 夾爪閉合動作
        /// </summary>
        /// <remarks>clamp1/clamp2: 選擇要閉合的夾爪</remarks>
        public void ClampClose(EClampId clampId)
        {
            if (clampId == EClampId.Clamp1)
                epcio.RioOutput(epcio.Clamp1OpenClose, true);
            else if (clampId == EClampId.Clamp2)
                epcio.RioOutput(epcio.Clamp2OpenClose, true);
        }

        /// <summary>
        /// 檢查指定夾爪是否在閉合狀態
        /// </summary>
        /// <remarks>clamp1/clamp2: 選擇要檢查的夾爪</remarks>
        /// <returns>檢查結果</returns>
        public bool IsClampClose(bool clamp1 = false, bool clamp2 = false)
        {
            if (clamp1)
                if (!epcio.Clamp1CloseLs.Value)
                    return false;

            if (clamp2)
                if (!epcio.Clamp2CloseLs.Value)
                    return false;

            return true;
        }

        /// <summary>
        /// 等待夾爪到閉合位置
        /// </summary>
        /// <remarks>clamp1/clamp2: 選擇要等待的夾爪</remarks>
        public async Task WaitingForClampClose(bool clamp1 = false, bool clamp2 = false)
        {
            await Task.Run(() =>
            {
                while (!IsClampClose(clamp1, clamp2))
                { }
            });
        }

        /********************
         * 放回成品
         *******************/
        /// <summary>
        /// 將成品放回Tray
        /// </summary>
        /// <param name="barrelTrayNo">成品Tray編號</param>
        /// <remarks>固定使用Clamp2</remarks>
        public async Task PlaceProduct(int barrelTrayNo)
        {
            ObjectMotion objectMotion = new ObjectMotion();
            Tray trays = Tray.Instance;

            // 確認伺服軸群組
            if (ActionGroup.ActionGroupId == EActionGroup.XY_ClampTray)
            {
                ActionGroup.ClampSideStatus = ESideStatus.Assembly;

                // Clamp2夾爪在閉合狀態(有夾持部品)才能動作
                if (epcio.Clamp2CloseLs.Value)
                {
                    epcio.SetSpeed(servoClampSpeed: EServoSpeed.High,
                                   servoTraySpeed: EServoSpeed.High);

                    // 定位
                    var feeder = trays.FeederList.Find(x => x.FeederId == barrelTrayNo);
                    if (feeder.Effective && feeder.PartEnable)
                    {
                        var tray = trays.GetTrayData(feeder.Part);
                        if (tray != null)
                        {
                            var pMatrix = tray.PointMatrix;
                            if (pMatrix != null)
                            {
                                trays.MoveNext(tray.Name);
                                await objectMotion.ClampToTray(EClampId.Clamp2, tray.Name);

                                // 夾爪下降
                                ClampDown(EClampId.Clamp2);
                                //clamp.ClampSlideCylinderDown();
                                //await clamp.WaitingForSlideCylinderDown();
                                await WaitingForClampDown(clamp2: true);
                                await Task.Delay(Clamp1.DelayTime1);

                                // 放置
                                ClampOpen(EClampId.Clamp2);
                                await WaitingForClampOpen(clamp2: true);
                                await Task.Delay(Clamp1.DelayTime2);

                                // 夾爪上升
                                ClampUp(EClampId.Clamp2);
                                //clamp.ClampSlideCylinderUp();
                                //await clamp.WaitingForSlideCylinderUp();
                                await WaitingForClampUp(clamp2: true);
                            }
                        }
                    }
                }

                ActionGroup.ClampSideStatus = ESideStatus.StandBy;
            }
        }
    }
}
