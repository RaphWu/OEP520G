using Imageproject.Services;
using Imageproject.VisionClient;
using System;
using System.Windows;

namespace Imageproject.Views
{
    /// <summary>
    /// Interaction logic for ImageDemo.xaml
    /// </summary>
    public partial class ImageDemo : Window
    {
        // Singleton單例模式
        private static readonly Lazy<ImageDemo> lazy = new Lazy<ImageDemo>(() => new ImageDemo());
        public static ImageDemo Instance => lazy.Value;

        private ImageDemo()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
            this.Closing += Window_Closing;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            if (this.Owner.IsLoaded)
            {
                this.Owner.Activate();
                e.Cancel = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ClientPage());
        }
    }
}
