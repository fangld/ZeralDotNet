using System.Windows;
using System.Windows.Forms;
using ZeraldotNet.LibBitTorrent;

namespace UserInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private OpenFileDialog _openFileDialog;
        private FolderBrowserDialog _folderBrowserDialog;
        //private FolderBrowserDialog 

        private string _fileName;
        private string _path;
        private Task _task;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            _openFileDialog = new OpenFileDialog();
            _openFileDialog.DefaultExt = ".torrent";
            _folderBrowserDialog = new FolderBrowserDialog();
            _task = new Task();
            _task.OnMessage += _task_OnMessage;
        }

        void _task_OnMessage(object sender, string e)
        {
            Dispatcher.Invoke(() =>
                                  {
                                      rtbLog.AppendText(e);
                                      rtbLog.AppendText("\n");
                                      rtbLog.ScrollToEnd();
                                  });
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            if(_openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _fileName = _openFileDialog.FileName;
                tbOpenFile.Text = _fileName;
            }
        }

        private void btnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (_folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _path = _folderBrowserDialog.SelectedPath;
                tbOpenFolder.Text = _path;
            }
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            _task.OnFinished += (o, args) => System.Windows.MessageBox.Show("Download is finished!");
            _task.TorrentFileName = _fileName;
            _task.SaveAsDirectory = _path;
            _task.Start();
        }
    }
}
