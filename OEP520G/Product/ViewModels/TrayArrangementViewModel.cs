using EPCIO;
using OEP520G.Core;
using OEP520G.Parameter;
using OEP520G.Product.Views;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;

namespace OEP520G.Product.ViewModels
{
    public class TrayArrangementViewModel : BindableBase, IActiveAware, IProductManager
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly Nozzle nozzle = Nozzle.Instance;
        //private readonly Motion motion = Motion.Instance;
        //private readonly List<Motion.Servo> servos = Motion.Instance.Servos;
        //private readonly IO io = IO.Instance;
        private readonly Tray tray = Tray.Instance;

        // Tray選擇列表
        public class OriginListInfo
        {
            public int OriginNo { get; set; }
            public double OriginX { get; set; }
            public double OriginY { get; set; }
            public double OriginR { get; set; }
            public string PartName { get; set; }
        }

        public class MatrixListInfo
        {
            public int PosX { get; set; }
            public int PosY { get; set; }
            public double MatrixX { get; set; }
            public double MatrixY { get; set; }
            public double MatrixR { get; set; }
        }

        private List<string> TraySelect = new List<string>();
        private List<OriginListInfo> OriginList = new List<OriginListInfo>();
        private List<MatrixListInfo> MatrixList = new List<MatrixListInfo>();

