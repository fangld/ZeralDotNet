using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using Microsoft.Win32;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Downloads;

namespace ZeralUI
{
    /// <summary>
    /// mainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class mainWindow : Window
    {
        #region Fields

        private Flag finishFlag;
        private Parameters downloadParameters;

        #endregion

        public mainWindow()
        {
            InitializeComponent();
            finishFlag = new Flag();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Torrent File";
            ofd.Filter = "All Files (*.*)|*.*";
            ofd.ShowDialog(this);
            if(ofd.FileName.Length == 0)
            {
                return;
            }

            downloadParameters = new Parameters();
            downloadParameters.ResponseFile = ofd.FileName;

            //Download test
            button1.IsEnabled = false;

            richTextBox1.Document.Blocks.Clear();
            Thread thread = new Thread(DownloadStuff);
            thread.Name = "Download Thread";
            thread.Start();
        }

        private void DownloadStuff()
        {
            try
            {
                Download.StartDownload(downloadParameters, finishFlag, StatusThread, ErrorThread, Finish);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Finish()
        {
            labelTimeEstimate.Content = "Download Succeeded!";
            progressBar1.Value = 100;
            labelDownloadRate.Content = string.Empty;
        }

        private void StatusThread(string message, double downloadRate, double uploadRate, double fractionDone, double timeEstimate)
        {
            Dispatcher.BeginInvoke(new DispatcherPriority(), new StatusDelegate(ShowStatus), message, downloadRate,
                                   uploadRate, fractionDone, timeEstimate);
        }

        private void ErrorThread(string message)
        {
            Dispatcher.BeginInvoke(new DispatcherPriority(), new ErrorDelegate(ShowError), message);
        }

        private void ShowStatus(string message, double downloadRate, double uploadRate, double fractionDone, double timeEstimate)
        {
            if (message.Length == 0)
            {
                richTextBox1.AppendText(message);
                richTextBox1.AppendText("\r\n");
            }

            if (downloadRate >= 0.0)
            {
                labelDownloadRate.Content = (downloadRate/1024).ToString("0.00") + " kB/s";
            }

            if (uploadRate >= 0.0)
            {
                labelUploadRate.Content = (uploadRate/1024).ToString("0.00") + " kB/s";
            }

            if (fractionDone >= 0 )
            {
                progressBar1.Value = (int) (fractionDone*100);
                labelPercentage.Content = (fractionDone*100).ToString("0.00%");
            }

            if (timeEstimate >0)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(timeEstimate);
                labelTimeEstimate.Content =
                    string.Format("{0} hours {1} minutes {2} secondes", timeSpan.Hours + (timeSpan.Days*24),
                                  timeSpan.Minutes, timeSpan.Seconds);
            }
        }

        private void ShowError(string message)
        {
            if (message.Length == 0)
            {
                richTextBox1.AppendText(string.Format("Error: {0}\r\n", message));
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            finishFlag.Set();
        }
    }
}
