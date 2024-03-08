using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prototype
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 0;  
            panel5.Visible = true;
            panel6.Visible = false;
            panel7.Visible = false;
        }

        private void DisplayCertificatesInDataGridView()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "certificates", "AllCertificates.json");

            List<ElectricityCertificate> certificates = ElectricityCertificate.LoadCertificatesFromJson(filePath);
            dataGridView1.DataSource = certificates;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
            DisplayCertificatesInDataGridView();
            panel5.Visible = false;
            panel6.Visible = true;
            panel7.Visible = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            panel5.Visible=true;
            panel6.Visible=false;
            panel7.Visible = false;
        }

        private void Upload_Cert_Click(object sender, EventArgs e)
        {            
            string amount = customTextBox3.Text;
            string device = customTextBox2.Text;
            string ownership = customTextBox1.Text;
            DateTime date = DateTime.Now; 

            ElectricityCertificate certificate = new ElectricityCertificate(date, amount,device,ownership);

            // Specify the file path
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string certificatesDirectory = Path.Combine(appDirectory, "certificates");
            string filePath = Path.Combine(certificatesDirectory, "AllCertificates.json");

            // Save the certificate
            ElectricityCertificate.SaveCertificateToJson(certificate, filePath);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 2;
            panel5.Visible = false;
            panel6.Visible = false;
            panel7.Visible = true;
        }
    }
}
