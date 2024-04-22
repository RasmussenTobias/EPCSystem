using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace prototype
{
    public class ElectricityCertificate
    {
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public string Quality { get; set; }
        public double Loss { get; set; }
        public string Device { get; set; }
        public string Ownership { get; set; }

        public ElectricityCertificate(DateTime date, double amount, string quality, double loss, string device, string ownership)
        {
            Date = date;
            Amount = amount;
            Quality = quality; 
            Loss = loss;
            Device = device;
            Ownership = ownership;
        }

        public static void SaveCertificateToJson(ElectricityCertificate certificate, string filePath)
        {
            List<ElectricityCertificate> certificates = new List<ElectricityCertificate>();
            if (File.Exists(filePath))
            {
                string existingData = File.ReadAllText(filePath);
                certificates = JsonConvert.DeserializeObject<List<ElectricityCertificate>>(existingData) ?? new List<ElectricityCertificate>();
            }
            certificates.Add(certificate);

            string json = JsonConvert.SerializeObject(certificates, Formatting.Indented);

            string directoryPath = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(directoryPath);

            File.WriteAllText(filePath, json);
            MessageBox.Show($"Certificate saved to: {filePath}", "Certificate Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static List<ElectricityCertificate> LoadCertificatesFromJson(string filePath)
        {
            List<ElectricityCertificate> certificates = new List<ElectricityCertificate>();
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                certificates = JsonConvert.DeserializeObject<List<ElectricityCertificate>>(json) ?? new List<ElectricityCertificate>();
            }
            return certificates;
        }

    }
}
