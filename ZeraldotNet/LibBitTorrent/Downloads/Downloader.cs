using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Storages;
using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.PiecePickers;

namespace ZeraldotNet.LibBitTorrent.Downloads
{
    public class Downloader : IDownloader
    {
        #region Private Fields

        private IStorageWrapper storageWrapper;
        private IPiecePicker piecePicker;
        private int backLog;
        private double maxRatePeriod;
        private Measure downloadMeasure;
        private int piecesNumber;
        private double snubTime;
        private DataDelegate measureFunction;
        private List<SingleDownload> downloads;

        #endregion

        #region Public Properties

        public int BackLog
        {
            get
            {
                return backLog;
            }
        }

        public Measure DownloadMeasure
        {
            get
            {
                return downloadMeasure;
            }
        }

        public DataDelegate MeasureFunction
        {
            get
            {
                return measureFunction;
            }
        }

        public double MaxRatePeriod
        {
            get
            {
                return maxRatePeriod;
            }
        }

        public IPiecePicker PiecePicker
        {
            get
            {
                return piecePicker;
            }
        }

        public List<SingleDownload> Downloads
        {
            get
            {
                return downloads;
            }
        }

        public double SnubTime
        {
            get
            {
                return snubTime;
            }
        }

        public IStorageWrapper StorageWrapper
        {
            get
            {
                return storageWrapper;
            }
        }

        public int PiecesNumber
        {
            get
            {
                return piecesNumber;
            }
        }

        #endregion

        #region Constructors

        public Downloader(IStorageWrapper storageWrapper, IPiecePicker piecePicker, int backLog, double maxRatePeriod,
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

        #endregion

        #region IDownloader Members

        public ISingleDownload MakeDownload(IConnection connection)
        {
            SingleDownload singleDownload = new SingleDownload(this, connection);
            downloads.Add(singleDownload);
            return singleDownload;
        }

        #endregion
    }
}
