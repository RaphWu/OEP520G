using System;
using System.Data;
using System.Windows.Controls;

namespace OEP520G.Product.Views
{
    /// <summary>
    /// TrayFeeder.xaml 的互動邏輯
    /// </summary>
    public partial class TrayFeeder : UserControl
    {
        public TrayFeeder()
        {
            InitializeComponent();

            DataTable dt = new DataTable();
            dt.Columns.Add("ActivePart", typeof(bool));
            dt.Columns.Add("PartNo", typeof(string));
            dt.Columns.Add("ActivePictureBeforePick", typeof(bool));
            dt.Columns.Add("PictureBeforePick", typeof(string));
            dt.Columns.Add("ActivePictureBeforeAssembly", typeof(bool));
            dt.Columns.Add("PictureBeforeAssembly", typeof(string));
            dt.Columns.Add("NextX", typeof(int));
            dt.Columns.Add("NextY", typeof(int));
            dt.Columns.Add("NextTray", typeof(int));
            dt.Columns.Add("NextFeedTray", typeof(long));
            dt.Columns.Add("CenterX", typeof(double));
            dt.Columns.Add("CenterY", typeof(double));

            Random rnd = new Random();
            DataRow row;
            for (int no = 1; no < 10; no++)
            {
                int PickCount = rnd.Next(10, 30);
                int DiscardCount = rnd.Next(10);

                row = dt.NewRow();
                row["ActivePart"] = false;
                row["PartNo"] = "String";
                row["ActivePictureBeforePick"] = false;
                row["PictureBeforePick"] = "String";
                row["ActivePictureBeforeAssembly"] = false;
                row["PictureBeforeAssembly"] = "String";
                row["NextX"] = rnd.Next(1, 12);
                row["NextY"] = rnd.Next(1, 12);
                row["NextTray"] = rnd.Next(1, 12);
                row["NextFeedTray"] = 0;
                row["CenterX"] = (double)rnd.Next(50000, 300000) / 1000;
                row["CenterY"] = (double)rnd.Next(50000, 300000) / 1000;
                dt.Rows.Add(row);
            }
            TrayFeederDataGrid.ItemsSource = dt.DefaultView;
        }
    }
}
