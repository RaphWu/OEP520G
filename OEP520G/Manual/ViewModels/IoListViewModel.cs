using EPCIO;
using EPCIO.IoSystem;
using Force.DeepCloner;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows.Data;

namespace OEP520G.Manual.ViewModels
{
    public class IoListViewModel : BindableBase, IActiveAware
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly IO io = new IO();

        private enum EScreenCode
        {
            All,
            LocalIo,
            RioInput,
            RioOutput
        }

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
                    // TODO: 換頁沒更新
                    epcio.HomeSensorTrigger += LioChanged;
                    epcio.HomeSensorClear += LioChanged;
                    epcio.LimitSwitchPositiveTrigger += LioChanged;
                    epcio.LimitSwitchPositiveClear += LioChanged;
                    epcio.LimitSwitchNegativeTrigger += LioChanged;
                    epcio.LimitSwitchNegativeClear += LioChanged;
                    epcio.RioInputChanged += RioChenged;
                }
                else
                {
                    epcio.HomeSensorTrigger -= LioChanged;
                    epcio.HomeSensorClear -= LioChanged;
                    epcio.LimitSwitchPositiveTrigger -= LioChanged;
                    epcio.LimitSwitchPositiveClear -= LioChanged;
                    epcio.LimitSwitchNegativeTrigger -= LioChanged;
                    epcio.LimitSwitchNegativeClear -= LioChanged;
                    epcio.RioInputChanged -= RioChenged;
                }
            }
        }

        // 輸出CheckBox
        public DelegateCommand<string> SetRioOutputCommand { get; set; }

        // 測試用
        //public DelegateCommand TestCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public IoListViewModel()
        {
            OutputTypeSelect = "RealTime";

            RefreshSource();

            // 輸出CheckBox
            SetRioOutputCommand = new DelegateCommand<string>(RioOutput);

            // 測試用
            //TestCommand = new DelegateCommand(TEST);
        }

        /********************
         * DataGrid
         *******************/
        /// <summary>
        /// Local Io變更Event
        /// </summary>
        private void LioChanged(object sender, IoEventArgs e)
        {
            if (e != null)
            {
                io.LioOutput((Servo)e.Object, (EIoType)e.IoType);
                RefreshSource(EScreenCode.LocalIo);
            }
        }

        /// <summary>
        /// Remote Io變更Event
        /// </summary>
        private void RioChenged(object sender, IoEventArgs e)
        {
            if (e != null)
            {
                io.RioInput((RemoteIo)e.Object);
                RefreshSource(EScreenCode.RioInput);
            }
        }

        /// <summary>
        /// 輸出動作
        /// </summary>
        private void RioOutput(string ioCode)
        {
            RemoteIo ri = RemoteIoOutputSource.Find(x => x.IoCode == ioCode);
            epcio.RioOutput(ri, ri.Value);
            io.RioOutputChanged(ri);
            RefreshSource(EScreenCode.RioOutput);
        }

        /// <summary>
        /// 更新DataGrid
        /// </summary>
        private void RefreshSource(EScreenCode sc = EScreenCode.All)
        {
            if (sc == EScreenCode.All || sc == EScreenCode.LocalIo)
            {
                //LocalIoSource = null;
                //LocalIoSource = new List<LocalIo_No>(io.LocalIoList);
                LocalIoSource = io.LocalIoList.DeepClone();
            }

            if (sc == EScreenCode.All || sc == EScreenCode.RioInput)
            {
                //RemoteIoInputSource = null;
                //RemoteIoInputSource = new List<RemoteIo_No>(io.RemoteIoInputList);
                RemoteIoInputSource = io.RemoteIoInputList.DeepClone();
            }

            if (sc == EScreenCode.All || sc == EScreenCode.RioOutput)
            {
                if (OutputTypeSelect == "RealTime")
                {
                    //RemoteIoOutputSource = null;
                    //RemoteIoOutputSource = new List<RemoteIo_No>(io.RemoteIoOutputList);
                    RemoteIoOutputSource = io.RemoteIoOutputList.DeepClone();
                }
            }
        }

        /********************
         * 繫結
         *******************/
        private List<LocalIo_No> _localIoSource;
        public List<LocalIo_No> LocalIoSource
        {
            get { return _localIoSource; }
            set { SetProperty(ref _localIoSource, value); }
        }

        private List<RemoteIo_No> _inputIoSource;
        public List<RemoteIo_No> RemoteIoInputSource
        {
            get { return _inputIoSource; }
            set { SetProperty(ref _inputIoSource, value); }
        }

        private List<RemoteIo_No> _outputIoSource;
        public List<RemoteIo_No> RemoteIoOutputSource
        {
            get { return _outputIoSource; }
            set { SetProperty(ref _outputIoSource, value); }
        }

        private string _outputTypeSelect;
        public string OutputTypeSelect
        {
            get { return _outputTypeSelect; }
            set { SetProperty(ref _outputTypeSelect, value); }
        }

        /********************
         * 測試用
         *******************/
        //private void TEST()
        //    => epcio.ErrorReset();
    }
}
