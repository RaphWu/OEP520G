using Imageproject;
using Imageproject.Contracts;
using Imageproject.Services;
using OEP520G.Core.ViewModels;
using OEP520G.Core.Views;
using OEP520G.Parameter;
using OEP520G.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using System;
using System.Windows;
using System.Windows.Data;

namespace OEP520G
{
    /// <summary>
    /// App.xaml的互動邏輯
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //    // 在容器中將ApplicationCommands註冊為單例(singleton)
            //    containerRegistry.RegisterSingleton<IApplicationCommands, ApplicationCommands>();

            // Dialog，品種增修刪使用
            containerRegistry.RegisterDialog<CrudDialog, CrudDialogViewModel>();
            containerRegistry.RegisterDialog<MessageDialog, MessageDialogViewModel>();
            containerRegistry.Register<IImage, ImageService>();
        }

        //protected override void ConfigureViewModelLocator()
        //{
        //    base.ConfigureViewModelLocator();

        //    ViewModelLocationProvider.Register<FieldDialog, FieldDialogViewModel>();
        //    ViewModelLocationProvider.Register(typeof(TraySetting).ToString(), typeof(TrayFeederDescriptionViewModel));

        //    ViewModelLocationProvider.Register(typeof(SystemComp).ToString(), typeof(MainWindowViewModel));
        //    ViewModelLocationProvider.Register(typeof(MotionCardParameter).ToString(), typeof(MotionParameterViewModel));
        //    ViewModelLocationProvider.Register(typeof(Servo).ToString(), typeof(ServoParameterViewModel));
        //}

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<ImageprojectModule>();
            moduleCatalog.AddModule<TcpipServer.TcpipServerModule>();
        }
    }

    /************************
     * Converter
     ***********************/
    /// <summary>
    /// Converter: bool反相
    /// </summary>
    /// <example>參閱: https://blog.csdn.net/qq_40666028/article/details/96188486 </example>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => !(bool)value;

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// Converter: bool互轉Visible/Hidden
    /// </summary>
    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToVisibleHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => ((bool)value) ? "Visible" : "Hidden";

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => ((string)value == "Visible") ? true : false;
    }

    /// <summary>
    /// Converter: bool互轉Visible/Collapsed
    /// </summary>
    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToVisibleCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => ((bool)value) ? "Visible" : "Collapsed";

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => ((string)value == "Visible") ? true : false;
    }

    /// <summary>
    /// Converter: bool互轉Visible/Collapsed(反相)
    /// </summary>
    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToVisibleCollapsedInvertConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => ((bool)value) ? "Collapsed" : "Visible";

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => ((string)value == "Collapsed") ? true : false;
    }

    /// <summary>
    /// Converter: bool互轉文字
    /// </summary>
    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => ((bool)value) ? "True" : "False";

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            => (value.ToString() == "True") ? true : false;
    }

    /// <summary>
    /// Converter: RadioButton的item互轉Enum
    /// </summary>
    /// <example>參閱: https://stackoverflow.com/questions/9212873/binding-radiobuttons-group-to-a-property-in-wpf </example>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(parameter is string ParameterString))
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object paramvalue = Enum.Parse(value.GetType(), ParameterString);
            return paramvalue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var valueAsBool = (bool)value;

            if (!(parameter is string ParameterString) || !valueAsBool)
            {
                try
                {
                    return Enum.Parse(targetType, "0");
                }
                catch (Exception)
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            return Enum.Parse(targetType, ParameterString);
        }
    }

    /// <summary>
    /// Converter: Tray排列方式轉換
    /// </summary>
    [ValueConversion(typeof(EArrangement), typeof(string))]
    public class TrayOrientationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                return (EArrangement)value == EArrangement.Staggered
                    ? "交錯排列" : "依序排列";
            else
                return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                return ((string)value == "交錯排列")
                    ? EArrangement.Staggered : EArrangement.InOrder;
            else
                return EArrangement.InOrder;
        }
    }

    /// <summary>
    /// Converter: Tray排列方向轉換
    /// </summary>
    [ValueConversion(typeof(EDirection), typeof(string))]
    public class TrayDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                return (EDirection)value == EDirection.Horizontal
                        ? "水平排列" : "垂直排列";
            else
                return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                return ((string)value).Substring(0, 2) == "垂直"
                    ? EDirection.Vertical : EDirection.Horizontal;
            else
                return EDirection.Horizontal;
        }
    }

    /// <summary>
    /// Converter: RadioButton用，將ConverterParameter的設定值存入IsChecked繫結的變數
    /// </summary>
    public class RadioButtonToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //value是radiobutton接到binding變數的值後，來呼叫converter
            //converter負責判定接到的值是代表true還是false
            if (value == null || parameter == null)
                return null;

            return value.ToString().Equals(parameter.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //value是目前radiobutton的true/false
            //在這裡把parameter傳回ViewModel
            if (value == null || parameter == null)
                return null;

            return (bool)value ? parameter.ToString() : string.Empty;
        }
    }

    /// <summary>
    /// Converter: RadioButton用，將ConverterParameter的設定值存入IsChecked繫結的變數
    /// </summary>
    public class RadioButtonToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //value是radiobutton接到binding變數的值後，來呼叫converter
            //converter負責判定接到的值是代表true還是false
            if (value == null || parameter == null)
                return null;

            return value.ToString().Equals(parameter.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //value是目前radiobutton的true/false
            //在這裡把parameter傳回ViewModel
            if (value == null || parameter == null)
                return null;

            return (bool)value ? int.Parse(parameter.ToString()) : -1;
        }
    }
}
