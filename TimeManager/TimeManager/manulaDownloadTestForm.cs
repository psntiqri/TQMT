using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TimeManager
{
    public partial class manulaDownloadTestForm : Form
    {
        IList<AttendanceData>  data = new BindingList<AttendanceData>();
        
        const string devicename1 = "AC100";
        const string ipaddress1 = "192.168.20.204";
        const int deviceno1 = 4;
        const int portno1 = 4370;
        const int commkey1 = 4;

        private bool isDownloading = false;
        private bool isStopDownloadIssued = false;

        public manulaDownloadTestForm()
        {
            InitializeComponent();
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            if (downloadButton.Text == "Start Downloading")
            {
                isStopDownloadIssued = false;

                if (axBioBridgeSDK1.Connect_TCPIP(devicename1, deviceno1, ipaddress1, portno1, commkey1) == 0)
                {
                    //this.Text = "Connected";
                    downloadButton.Text = "Stop Downloading";
                    progressBar1.Visible = true;
                    timer1.Interval = int.Parse(poolingIntervalTextBox.Text) * 1000;
                    timer1.Start();
                }
                else
                {
                    downloadButton.Text = "Start Downloading";
                    progressBar1.Visible = false;
                    //this.Text = "Not Connected";
                }
            }
            else
            {
                downloadButton.Text = "Start Downloading";
                progressBar1.Visible = false;
            }
        }

        private void downloadData()
        {
            isDownloading = true;

            int logSize = 0;
            int year = 0;
            int month = 0;
            int day = 0;
            int hour = 0;
            int minute = 0;
            int sec = 0;
            int verifyMode = 0;
            int inOutMode = 0;
            int workCode = 0;
            int enrollNo = 0;

            if (axBioBridgeSDK1.ReadGeneralLog(ref logSize) == 0)
            {
                while (axBioBridgeSDK1.GetGeneralLog(ref enrollNo, ref year, ref month, ref day, ref hour, ref minute,
                                                  ref sec, ref verifyMode, ref inOutMode, ref workCode) == 0)
                {
                    var attendanceRecord = new AttendanceData()
                    {
                        EnrollNumber = enrollNo,
                        AttendanceDate = string.Format(@"{0}/{1}/{2}", day, month, year),
                        AttendanceTime = string.Format(@"{0}:{1}:{2}", hour, minute, sec),
                    };

                    if (data.Any(a => a.EnrollNumber == attendanceRecord.EnrollNumber &&
                        a.AttendanceDate == attendanceRecord.AttendanceDate &&
                        a.AttendanceTime == attendanceRecord.AttendanceTime))
                    {
                        continue;
                    }
                    data.Add(attendanceRecord);

                    ListViewItem item = new ListViewItem();
                    item.Text = attendanceRecord.EnrollNumber.ToString();
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item,attendanceRecord.AttendanceDate));
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, attendanceRecord.AttendanceTime));

                    listView1.Items.Insert(0, item);
                }
            }

            isDownloading = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (isStopDownloadIssued)
            {
                timer1.Stop();
                axBioBridgeSDK1.Disconnect();
                downloadButton.Text = "Start Downloading";
                progressBar1.Visible = false;

                isStopDownloadIssued = false;
                return;
            }

            if (isDownloading) return;

            downloadData();
        }
    }

    public class AttendanceData
    {
        public int EnrollNumber { get; set; }
        public string AttendanceDate { get; set; }
        public string AttendanceTime { get; set; }
    }
}
