using Force.DeepCloner;
using OEP520G.Core;
using OEP520G.Product;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace OEP520G.Automatic.ViewModels
{
    public class AutoSequenceViewModel : BindableBase, IActiveAware, IProductManager
    {
        private readonly AutoSequence _autoSequence = AutoSequence.Instance;
        //private readonly AutoOperation autoOperation = AutoOperation.Instance;
        //private readonly SimulateData _sim = new SimulateData();

        // 滑鼠右鍵
        public DelegateCommand ShowTraySettingCommand { get; private set; }

        // DataGrid
        public DelegateCommand<object> RowEditEndingCommand { get; private set; }
        public DelegateCommand<object> CellEditEndingCommand { get; private set; }

        // 視窗Active/Deactive
        public event EventHandler IsActiveChanged;
        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                if (_isActive)
                {
                    ReadData();
                    AllowEdit = false;
                }
            }
        }

        /// <summary>
        /// Event Aggregator
        /// </summary>
        private readonly IEventAggregator _ea;

        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public AutoSequenceViewModel(IEventAggregator ea)
        {
            _ea = ea;

            SequenceDataSource = new ObservableCollection<SequenceData>();

            HeadSelecterSource = new List<string>
            {
                EHead.Dispensor.ToString(), // 1
                EHead.Nozzle01.ToString(),  // 2
                EHead.Nozzle02.ToString(),  // 3
                EHead.Nozzle03.ToString(),  // 4
                EHead.Nozzle04.ToString(),  // 5
                EHead.Nozzle05.ToString(),  // 6
                EHead.Nozzle06.ToString(),  // 7
                EHead.Nozzle07.ToString(),  // 8
                EHead.Nozzle08.ToString(),  // 9
                EHead.Nozzle09.ToString(),  // 10
                EHead.Nozzle10.ToString(), // 11
                EHead.Nozzle11.ToString(), // 12
                EHead.Camera.ToString()    // 13
            };

            ActionSelecterSource = new List<string>
            {
                EAction.PlacePart.ToString() ,   // 1
                EAction.PickUp01.ToString() ,  // 2
                EAction.PickUp02.ToString() ,  // 3
                EAction.PickUp03.ToString() ,  // 4
                EAction.PickUp04.ToString() ,  // 5
                EAction.PickUp05.ToString() ,  // 6
                EAction.PickUp06.ToString() ,  // 7
                EAction.PickUp07.ToString() ,  // 8
                EAction.PickUp08.ToString() ,  // 9
                EAction.PickUp09.ToString() ,  // 10
                EAction.PickUp10.ToString(),  // 11
                EAction.PickUp11.ToString(),  // 12
                EAction.Dispensing.ToString() ,   // 13
                EAction.GlueAmountCheck.ToString() ,// 14
                EAction.PreDispensing.ToString() , // 15
                EAction.ClearGlue.ToString() ,   // 16
                EAction.ImageCheck.ToString()  // 17
            };

            TargetSelecterSource = new List<string>
            {
                ETarget.Stage.ToString() , // 1
                ETarget.Tray01.ToString(), // 2
                ETarget.Tray02.ToString(),
                ETarget.Tray03.ToString(),
                ETarget.Tray04.ToString(),
                ETarget.Tray05.ToString(),
                ETarget.Tray06.ToString(),
                ETarget.Tray07.ToString(),
                ETarget.Tray08.ToString(),
                ETarget.Tray09.ToString(),
                ETarget.Tray10.ToString(),
                ETarget.Tray11.ToString(),
                ETarget.Tray12.ToString(),
                ETarget.Tray13.ToString(),
                ETarget.Tray14.ToString(),
                ETarget.Tray15.ToString(),
                ETarget.Tray16.ToString(),
                ETarget.Tray17.ToString(),
                ETarget.Tray18.ToString(),
                ETarget.Tray19.ToString(),
                ETarget.Tray20.ToString(),
                ETarget.Tray21.ToString(),
                ETarget.Tray22.ToString(),
                ETarget.Tray23.ToString(),
                ETarget.Tray24.ToString(),
                ETarget.Tray25.ToString(),
                ETarget.Tray26.ToString(),
                ETarget.Tray27.ToString(),
                ETarget.Tray28.ToString(),
                ETarget.Tray29.ToString(),
                ETarget.Tray30.ToString()  // 31
            };

            //HeadSelecterSource = TypeOfHead.GetHeadString();
            //ActionSelecterSource = TypeOfAction.GetActionString(Enum.Parse<EHead>(HeadSelecter));
            //TargetSelecterSource = TypeOfTarget.GetTargetString(Enum.Parse<EAction>(ActionSelecterSource[0]));
            //PartIdSelecterSource = _sim.VisualList;
            //TrayIdSelecterSource = _sim.VisualList;

            ReadData();
            AllowEdit = false;

            ShowTraySettingCommand = new DelegateCommand(ShowTraySetting);
            RowEditEndingCommand = new DelegateCommand<object>(RowEditEnding);
            CellEditEndingCommand = new DelegateCommand<object>(CellEditEnding);

            // 訂閱ProductSwitch事件，並設定呼叫SwProduct函式
            _ea.GetEvent<OnSwitchProduct>().Subscribe(onProductChangeover);
            _ea.GetEvent<AfterSwitchProduct>().Subscribe(afterProductChangeover);

            // 全域Save事件
            WriteDataCommand = new DelegateCommand(WriteData);
            ApplicationCommands.WriteCommand.RegisterCommand(WriteDataCommand);
        }

        /// <inheritdoc/>
        /// <param name="productId">切換後的品種ID</param>
        public void onProductChangeover(string productId)
        {
        }

        /// <inheritdoc/>
        /// <param name="productId">切換後的品種ID</param>
        public void afterProductChangeover(string productId)
        {
            _autoSequence.ReadParameter();
            ReadData();
        }

        /********************
        * 參數作業
        *******************/
        /// <summary>
        /// 全域Save事件
        /// </summary>
        private void WriteData()
        {
            if (IsActive)
            {

                _autoSequence.WriteParameter(SequenceDataSource);

                // 重讀排序後的資料
                ReadData();
            }
        }

        /// <summary>
        /// 取得參數值
        /// </summary>
        public void ReadData()
        {
            SelectTree = AutoActionSelector.Instance.Selector;

            SequenceDataSource.Clear();

            foreach (var data in _autoSequence.SequenceDataList)
                SequenceDataSource.Add(data);

            //ReadSequenceDataList();
            //RefreshDataGridSource();
        }

        /********************
        * DataGrid
        *******************/
        private void ReadSequenceDataList()
        {

            //_infoDataGrid.Clear();
            //foreach (SequenceData sd in _autoSequence.SequenceDataList)
            //{
            // _infoDataGrid.Add(new SequenceData()
            // {
            // Sequence = sd.Sequence,
            // HeadSelecter = sd.HeadSelecter,
            // ActionSelecter = sd.ActionSelecter,
            // TargetSelecter = sd.TargetSelecter,
            // PartIdSelecter = sd.PartIdSelecter,
            // TrayIdSelecter = sd.TrayIdSelecter,
            // Effective = sd.Effective,
            // ImageProcessing = sd.ImageProcessing,
            // LaunchStageAfterProcedure = sd.LaunchStageAfterProcedure,
            // SkipAlignmentBeforeCarry = sd.SkipAlignmentBeforeCarry,
            // OpenClampWhenAssembly = sd.OpenClampWhenAssembly,
            // StageReturn0AfterCarry = sd.StageReturn0AfterCarry,
            // SkipPositionCheckWhenAssembly = sd.SkipPositionCheckWhenAssembly,
            // OpenClampWhenSingleProcedure = sd.OpenClampWhenSingleProcedure,
            // MeasureHighAfterAssembly = sd.MeasureHighAfterAssembly,
            // GetCenterAfterStageRotate = sd.GetCenterAfterStageRotate,

            // OriginSequence = sd.Sequence,
            // Edited = ' '
            // });
            //}
        }

        //private void RefreshDataGridSource()
        //{
        // //ProductDataSource = new List<SequenceData>();
        // //ProductDataSource = Common.DeepClone(_infoDataGrid);
        // SequenceDataSource = new ObservableCollection<SequenceData>(InfoDataGrid.DeepClone());
        //}

        private void RowEditEnding(object editAction)
        {
            //if (null != SequenceDataItem)
            //{
            // if ((DataGridEditAction)editAction == DataGridEditAction.Commit)
            // {
            // int indexOrigin = SequenceDataItem.OriginSequence;
            // int index = _infoDataGrid.FindIndex(x => x.OriginSequence == indexOrigin);

            // if (!_infoDataGrid[index].Equals(SequenceDataItem))
            // {
            // _infoDataGrid[index].Sequence = SequenceDataItem.Sequence;
            // _infoDataGrid[index].HeadSelecter = SequenceDataItem.HeadSelecter;
            // _infoDataGrid[index].ActionSelecter = SequenceDataItem.ActionSelecter;
            // _infoDataGrid[index].TargetSelecter = SequenceDataItem.TargetSelecter;
            // _infoDataGrid[index].PartIdSelecter = SequenceDataItem.PartIdSelecter;
            // _infoDataGrid[index].TrayIdSelecter = SequenceDataItem.TrayIdSelecter;
            // _infoDataGrid[index].Effective = SequenceDataItem.Effective;
            // _infoDataGrid[index].ImageProcessing = SequenceDataItem.ImageProcessing;
            // _infoDataGrid[index].LaunchStageAfterProcedure = SequenceDataItem.LaunchStageAfterProcedure;
            // _infoDataGrid[index].SkipAlignmentBeforeCarry = SequenceDataItem.SkipAlignmentBeforeCarry;
            // _infoDataGrid[index].OpenClampWhenAssembly = SequenceDataItem.OpenClampWhenAssembly;
            // _infoDataGrid[index].StageReturn0AfterCarry = SequenceDataItem.StageReturn0AfterCarry;
            // _infoDataGrid[index].SkipPositionCheckWhenAssembly = SequenceDataItem.SkipPositionCheckWhenAssembly;
            // _infoDataGrid[index].OpenClampWhenSingleProcedure = SequenceDataItem.OpenClampWhenSingleProcedure;
            // _infoDataGrid[index].MeasureHighAfterAssembly = SequenceDataItem.MeasureHighAfterAssembly;
            // _infoDataGrid[index].GetCenterAfterStageRotate = SequenceDataItem.GetCenterAfterStageRotate;

            // //_infoDataGrid[index].OriginSequence = SequenceDataItem.Sequence;
            // //_infoDataGrid[index].Edited = '*';

            // _infoDataGrid.Sort((i, j) =>
            // {
            // if ((null == i) || (null == j))
            // return 1;
            // else
            // return i.Sequence - j.Sequence;
            // });
            // }
            // }

            // RefreshDataGridSource();
            //}
        }

        private void CellEditEnding(object para)
        {
            //if (SequenceDataItem != null)
            // SequenceDataItem.Edited = '*';
        }

        /********************
        * 滑鼠右鍵
        *******************/
        private void ShowTraySetting()
        {
            //TraySetting v = new TraySetting();
            //v.Show();
        }

        /********************
        * 繫結
        *******************/
        // DataGrid
        public ObservableCollection<SequenceData> SequenceDataSource
        {
            get { return _sequenceDataSource; }
            set { SetProperty(ref _sequenceDataSource, value); }
        }
        private ObservableCollection<SequenceData> _sequenceDataSource;

        public SequenceData SequenceDataItem
        {
            get { return _sequenceDataItem; }
            set { SetProperty(ref _sequenceDataItem, value); }
        }
        private SequenceData _sequenceDataItem;

        //public int ProductDataIndex
        //{
        // get { return _productDataIndex; }
        // set { SetProperty(ref _productDataIndex, value); }
        //}
        //private int _productDataIndex;

        // Field
        private List<AutoHead> _selectTree;
        public List<AutoHead> SelectTree
        {
            get { return _selectTree; }
            set { SetProperty(ref _selectTree, value); }
        }

        private string _headSelecter;
        public string HeadSelecter
        {
            get { return _headSelecter; }
            set
            {
                SetProperty(ref _headSelecter, value);
                SequenceDataItem.SelectedHead = value;
            }
        }


        public List<string> HeadSelecterSource
        {
            get { return _headSelecterSource; }
            set { SetProperty(ref _headSelecterSource, value); }
        }
        private List<string> _headSelecterSource;

        public List<string> ActionSelecterSource
        {
            get { return _actionSelecterSource; }
            set { SetProperty(ref _actionSelecterSource, value); }
        }
        private List<string> _actionSelecterSource;

        public List<string> TargetSelecterSource
        {
            get { return _targetSelecterSource; }
            set { SetProperty(ref _targetSelecterSource, value); }
        }
        private List<string> _targetSelecterSource;

        public ObservableCollection<string> PartIdSelecterSource
        {
            get { return _partIdSelecterSource; }
            set { SetProperty(ref _partIdSelecterSource, value); }
        }
        private ObservableCollection<string> _partIdSelecterSource;

        public ObservableCollection<string> TrayIdSelecterSource
        {
            get { return _trayIdSelecterSource; }
            set { SetProperty(ref _trayIdSelecterSource, value); }
        }
        private ObservableCollection<string> _trayIdSelecterSource;

        private int _barrelTrayNoSetting;
        public int BarrelTrayNoSetting
        {
            get { return _barrelTrayNoSetting; }
            set { _barrelTrayNoSetting = value; }
        }

        private int _productTrayNoSetting;

        public int ProductTrayNoSetting
        {
            get { return _productTrayNoSetting; }
            set { _productTrayNoSetting = value; }
        }


        /// <summary>
        /// 允許編輯
        /// </summary>
        public bool AllowEdit
        {
            get { return _allowEdit; }
            set { SetProperty(ref _allowEdit, value); }
        }
        private bool _allowEdit;
    }
}
