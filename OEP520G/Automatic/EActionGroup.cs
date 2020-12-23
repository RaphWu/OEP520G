using System;
using System.Collections.Generic;
using System.Text;

namespace OEP520G.Automatic
{
    /// <summary>
    /// 伺服軸群組代號
    /// </summary>
    public enum EActionGroup
    {
        /// <summary>
        /// X、Y一組；Clamp、Tray一組
        /// </summary>
        XY_ClampTray = 1,

        /// <summary>
        /// X、Tray一組；Clamp、Y一組
        /// </summary>
        XTray_ClampY = 2
    }
}
