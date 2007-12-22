using System;
using System.Threading;
using System.Windows.Forms;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Downloads;

namespace ZeraldotNet.UserInterface
{
    public partial class mainForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly Flag doneFlag;
        private Parameters downloadParameters;

        public mainForm()
        {
            InitializeComponent();

            doneFlag = new Flag();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Torrent File";
            ofd.Filter = "All Files(*.*)|*.*";
            ofd.ShowDialog(this);
            if (ofd.FileName.Length == 0)
            {
                return;
            }

            downloadParameters = new Parameters();
            downloadParameters.ResponseFile = ofd.FileName;

            btnDownload.Enabled = false;


            rtbLog.Clear();

            Thread thread = new Thread(new ThreadStart(DownloadStuff));
            thread.Name = "Download Thread";
            thread.Start();
        }

        public void DownloadStuff()
        {
            Download.StartDownload(downloadParameters, doneFlag, new StatusDelegate(MyStatus),
                                   new ErrorDelegate(MyError), new FinishedDelegate(MyFinish));
        }

        public void MyFinish()
        {
            lbTimeEstimate.Text = "Download successful!";
            pbPercentage.EditValue = 100;
            lbDownloadRate.Text = string.Empty;
        }

        public void MyStatus(string message, double downloadRate, double uploadRate, double fractionDone, double timeEstimate)
        {
            BeginInvoke(new StatusDelegate(Status),
                        new object[] {message, downloadRate, uploadRate, fractionDone, timeEstimate});
        }

        public void MyError(string message)
        {
            BeginInvoke(new ErrorDelegate(ShowError), new object[] {message});
        }

        public void Status(string message, double downloadRate, double uploadRate, double fractionDone, double timeEstimate)
        {
            if (message != null && message.Length != 0)
            {
                rtbLog.AppendText(message);
                rtbLog.AppendText("\r\n");
            }

            if (downloadRate >= 0.0)
            {
                lbDownloadRate.Text = (downloadRate/1024).ToString("0.00") + " kB/s";
            }

            if (uploadRate >= 0.0)
            {
                lbUploadRate.Text = (uploadRate/1024).ToString("0.00") + "kB/s";
            }

            if (fractionDone >= 0.0)
            {
                pbPercentage.EditValue = (int) (fractionDone*100);
                lbPercentage.Text = (fractionDone*100).ToString("0.00") + "%";
            }

            if (timeEstimate > 0.0)
            {
                TimeSpan ts = TimeSpan.FromSeconds(timeEstimate);
                lbTimeEstimate.Text =
                    string.Format("{0} hours {1} minutes {2} seconds", ts.Hours + (ts.Days*24), ts.Minutes, ts.Seconds);
            }
        }

        public void ShowError(string message)
        {
            if (message != null && message.Length != 0)
            {
                rtbLog.AppendText("Error: ");
                rtbLog.AppendText(message);
                rtbLog.AppendText("\r\n");
            }
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            doneFlag.Set();
        }
    }
}