using EPCIO;
using EPCIO.IoSystem;
using OEP520G.Core;
using OEP520G.Functions;
using OEP520G.Parameter;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace OEP520G.Manual.ViewModels
{
    public class IoActionViewModel : BindableBase, IActiveAware
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly Nozzle nozzle = new Nozzle();
        private readonly Stage stage = Stage.Instance;

        // 視窗Active/Deactive
        public event EventHandler IsActiveChanged;
        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                if (value)
                {
                }
                else
                {
                }
            }
        }

        // 按鍵-台車
        public DelegateCommand StageClampOpenCommand { get; private set; }
        public DelegateCommand StageClampCloseCommand { get; private set; }
        public DelegateCommand StageVaccumOnCommand { get; private set; }
        public DelegateCommand StageVaccumOffCommand { get; private set; }

        // 按鍵-比例閥
        public DelegateCommand EpRegulatorSetCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public IoActionViewModel()
        {
            EpRegulatorPressure = 0;
            OverpressureSource = new List<double> { 0.0, 0.1, 0.2, 0.3, 0.4, 0.5 };
            OverpressureItem = 0;

            IoChoiceSource = epcio.RioOutputList;
            //foreach (RemoteIo rio in epcio.RioOutputList)
            //    IoChoiceSource.Add(rio.Name);
            //IoChoiceItem = IoChoiceSource[0];

            SpeedTestSelected = "OFF";

            // 按鍵-台車
            StageClampOpenCommand = new DelegateCommand(StageClampOpen);
            StageClampCloseCommand = new DelegateCommand(StageClampClose);
            StageVaccumOnCommand = new DelegateCommand(StageVaccumOn);
            StageVaccumOffCommand = new DelegateCommand(StageVaccumOff);

            // 按鍵-比例閥
            EpRegulatorSetCommand = new DelegateCommand(EpRegulatorSet);
        }




        /********************
         * 按鍵-台車
         *******************/
        private void StageClampOpen()
            => stage.StageClampOpen();

        private void StageClampClose()
               => stage.StageClampClose();

        private void StageVaccumOn()
            => stage.StageVaccumOn();

        private void StageVaccumOff()
            => stage.StageVaccumOff();

        /********************
         * 按鍵-比例閥
         *******************/
        private void EpRegulatorSet()
            => stage.StageClampOpen();


        /********************
		 * 繫結
		 *******************/
        // 比例閥
        public double EpRegulatorPressure
        {
            get { return _epRegulatorPressure; }
            set { SetProperty(ref _epRegulatorPressure, value); }
        }
        private double _epRegulatorPressure;

        public List<double> OverpressureSource
        {
            get { return _overpressureSource; }
            set { SetProperty(ref _overpressureSource, value); }
        }
        private List<double> _overpressureSource;

        public double OverpressureItem
        {
            get { return _overpressureItem; }
            set { SetProperty(ref _overpressureItem, value); }
        }
        private double _overpressureItem;

        // IO選擇
        public List<RemoteIo> IoChoiceSource
        {
            get { return _ioChoiceSource; }
            set { SetProperty(ref _ioChoiceSource, value); }
        }
        private List<RemoteIo> _ioChoiceSource;

        public RemoteIo IoChoiceItem
        {
            get { return _ioChoiceItem; }
            set { SetProperty(ref _ioChoiceItem, value); }
        }
        private RemoteIo _ioChoiceItem;

        // IO速度測試
        public string SpeedTestSelected
        {
            get { return _speedTestSelected; }
            set { SetProperty(ref _speedTestSelected, value); }
        }
        private string _speedTestSelected;
    }
}
