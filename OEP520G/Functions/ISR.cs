using EPCIO;
using OEP520G.Core;
using System;
using System.Windows;

namespace OEP520G.Functions
{
    public class ISR
    {
        private readonly Epcio epcio = Epcio.Instance;

        /// <summary>
        /// 建構函式
        /// </summary>
        public ISR()
        {
            //io.EmsPress += EmsPress;
        }

        /// <summary>
        /// 緊急停止
        /// </summary>
        private void EmsPress(object sender, EventArgs e)
        {
            Common.log.ImportantEvent("Emergency Stop Push Down!");
            MessageBox.Show("緊急停止被觸發。<br/>請確認問題排除後，按確認鍵重置！", "EMS", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
            epcio.ErrorReset();
        }
    }
}
