using Prism.Events;
using System;

namespace OEP520G.Core
{ /********************
     * StatusBar顯示
     * 1.以下僅說明訊息顯示部分的使用。
     *   A.在有顯示StatusBar需求的物件開頭，使用下列程式引入StatusBar物件：
     *     private readonly StatusBar statusBar = StatusBar.Instance;
     *
     *   B.顯示訊息使用以下函式：
     *     statusBar.ShowStatusBarMessage("訊息輸入在此");
     *
     * 2.需隨時更新的部分在MainWindowViewModel.cs內，
     *   Timer名稱為StatusBarUpdateTimer，更新時間為0.5秒
     *******************/

    /// <summary>
    /// 訊息識別字
    /// </summary>
    public enum EStatusBarContextName
    {
        MESSAGE,
        PROGRESS_BAR,
        AUTHORITY,
        PRODUCT,
        COORDINATION,
        TIME,
        DEBUG
    }

    /// <summary>
    /// 主視窗Title標題資料
    /// </summary>
    public class StatusBarData
    {
        public EStatusBarContextName Name { get; set; }
        public string Message { get; set; }
    }

    public class StatusBar
    {
        // Singleton單例模式
        private static readonly Lazy<StatusBar> lazy = new Lazy<StatusBar>(() => new StatusBar());
        public static StatusBar Instance => lazy.Value;

        /// <summary>
        /// 顯示StatusBar訊息(簡化版)
        /// </summary>
        /// <param name="msg">要顯示的訊息</param>
        public void ShowStatusBarMessage(string msg)
        {
            Common.EA.GetEvent<StatusBarSetter>().Publish(new StatusBarData()
            {
                Name = EStatusBarContextName.MESSAGE,
                Message = msg
            });
        }
    }
}
