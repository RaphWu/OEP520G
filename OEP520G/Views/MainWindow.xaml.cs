using CSharpCore;
using CSharpCore.Security;
using Fluent;
using Fluent.Localization.Languages;
using Imageproject.Models;
using Imageproject.Services;
using Imageproject.Views;
using OEP520G.Automatic.Views;
using OEP520G.Manual.Views;
using OEP520G.Parameter.Views;
using OEP520G.Product.Views;
using OEP520G.Teaching.Views;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using System.Windows;
using Unity;

namespace OEP520G.Views
{
    /// <summary>
    /// MainWindow.xaml的互動邏輯
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        private readonly List<Authorization> _authorityList = Authority.Instance.AuthorityList;
        private readonly int _authority = Authority.Instance.ActiveAuthority.Level;

        // Prism的IoC及Region
        private readonly IContainerExtension container;
        private readonly IRegionManager regionManager;
        private IRegion region;

        // View實例化
        private Zero viewZero;
        private AutoOperation viewAutoOperation;
        private AutoSequence viewAutoProductData;
        private TrayFeeder viewTrayFeeder;
        private IoList viewIoList;
        private CylinderAction viewCylinderAction;
        private IoAction viewIoAction;
        private TraySetting viewTraySetting;
        private GlueParameters viewGlueParameters;
        private CleanGlueSetting viewGlueCleanSetting;
        private MachineInfo viewSystemComp;
        private SystemReferPoint viewSystemReferPoint;
        private Nozzle viewNozzle;
        private Clamp viewClamp;
        private Stage viewStage;
        private TeachSystem viewTeachSystem;
        private MeasuringHeight viewMeasuringHeight;
        private MeasuringStageHeight viewMeasuringStageHeight;
        private RotationCenter viewRotationCenter;
        private EccentricCorrect viewEccentricCorrect;
        private NozzleCorrect viewNozzleCorrect;
        private ClampCorrect viewClampCorrect;
        private FlyingCorrect viewCameraFlyingCorrect;
        private TrayArrangement viewTrayArrangement;
        private CameraCorrect viewCameraCorrect;
        private ProductionStatistics viewProductionStatistics;
        private MotionCardParameter viewMotionCardParameter;
        private ServoParameter viewServoParameter;
        private MoveCamera viewMoveCamera;
        private FixCamera viewFixCamera;
        private DiscardBox viewDiscardBox;
        private AirPressureSetting viewAirPressureSetting;
        private ServoTester viewServoTester;
        private ProductView viewProductionSelect;

        /// <summary>
        /// Shell
        /// </summary>
        public MainWindow(IContainerExtension ce, IRegionManager rm)
        {
            InitializeComponent();

            // MaterialDesignInXAML Advanced Theming
            //Color primaryColor = Colors.Teal;
            //Color secondaryColor = Colors.Orange;
            //var paletteHelper = new PaletteHelper();

            //IBaseTheme baseTheme = MaterialDesignThemes.Wpf.Theme.Light;
            //ITheme theme = MaterialDesignThemes.Wpf.Theme.Create(baseTheme, primaryColor, secondaryColor);
            //paletteHelper.SetTheme(theme);

            // Prism的IoC及Region
            container = ce;
            regionManager = rm;

            this.Loaded += MainWindow_Loaded;

            // Fluent.Ribbon Localization
            RibbonLocalization.Current.Localization = new OepLocalization();
        }

