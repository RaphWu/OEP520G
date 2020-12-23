using EPCIO;
using OEP520G.Functions;
using OEP520G.Parameter;
using Prism.Commands;
using Prism.Mvvm;

namespace OEP520G.Manual.ViewModels
{
    public class CylinderActionViewModel : BindableBase
    {
        private readonly Epcio epcio = Epcio.Instance;
        private readonly Nozzle nozzle = Nozzle.Instance;
        private readonly Clamp clamp = Clamp.Instance;

        // 吸嘴狀態
        private readonly double[] scaleList = new double[] { 0.01, 0.05, 0.1, 1, 5 };
        private double scale;
        private bool[] stateNozzle = new bool[Nozzle.MAX_NOZZLE];

        // 夾爪狀態
        private bool[] stateClamp = new bool[Clamp.MAX_CLAMP];

        /********************
         * 按鍵
         *******************/
        public DelegateCommand<string> SelectNozzleCommand { get; private set; }
        public DelegateCommand<string> UnselectNozzleCommand { get; private set; }
        public DelegateCommand SelectAllNozzleCommand { get; private set; }
        public DelegateCommand UnselectAllNozzleCommand { get; private set; }
        public DelegateCommand InhaleCommand { get; private set; }
        public DelegateCommand VaccumOffCommand { get; private set; }
        public DelegateCommand ExhaleCommand { get; private set; }
        public DelegateCommand CylinderUpCommand { get; private set; }
        public DelegateCommand CylinderDownCommand { get; private set; }
        public DelegateCommand AxisZUpCommand { get; private set; }
        public DelegateCommand AxisZDownCommand { get; private set; }

        public DelegateCommand<string> SelectClampCommand { get; private set; }
        public DelegateCommand<string> UnselectClampCommand { get; private set; }
        public DelegateCommand SelectAllClampCommand { get; private set; }
        public DelegateCommand UnselectAllClampCommand { get; private set; }
        public DelegateCommand ClampOpenCommand { get; private set; }
        public DelegateCommand ClampCloseCommand { get; private set; }
        public DelegateCommand ClampUpCommand { get; private set; }
        public DelegateCommand ClampDownCommand { get; private set; }
        public DelegateCommand ClampCylinderUpCommand { get; private set; }
        public DelegateCommand ClampCylinderDownCommand { get; private set; }

        /// <summary>
        /// 建構函式
        /// </summary>
        public CylinderActionViewModel()
        {
            UnselectAllNozzle();
            UnselectAllClamp();

            ActionScaleZSelected = scaleList[0].ToString();

            // 按鍵繫結
            SelectNozzleCommand = new DelegateCommand<string>(SelectNozzle);
            UnselectNozzleCommand = new DelegateCommand<string>(UnselectNozzle);
            SelectAllNozzleCommand = new DelegateCommand(SelectAllNozzle);
            UnselectAllNozzleCommand = new DelegateCommand(UnselectAllNozzle);
            InhaleCommand = new DelegateCommand(Inhale);
            VaccumOffCommand = new DelegateCommand(VaccumOff);
            ExhaleCommand = new DelegateCommand(Exhale);
            CylinderUpCommand = new DelegateCommand(CylinderUp);
            CylinderDownCommand = new DelegateCommand(CylinderDown);
            AxisZUpCommand = new DelegateCommand(AxisZUp);
            AxisZDownCommand = new DelegateCommand(AxisZDown);

            SelectClampCommand = new DelegateCommand<string>(SelectClamp);
            UnselectClampCommand = new DelegateCommand<string>(UnselectClamp);
            SelectAllClampCommand = new DelegateCommand(SelectAllClamp);
            UnselectAllClampCommand = new DelegateCommand(UnselectAllClamp);
            ClampOpenCommand = new DelegateCommand(ClampOpen);
            ClampCloseCommand = new DelegateCommand(ClampClose);
            ClampUpCommand = new DelegateCommand(ClampUp);
            ClampDownCommand = new DelegateCommand(ClampDown);
            ClampCylinderUpCommand = new DelegateCommand(ClampCylinderUp);
            ClampCylinderDownCommand = new DelegateCommand(ClampCylinderDown);
        }

