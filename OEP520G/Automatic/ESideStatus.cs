using System;
using System.Collections.Generic;
using System.Text;

namespace OEP520G.Automatic
{
    /// <summary>
    /// 組裝側或夾爪側的狀態代號
    /// </summary>
    public enum ESideStatus
    {
        Discard = 0x0001,   // 抛料
        GetPart = 0x0010,   // 取料
        Assembly = 0x0020,  // 組裝
        StandBy = 0x8000    // 待命
    }
}
