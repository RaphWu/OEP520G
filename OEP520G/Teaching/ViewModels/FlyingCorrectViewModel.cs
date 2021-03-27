using EPCIO;
using Imageproject.Contracts;
using OEP520G.Core;
using OEP520G.Functions;
using OEP520G.Parameter;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace OEP520G.Teaching.ViewModels
{
    public class FlyingCorrectViewModel : BindableBase, IActiveAware
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly Nozzle nozzle = Nozzle.Instance;
        private readonly FlyCorrect flyCorrect;

        /********************
         * 按鍵
         *******************/
        public DelegateCommand SelectAllNozzleCommand { get; private set; }
        public DelegateCommand UnselectAllNozzleCommand { get; private set; }
        public DelegateCommand<string> SelectAllCheckBoxCommand { get; private set; }
        public DelegateCommand<string> UnselectAllCheckBoxCommand { get; private set; }

        public DelegateCommand StartCorrectCommand { get; private set; }
        public DelegateCommand StopCorrectCommand { get; private set; }
        public DelegateCommand UpdateCommand { get; private set; }
        public DelegateCommand PredictedCoordinatesCommand { get; private set; }

        // Cancellation Token
        private CancellationTokenSource cts = new CancellationTokenSource();

        // 視窗Active/Deactive
        public event EventHandler IsActiveChanged;
        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                epcio.SafetyPosition();

                if (!value)
                {
                    // 離開頁面時回至安全位置
                    epcio.SafetyPosition();
                }
            }
        }

        private readonly ICamera _image;

        /// <summary>
        /// 建構函式
        /// </summary>
        public FlyingCorrectViewModel(ICamera image)
        {
            _image = image;
            flyCorrect = new FlyCorrect(_image, false);

            InStopEnable = true;
            InRunEnable = false;

            FlyWaySelected = "ENC";

            SelectAllNozzle();

            // 按鍵繫結
            SelectAllNozzleCommand = new DelegateCommand(SelectAllNozzle);
            UnselectAllNozzleCommand = new DelegateCommand(UnselectAllNozzle);
            SelectAllCheckBoxCommand = new DelegateCommand<string>(SelectAllCheckBox);
            UnselectAllCheckBoxCommand = new DelegateCommand<string>(UnselectAllCheckBox);

            StartCorrectCommand = new DelegateCommand(StartCorrect);
            StopCorrectCommand = new DelegateCommand(StopCorrect);
            UpdateCommand = new DelegateCommand(Update);
            //PredictedCoordinatesCommand = new DelegateCommand(PredictedCoordinates);
        }

        /// <summary>
        /// 將校正資料更新為飛行資料
        /// </summary>
        private void Update()
        {
            for (int idx = 0; idx < Nozzle.MAX_NOZZLE * 3; idx++)
            {
                FlyData fdi = flyCorrect.FlyDatas[idx];
                fdi.NozzleEnable = new bool[Nozzle.MAX_NOZZLE];

                //if (fdi.Update)
                //{
                //fdi.TimeMarker = fdi.NewTimeMarker;
                //fdi.EncMarker = fdi.NewEncMarker;
                //}
            }

            RefreshDataSource();
        }

        /********************
         * DataGrid
         *******************/
        private ObservableCollection<FlyData> flyDataSource;

        public void RefreshDataSource()
        {
            flyDataSource = new ObservableCollection<FlyData>();
            flyCorrect.FlyDatas.ForEach(x => flyDataSource.Add(x));
            DataGridSource = flyDataSource;
        }

        /********************
         * 按鍵
         *******************/
        private async void StartCorrect()
        {
            InStopEnable = false;
            InRunEnable = true;

            cts.Dispose();
            cts = new CancellationTokenSource();

            if (UltraHighSpeedSelected)
            {
                await flyCorrect.StartCorrect(EServoSpeed.UltraHigh, FlyWaySelected, cts);
                RefreshDataSource();
            }

            if (HighSpeedSelected)
            {
                await flyCorrect.StartCorrect(EServoSpeed.High, FlyWaySelected, cts);
                RefreshDataSource();
            }

            if (MiddleSpeedSelected)
            {
                await flyCorrect.StartCorrect(EServoSpeed.Middle, FlyWaySelected, cts);
                RefreshDataSource();
            }

            // TODO: 讀取結果

            //RefreshDataSource();

            InStopEnable = true;
            InRunEnable = false;
        }

        private void StopCorrect()
        {
            if (cts != null && !cts.IsCancellationRequested)
                cts.Cancel();
        }

        private void SelectAllNozzle()
        {
            NozzleSelect1 = true;
            NozzleSelect2 = true;
            NozzleSelect3 = true;
            NozzleSelect4 = true;
            NozzleSelect5 = true;
            NozzleSelect6 = true;
            NozzleSelect7 = true;
            NozzleSelect8 = true;
            NozzleSelect9 = true;
            NozzleSelect10 = true;
            NozzleSelect11 = true;
        }

        private void UnselectAllNozzle()
        {
            NozzleSelect1 = false;
            NozzleSelect2 = false;
            NozzleSelect3 = false;
            NozzleSelect4 = false;
            NozzleSelect5 = false;
            NozzleSelect6 = false;
            NozzleSelect7 = false;
            NozzleSelect8 = false;
            NozzleSelect9 = false;
            NozzleSelect10 = false;
            NozzleSelect11 = false;
        }

        private void SelectAllCheckBox(string speed)
        {
            if (speed == GlobalString.SpeedName[0])
            {
                flyCorrect.FlyDatas[0].Update = true;
                flyCorrect.FlyDatas[1].Update = true;
                flyCorrect.FlyDatas[2].Update = true;
                flyCorrect.FlyDatas[3].Update = true;
                flyCorrect.FlyDatas[4].Update = true;
                flyCorrect.FlyDatas[5].Update = true;
                flyCorrect.FlyDatas[6].Update = true;
                flyCorrect.FlyDatas[7].Update = true;
                flyCorrect.FlyDatas[8].Update = true;
                flyCorrect.FlyDatas[9].Update = true;
                flyCorrect.FlyDatas[10].Update = true;
            }
            else if (speed == GlobalString.SpeedName[1])
            {
                flyCorrect.FlyDatas[11].Update = true;
                flyCorrect.FlyDatas[12].Update = true;
                flyCorrect.FlyDatas[13].Update = true;
                flyCorrect.FlyDatas[14].Update = true;
                flyCorrect.FlyDatas[15].Update = true;
                flyCorrect.FlyDatas[16].Update = true;
                flyCorrect.FlyDatas[17].Update = true;
                flyCorrect.FlyDatas[18].Update = true;
                flyCorrect.FlyDatas[19].Update = true;
                flyCorrect.FlyDatas[20].Update = true;
                flyCorrect.FlyDatas[21].Update = true;
            }
            else if (speed == GlobalString.SpeedName[2])
            {
                flyCorrect.FlyDatas[22].Update = true;
                flyCorrect.FlyDatas[23].Update = true;
                flyCorrect.FlyDatas[24].Update = true;
                flyCorrect.FlyDatas[25].Update = true;
                flyCorrect.FlyDatas[26].Update = true;
                flyCorrect.FlyDatas[27].Update = true;
                flyCorrect.FlyDatas[28].Update = true;
                flyCorrect.FlyDatas[29].Update = true;
                flyCorrect.FlyDatas[30].Update = true;
                flyCorrect.FlyDatas[31].Update = true;
                flyCorrect.FlyDatas[32].Update = true;
            }
        }

        private void UnselectAllCheckBox(string speed)
        {
            switch (speed)
            {
                case "ULTRA_HIGH":
                    flyCorrect.FlyDatas[0].Update = false;
                    flyCorrect.FlyDatas[1].Update = false;
                    flyCorrect.FlyDatas[2].Update = false;
                    flyCorrect.FlyDatas[3].Update = false;
                    flyCorrect.FlyDatas[4].Update = false;
                    flyCorrect.FlyDatas[5].Update = false;
                    flyCorrect.FlyDatas[6].Update = false;
                    flyCorrect.FlyDatas[7].Update = false;
                    flyCorrect.FlyDatas[8].Update = false;
                    flyCorrect.FlyDatas[9].Update = false;
                    flyCorrect.FlyDatas[10].Update = false;
                    break;
                case "HIGH":
                    flyCorrect.FlyDatas[11].Update = false;
                    flyCorrect.FlyDatas[12].Update = false;
                    flyCorrect.FlyDatas[13].Update = false;
                    flyCorrect.FlyDatas[14].Update = false;
                    flyCorrect.FlyDatas[15].Update = false;
                    flyCorrect.FlyDatas[16].Update = false;
                    flyCorrect.FlyDatas[17].Update = false;
                    flyCorrect.FlyDatas[18].Update = false;
                    flyCorrect.FlyDatas[19].Update = false;
                    flyCorrect.FlyDatas[20].Update = false;
                    flyCorrect.FlyDatas[21].Update = false;
                    break;
                case "MIDDLE":
                    flyCorrect.FlyDatas[22].Update = false;
                    flyCorrect.FlyDatas[23].Update = false;
                    flyCorrect.FlyDatas[24].Update = false;
                    flyCorrect.FlyDatas[25].Update = false;
                    flyCorrect.FlyDatas[26].Update = false;
                    flyCorrect.FlyDatas[27].Update = false;
                    flyCorrect.FlyDatas[28].Update = false;
                    flyCorrect.FlyDatas[29].Update = false;
                    flyCorrect.FlyDatas[30].Update = false;
                    flyCorrect.FlyDatas[31].Update = false;
                    flyCorrect.FlyDatas[32].Update = false;
                    break;
            }
        }

        /********************
         * 繫結
         *******************/
        // DataGrid
        private ObservableCollection<FlyData> _dataGridSource;
        public ObservableCollection<FlyData> DataGridSource
        {
            get { return _dataGridSource; }
            set { SetProperty(ref _dataGridSource, value); }
        }

        //private FlyData _flyDataSelected;
        //public FlyData FlyDataSelected
        //{
        //    get { return _flyDataSelected; }
        //    set { SetProperty(ref _flyDataSelected, value); }
        //}

        // 吸嘴
        private bool _nozzleSelect1;
        public bool NozzleSelect1
        {
            get { return _nozzleSelect1; }
            set { SetProperty(ref _nozzleSelect1, value); }
        }

        private bool _nozzleSelect2;
        public bool NozzleSelect2
        {
            get { return _nozzleSelect2; }
            set { SetProperty(ref _nozzleSelect2, value); }
        }

        private bool _nozzleSelect3;
        public bool NozzleSelect3
        {
            get { return _nozzleSelect3; }
            set { SetProperty(ref _nozzleSelect3, value); }
        }

        private bool _nozzleSelect4;
        public bool NozzleSelect4
        {
            get { return _nozzleSelect4; }
            set { SetProperty(ref _nozzleSelect4, value); }
        }

        private bool _nozzleSelect5;
        public bool NozzleSelect5
        {
            get { return _nozzleSelect5; }
            set { SetProperty(ref _nozzleSelect5, value); }
        }

        private bool _nozzleSelect6;
        public bool NozzleSelect6
        {
            get { return _nozzleSelect6; }
            set { SetProperty(ref _nozzleSelect6, value); }
        }

        private bool _nozzleSelect7;
        public bool NozzleSelect7
        {
            get { return _nozzleSelect7; }
            set { SetProperty(ref _nozzleSelect7, value); }
        }

        private bool _nozzleSelect8;
        public bool NozzleSelect8
        {
            get { return _nozzleSelect8; }
            set { SetProperty(ref _nozzleSelect8, value); }
        }

        private bool _nozzleSelect9;
        public bool NozzleSelect9
        {
            get { return _nozzleSelect9; }
            set { SetProperty(ref _nozzleSelect9, value); }
        }

        private bool _nozzleSelect10;
        public bool NozzleSelect10
        {
            get { return _nozzleSelect10; }
            set { SetProperty(ref _nozzleSelect10, value); }
        }

        private bool _nozzleSelect11;
        public bool NozzleSelect11
        {
            get { return _nozzleSelect11; }
            set { SetProperty(ref _nozzleSelect11, value); }
        }

        // 速度
        private bool _ultraHighSpeedSelected;
        public bool UltraHighSpeedSelected
        {
            get { return _ultraHighSpeedSelected; }
            set { SetProperty(ref _ultraHighSpeedSelected, value); }
        }

        private bool _highSpeedSelected;
        public bool HighSpeedSelected
        {
            get { return _highSpeedSelected; }
            set { SetProperty(ref _highSpeedSelected, value); }
        }

        private bool _middleSpeedSelected;
        public bool MiddleSpeedSelected
        {
            get { return _middleSpeedSelected; }
            set { SetProperty(ref _middleSpeedSelected, value); }
        }

        // 飛行演算方式
        private string _flyWaySelected;
        public string FlyWaySelected
        {
            get { return _flyWaySelected; }
            set { SetProperty(ref _flyWaySelected, value); }
        }


        // 
        private bool _notRunningEnabled;
        public bool InStopEnable
        {
            get { return _notRunningEnabled; }
            set { SetProperty(ref _notRunningEnabled, value); }
        }

        // 
        private bool _runningEnabled;
        public bool InRunEnable
        {
            get { return _runningEnabled; }
            set { SetProperty(ref _runningEnabled, value); }
        }
    }
}

