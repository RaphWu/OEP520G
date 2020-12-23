using System.Data;
using System.Windows.Controls;

namespace OEP520G.Manual.Views
{
    /// <summary>
    /// AirPressureSetting.xaml 的互動邏輯
    /// </summary>
    public partial class AirPressureSetting : UserControl
    {
        public AirPressureSetting()
        {
            InitializeComponent();

            DataTable dt;
            DataRow row;

            dt = new DataTable();
            dt.Columns.Add("No", typeof(int));
            dt.Columns.Add("OverPush00", typeof(double));
            dt.Columns.Add("OverPush01", typeof(double));
            dt.Columns.Add("OverPush02", typeof(double));
            dt.Columns.Add("OverPush03", typeof(double));
            dt.Columns.Add("OverPush04", typeof(double));
            dt.Columns.Add("OverPush05", typeof(double));
            dt.Columns.Add("OverPush06", typeof(double));
            dt.Columns.Add("OverPush07", typeof(double));
            dt.Columns.Add("OverPush08", typeof(double));
            dt.Columns.Add("OverPush09", typeof(double));
            dt.Columns.Add("OverPush10", typeof(double));

            for (int i = 1; i <= 30; i++)
            {
                row = dt.NewRow();
                row["No"] = i;
                row["OverPush00"] = 0.0;
                row["OverPush01"] = 0.0;
                row["OverPush02"] = 0.0;
                row["OverPush03"] = 0.0;
                row["OverPush04"] = 0.0;
                row["OverPush05"] = 0.0;
                row["OverPush06"] = 0.0;
                row["OverPush07"] = 0.0;
                row["OverPush08"] = 0.0;
                row["OverPush09"] = 0.0;
                row["OverPush10"] = 0.0;
                dt.Rows.Add(row);
            }

            AirPressureSettingDataGrid.ItemsSource = dt.DefaultView;
        }
    }
}