        /********************
         * 吸嘴選擇
         *******************/
        /// <summary>
        /// 設定吸嘴按鍵
        /// </summary>
        /// <param name="noz">吸嘴編號(0~10)</param>
        /// <param name="value">On/Off值</param>
        private void SetNozzleButton(int noz, bool value)
        {
            switch (noz)
            {
                case 0:
                    NozzleSelect0 = value;
                    break;
                case 1:
                    NozzleSelect1 = value;
                    break;
                case 2:
                    NozzleSelect2 = value;
                    break;
                case 3:
                    NozzleSelect3 = value;
                    break;
                case 4:
                    NozzleSelect4 = value;
                    break;
                case 5:
                    NozzleSelect5 = value;
                    break;
                case 6:
                    NozzleSelect6 = value;
                    break;
                case 7:
                    NozzleSelect7 = value;
                    break;
                case 8:
                    NozzleSelect8 = value;
                    break;
                case 9:
                    NozzleSelect9 = value;
                    break;
                case 10:
                    NozzleSelect10 = value;
                    break;
            }
        }

        /// <summary>
        /// 選擇吸嘴
        /// </summary>
        /// <param name="noz">吸嘴編號("0"~"10")</param>
        private void SelectNozzle(string noz)
            => SelectNozzle(int.Parse(noz));

        /// <summary>
        /// 選擇吸嘴
        /// </summary>
        /// <param name="noz">吸嘴編號(0~10)</param>
        private void SelectNozzle(int noz)
        {
            stateNozzle[noz] = true;
            SetNozzleButton(noz, true);
        }

        /// <summary>
        /// 取消選擇吸嘴
        /// </summary>
        /// <param name="noz">吸嘴編號("0"~"10")</param>
        private void UnselectNozzle(string noz)
            => UnselectNozzle(int.Parse(noz));

        /// <summary>
        /// 取消選擇吸嘴
        /// </summary>
        /// <param name="noz">吸嘴編號(0~10)</param>
        private void UnselectNozzle(int noz)
        {
            // 狀態
            stateNozzle[noz] = false;
            SetNozzleButton(noz, false);
        }

        /// <summary>
        /// 選擇全部吸嘴
        /// </summary>
        private void SelectAllNozzle()
        {
            for (int noz = 0; noz < Nozzle.MAX_NOZZLE; noz++)
                SelectNozzle(noz);
        }

        /// <summary>
        /// 取消選擇全部吸嘴
        /// </summary>
        private void UnselectAllNozzle()
        {
            for (int noz = 0; noz < Nozzle.MAX_NOZZLE; noz++)
                UnselectNozzle(noz);
        }

        /********************
         * 吸嘴動作
         *******************/
        private void Inhale()
        {
            for (int noz = 0; noz < Nozzle.MAX_NOZZLE; noz++)
                if (stateNozzle[noz])
                    nozzle.NozzleVaccumOn((ENozzleId)noz);
        }

        private void Exhale()
        {
            for (int noz = 0; noz < Nozzle.MAX_NOZZLE; noz++)
                if (stateNozzle[noz])
                    nozzle.NozzleBlowOn((ENozzleId)noz);
        }

        private void VaccumOff()
        {
            for (int noz = 0; noz < Nozzle.MAX_NOZZLE; noz++)
                if (stateNozzle[noz])
                    nozzle.NozzleOff((ENozzleId)noz);
        }

        private void CylinderUp()
        {
            for (int noz = 0; noz < Nozzle.MAX_NOZZLE; noz++)
                if (stateNozzle[noz])
                    nozzle.NozzleUp((ENozzleId)noz);
        }

        private void CylinderDown()
        {
            for (int noz = 0; noz < Nozzle.MAX_NOZZLE; noz++)
                if (stateNozzle[noz])
                    nozzle.NozzleDown((ENozzleId)noz);
        }

        private void AxisZUp()
        {
            for (int noz = 0; noz < Nozzle.MAX_NOZZLE; noz++)
                if (stateNozzle[noz])
                    epcio.JogTo(EServoId.Z, scale * -1, epcio.ServoZ.HighSpeedRate);
        }

