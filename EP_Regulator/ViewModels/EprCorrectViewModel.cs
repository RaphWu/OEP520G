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
            EprPressure = 1.5;
            Ratio = 1.0;
        }

        private DelegateCommand _SetPressureCommand;
        public DelegateCommand SetPressureCommand
            => _SetPressureCommand ??= new DelegateCommand(ExecuteSetPressureCommand);
        void ExecuteSetPressureCommand()
        {
            _epr.SetEprPressure(EprPressure, Ratio);
        }

        /********************
         * Data Binding
         ********************/
        bool triggerByUi = true;

        private double _EprPressure;
        public double EprPressure
        {
            get { return _EprPressure; }
            set
            {
                SetProperty(ref _EprPressure, value);

                if (triggerByUi)
                {
                    triggerByUi = false;
                    DacVoltage = value * Ratio;
                    triggerByUi = true;
                }
            }
        }

        private double _DacVoltage;
        public double DacVoltage
        {
            get { return _DacVoltage; }
            set
            {
                SetProperty(ref _DacVoltage, value);

                if (triggerByUi)
                {
                    triggerByUi = false;
                    _EprPressure = value / Ratio;
                    triggerByUi = true;
                }
            }
        }

        private double _Ratio;
        public double Ratio
        {
            get { return _Ratio; }
            set
            {
                SetProperty(ref _Ratio, value);

                if (triggerByUi)
                {
                    triggerByUi = false;
                    DacVoltage = EprPressure * Ratio;
                    triggerByUi = true;
                }
            }
        }
    }
}