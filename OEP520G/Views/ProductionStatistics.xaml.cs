using System;
using System.Data;
using System.Windows.Controls;

namespace OEP520G.Views
{
    /// <summary>
    /// ProductionStatistics.xaml 的互動邏輯
    /// </summary>
    public partial class ProductionStatistics : UserControl
    {
        public ProductionStatistics()
        {
            InitializeComponent();

            DataTable dt = new DataTable();
            dt.Columns.Add("No", typeof(long));
            dt.Columns.Add("MachineId", typeof(string));
            dt.Columns.Add("ProductId", typeof(string));
            dt.Columns.Add("StartTime", typeof(string));
            dt.Columns.Add("StopTime", typeof(string));
            dt.Columns.Add("CycleCount", typeof(long));
            dt.Columns.Add("PickCount", typeof(long));
            dt.Columns.Add("DiscardCount", typeof(long));
            dt.Columns.Add("DiscardRate", typeof(double));
            dt.Columns.Add("CycleTime", typeof(double));

            Random rnd = new Random();
            DataRow row;
            for (int no = 1; no < 10; no++)
            {
                int PickCount = rnd.Next(10, 30);
                int DiscardCount = rnd.Next(10);

                row = dt.NewRow();
                row["No"] = no;
                row["MachineId"] = "MachineId";
                row["ProductId"] = "ProductId";
                row["StartTime"] = DateTime.Now.ToString();
                row["StopTime"] = DateTime.Now.ToString();
                row["CycleCount"] = rnd.Next(1, 20);
                row["PickCount"] = PickCount;
                row["DiscardCount"] = DiscardCount;
                row["DiscardRate"] = (double)DiscardCount / (double)PickCount;
                row["CycleTime"] = (double)(rnd.Next(10000, 20000)) / 1000;
                dt.Rows.Add(row);
            }
            ProdictionDataGrid.ItemsSource = dt.DefaultView;
        }
    }
}
