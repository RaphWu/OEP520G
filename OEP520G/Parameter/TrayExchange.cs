using System;
using System.Collections.Generic;
using System.Text;

namespace OEP520G.Parameter
{
    /// <summary>
    /// CRUD用
    /// </summary>
    public class TrayExchange
    {
        public string TrayName { get; set; } // Tray ID
        public string NewTrayName { get; set; } // New Tray ID
        public string Memo { get; set; } // 註解
    }
}
