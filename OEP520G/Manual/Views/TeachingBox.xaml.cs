using CSharpCore;
using CSharpCore.FileSystem;
using System;
using System.ComponentModel;
using System.Windows;

namespace OEP520G.Manual.Views
{
    /// <summary>
    /// TeachingBox.xaml 的互動邏輯
    /// </summary>
    public partial class TeachingBox : Window
    {
        // Singleton單例模式
        private static readonly Lazy<TeachingBox> lazy = new Lazy<TeachingBox>(() => new TeachingBox());
        public static TeachingBox Instance => lazy.Value;

        private TeachingBox()
        {
            InitializeComponent();
        }

        //public void ShowObjectPanel(object sender, RoutedEventArgs e)
        //{
        //    ObjectMove v = ObjectMove.Instance;
        //    v.Owner = this;
        //    v.Show();
        //}

        // TODO: 視窗開啟時復原視窗位置
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
        //}

        /********************
         * 保存視窗位置
         *******************/
        // .ini存檔時檔案名稱及Section名稱
        private readonly string fileName = "OEP520G.ini";
        private readonly string sectionName = "TeachingBox";

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
