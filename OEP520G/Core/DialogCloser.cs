//using System.Windows;

//// TODO: IDialogService取代

//namespace OEP520G.Core
//{
//    /// <summary>
//    /// 由ViewModel關閉Dialog方法定義；註冊DialogResult為DependencyProperty
//    /// </summary>
//    /// <remarks>
//    /// 1. 在Dialog視窗 &lt;Window&lg; 標籤中加入以下兩行：
//    ///        xmlns:xc="clr-namespace:OEP520G.Core"
//    ///        xc:DialogCloser.DialogResult="{Binding DialogResult}"
//    /// 2. Button處理：(參考第一條連結)
//    ///        OK鍵要加 IsDefault="True"
//    ///        Cancel鍵要加 IsCancel="True"
//    /// 3. 在ViewModel中，繫結DialogResult，Type為bool?：(註：此為Prism框架的程式碼)
//    ///        private bool? _dialogResult;
//    ///        public bool? DialogResult
//    ///        {
//    ///            get { return _dialogResult; }
//    ///            set { SetProperty(ref _dialogResult, value); }
//    ///        }
//    /// 4. 在ViewModel中，欲關閉Dialog時，執行此行程式：
//    ///        DialogResult = true;
//    /// 5. 主程式完全結束原本應該使用ShutdownMode(參見第三條連結)，但Fluent:RibbonWindow好像未繼承此屬性，無法使用，
//    ///    又因為ShowDialog()呼叫的對話框，在關閉時並不會GC(參考第一條連結)，
//    ///    導致主程式結束時無法完全退出，仍會占用Thread。
//    ///    故主程式結束時須執行此行程式(加在Closing事件中)：
//    ///        Application.Current.Shutdown();
//    ///    或(其差別參見第六條連結)
//    ///        Environment.Exit(Environment.ExitCode);
//    /// </remarks>
//    /// <see cref="https://docs.microsoft.com/zh-tw/dotnet/framework/wpf/app-development/dialog-boxes-overview#setting-the-modal-dialog-result"/>
//    /// <see cref="https://docs.microsoft.com/zh-tw/dotnet/api/system.windows.window.dialogresult?view=netcore-3.1"/>
//    /// <see cref="https://docs.microsoft.com/zh-tw/dotnet/api/system.windows.application.shutdownmode?view=netcore-3.1"/>
//    /// <see cref="https://stackoverflow.com/questions/501886/how-should-the-viewmodel-close-the-form/3329467#3329467"/>
//    /// <see cref="http://blog.excastle.com/2010/07/25/mvvm-and-dialogresult-with-no-code-behind/"/>
//    /// <see cref="https://www.cnblogs.com/carsonzhu/p/7211688.html"/>
//    public static class DialogCloser
//    {
//        public static readonly DependencyProperty DialogResultProperty = DependencyProperty.RegisterAttached(
//                "DialogResult", typeof(bool?), typeof(DialogCloser),
//                new PropertyMetadata(DialogResultChanged)
//            );

//        private static void DialogResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            var window = d as Window;
//            if (window != null)
//                window.DialogResult = e.NewValue as bool?;
//        }

//        public static void SetDialogResult(Window target, bool? value)
//        {
//            target.SetValue(DialogResultProperty, value);
//        }
//    }
//}
