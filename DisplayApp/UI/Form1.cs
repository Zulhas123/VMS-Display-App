using DisplayApp.Properties;
using System;
using System.Drawing;
using System.Windows.Forms;
using DisplayApp.Model;
using System.Configuration;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using Timer = System.Windows.Forms.Timer;
using Newtonsoft.Json.Linq;

namespace DisplayApp.UI
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox2.Visible = false;
            mainGroupBox.Visible = true;
            lblDate.Text = @"Date : " + DateTime.Now.ToString("dd-MMM-yyyy");
            Timer timer = new Timer();
            timer.Interval = (1 * 50); // 10 secs
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }


        private void timer_Tick(object sender, EventArgs e)
        {
            // count++;
            lblClock.Text = @"Time : " + DateTime.Now.ToString("h:mm:ss tt");
            //string interFace = ConfigurationManager.AppSettings["InterfaceId"].ToString();
            //string macAddress = ConfigurationManager.AppSettings["MacAddress"].ToString();
            //string readerId = ConfigurationManager.AppSettings["ReaderId"].ToString();

        }
        void CheckLatestPunch()
        {
            try
            {
                var csn = ConfigurationManager.AppSettings["ControllerSn"].ToString();
                var readerId = ConfigurationManager.AppSettings["ReaderId"].ToString();
                var lastDislayValue = ConfigurationManager.AppSettings["LastDisplayId"];

                //http://localhost:62474/api/PunchHistory/GetVisitorInfo/controlerSn=423134525&readerNo=2&lastDisplayId=279881
               
                while (true)
                {
                    var url = new Uri("http://localhost:62474/api/PunchHistory/GetVisitorInfo?controlerSn=" + csn + "&readerNo=" + readerId + "&lastDisplayId=" + lastDislayValue);
                    var httpClient = new HttpClient();
                    var res = httpClient.GetAsync(url);
                    var result = res.Result.Content.ReadAsStringAsync().Result;
                    var visitorData = JsonConvert.DeserializeObject<VisitorDataVm>(result);
                    if (visitorData != null)
                    {
                        lblName.Invoke(new Action(() => lblName.Text = visitorData.Name));
                        lblPunchTime.Invoke(new Action(() => lblPunchTime.Text = visitorData.Name));
                        lblPunchTime.Invoke(new Action(() => lblPunchTime.Text = visitorData.PunchTime.ToString("h:mm:ss tt")));
                        Base64ToImage(visitorData.Image);
                        lastDislayValue = visitorData.LastId.ToString();

                        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        config.AppSettings.Settings["LastDisplayId"].Value = lastDislayValue;
                        config.Save(ConfigurationSaveMode.Modified);
                        ConfigurationManager.RefreshSection("appSettings");




                    }
                    Thread.Sleep(1000);

                }
            }
            catch (Exception)
            {

                MessageBox.Show("Error");
            }
          
        }
        private void Base64ToImage(string base64String)
        {


            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);

                // Create a memory stream from the byte array
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    // Load the image from the memory stream
                    Image image = Image.FromStream(ms);

                    // Display the image in the PictureBox
                    pictureBox1.Invoke(new Action(() => pictureBox1.Image = image));

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            CheckLatestPunch();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {

        }
    }
}
