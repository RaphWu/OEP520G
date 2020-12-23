using EPCIO;
using OEP520G.Core;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace OEP520G.Parameter.ViewModels
{
    class MotionCardParameterViewModel : BindableBase, IActiveAware
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly Device module = new Device();

        // 視窗Active/Deactive
        public event EventHandler IsActiveChanged;
        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                //if (!value)
                    //epcio.UpdateMcclVar();
            }
        }

        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        // 事件聚合器引用
        private readonly IEventAggregator _ea;

        /// <summary>
        /// 建構函式
        /// </summary>
        public MotionCardParameterViewModel(IEventAggregator ea)
        {
            _ea = ea;

            // 全域Save事件
            WriteDataCommand = new DelegateCommand(WriteData);
            ApplicationCommands.WriteCommand.RegisterCommand(WriteDataCommand);

            GetData();
        }

        /********************
         * 參數作業
         *******************/
        /// <summary>
        /// 存回參數值
        /// </summary>
        public void WriteData()
        {
            if (IsActive)
            {
                // Z軸安全高度
                epcio.SafetyZ = SafetyZ;
                epcio.MaxSpeed = MaxSpeed;

                // 參數儲存
                epcio.CoordinateMode = CoordinateModeSelected;
                epcio.InterpolationTime = InterpolationTime;
                epcio.QueueSize = QueueSize;

                epcio.WriteParameter();

                // 重設新參數(不可在此呼叫，否則存檔動作會將軸卡資料歸零)
                //epcio.UpdateParam();
            }
        }

        /// <summary>
        /// 取得參數值
        /// </summary>
        public void GetData()
        {
            // Z軸安全高度
            SafetyZ = epcio.SafetyZ;
            MaxSpeed = epcio.MaxSpeed;

            // Coordinate Mode 座標型態
            CoordinateModeList = module.CoordinateModeList;
            CoordinateModeSelected = epcio.CoordinateMode;

            InterpolationTime = epcio.InterpolationTime;
            QueueSize = epcio.QueueSize;
        }

        /********************
         * 資料/方法繫結
         *******************/
        // Z軸安全高度
        private double _safetyZ;
        public double SafetyZ
        {
            get { return _safetyZ; }
            set { SetProperty(ref _safetyZ, value); }
        }

        // Max Speed 最大進給速度
        private double _maxSpeed;
        public double MaxSpeed
        {
            get { return _maxSpeed; }
            set { SetProperty(ref _maxSpeed, value); }
        }

        // Coordinate Mode 座標型態
        private List<string> _coordinateModeList;
        public List<string> CoordinateModeList
        {
            get { return _coordinateModeList; }
            set { SetProperty(ref _coordinateModeList, value); }
        }

        private int _coordinateModeSelected;
        public int CoordinateModeSelected
        {
            get { return (_coordinateModeSelected - 1); }
            set { SetProperty(ref _coordinateModeSelected, value + 1); }
        }

        // Interpolation Time 插值時間
        private int _interpolationTime;
        public int InterpolationTime
        {
            get { return _interpolationTime; }
            set { SetProperty(ref _interpolationTime, value); }
        }

        // Queue Size 運動命令緩衝區的大小
        private int _queueSize;
        public int QueueSize
        {
            get { return _queueSize; }
            set { SetProperty(ref _queueSize, value); }
        }
    }
}
