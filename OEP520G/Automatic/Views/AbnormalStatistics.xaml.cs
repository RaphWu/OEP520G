using System;
using System.ComponentModel;
using System.Data;
using System.Windows;

namespace OEP520G.Automatic.Views
{
    /// <summary>
    /// AbnormalStatistics.xaml 的互動邏輯
    /// </summary>
    public partial class AbnormalStatistics : Window
    {
        // Singleton單例模式
        private static readonly Lazy<AbnormalStatistics> lazy = new Lazy<AbnormalStatistics>(() => new AbnormalStatistics());
        public static AbnormalStatistics Instance => lazy.Value;

        private AbnormalStatistics()
        {
            InitializeComponent();

            DataTable dt = new DataTable();
            dt.Columns.Add("Date", typeof(string));
            dt.Columns.Add("Time", typeof(string));
            dt.Columns.Add("ProductId", typeof(string));
            dt.Columns.Add("Nozzle", typeof(string));
            dt.Columns.Add("Tray", typeof(string));
            dt.Columns.Add("Part", typeof(string));
            dt.Columns.Add("Code", typeof(string));
            dt.Columns.Add("Description", typeof(string));

            Random rnd = new Random();
            DataRow row;
            DateTime now = DateTime.Now;

            for (int speed = 3; speed >= 1; speed--)
            {
                for (int nozzle = 1; nozzle <= 11; nozzle++)
                {
                    row = dt.NewRow();
                    row["Date"] = now.Date.ToString();
                    row["Time"] = now.ToLongTimeString();
                    row["ProductId"] = "ProductId";
                    row["Nozzle"] = rnd.Next(1, 12);
                    row["Tray"] = rnd.Next(1, 13);
                    row["Part"] = "String";
                    row["Code"] = rnd.Next();
                    row["Description"] = "Description";
                    dt.Rows.Add(row);
                }
            }
            FlyingCameraDataGrid.ItemsSource = dt.DefaultView;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.Hide();
            if (this.Owner.IsLoaded)
            {
                this.Owner.Activate();
                e.Cancel = true;
            }
        }
    }
}
