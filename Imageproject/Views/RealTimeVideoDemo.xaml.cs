using System;
using System.Windows;

namespace Imageproject.Views
{
    /// <summary>
    /// Interaction logic for RealTimeVideoDemo.xaml
    /// </summary>
    public partial class RealTimeVideoDemo : Window
    {
        // Singleton單例模式
        private static readonly Lazy<RealTimeVideoDemo> lazy = new Lazy<RealTimeVideoDemo>(() => new RealTimeVideoDemo());
        public static RealTimeVideoDemo Instance => lazy.Value;

        public RealTimeVideoDemo()
        {
            InitializeComponent();
        }
    }
}
