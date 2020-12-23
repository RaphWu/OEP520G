using OEP520G.Core;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace OEP520G.Parameter.ViewModels
{
    public class HeadStageData
    {
        public int StageNo { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double R { get; set; }
        public double Z { get; set; }
    }

    public class StageViewModel : BindableBase, IActiveAware
    {
        private Stage stage = Stage.Instance;

        private List<HeadStageData> HeadStageDatas;

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
        public StageViewModel()
        {
            HeadStageDatas = new List<HeadStageData>();
            HeadStageDatas.Add(new HeadStageData { StageNo = 1 });

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
                stage.RotateCenter.X = HeadStageDatas[0].X;
                stage.RotateCenter.Y = HeadStageDatas[0].Y;

                stage.WriteParameter();
            }
        }

        /// <summary>
        /// 取得參數值
        /// </summary>
        public void ReadData()
        {
            HeadStageDatas[0].X = stage.RotateCenter.X;
            HeadStageDatas[0].Y = stage.RotateCenter.Y;
            //HeadStageDatas[0].Z = stage

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
            DataSource = HeadStageDatas;
        }

        /********************
         * 繫結
         *******************/
        private List<HeadStageData> _dataSource;
        public List<HeadStageData> DataSource
        {
            get { return _dataSource; }
            set { SetProperty(ref _dataSource, value); }
        }
    }
}