        private void AxisZDown()
        {
            for (int noz = 0; noz < Nozzle.MAX_NOZZLE; noz++)
                if (stateNozzle[noz])
                    epcio.JogTo(EServoId.Z, scale, epcio.ServoZ.HighSpeedRate);
        }

        /********************
         * 夾爪選擇
         *******************/
        /// <summary>
        /// 設定夾爪按鍵
        /// </summary>
        /// <param name="clp">夾爪編號(0~1)</param>
        /// <param name="value">On/Off值</param>
        private void SetClampButton(int clp, bool value)
        {
            switch (clp)
            {
                case 0:
                    ClampSelect0 = value;
                    break;
                case 1:
                    ClampSelect1 = value;
                    break;
            }
        }

        /// <summary>
        /// 選擇夾爪
        /// </summary>
        /// <param name="clp">夾爪編號("0"~"1")</param>
        private void SelectClamp(string clp)
            => SelectClamp(int.Parse(clp));

        /// <summary>
        /// 選擇夾爪
        /// </summary>
        /// <param name="clp">夾爪編號(0~1)</param>
        private void SelectClamp(int clp)
        {
            // 狀態
            stateClamp[clp] = true;
            SetClampButton(clp, true);
        }

        /// <summary>
        /// 取消選擇夾爪
        /// </summary>
        /// <param name="clp">夾爪編號("0"~"1")</param>
        private void UnselectClamp(string clp)
            => UnselectClamp(int.Parse(clp));

        /// <summary>
        /// 取消選擇夾爪
        /// </summary>
        /// <param name="clp">夾爪編號(0~1)</param>
        private void UnselectClamp(int clp)
        {
            // 狀態
            stateClamp[clp] = false;
            SetClampButton(clp, false);
        }

        /// <summary>
        /// 選擇全部夾爪
        /// </summary>
        private void SelectAllClamp()
        {
            SelectClamp(0);
            SelectClamp(1);
        }

        /// <summary>
        /// 取消選擇全部夾爪
        /// </summary>
        private void UnselectAllClamp()
        {
            UnselectClamp(0);
            UnselectClamp(1);
        }

        /********************
         * 夾爪動作
         *******************/
        private void ClampOpen()
        {
            if (stateClamp[0])
                clamp.ClampOpen(clamp1: true);
            else if (stateClamp[1])
                clamp.ClampOpen(clamp2: true);
        }

        private void ClampClose()
        {
            if (stateClamp[0])
                clamp.ClampClose(clamp1: true);
            else if (stateClamp[1])
                clamp.ClampClose(clamp2: true);
        }

        private void ClampUp()
        {
            if (stateClamp[0])
                clamp.ClampUp(clamp1: true);
            else if (stateClamp[1])
                clamp.ClampUp(clamp2: true);
        }

        private void ClampDown()
        {
            if (stateClamp[0])
                clamp.ClampDown(clamp1: true);
            else if (stateClamp[1])
                clamp.ClampDown(clamp2: true);
        }

        private void ClampCylinderUp()
            => clamp.ClampSlideCylinderUp();

        private void ClampCylinderDown()
            => clamp.ClampSlideCylinderDown();

        /********************
         * 繫結
         *******************/
        // 吸嘴
        private string _ActionScaleZSelected;
        public string ActionScaleZSelected
        {
            get { return _ActionScaleZSelected; }
            set
            {
                SetProperty(ref _ActionScaleZSelected, value);
                scale = double.Parse(value);
            }
        }

        // 吸嘴List
        private bool _nozzleSelect0;
        public bool NozzleSelect0
        {
            get { return _nozzleSelect0; }
            set { SetProperty(ref _nozzleSelect0, value); }
        }

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

        // 夾爪List
        private bool _clampSelect0;
        public bool ClampSelect0
        {
            get { return _clampSelect0; }
            set { SetProperty(ref _clampSelect0, value); }
        }

        private bool _clampSelect1;
        public bool ClampSelect1
        {
            get { return _clampSelect1; }
            set { SetProperty(ref _clampSelect1, value); }
        }
    }
}
