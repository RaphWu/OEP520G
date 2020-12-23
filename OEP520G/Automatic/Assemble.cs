using EPCIO;
using OEP520G.Automatic;
using OEP520G.Functions;
using OEP520G.Parameter;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OEP520G.Automatic
{
    public class Assemble
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly ObjectMotion objectMotion = new ObjectMotion();
        private readonly Clamp clamp = Clamp.Instance;
        private readonly Stage stage = Stage.Instance;
        private readonly Nozzle nozzle = Nozzle.Instance;
        private readonly Tray tray = Tray.Instance;

        /********************
         * 夾爪
         *******************/
        /// <summary>
        /// 夾爪放置Barrel到台車
        /// </summary>
        /// <remarks>固定使用Clamp1</remarks>
        public async Task ClampPlaceBarrel()
        {
            // 確認伺服軸群組
            if (ActionGroup.ActionGroupId == EActionGroup.XTray_ClampY)
            {
                ActionGroup.ClampSideStatus = ESideStatus.GetPart;

                // Clamp1夾爪在閉合狀態(有夾持部品)才能動作
                if (epcio.Clamp1CloseLs.Value)
                {
                    epcio.SetSpeed(servoClampSpeed: EServoSpeed.High,
                                   servoYSpeed: EServoSpeed.High);

                    // 定位
                    await objectMotion.ClampToStage(EClampId.Clamp1, waitingForMotionStop: false);
                    epcio.MoveTo(degreeR: 0);
                    await epcio.WaitingForMotionStop(waitingServoClamp: true,
                                                     waitingServoY: true,
                                                     waitingServoR: true);

                    // 台車夾片開
                    stage.StageClampOpen();
                    await stage.WaitingForClampOpen();

                    // 夾爪下降
                    clamp.ClampDown(EClampId.Clamp1);
                    clamp.ClampSlideCylinderDown();
                    await clamp.WaitingForSlideCylinderDown();
                    await clamp.WaitingForClampDown(clamp1: true);
                    await Task.Delay(clamp.Clamp1.DelayTime1);

                    // Stage真空開啟
                    stage.StageVaccumOn();

                    // 放開夾爪
                    clamp.ClampOpen(EClampId.Clamp1);
                    await clamp.WaitingForClampOpen(clamp1: true);
                    await Task.Delay(clamp.Clamp1.DelayTime2);

                    // 夾爪上升
                    clamp.ClampUp(EClampId.Clamp1);
                    clamp.ClampSlideCylinderUp();
                    await clamp.WaitingForSlideCylinderUp();
                    await clamp.WaitingForClampUp(clamp1: true);

                    // 台車夾片閉
                    stage.StageClampClose();
                    await stage.WaitingForClampClose();
                }

                ActionGroup.ClampSideStatus = ESideStatus.StandBy;
            }
        }
    }
}
