using OEP520G.Core;
using OEP520G.Parameter;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace OEP520G.Parameter.ViewModels
{
    public class NozzleData
    {
        public string ObjName { get; set; }
        public PointXYZ Position { get; set; }
        public IntPointXYZ Encoder { get; set; }
        public PointXYZ OffsetPosition { get; set; }
        public IntPointXYZ OffsetEncoder { get; set; }
    }

    public class NozzleViewModel : BindableBase, IActiveAware
    {
        private Nozzle nozzle = Nozzle.Instance;
        private readonly Dispenser dispenser = Dispenser.Instance;

        private List<NozzleData> NozzleDatas;

        // 視窗Active/Deactive
        public event EventHandler IsActiveChanged;
        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        public DelegateCommand UpdateDataCommand { get; private set; }
        public DelegateCommand RestoreCoordinateCommand { get; private set; }

        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public NozzleViewModel()
        {
            NozzleDatas = new List<NozzleData>();
            NozzleDatas.Add(new NozzleData { ObjName = "點膠針頭" });
            NozzleDatas.Add(new NozzleData { ObjName = "吸嘴1" });
            NozzleDatas.Add(new NozzleData { ObjName = "吸嘴2" });
            NozzleDatas.Add(new NozzleData { ObjName = "吸嘴3" });
            NozzleDatas.Add(new NozzleData { ObjName = "吸嘴4" });
            NozzleDatas.Add(new NozzleData { ObjName = "吸嘴5" });
            NozzleDatas.Add(new NozzleData { ObjName = "吸嘴6" });
            NozzleDatas.Add(new NozzleData { ObjName = "吸嘴7" });
            NozzleDatas.Add(new NozzleData { ObjName = "吸嘴8" });
            NozzleDatas.Add(new NozzleData { ObjName = "吸嘴9" });
            NozzleDatas.Add(new NozzleData { ObjName = "吸嘴10" });
            NozzleDatas.Add(new NozzleData { ObjName = "吸嘴11" });

            for (int noz = 0; noz <= Nozzle.MAX_NOZZLE; noz++)
            {
                NozzleDatas[noz].Position = new PointXYZ();
                NozzleDatas[noz].Encoder = new IntPointXYZ();
                NozzleDatas[noz].OffsetPosition = new PointXYZ();
                NozzleDatas[noz].OffsetEncoder = new IntPointXYZ();
            }

            ReadData();

            UpdateDataCommand = new DelegateCommand(UpdateData);
            RestoreCoordinateCommand = new DelegateCommand(RestoreCoordinate);

            // 全域Save事件
            WriteDataCommand = new DelegateCommand(WriteData);
            ApplicationCommands.WriteCommand.RegisterCommand(WriteDataCommand);
        }

        /// <summary>
        /// 全域Save事件
        /// </summary>
        private void WriteData()
        {
            if (IsActive)
            {
                dispenser.Position = NozzleDatas[0].Position;
                dispenser.Encoder = NozzleDatas[0].Encoder;
                dispenser.WriteParameter();

                for (int noz = 1; noz <= Nozzle.MAX_NOZZLE; noz++)
                {
                    nozzle.NozzleList[noz - 1].Position = NozzleDatas[noz].Position;
                    nozzle.NozzleList[noz - 1].Encoder = NozzleDatas[noz].Encoder;
                }
                nozzle.WriteParameter();
            }
        }

        /// <summary>
        /// 取得參數值
        /// </summary>
        public void ReadData()
        {
            NozzleDatas[0].Position = dispenser.Position;
            NozzleDatas[0].Encoder = dispenser.Encoder;

            for (int noz = 1; noz <= Nozzle.MAX_NOZZLE; noz++)
            {
                NozzleDatas[noz].Position = nozzle.NozzleList[noz - 1].Position;
                NozzleDatas[noz].Encoder = nozzle.NozzleList[noz - 1].Encoder;
            }

            RefreshDataGrid();
        }

        /********************
         * 按鍵
         *******************/
        private void UpdateData() => WriteData();

        private void RestoreCoordinate() => ReadData();

        /********************
         * DataGrid
         *******************/
        private void RefreshDataGrid()
        {
            DataSource = null;
            DataSource = NozzleDatas;
        }

        /********************
         * 繫結
         *******************/
        private List<NozzleData> _dataSource;
        public List<NozzleData> DataSource
        {
            get { return _dataSource; }
            set { SetProperty(ref _dataSource, value); }
        }
    }
}
