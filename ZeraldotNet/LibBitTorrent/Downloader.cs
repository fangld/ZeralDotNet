using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Storages;

namespace ZeraldotNet.LibBitTorrent
{
    public delegate bool WantDelegate(int piece);

    public class Downloader : IDownloader
    {
        private StorageWrapper storageWrapper;

        private PiecePicker piecePicker;
        private int backLog;
        private double maxRatePeriod;
        private Measure downloadMeasure;
        private int piecesNumber;
        private double snubTime;
        private DataDelegate measureFunction;
        private List<SingleDownload> downloads;

        public Downloader(StorageWrapper storageWrapper, PiecePicker piecePicker, int backLog, double maxRatePeriod,
            int piecesNumber, Measure downloadMeasure, double snubTime, DataDelegate measureFunction)
        {
            this.storageWrapper = storageWrapper;
            this.piecePicker = piecePicker;
            this.backLog = backLog;
            this.maxRatePeriod = maxRatePeriod;
            this.downloadMeasure = downloadMeasure;
            this.piecesNumber = piecesNumber;
            this.snubTime = snubTime;
            this.measureFunction = measureFunction;
            this.downloads = new List<SingleDownload>();
        }

        #region IDownloader Members

        public ISingleDownload MakeDownload(Connection connection)
        {
            SingleDownload singleDownload = new SingleDownload(this, connection);
            downloads.Add(singleDownload);
            return singleDownload;
        }

        #endregion
    }
}
