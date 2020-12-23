using EPCIO;
using OEP520G.Automatic;
using OEP520G.Functions;
using OEP520G.Parameter;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OEP520G.Automatic
{
    /// <summary>
    /// 抛料作業
    /// </summary>
    public class Dump
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly Machine machine = Machine.Instance;
        private readonly ObjectMotion objectMotion = new ObjectMotion();
        private readonly Nozzle nozzle = Nozzle.Instance;
        private readonly Clamp clamp = Clamp.Instance;

        private readonly PickUpPart getPart = new PickUpPart();

        /********************
         * 吸嘴抛料
         *******************/
        /// <summary>
        /// 吸嘴全抛料
        /// </summary>
        public async Task AssembleDiscardAll()
            => await AssembleDiscardWithAxisMove(new ENozzleId[]
            {
                ENozzleId.Nozzle01,
                ENozzleId.Nozzle02,
                ENozzleId.Nozzle03,
                ENozzleId.Nozzle04,
                ENozzleId.Nozzle05,
                ENozzleId.Nozzle06,
                ENozzleId.Nozzle07,
                ENozzleId.Nozzle08,
                ENozzleId.Nozzle09,
                ENozzleId.Nozzle10,
                ENozzleId.Nozzle11
            }, true);

        /// <summary>
        /// 吸嘴抛料
        /// </summary>
        /// <param name="nozzleId">吸嘴ID</param>
        public async Task AssembleDiscard(ENozzleId nozzleId)
            => await AssembleDiscardWithAxisMove(new ENozzleId[] { nozzleId }, true);

        /// <summary>
        /// 吸嘴抛料
        /// </summary>
        /// <param name="nozzleIdList">吸嘴ID列表</param>
        /// <param name="servoPositioning">伺服軸是否先定位</param>
        private async Task AssembleDiscardWithAxisMove(ENozzleId[] nozzleIdList, bool servoPositioning)
        {
            ActionGroup.AssemblySideStatus = ESideStatus.Discard;

            epcio.SetSpeed(servoXSpeed: EServoSpeed.Middle);

            // 吸嘴定位
            await objectMotion.NozzleToDiscardBox(servoPositioning);

            // 吸嘴伸出
            nozzle.NozzleDown(nozzleIdList);
            await nozzle.WaitingForNozzleDown(nozzleIdList);

            // 吐氣次數
            int blowTotal = machine.AssembleDiscardExhaleNumbers;

            // 抛料動作
            for (int blowCounter = 1; blowCounter <= blowTotal; blowCounter++)
            {
                // 吐氣
                nozzle.NozzleBlowOn(nozzleIdList);

                // 吐氣時間
                await Task.Delay(machine.AssembleDiscardExhaleTime);

                // 吐氣關閉
                nozzle.NozzleOff(nozzleIdList);

                // 吐氣間隔時間
                if (blowCounter < blowTotal)
                    await Task.Delay(machine.AssembleDiscardGapTime);
            }

            // 吸嘴縮回
            nozzle.ALlNozzleUp();

            // 安全位置
            if (!epcio.IsServoZSafety())
            {
                epcio.MoveTo(positionZ: epcio.SafetyZ);
                await epcio.WaitingForMotionStop(waitingServoZ: true);
            }

            ActionGroup.AssemblySideStatus = ESideStatus.StandBy;
        }

        /********************
         * 夾爪抛料
         *******************/
        /// <summary>
        /// 夾爪抛料
        /// </summary>
        /// <param name="clamp1">夾爪1</param>
        /// <param name="clamp2">夾爪2</param>
        public async Task ClampDump(bool clamp1 = false, bool clamp2 = false)
        {
            if (clamp1 || clamp2)
                await ClampDiscardWithAxisMove(clamp1, clamp2, true);
        }

        /// <summary>
        /// 夾爪抛料
        /// </summary>
        /// <param name="clamp1">夾爪1</param>
        /// <param name="clamp2">夾爪2</param>
        /// <param name="servoPositioning">伺服軸是否先定位</param>
        private async Task ClampDiscardWithAxisMove(bool clamp1, bool clamp2, bool servoPositioning)
        {
            // 確認伺服軸群組
            if (ActionGroup.ActionGroupId == EActionGroup.XTray_ClampY)
            {
                ActionGroup.ClampSideStatus = ESideStatus.Discard;

                epcio.SetSpeed(servoClampSpeed: EServoSpeed.Middle,
                               servoTraySpeed: EServoSpeed.Middle
                               );

                // 夾爪定位
                await objectMotion.ClampToDiscardBox(servoPositioning);

                // 夾爪下降
                clamp.ClampDown(clamp1, clamp2);
                await clamp.WaitingForClampDown(clamp1, clamp2);

                // 開合次數
                int discardTotal = machine.SemiFinishedDiscardOpenCloseNumbers;

                // 抛料
                for (int discardCounter = 1; discardCounter <= discardTotal; discardCounter++)
                {
                    // 開
                    clamp.ClampOpen(clamp1, clamp2);
                    await clamp.WaitingForClampOpen(clamp1, clamp2);

                    if (discardCounter < discardTotal)
                    {
                        // 延時
                        await Task.Delay(machine.SemiFinishedDiscardOpenCloseTime);

                        // 合
                        clamp.ClampClose(clamp1, clamp2);
                        await clamp.WaitingForClampClose(clamp1, clamp2);
                    }
                    // 延時
                    await Task.Delay(machine.SemiFinishedDiscardOpenCloseTime);
                }

                // 夾爪上升
                clamp.ClampUp(clamp1, clamp2);
                await clamp.WaitingForClampUp(clamp1, clamp2);

                ActionGroup.ClampSideStatus = ESideStatus.StandBy;
            }
        }

        /********************
         * 台車抛料
         *******************/
        /// <summary>
        /// 台車抛料
        /// </summary>
        public async Task StageDiscard()
            => await StageDiscardWithAxisMove(true);

        /// <summary>
        /// 台車抛料
        /// </summary>
        /// <param name="servoPositioning">伺服軸是否先定位</param>
        /// <remarks>固定使用Clamp2</remarks>
        private async Task StageDiscardWithAxisMove(bool servoPositioning)
        {
            // 確認台車上是否有部品
            if (epcio.StageClampCloseLs.Value)
            {
                // 確認伺服軸群組
                if (ActionGroup.ActionGroupId == EActionGroup.XTray_ClampY)
                {
                    // 夾取台車上的部品
                    await getPart.ClampPickUpProduct();

                    // 抛料
                    await ClampDump(clamp2: true);

                    ActionGroup.ClampSideStatus = ESideStatus.StandBy;
                }
            }
        }

        /********************
         * 全抛料
         *******************/
        /// <summary>
        /// 執行吸嘴及夾爪全抛料
        /// </summary>
        public async Task DiscardAll()
        {
            // 確認伺服軸群組
            if (ActionGroup.ActionGroupId == EActionGroup.XTray_ClampY)
            {
                bool AssembleSideFinished = false;
                bool ClampSideFinished = false;

                await epcio.WaitingForAllServoMotionStop();
                await epcio.MoveServoZToSafety();

                // 吸嘴抛料
                _ = Task.Run(async () =>
                {
                    await AssembleDiscardWithAxisMove(new ENozzleId[]
                    {
                        ENozzleId.Nozzle01,
                        ENozzleId.Nozzle02,
                        ENozzleId.Nozzle03,
                        ENozzleId.Nozzle04,
                        ENozzleId.Nozzle05,
                        ENozzleId.Nozzle06,
                        ENozzleId.Nozzle07,
                        ENozzleId.Nozzle08,
                        ENozzleId.Nozzle09,
                        ENozzleId.Nozzle10,
                        ENozzleId.Nozzle11
                    }, true);

                    AssembleSideFinished = true;
                });

                _ = Task.Run(async () =>
                {
                    // 夾爪抛料
                    await ClampDump(true, true);

                    // Stage抛料
                    await StageDiscard();

                    ClampSideFinished = true;
                });

                await Task.Yield();
                while (!ClampSideFinished || !AssembleSideFinished)
                { }
            }
        }
    }
}
