using OEP520G.Core;
using Prism.Ioc;
using System.CodeDom;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static OEP520G.Product.ViewModels.TraySettingViewModel;

/****************************
 * 說明：PanAndZoom套件支援WPF及Avalonia，但不支援MVVM，
 *      故其相關功能寫在View，而沒有寫在ViewModel。
 * 　　　與ViewModel之間的溝通使用EventAggregator。
 ***************************/
namespace OEP520G.Product.Views
{
    /// <summary>
    /// TraySetting.xaml 的互動邏輯
    /// </summary>
    public partial class TraySetting : UserControl
    {
        public TraySetting()
        {
            InitializeComponent();

            // PanAndZoom
            //zoomBorder.KeyDown += ZoomBorder_KeyDown;

            //zoomBorder.
            //containerRegistry.RegisterInstance(typeof(TraySetting), TraySetting);
        }

        /// <summary>
        /// 滑鼠右鍵點選方塊處理
        /// </summary>
        private new void MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Common.EA.GetEvent<PanAndZoomRightClick>().Publish((int)((Grid)sender).Tag);
        }

        /********************
         * PanAndZoom
         *******************/
        //private void ZoomBorder_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.F)
        //        zoomBorder.Uniform();

        //    if (e.Key == Key.R)
        //        zoomBorder.Reset();

        //    if (e.Key == Key.T)
        //    {
        //        zoomBorder.ToggleStretchMode();
        //        zoomBorder.AutoFit();
        //    }
        //}

        /// <summary>
        /// 自動縮放最適大小
        /// </summary>
        private void AutoFit_Click(object sender, RoutedEventArgs e)
        {
            //vm.AxisLineOnOff(false);
            //zoomBorder.Uniform();
            zoomBorder.AutoFit();
            //vm.AxisLineOnOff(true);
        }
    }
}
