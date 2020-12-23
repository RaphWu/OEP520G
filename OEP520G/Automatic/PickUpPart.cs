using EPCIO;
using OEP520G.Automatic;
using OEP520G.Functions;
using OEP520G.Parameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OEP520G.Automatic
{
    public class PickUpPart
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly ObjectMotion objectMotion = new ObjectMotion();
        private readonly Clamp clamp = Clamp.Instance;
        private readonly Stage stage = Stage.Instance;
        private readonly Nozzle nozzles = Nozzle.Instance;
        private readonly Tray trays = Tray.Instance;

        /********************
         * 吸嘴
         *******************/
        /// <summary>
        /// 吸嘴取料
        /// </summary>
        /// <param name="nozzleId">吸嘴ID</param>
        /// <param name="trayName">Tray盤Name</param>
        public async Task NozzlePickUpPart(ENozzleId nozzleId, string trayName)
        {
            // 確認伺服軸群組
            if (ActionGroup.ActionGroupId == EActionGroup.XTray_ClampY)
            {
                var nozzle = nozzles.NozzleList[(int)nozzleId];
                var tray = trays.GetTrayData(trayName);

                var pMatrix = tray.PointMatrix.Where(p => p.PointNo == tray.CurrentPoint).First();

                // 定位
                epcio.MoveTo(positionX: pMatrix.PointMatrixX,
                             positionTray: pMatrix.PointMatrixY);
                await epcio.WaitingForMotionStop(waitingServoX: true,
                                                 waitingServoTray: true);

                epcio.MoveTo(positionZ: epcio.SafetyZ + 5);
                nozzles.NozzleDown(nozzleId);
                await nozzles.WaitingForNozzleDown(nozzleId);
                await epcio.WaitingForMotionStop(waitingServoZ: true);

                nozzles.NozzleVaccumOn(nozzleId);
                //await nozzles.WaitingForNozzleUp

                await Task.Delay(400);

                epcio.MoveTo(positionZ: epcio.SafetyZ);
                nozzles.NozzleUp(nozzleId);
                await nozzles.WaitingForNozzleUp(nozzleId);
                await epcio.WaitingForMotionStop(waitingServoZ: true);
            }
        }

        /********************
         * 夾爪
         *******************/
        /// <summary>
        /// 夾爪從料盤夾取Barrel
        /// </summary>
        /// <param name="barrelTrayNo">成品Tray編號</param>
        /// <remarks>固定使用Clamp1</remarks>
        public async Task ClampPickUpBarrel(int barrelTrayNo)
        {
            // 確認伺服軸群組
            if (ActionGroup.ActionGroupId == EActionGroup.XY_ClampTray)
            {
                ActionGroup.ClampSideStatus = ESideStatus.GetPart;

                // Clamp1夾爪在張開狀態(無夾持部品)才能動作
                if (epcio.Clamp1OpenLs.Value)
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
                                await objectMotion.ClampToTray(EClampId.Clamp1, tray.Name);

                                // 夾爪下降
                                clamp.ClampDown(EClampId.Clamp1);
                                //clamp.ClampSlideCylinderDown();
                                //await clamp.WaitingForSlideCylinderDown();
                                await clamp.WaitingForClampDown(clamp1: true);
                                await Task.Delay(clamp.Clamp1.DelayTime1);

                                // 夾取
                                clamp.ClampClose(EClampId.Clamp1);
                                await clamp.WaitingForClampClose(clamp1: true);
                                await Task.Delay(clamp.Clamp1.DelayTime2);

                                // 夾爪上升
                                clamp.ClampUp(EClampId.Clamp1);
                                //clamp.ClampSlideCylinderUp();
                                //await clamp.WaitingForSlideCylinderUp();
                                await clamp.WaitingForClampUp(clamp1: true);
                            }
                        }
                    }
                }
            }

            ActionGroup.ClampSideStatus = ESideStatus.StandBy;
        }

        /********************
         * 台車取料
         *******************/
        /// <summary>
        /// 夾爪從Stage夾取成品/半成品
        /// </summary>
        /// <remarks>固定使用Clamp2</remarks>
        /// <param name="stageClampCloseWhenFinished">夾取結束後是否關閉台車夾片? (自動作業取成品、放Barrel的連續動作時使用false)</param>
        public async Task ClampPickUpProduct(bool stageClampCloseWhenFinished = true)
        {
            // 確認伺服軸群組
            if (ActionGroup.ActionGroupId == EActionGroup.XTray_ClampY)
            {
                //await epcio.WaitingForMotionStop(waitingServoClamp: true, waitingServoY: true);
                ActionGroup.ClampSideStatus = ESideStatus.GetPart;

                // Clamp2夾爪在張開狀態(無夾持部品)才能動作
                if (epcio.Clamp2OpenLs.Value)
                {
                    epcio.SetSpeed(servoClampSpeed: EServoSpeed.High,
                                   servoYSpeed: EServoSpeed.High);

                    // 定位
                    await objectMotion.ClampToStage(EClampId.Clamp2, waitingForMotionStop: false);
                    epcio.MoveTo(degreeR: 0);
                    await epcio.WaitingForMotionStop(waitingServoClamp: true,
                                                     waitingServoY: true,
                                                     waitingServoR: true);

                    // 台車夾片開
                    stage.StageClampOpen();
                    await stage.WaitingForClampOpen();

                    // 夾爪下降
                    clamp.ClampDown(EClampId.Clamp2);
                    //clamp.ClampSlideCylinderDown();
                    //await clamp.WaitingForSlideCylinderDown();
                    await clamp.WaitingForClampDown(clamp2: true);
                    await Task.Delay(clamp.Clamp2.DelayTime1);

                    // 夾取
                    clamp.ClampClose(EClampId.Clamp2);
                    await clamp.WaitingForClampClose(clamp2: true);
                    await Task.Delay(clamp.Clamp2.DelayTime2);

                    // Stage真空關閉
                    stage.StageVaccumOff();

                    // 夾爪上升
                    clamp.ClampUp(EClampId.Clamp2);
                    //clamp.ClampSlideCylinderUp();
                    //await clamp.WaitingForSlideCylinderUp();
                    await clamp.WaitingForClampUp(clamp2: true);

                    // 台車夾片閉
                    if (stageClampCloseWhenFinished)
                    {
                        stage.StageClampClose();
                        await stage.WaitingForClampClose();
                    }
                }

                ActionGroup.ClampSideStatus = ESideStatus.StandBy;
            }
        }
    }
}