        //
        private const string ARRANGEMENT_NAME = "排列組合";

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
                    UpdateSource();
                    epcio.SetSpeed(EServoSpeed.High);
                }
            }
        }

        public DelegateCommand ActivateTrayFeedCommand { get; private set; }
        public DelegateCommand TraySelectChangedCommand { get; private set; }
        public DelegateCommand GetCurrentPositionCommand { get; private set; }
        public DelegateCommand ApplyPositionCommand { get; private set; }
        public DelegateCommand MatrixChangedCommand { get; private set; }
        public DelegateCommand ForwardCommand { get; private set; }
        public DelegateCommand BackwardCommand { get; private set; }

        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        // 事件聚合器引用
        private readonly IEventAggregator _ea;

        /// <summary>
        /// 建構函式
        /// </summary>
        public TrayArrangementViewModel(IEventAggregator ea)
        {
            _ea = ea;

            //TraySelectChanged();
            //RecreateSource_Tray_Origin();
            //RecreateSource_Matrix(ARRANGEMENT_NAME);
            //TraySelectItem = ARRANGEMENT_NAME;
            UpdateSource();
            SwitchDataGrid();

            ActivateTrayFeedCommand = new DelegateCommand(ActivateTrayFeed);
            TraySelectChangedCommand = new DelegateCommand(TraySelectChanged);
            GetCurrentPositionCommand = new DelegateCommand(GetCurrentPosition);
            ApplyPositionCommand = new DelegateCommand(ApplyPosition);
            MatrixChangedCommand = new DelegateCommand(MatrixChanged);
            ForwardCommand = new DelegateCommand(Forward);
            BackwardCommand = new DelegateCommand(Backward);

            _ea.GetEvent<OnSwitchProduct>().Subscribe(onProductChangeover);
            _ea.GetEvent<AfterSwitchProduct>().Subscribe(afterProductChangeover);

            // 全域Save事件
            WriteDataCommand = new DelegateCommand(WriteData);
            ApplicationCommands.WriteCommand.RegisterCommand(WriteDataCommand);
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
            }
        }


        /// <summary>
        /// 取得參數值
        /// </summary>
        public void ReadData()
        {
        }

        /********************
         * 頁面切換
         *******************/
        private readonly IContainerExtension container;
        private readonly IRegion region;
        private void ActivateTrayFeed()
            => region.Activate(container.Resolve<TraySetting>());

        /********************
         * Source
         *******************/
        /// <inheritdoc/>
        /// <param name="productId">切換後的品種ID</param>
        public void onProductChangeover(string productId)
        {
        }

        /// <inheritdoc/>
        /// <param name="productId">切換後的品種ID</param>
        public void afterProductChangeover(string productId)
        {
            tray.TrayHasChanged = true;
        }

        private void SwitchDataGrid()
        {
            if (TraySelectItem == ARRANGEMENT_NAME)
            {
                OriginListVisibility = true;
                MatrixListVisibility = false;
            }
            else
            {
                OriginListVisibility = false;
                MatrixListVisibility = true;
            }
        }

        private void UpdateSource(bool updateAll = true)
        {
            if (tray.TrayHasChanged)
            {
                RecreateSource_Tray_Origin();
                RecreateSource_Matrix(ARRANGEMENT_NAME);

                updateAll = true;
            }

            if (updateAll)
            {
                TraySelectSource = null;
                TraySelectSource = TraySelect;
                TraySelectIndex = 0;

                OriginListSource = null;
                OriginListSource = OriginList;
                OriginListIndex = 0;
            }

            MatrixListSource = null;
            MatrixListSource = MatrixList;
            MatrixListIndex = 0;

            tray.TrayHasChanged = false;
        }

        private void RecreateSource_Tray_Origin()
        {
            //TraySelect.Clear();
            //OriginList.Clear();

            //TraySelect.Add(ARRANGEMENT_NAME);

            //int OriginListNo = 1;
            //foreach (TrayData ti in tray.TrayList)
            //{
            //    TraySelect.Add(ti.Name);

            //    OriginList.Add(new OriginListInfo()
            //    {
            //        OriginNo = OriginListNo++,
            //        OriginX = ti.OriginX,
            //        OriginY = ti.OriginY,
            //        OriginR = ti.OriginR,
            //        PartName = ti.Name
            //    });
            //}
        }

        private void RecreateSource_Matrix(string trayName)
        {
            //MatrixList.Clear();

            //if (trayName != ARRANGEMENT_NAME)
            //{
            //    tray.GetTray(trayName, out TrayData ti);

            //    if (ti != null)
            //    {
            //        int totalPm = ti.PositionNo.Count;

            //        for (int idx = 0; idx < totalPm; idx++)
            //        {
            //            MatrixList.Add(new MatrixListInfo()
            //            {
            //                PosX = ti.PositionX[idx],
            //                PosY = ti.PositionY[idx],
            //                MatrixX = ti.PointMatrixX[idx],
            //                MatrixY = ti.PointMatrixY[idx],
            //                MatrixR = ti.PointMatrixR[idx]
            //            });
            //        }
            //    }
            //}
        }

        /********************
         * 
         *******************/
        private void TraySelectChanged()
        {
            SwitchDataGrid();
            UpdateSource(false);
            RecreateSource_Matrix(TraySelectItem);
        }

        private void MatrixChanged()
        {
            if (CameraSynchronize)
            {
                var t = tray.GetTrayData(TraySelectItem);

                epcio.MoveTo(
                    positionX: t.DatumX + t.OffsetX + MatrixListItem.MatrixX,
                    positionY: t.DatumY + t.OffsetY + MatrixListItem.MatrixY
                    );

                epcio.WaitingForMotionStop(waitingServoX: true, waitingServoTray: true);

                PointPositionX = epcio.ServoX.GetCurrentPosition();
                PointPositionY = epcio.ServoY.GetCurrentPosition();
            }
        }

        /********************
         * 按鍵
         *******************/
        private void Forward()
        {
            if (TraySelectItem == ARRANGEMENT_NAME)
            {
                OriginListIndex++;
                if (OriginListIndex >= OriginListSource.Count)
                    OriginListIndex = OriginListSource.Count - 1;
            }
            else
            {
                MatrixListIndex++;
                if (MatrixListIndex >= MatrixListSource.Count)
                    MatrixListIndex = MatrixListSource.Count - 1;
            }
        }

        private void Backward()
        {
            if (TraySelectItem == ARRANGEMENT_NAME)
            {
                OriginListIndex--;
                if (OriginListIndex < 0)
                    OriginListIndex = 0;
            }
            else
            {
                MatrixListIndex--;
                if (MatrixListIndex < 0)
                    MatrixListIndex = 0;
            }
        }

        private void GetCurrentPosition()
        {
            var t = tray.GetTrayData(TraySelectItem);

            CurrentPositionX = epcio.ServoX.GetCurrentPosition() + t.DatumX;
            CurrentPositionY = epcio.ServoY.GetCurrentPosition() + t.DatumY;
            PointPositionX = CurrentPositionX;
            PointPositionY = CurrentPositionY;
        }

        private void ApplyPosition()
        {

        }

        /********************
         * ListBox繫結
         *******************/
        private List<string> _trayTraySelectSource;
        public List<string> TraySelectSource
        {
            get { return _trayTraySelectSource; }
            set { SetProperty(ref _trayTraySelectSource, value); }
        }

        private string _trayTraySelectItem;
        public string TraySelectItem
        {
            get { return _trayTraySelectItem; }
            set { SetProperty(ref _trayTraySelectItem, value); }
        }

        private int _trayTraySelectIndex;
        public int TraySelectIndex
        {
            get { return _trayTraySelectIndex; }
            set { SetProperty(ref _trayTraySelectIndex, value); }
        }

        /********************
         * 排列組合DataGrid繫結
         *******************/
        private List<OriginListInfo> _originListSource;
        public List<OriginListInfo> OriginListSource
        {
            get { return _originListSource; }
            set { SetProperty(ref _originListSource, value); }
        }

        private OriginListInfo _originListItem;
        public OriginListInfo OriginListItem
        {
            get { return _originListItem; }
            set { SetProperty(ref _originListItem, value); }
        }

        private int _originListIndex;
        public int OriginListIndex
        {
            get { return _originListIndex; }
            set { SetProperty(ref _originListIndex, value); }
        }

        private bool _originListVisibility;
        public bool OriginListVisibility
        {
            get { return _originListVisibility; }
            set { SetProperty(ref _originListVisibility, value); }
        }

        /********************
         * Tray座標列表DataGrid繫結
         *******************/
        private List<MatrixListInfo> _matrixListSource;
        public List<MatrixListInfo> MatrixListSource
        {
            get { return _matrixListSource; }
            set { SetProperty(ref _matrixListSource, value); }
        }

        private MatrixListInfo _matrixListItem;
        public MatrixListInfo MatrixListItem
        {
            get { return _matrixListItem; }
            set { SetProperty(ref _matrixListItem, value); }
        }

        private int _matrixListIndex;
        public int MatrixListIndex
        {
            get { return _matrixListIndex; }
            set { SetProperty(ref _matrixListIndex, value); }
        }

        private bool _matrixListVisibility;
        public bool MatrixListVisibility
        {
            get { return _matrixListVisibility; }
            set { SetProperty(ref _matrixListVisibility, value); }
        }



        /********************
         * 影像繫結
         *******************/


        /********************
         * 座標區繫結
         *******************/
        private bool _cameraSynchronize;
        public bool CameraSynchronize
        {
            get { return _cameraSynchronize; }
            set { SetProperty(ref _cameraSynchronize, value); }
        }

        private double _currentPositionX;
        public double CurrentPositionX
        {
            get { return _currentPositionX; }
            set { SetProperty(ref _currentPositionX, value); }
        }

        private double _currentPositionY;
        public double CurrentPositionY
        {
            get { return _currentPositionY; }
            set { SetProperty(ref _currentPositionY, value); }
        }

        private double _pointPositionX;
        public double PointPositionX
        {
            get { return _pointPositionX; }
            set { SetProperty(ref _pointPositionX, value); }
        }

        private double _pointPositionY;
        public double PointPositionY
        {
            get { return _pointPositionY; }
            set { SetProperty(ref _pointPositionY, value); }
        }
    }
}
