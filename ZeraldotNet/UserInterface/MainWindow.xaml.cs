using System.Windows;
using System.Windows.Documents;
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
        private Task _task;

        private const string winedtTorrentFile = @"E:\Bittorrent\Torrents\winedt70.exe.torrent";
        private const string winedtSaveAsDirectory = @"E:\Winedt70";
        private const string sumatraPDFTorrentFile = @"E:\Bittorrent\Torrents\SumatraPDF-2.1.1-install.exe.torrent";
        private const string sumatraPDFSaveAsDirectory = @"E:\SumatraPDF";
        private const string greenThemepackTorrentFile = @"E:\Bittorrent\Torrents\Green.themepack.torrent";
        private const string greenThemepackSaveAsDirectory = @"E:\GreenThemepack";
        private const string foobarTorrentFile = @"E:\Bittorrent\Torrents\foobar2000_v1.2.exe.torrent";
        private const string foobarSaveAsDirectory = @"E:\foobar2000";

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            _openFileDialog = new OpenFileDialog();
            _openFileDialog.DefaultExt = ".torrent";
            _folderBrowserDialog = new FolderBrowserDialog();

            tbTorrentFile.Text = foobarTorrentFile;
            tbSaveAsDirectory.Text = foobarSaveAsDirectory;
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
                tbTorrentFile.Text = _openFileDialog.FileName;
            }
        }

        private void btnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (_folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbSaveAsDirectory.Text = _folderBrowserDialog.SelectedPath;
            }
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            _task = new Task();
            _task.OnMessage += _task_OnMessage;
            _task.OnFinished += (o, args) => System.Windows.MessageBox.Show("Download is finished!");
            _task.Start(tbTorrentFile.Text, tbSaveAsDirectory.Text);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (_task != null)
            {
                _task.Stop();
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            rtbLog.Clear();
        }
    }
}