        /// <summary>
        /// 
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 將View注入容器
            viewZero = container.Resolve<Zero>();
            viewAutoOperation = container.Resolve<AutoOperation>();
            viewAutoProductData = container.Resolve<AutoSequence>();
            viewTrayFeeder = container.Resolve<TrayFeeder>();
            viewIoList = container.Resolve<IoList>();
            viewCylinderAction = container.Resolve<CylinderAction>();
            viewIoAction = container.Resolve<IoAction>();
            viewTraySetting = container.Resolve<TraySetting>();
            viewGlueParameters = container.Resolve<GlueParameters>();
            viewGlueCleanSetting = container.Resolve<CleanGlueSetting>();
            viewSystemComp = container.Resolve<MachineInfo>();
            viewSystemReferPoint = container.Resolve<SystemReferPoint>();
            viewNozzle = container.Resolve<Nozzle>();
            viewClamp = container.Resolve<Clamp>();
            viewStage = container.Resolve<Stage>();
            viewTeachSystem = container.Resolve<TeachSystem>();
            viewMeasuringHeight = container.Resolve<MeasuringHeight>();
            viewMeasuringStageHeight = container.Resolve<MeasuringStageHeight>();
            viewRotationCenter = container.Resolve<RotationCenter>();
            viewEccentricCorrect = container.Resolve<EccentricCorrect>();
            viewNozzleCorrect = container.Resolve<NozzleCorrect>();
            viewClampCorrect = container.Resolve<ClampCorrect>();
            viewCameraFlyingCorrect = container.Resolve<FlyingCorrect>();
            viewTrayArrangement = container.Resolve<TrayArrangement>();
            viewCameraCorrect = container.Resolve<CameraCorrect>();
            viewProductionStatistics = container.Resolve<ProductionStatistics>();
            viewMotionCardParameter = container.Resolve<MotionCardParameter>();
            viewServoParameter = container.Resolve<ServoParameter>();
            viewMoveCamera = container.Resolve<MoveCamera>();
            viewFixCamera = container.Resolve<FixCamera>();
            viewDiscardBox = container.Resolve<DiscardBox>();
            viewAirPressureSetting = container.Resolve<AirPressureSetting>();
            viewServoTester = container.Resolve<ServoTester>();
            viewProductionSelect = container.Resolve<ProductView>();

            // 將View註冊到Region
            region = regionManager.Regions["MainRegion"];

