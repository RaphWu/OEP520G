using CSharpCore;
using CSharpCore.FileSystem;
using System;
using System.ComponentModel;
using System.Windows;

namespace OEP520G.Manual.Views
{
    /// <summary>
    /// ObjectMove.xaml 的互動邏輯
    /// </summary>
    public partial class ObjectMove : Window
    {
        // Singleton單例模式
        private static readonly Lazy<ObjectMove> lazy = new Lazy<ObjectMove>(() => new ObjectMove());
        public static ObjectMove Instance => lazy.Value;

        private ObjectMove()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RestorePosition();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SavePosition();

            this.Hide();
            if (this.Owner.IsLoaded)
            {
                this.Owner.Activate();
                e.Cancel = true;
            }
        }

        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    this.Hide();
        //    if (this.Owner.IsLoaded)
        //    {
        //        this.Owner.Activate();
        //        e.Cancel = true;
        //    }
        //}

        /********************
         * 保存視窗位置
         *******************/
        // .ini存檔時檔案名稱及Section名稱
        private readonly string fileName = "OEP520G.ini";
        private readonly string sectionName = "ObjectBox";

        public void RestorePosition()
        {
            IniFile iniFile = new IniFile(fileName);

            this.Left = double.Parse(iniFile.ReadIniFile(sectionName, "Left", "100.0"));
            this.Top = double.Parse(iniFile.ReadIniFile(sectionName, "Top", "100.0"));
        }

        public void SavePosition()
        {
            IniFile iniFile = new IniFile(fileName);

            iniFile.WriteIniFile(sectionName, "Left", this.Left);
            iniFile.WriteIniFile(sectionName, "Top", this.Top);
        }
    }
}
