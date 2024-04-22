using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace prototype
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 0;
            panel5.Visible = true;
            panel6.Visible = false;
            panel7.Visible = false;
            panel9.Visible = false;
        }

        private void DisplayCertificatesInDataGridView()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "certificates", "AllCertificates.json");

            List<ElectricityCertificate> certificates = ElectricityCertificate.LoadCertificatesFromJson(filePath);
            dataGridView1.DataSource = certificates;
        }

        private double GetTotalElectricity()
        {
            string mwhFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "electricityMWh.json");
            double totalElectricity = ElectricityPool.LoadElectricityFromJson(mwhFilePath);
            return totalElectricity;
        }

        private void DisplayLabels()
        {
            string labelsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "electricityLabels.json");
            var labels = ElectricityPool.LoadLabelsFromJson(labelsFilePath);

            dataGridViewLabels.DataSource = labels;
            dataGridViewLabels.Columns["Quality"].HeaderText = "Quality";
            dataGridViewLabels.Columns["Producer"].HeaderText = "Producer";
            dataGridViewLabels.Columns["Device"].HeaderText = "Device";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
            DisplayCertificatesInDataGridView();
            panel5.Visible = false;
            panel6.Visible = true;
            panel7.Visible = false;
            panel9.Visible = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            panel5.Visible = true;
            panel6.Visible = false;
            panel7.Visible = false;
            panel9.Visible = false;

            string mwhFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "electricityMWh.json");
            string labelsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "electricityLabels.json");

            double totalElectricity = ElectricityPool.LoadElectricityFromJson(mwhFilePath);
            var labels = ElectricityPool.LoadLabelsFromJson(labelsFilePath);
        }

        private void Upload_Cert_Click(object sender, EventArgs e)
        {
            string amountStr = customTextBox3.Text;
            string device = customTextBox2.Text;
            string quality = comboBox1.Text;
            string ownership = customTextBox1.Text;

            double amount;
            if (!double.TryParse(amountStr, out amount))
            {
                MessageBox.Show("Invalid amount format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string lossStr = customTextBox4.Text;
            double loss;
            if (!double.TryParse(lossStr, out loss))
            {
                MessageBox.Show("Invalid amount format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DateTime date = DateTime.Now;

            ElectricityCertificate certificate = new ElectricityCertificate(date, amount, quality, loss, device, ownership);

            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string certificatesDirectory = Path.Combine(appDirectory, "certificates");
            string filePath = Path.Combine(certificatesDirectory, "AllCertificates.json");

            ElectricityCertificate.SaveCertificateToJson(certificate, filePath);
            UpdateElectricityPool(amount, quality, ownership, device, loss);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 2;
            panel5.Visible = false;
            panel6.Visible = false;
            panel7.Visible = true;
            panel9.Visible = false;
        }

        private void UpdateElectricityPool(double amountInKWh, string quality, string producer, string device, double lossPercentage)
        {
            string mwhFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "electricityMWh.json");
            string labelsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "electricityLabels.json");

            double netAmountInKWh = amountInKWh * (1 - lossPercentage / 100);
            ElectricityPool.AddElectricityToFile(netAmountInKWh, mwhFilePath);

            List<Label> labelsToAdd = new List<Label>();
            for (int i = 0; i < netAmountInKWh; i++)
            {
                labelsToAdd.Add(new Label(quality, producer, device));
            }
            ElectricityPool.AppendLabelsToFile(labelsToAdd, labelsFilePath);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 3;
            panel5.Visible = false;
            panel6.Visible = false;
            panel7.Visible = false;
            panel9.Visible = true;

            // Display labels in DataGridView
            DisplayLabels();

            // Show total units
            double totalUnits = GetTotalElectricity();
            label11.Text = "Total units: " + totalUnits + " kWh ";

            // Display stacked bar chart
            DisplayStackedColumnChart();
        }

        private void DisplayStackedColumnChart()
        {
            string labelsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "electricityLabels.json");
            var labels = ElectricityPool.LoadLabelsFromJson(labelsFilePath);

            // Group the labels by Quality and Producer
            var groupedLabels = labels.GroupBy(l => new { l.Quality, l.Producer })
                                      .Select(g => new { Quality = g.Key.Quality, Producer = g.Key.Producer, Count = g.Count() })
                                      .ToList();

            // Prepare the data for the chart
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Quality", typeof(string));

            // Get distinct producers
            List<string> producers = groupedLabels.Select(l => l.Producer).Distinct().OrderBy(p => p).ToList();

            // Add columns for each producer
            foreach (string producer in producers)
            {
                dataTable.Columns.Add(producer, typeof(int));
            }

            // Add rows for each quality
            var qualities = groupedLabels.Select(l => l.Quality).Distinct().OrderBy(q => q).ToList();
            foreach (string quality in qualities)
            {
                DataRow row = dataTable.NewRow();
                row["Quality"] = quality;

                foreach (string producer in producers)
                {
                    var count = groupedLabels.FirstOrDefault(l => l.Quality == quality && l.Producer == producer);
                    row[producer] = count?.Count ?? 0;
                }

                dataTable.Rows.Add(row);
            }

            // Clear existing series from the chart
            chart1.Series.Clear();

            // Add series to the chart and bind data
            foreach (string producer in producers)
            {
                Series series = new Series(producer);
                series.ChartType = SeriesChartType.StackedColumn;
                series.IsVisibleInLegend = false; // Hide from legend

                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    DataPoint dataPoint = new DataPoint();
                    dataPoint.SetValueXY(dataTable.Rows[i]["Quality"], dataTable.Rows[i][producer]);
                    series.Points.Add(dataPoint);
                }

                chart1.Series.Add(series);
            }

            // Set chart title and axis labels
            chart1.Titles.Clear();
            chart1.Titles.Add("Stacked Bar Chart");
            chart1.ChartAreas[0].AxisX.Title = "Quality";
            chart1.ChartAreas[0].AxisY.Title = "Count";

            // Remove tooltips
            foreach (Series series in chart1.Series)
            {
                foreach (DataPoint point in series.Points)
                {
                    point.ToolTip = "";
                }
            }

            // Customize chart appearance
            chart1.ChartAreas[0].BackColor = Color.Transparent; // Remove background
            chart1.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 10f); // Change X-axis label font size
            chart1.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White; // Set X-axis label color to white
            chart1.ChartAreas[0].AxisY.LabelStyle.Font = new Font("Arial", 10f); // Change Y-axis label font size
            chart1.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White; // Set Y-axis label color to white
            chart1.ChartAreas[0].AxisX.TitleForeColor = Color.White; // Set X-axis title color to white
            chart1.ChartAreas[0].AxisY.TitleForeColor = Color.White; // Set Y-axis title color to white
            chart1.Titles[0].ForeColor = Color.White; // Set chart title color to white
        }
    }
}
