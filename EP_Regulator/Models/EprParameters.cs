using System;
using System.Collections.Generic;
using System.Text;

namespace OEP520G.EPRegulator.Models
{
    public class EprParameters
    {
        // 壓力限度(kg)
        public const double MAX_PRESSURE = 5.0;
        public const double MIN_PRESSURE = 0.0;

        // DAC輸出限度(V)
        public const double MAX_DAC_OUTPUT = 10.0;
        public const double MIN_DAC_OUTPUT = 0.0;

        // 輸出壓力換算DAC壓力的比值
        public const double RATIO_KG_TO_V = (MAX_DAC_OUTPUT - MIN_DAC_OUTPUT) / (MAX_PRESSURE - MIN_PRESSURE);
    }
}
