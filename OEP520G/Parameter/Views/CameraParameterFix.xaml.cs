using System;
using System.Data;
using System.Windows.Controls;

namespace OEP520G.Parameter.Views
{
    /// <summary>
    /// CameraParameterFix.xaml 的互動邏輯
    /// </summary>
    public partial class CameraParameterFix : UserControl
    {
        public CameraParameterFix()
        {
            InitializeComponent();

            // TODO: 合併儲存格
            DataTable dt = new DataTable();
            dt.Columns.Add("NozzleNo", typeof(int));
            dt.Columns.Add("ShiftX", typeof(double));
            dt.Columns.Add("ShiftY", typeof(double));
            dt.Columns.Add("MirrorX", typeof(double));
            dt.Columns.Add("MirrorY", typeof(double));

            Random rnd = new Random();
            DataRow row;
            for (int speed = 4; speed >= 1; speed--)
            {
                for (int nozzle = 1; nozzle <= 11; nozzle++)
                {
                    row = dt.NewRow();
                    row["NozzleNo"] = nozzle;
                    row["ShiftX"] = (double)(rnd.Next(-20 * speed, -20 * (speed - 1))) / 1000;
                    row["ShiftY"] = (double)(rnd.Next(-20 * speed, -20 * (speed - 1))) / 1000;
                    row["MirrorX"] = (double)(rnd.Next(-20 * speed, -20 * (speed - 1))) / 1000;
                    row["MirrorY"] = (double)(rnd.Next(-20 * speed, -20 * (speed - 1))) / 1000;
                    dt.Rows.Add(row);
                }
            }
            CameraParameterFixDataGrid.ItemsSource = dt.DefaultView;
        }
    }
}
