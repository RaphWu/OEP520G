using EPCIO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OEP520G.Automatic
{
    /// <summary>
    /// 自動作業狀態操作
    /// </summary>
    public class ActionGroup
    {
        private readonly Epcio epcio = Epcio.Instance;

        /// <summary>
        /// 自動作業是否運轉中
        /// </summary>
        private static bool AutoIsInOperation { get; set; }

        /// <summary>
        /// 伺服軸群組
        /// </summary>
        public static EActionGroup ActionGroupId { get; private set; }

        /// <summary>
        /// 組裝側狀態
        /// </summary>
        public static ESideStatus AssemblySideStatus { get; set; }

        /// <summary>
        /// 夾爪側狀態
        /// </summary>
        public static ESideStatus ClampSideStatus { get; set; }

        /// <summary>
        /// 返回isRunning旗標
        /// </summary>
        /// <returns>isRunning旗標</returns>
        public bool IsRunning() => AutoIsInOperation;

        /// <summary>
        /// 設定isRunning旗標
        /// </summary>
        /// <param name="status">旗標狀態</param>
        public void SetRunning(bool status)
            => AutoIsInOperation = status;

        /// <summary>
        /// 切換伺服軸群組代號
        /// </summary>
        /// <param name="actionGroup">指定代號</param>
        /// <remarks>
        /// 1.必須在伺服軸全都在停止運動狀態時才可切換。<br/>
        /// 2.若指定為Switch(預設)，則交換；否則切換為指定值。
        /// </remarks>
        public async Task SwitchActionGroup(EActionGroup actionGroup)
        {
            // 確認伺服軸停止時才可切換
            await epcio.WaitingForMotionStop(waitingServoX: true,
                                             waitingServoY: true,
                                             waitingServoClamp: true,
                                             waitingServoTray: true);

            if (actionGroup == EActionGroup.XY_ClampTray)
                ActionGroupId = EActionGroup.XY_ClampTray;
            else if (actionGroup == EActionGroup.XTray_ClampY)
                ActionGroupId = EActionGroup.XTray_ClampY;
        }
    }
}
