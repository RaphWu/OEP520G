using EPCIO;
using OEP520G.Core;
using OEP520G.Functions;
using OEP520G.Parameter;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace OEP520G.Teaching.ViewModels
{
    public class ClampCorrectViewModel : BindableBase, IActiveAware
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly Machine machine = Machine.Instance;
        private readonly Nozzle nozzle = Nozzle.Instance;
        private readonly Clamp clamp = Clamp.Instance;
        private readonly Stage stage = Stage.Instance;

        private readonly ObjectMotion objectMotion = new ObjectMotion();
        private readonly MeasureHeight measureHeight = new MeasureHeight();

        private EClampId _clampId;

        // 按鍵
        public DelegateCommand StartCorrectCommand { get; private set; }
        public DelegateCommand UpdateDataCommand { get; private set; }
        public DelegateCommand AllClampUpCommand { get; private set; }

        // 視窗Active/Deactive
        public event EventHandler IsActiveChanged;
        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                if (!_isActive)
                    ReadData();
            }
        }

        // 全域Save事件
        public DelegateCommand WriteDataCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public ClampCorrectViewModel()
        {
            ReadData();

            // 按鍵
            StartCorrectCommand = new DelegateCommand(StartCorrect);
            UpdateDataCommand = new DelegateCommand(WriteData);
            AllClampUpCommand = new DelegateCommand(AllClampUp);

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
                clamp.Clamp1.ConvertToMoveCamera.X = Clamp1X;
                clamp.Clamp1.ConvertToMoveCamera.Y = Clamp1Y;
                clamp.Clamp2.ConvertToMoveCamera.X = Clamp2X;
                clamp.Clamp2.ConvertToMoveCamera.Y = Clamp2Y;

                clamp.WriteParameter();
            }
        }

        /// <summary>
        /// 取得參數值
        /// </summary>
        public void ReadData()
        {
            //clamp.ReadParameter();

            Clamp1X = clamp.Clamp1.ConvertToMoveCamera.X;
            Clamp1Y = clamp.Clamp1.ConvertToMoveCamera.Y;
            Clamp2X = clamp.Clamp2.ConvertToMoveCamera.X;
            Clamp2Y = clamp.Clamp2.ConvertToMoveCamera.Y;
        }

        /********************
         * 參數作業
         *******************/
        /// <summary>
        /// IO動作間Delay(ms)
        /// </summary>
        private const int actionDelay = 2000;

        /// <summary>
        /// 啟動校正
        /// </summary>
        private async void StartCorrect()
        {
            if (_clampId == EClampId.Clamp1 || _clampId == EClampId.Clamp2)
            {
                await epcio.SafetyPosition();

                // 夾爪位置確認
                Task<MessageBoxResult> confirmClamp = new Task<MessageBoxResult>(() =>
                {
                    return MessageBox.Show("請移動指定的夾爪至下壓位置上方，按下【確定】執行壓印。", "黏土壓印",
                        MessageBoxButton.OKCancel, MessageBoxImage.Information, MessageBoxResult.OK,
                        MessageBoxOptions.DefaultDesktopOnly);
                });
                confirmClamp.Start();
                await confirmClamp.ContinueWith(async x =>
                {
                    if (x.Result == MessageBoxResult.OK)
                    {
                        PointXY posClamp = new PointXY
                        {
                            X = epcio.ServoClamp.GetCurrentPosition(),
                            Y = epcio.ServoTray.GetCurrentPosition()
                        };

                        // 夾爪下壓黏土
                        clamp.ClampDown(_clampId);
                        await Task.Delay(actionDelay);

                        // 夾爪回原位
                        clamp.ClampUp(_clampId);
                        //await Task.Delay(actionDelay);

                        // 移動相機位置確認
                        Task<MessageBoxResult> confirmCamera = new Task<MessageBoxResult>(() =>
                        {
                            return MessageBox.Show("請將移動相機移至壓印的中心點，再按下【確定】執行校正。", "壓印中心位置確認",
                                MessageBoxButton.OKCancel, MessageBoxImage.Information, MessageBoxResult.OK,
                                MessageBoxOptions.DefaultDesktopOnly);
                        });
                        confirmCamera.Start();
                        await confirmCamera.ContinueWith(x =>
                       {
                           if (x.Result == MessageBoxResult.OK)
                           {
                               // 座標計算
                               PointXY posCamera = new PointXY
                               {
                                   X = epcio.ServoX.GetCurrentPosition(),
                                   Y = epcio.ServoTray.GetCurrentPosition()
                               };

                               // 換算與移動相機的相對關係
                               PointXY convertClamp = new PointXY
                               {
                                   X = posCamera.X - stage.RotateCenter.X,
                                   Y = posClamp.Y - posCamera.Y
                               };

                               // 儲存
                               if (_clampId == EClampId.Clamp1)
                               {
                                   Clamp1X = convertClamp.X;
                                   Clamp1Y = convertClamp.Y;
                                   clamp.Clamp1.StageCoordination.X = posClamp.X + convertClamp.X;
                                   clamp.Clamp1.StageCoordination.Y = stage.RotateCenter.Y + convertClamp.Y;
                               }
                               else if (_clampId == EClampId.Clamp2)
                               {
                                   Clamp2X = convertClamp.X;
                                   Clamp2Y = convertClamp.Y;
                                   clamp.Clamp2.StageCoordination.X = posClamp.X + convertClamp.X;
                                   clamp.Clamp2.StageCoordination.Y = stage.RotateCenter.Y + convertClamp.Y;
                               }
                           }
                       });
                    }
                });
            }
        }

        /// <summary>
        /// 全部夾爪上升
        /// </summary>
        private void AllClampUp() => clamp.AllClampUp();

        /********************
         * 繫結
         *******************/
        private string _clampSelect;
        public string ClampSelect
        {
            get { return _clampSelect; }
            set
            {
                SetProperty(ref _clampSelect, value);
                _clampId = Enum.Parse<EClampId>(value);
            }
        }

        // 座標
        private double _clamp1X;
        public double Clamp1X
        {
            get { return _clamp1X; ; }
            set { SetProperty(ref _clamp1X, value); }
        }

        private double _clamp1Y;
        public double Clamp1Y
        {
            get { return _clamp1Y; ; }
            set { SetProperty(ref _clamp1Y, value); }
        }

        private double _clamp2X;
        public double Clamp2X
        {
            get { return _clamp2X; ; }
            set { SetProperty(ref _clamp2X, value); }
        }

        private double _clamp2Y;
        public double Clamp2Y
        {
            get { return _clamp2Y; ; }
            set { SetProperty(ref _clamp2Y, value); }
        }
    }
}
