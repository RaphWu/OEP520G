using Prism.Events;
using System;
using System.Collections.Generic;
using System.Text;

// 全域的Prism Event Aggregator註冊
namespace OEP520G.Core
{
    /// <summary>
    /// 接收主視窗Title標題的事件聚合器
    /// </summary>
    public class WindowTitleSetter : PubSubEvent<WindowTitleData> { }

    /// <summary>
    /// 接收主視窗Title標題的事件聚合器
    /// </summary>
    public class StatusBarSetter : PubSubEvent<StatusBarData> { }

    /// <summary>
    /// 品種更換的共用事件聚合器：當作業品種有更換時，會發佈此事件
    /// </summary>
    /// <remarks> string => 新品種ID </remarks>
    public class OnSwitchProduct : PubSubEvent<string>
    {
    }

    /// <summary>
    /// 品種更換的共用事件聚合器：當作業品種有更換時，會發佈此事件
    /// </summary>
    /// <remarks> string => 新品種ID </remarks>
    public class AfterSwitchProduct : PubSubEvent<string> { }
}
