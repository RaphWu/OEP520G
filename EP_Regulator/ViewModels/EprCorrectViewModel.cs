using OEP520G.EPRegulator.Contracts;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OEP520G.EPRegulator.ViewModels
{
    public class EprCorrectViewModel : BindableBase
    {
        private readonly IEpr _epr;

        public EprCorrectViewModel(IEpr epr)
        {
            _epr = epr;
        }

        /********************
         * Data Binding
         ********************/

        private double _EprPressure;
        public double EprPressure
        {
            get { return _EprPressure; }
            set
            {
                SetProperty(ref _EprPressure, value);
                _epr.SetEprPressure(value);
            }
        }
    }
}
