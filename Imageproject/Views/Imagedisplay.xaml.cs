using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.Windows;


namespace Imageproject.Views
{
    /// <summary>
    /// Image_display.xaml 的互動邏輯
    /// </summary>
    public partial class Imagedisplay : MetroWindow
    {
        // Singleton單例模式
        private static readonly Lazy<Imagedisplay> lazy = new Lazy<Imagedisplay>(() => new Imagedisplay());
        public static Imagedisplay Instance => lazy.Value;

        private Imagedisplay()
        {
            InitializeComponent();
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            //SavePosition();

            this.Hide();
            if (this.Owner.IsLoaded)
            {
                this.Owner.Activate();
                e.Cancel = true;
            }
        }
    }
}