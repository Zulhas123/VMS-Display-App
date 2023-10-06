using DisplayApp.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using Timer = System.Windows.Forms.Timer;

namespace DisplayApp.UI
{
    public partial class frmTest1 : Form
    {
        public frmTest1()
        {
            InitializeComponent();
        }

        private void frmTest1_Load(object sender, EventArgs e)
        {
            //pictureBox2.Visible = false;
            mainGroupBox.Visible = true;
            lblDate.Text = @"Date : " + DateTime.Now.ToString("dd-MMM-yyyy");
            Timer timer = new Timer();
            timer.Interval = (1 * 50); // 10 secs
            timer.Tick += new EventHandler(timer1_Tick);
            timer.Start();
            if (!backgroundWorker.IsBusy)
            {
                backgroundWorker.RunWorkerAsync();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblClock.Text = @"Time : " + DateTime.Now.ToString("h:mm:ss tt");
        }

        private List<VisitorDataVm> latestVisitorData = new List<VisitorDataVm>();
        private List<System.Drawing.Image> latestVisitorImages = new List<System.Drawing.Image>();
        private readonly object lockObject = new object();

        //string image = @"E:\ManagementConsoleImage\Resources\dvImage.jpg";


        void CheckLatestPunch()
        {
            try
            {
                var csn = ConfigurationManager.AppSettings["ControllerSn"].ToString();
                var readerId = ConfigurationManager.AppSettings["ReaderId"].ToString();

                while (true)
                {
                    var url = new Uri("http://localhost:62474/api/PunchHistory/GetPunchHistoryListData?controlerSn=" + csn + "&readerNo=" + readerId);
                    var httpClient = new HttpClient();
                    var res = httpClient.GetAsync(url);
                    var result = res.Result.Content.ReadAsStringAsync().Result;
                    var visitorDataList = JsonConvert.DeserializeObject<List<VisitorDataVm>>(result);

                    if (visitorDataList != null && visitorDataList.Count > 0)
                    {
                        lock (lockObject)
                        {
                            // Assuming latestVisitorData is a List<VisitorDataVm>
                            latestVisitorData.RemoveAll(v => visitorDataList.Any(item => item.LastId == v.LastId));

                            // Assuming latestVisitorImages is a List<Image>
                            latestVisitorImages.RemoveAll(img => visitorDataList.Any(item => ImageToBase64(img) == item.Image));

                            latestVisitorData.AddRange(visitorDataList);

                            // Assuming Base64ToImage is a function that converts base64 string to Image
                            latestVisitorImages.AddRange(visitorDataList.Select(item => Base64ToImage(item.Image)));

                            latestVisitorData.Sort((a, b) => b.PunchTime.CompareTo(a.PunchTime));
                            latestVisitorData = latestVisitorData?.Take(5).ToList();

                            UpdateUI();
                        }
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        string ImageToBase64(System.Drawing.Image image)
        {
            try
            {
                if (image == null)
                {
                    Console.WriteLine("Input image is null.");
                    return string.Empty;
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, image.RawFormat);

                    string base64String = Convert.ToBase64String(ms.ToArray());
                    return base64String;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error converting Image to Base64: " + ex.Message);
                return string.Empty;
            }
        }

        System.Drawing.Image Base64ToImage(string base64String)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64String);

                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
                    return image;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error converting Base64 to Image: " + ex.Message);
                return null; 
            }
        }
        void UpdateUI()
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new Action(UpdateUI));
            }
            else
            {
                listBox1.Items.Clear();
                foreach (var data in latestVisitorData)
                {
                    listBox1.Items.Add($"{"Name : "+ data.Name} - {"Punch Time : " + data.PunchTime.ToString("h:mm:ss tt")}");
                }
            }
            if (listBox2.InvokeRequired)
            {
                listBox2.Invoke(new Action(UpdateUI));
            }
            else
            {
                listBox2.Items.Clear();


                foreach (var image in latestVisitorImages)
                {
                    //listBox2.Items.Add(image);
                }
            }
        }


        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            CheckLatestPunch();
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
