using System.Data;
using System.Windows.Controls;

namespace OEP520G.Automatic.Views
{
    /// <summary>
    /// GlueParameters.xaml 的互動邏輯
    /// </summary>
    public partial class GlueParameters : UserControl
    {
        public GlueParameters()
        {
            InitializeComponent();

            DataTable dt;
            DataRow row;

            // 點膠動作
            dt = new DataTable();
            dt.Columns.Add("No", typeof(int));
            dt.Columns.Add("Action", typeof(string));
            dt.Columns.Add("ShiftX", typeof(double));
            dt.Columns.Add("ShiftY", typeof(double));
            dt.Columns.Add("ShiftZ", typeof(double));
            dt.Columns.Add("ShiftXt", typeof(double));
            dt.Columns.Add("GroupNo", typeof(int));

            row = dt.NewRow();
            row["No"] = 1;
            row["Action"] = "移動";
            row["ShiftX"] = 2.8;
            row["ShiftY"] = 0.0;
            row["ShiftZ"] = 5.0;
            row["ShiftXt"] = -10.0;
            row["GroupNo"] = 1;
            dt.Rows.Add(row);

            row = dt.NewRow();
            row["No"] = 2;
            row["Action"] = "畫線";
            row["ShiftX"] = 2.88;
            row["ShiftY"] = 0.0;
            row["ShiftZ"] = 4.4;
            row["ShiftXt"] = 0.0;
            row["GroupNo"] = 1;
            dt.Rows.Add(row);

            row = dt.NewRow();
            row["No"] = 3;
            row["Action"] = "畫像結果";
            row["ShiftX"] = 0.0;
            row["ShiftY"] = 0.0;
            row["ShiftZ"] = 2.88;
            row["ShiftXt"] = 0.0;
            row["GroupNo"] = 1;
            dt.Rows.Add(row);

            row = dt.NewRow();
            row["No"] = 4;
            row["Action"] = "旋轉";
            row["ShiftX"] = 0.0;
            row["ShiftY"] = 0.0;
            row["ShiftZ"] = 4.4;
            row["ShiftXt"] = 360.0;
            row["GroupNo"] = 1;
            dt.Rows.Add(row);

            DispensingActionDataGrid.ItemsSource = dt.DefaultView;

            // 點膠參數
            dt = new DataTable();
            dt.Columns.Add("GroupNo", typeof(int));
            dt.Columns.Add("DspSpeed", typeof(double));
            dt.Columns.Add("SpeedA", typeof(double));
            dt.Columns.Add("SWait", typeof(int));
            dt.Columns.Add("EShot", typeof(int));
            dt.Columns.Add("PreStop", typeof(double));
            dt.Columns.Add("EWait", typeof(int));
            dt.Columns.Add("UpXY", typeof(double));
            dt.Columns.Add("UpZ", typeof(double));
            dt.Columns.Add("UpSpeed", typeof(double));
            dt.Columns.Add("UpDelay", typeof(double));
            dt.Columns.Add("UpWay", typeof(int));

            row = dt.NewRow();
            row["GroupNo"] = 1;
            row["DspSpeed"] = 50.0;
            row["SpeedA"] = 200.0;
            row["SWait"] = -150;
            row["EShot"] = 0;
            row["PreStop"] = 30.0;
            row["EWait"] = 0;
            row["UpXY"] = 0.0;
            row["UpZ"] = 0.0;
            row["UpSpeed"] = 0.0;
            row["UpDelay"] = 0.0;
            row["UpWay"] = 0;
            dt.Rows.Add(row);

            DispensingParameterDataGrid.ItemsSource = dt.DefaultView;
        }
    }
}
