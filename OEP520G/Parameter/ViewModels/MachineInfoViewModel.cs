using OEP520G.Core;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using System;

namespace OEP520G.Parameter.ViewModels
{
    public class MachineInfoViewModel : BindableBase, IActiveAware
    {
        // Model
        private readonly Machine machine = Machine.Instance;
        //private readonly ProductManager pm = ProductManager.Instance;

        // 視窗Active/Deactive
        public event EventHandler IsActiveChanged;
        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        // Window Loaded
        public DelegateCommand HandleLoadedCommand { get; private set; }

        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public MachineInfoViewModel()
        {
            // Window Loaded
            HandleLoadedCommand = new DelegateCommand(HandleLoaded);

            // 全域Save事件
            WriteDataCommand = new DelegateCommand(WriteData);
            ApplicationCommands.WriteCommand.RegisterCommand(WriteDataCommand);
        }

        /// <summary>
        /// Window Loaded
        /// </summary>
        private void HandleLoaded() => ReadData();

        /// <summary>
        /// 讀取Machine資料
        /// </summary>
        private void ReadData()
        {
            MachineId = Machine.MachineId;
            MachineName = Machine.MachineName;
        }

        /// <summary>
        /// 儲存Machine資料
        /// </summary>
        public void WriteData()
        {
            if (IsActive)
            {
                Machine.MachineId = MachineId;
                Machine.MachineName = MachineName;

                machine.WriteParameter();
            }
        }

        /********************
         * 資料/方法繫結
         *******************/
        private string _machineId;
        public string MachineId
        {
            get { return _machineId; }
            set { SetProperty(ref _machineId, value); }
        }

        private string _machineName;
        public string MachineName
        {
            get { return _machineName; }
            set { SetProperty(ref _machineName, value); }
        }
    }
}
