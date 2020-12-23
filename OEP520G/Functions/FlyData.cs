using System;
using System.Collections.Generic;
using System.Text;

namespace OEP520G.Functions
{
    public class FlyData : FlyDataStruct
    {
        public string SpeedName { get; set; }
        public bool Update { get; set; }
        public bool[] NozzleEnable { get; set; }
    }
}

