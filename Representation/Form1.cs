using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace Representation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<double> list_A = new List<double>() { 0.1698, 0.3765, 0.1328, 0.4848, 0.4958, 0.1325, 0.5128, 0.4341, 0.3333, 0.1684, 0.6466 };
            List<int> list_B = new List<int>() { 424, 359, 295, 54, 394, 134, 208, 116, 298, 104, 156 };

            

            Series FScore = chart1.Series.Add("F-1Score");
            FScore.Points.DataBindXY(list_B, list_A);
            FScore.ChartType = SeriesChartType.Point;
            FScore.Color = Color.BlueViolet;
            FScore.BorderWidth = 3;
        }
    }
}