            region.Add(viewZero);
            region.Add(viewAutoOperation);
            region.Add(viewAutoProductData);
            region.Add(viewTrayFeeder);
            region.Add(viewIoList);
            region.Add(viewCylinderAction);
            region.Add(viewIoAction);
            region.Add(viewTraySetting);
            region.Add(viewGlueParameters);
            region.Add(viewGlueCleanSetting);
            region.Add(viewSystemComp);
            region.Add(viewSystemReferPoint);
            region.Add(viewNozzle);
            region.Add(viewClamp);
            region.Add(viewStage);
            region.Add(viewTeachSystem);
            region.Add(viewMeasuringHeight);
            region.Add(viewMeasuringStageHeight);
            region.Add(viewRotationCenter);
            region.Add(viewEccentricCorrect);
            region.Add(viewNozzleCorrect);
            region.Add(viewClampCorrect);
            region.Add(viewCameraFlyingCorrect);
            region.Add(viewTrayArrangement);
            region.Add(viewCameraCorrect);
            region.Add(viewProductionStatistics);
            region.Add(viewMotionCardParameter);
            region.Add(viewServoParameter);
            region.Add(viewMoveCamera);
            region.Add(viewFixCamera);
            region.Add(viewDiscardBox);
            region.Add(viewAirPressureSetting);
            region.Add(viewServoTester);
            region.Add(viewProductionSelect);
        }

        /********************
         * Button.Click()動作
         *******************/
        private void ShowZero(object sender, RoutedEventArgs e)
        {
            region.Activate(viewZero);
        }

        private void ShowAutoOperation(object sender, RoutedEventArgs e)
        {
            region.Activate(viewAutoOperation);
        }

        private void ShowAutoProductData(object sender, RoutedEventArgs e)
        {
            region.Activate(viewAutoProductData);
        }

        private void ShowTrayFeeder(object sender, RoutedEventArgs e)
        {
            region.Activate(viewTrayFeeder);
        }

        private void ShowIoList(object sender, RoutedEventArgs e)
        {
            region.Activate(viewIoList);
        }

        private void ShowCylinderAction(object sender, RoutedEventArgs e)
        {
            region.Activate(viewCylinderAction);
        }

        private void ShowIoAction(object sender, RoutedEventArgs e)
        {
            region.Activate(viewIoAction);
        }

        private void ShowTrayFeederDescription(object sender, RoutedEventArgs e)
        {
            region.Activate(viewTraySetting);
        }

        private void ShowGlueParameters(object sender, RoutedEventArgs e)
        {
            region.Activate(viewGlueParameters);
        }

        private void ShowGlueCleanSetting(object sender, RoutedEventArgs e)
        {
            region.Activate(viewGlueCleanSetting);
        }

        private void ShowSystemComp(object sender, RoutedEventArgs e)
        {
            region.Activate(viewSystemComp);
        }

        private void ShowSystemReferPoint(object sender, RoutedEventArgs e)
        {
            region.Activate(viewSystemReferPoint);
        }

        private void ShowNozzle(object sender, RoutedEventArgs e)
        {
            region.Activate(viewNozzle);
        }

        private void ShowClamp(object sender, RoutedEventArgs e)
        {
            region.Activate(viewClamp);
        }

        private void ShowStage(object sender, RoutedEventArgs e)
        {
            region.Activate(viewStage);
        }

        private void ShowTeachSystem(object sender, RoutedEventArgs e)
        {
            region.Activate(viewTeachSystem);
        }

        private void ShowMeasuringHeight(object sender, RoutedEventArgs e)
        {
            region.Activate(viewMeasuringHeight);
        }

        private void ShowMeasuringStageHeight(object sender, RoutedEventArgs e)
        {
            region.Activate(viewMeasuringStageHeight);
        }

        private void ShowRotationCenter(object sender, RoutedEventArgs e)
        {
            region.Activate(viewRotationCenter);
        }

        private void ShowEccentricCorrect(object sender, RoutedEventArgs e)
        {
            region.Activate(viewEccentricCorrect);
        }

        private void ShowNozzleCorrect(object sender, RoutedEventArgs e)
        {
            region.Activate(viewNozzleCorrect);
        }

        private void ShowClampCorrect(object sender, RoutedEventArgs e)
        {
            region.Activate(viewClampCorrect);
        }

        private void ShowCameraFlyingCorrect(object sender, RoutedEventArgs e)
        {
            region.Activate(viewCameraFlyingCorrect);
        }

        private void ShowTrayArrangement(object sender, RoutedEventArgs e)
        {
            region.Activate(viewTrayArrangement);
        }

        private void ShowCameraCorrect(object sender, RoutedEventArgs e)
        {
            region.Activate(viewCameraCorrect);
        }

        private void ShowProductionStatistics(object sender, RoutedEventArgs e)
        {
            region.Activate(viewProductionStatistics);
        }

        private void ShowMotionParameter(object sender, RoutedEventArgs e)
        {
            region.Activate(viewMotionCardParameter);
        }

        private void ShowServoParameter(object sender, RoutedEventArgs e)
        {
            region.Activate(viewServoParameter);
        }

        //private void FixCameraDeviceSetting(object sender, RoutedEventArgs e)
        //{
        //    ImageClass _image = new ImageClass();
        //    _image.DeviceSetting(CameraId.FixCamera);
        //}

        //private void FixCameraPropertSetting(object sender, RoutedEventArgs e)
        //{
        //    _image.PropertSetting(CameraId.FixCamera);
        //}

        //private void MoveCameraDeviceSetting(object sender, RoutedEventArgs e)
        //{
        //    //_image.DeviceSetting(CameraId.MoveCamera);
        //}

        //private void MoveCameraPropertSetting(object sender, RoutedEventArgs e)
        //{
        //    //_image.PropertSetting(CameraId.MoveCamera);
        //}

        private void ShowDiscardBox(object sender, RoutedEventArgs e)
        {
            region.Activate(viewDiscardBox);
        }

        private void ShowAirPressureSetting(object sender, RoutedEventArgs e)
        {
            region.Activate(viewAirPressureSetting);
        }

        private void ShowServoTester(object sender, RoutedEventArgs e)
        {
            region.Activate(viewServoTester);
        }

        private void ShowProductionSelect(object sender, RoutedEventArgs e)
        {
            region.Activate(viewProductionSelect);
        }

        /********************
         * OnClick Event
         *******************/
        private void ShowTeachingBox(object sender, RoutedEventArgs e)
        {
            TeachingBox v = TeachingBox.Instance;
            v.Owner = this;
            v.Show();
        }

        private void ShowObjectPanel(object sender, RoutedEventArgs e)
        {
            ObjectMove v = ObjectMove.Instance;
            v.Owner = this;
            v.Show();
        }

        private void ShowAbnormalStatistics(object sender, RoutedEventArgs e)
        {
            AbnormalStatistics v = AbnormalStatistics.Instance;
            v.Owner = this;
            v.Show();
        }
    }
}
