using EpcioSeries;
using OEP520G.EPRegulator.Contracts;
using System;
using EPCIO;
using System.Collections.Generic;
using System.Text;

namespace OEP520G.EPRegulator.Services
{
    public class EprService : IEpr
    {
        private readonly Epcio epcio = Epcio.Instance;

        // ctor
        public EprService()
        {
        }

        public void SetEprPressure(double kg)
            => epcio.SetEprPressure(kg);

        public void SetEprPressure(double kg, double ratio)
            => epcio.SetEprPressure(kg, ratio);
    }
}
