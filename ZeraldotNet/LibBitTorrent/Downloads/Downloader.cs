using System.Collections.Generic;
using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.Storages;

namespace ZeraldotNet.LibBitTorrent.Downloads
{
    public abstract class Downloader
    {
        #region Fields

        protected IStorageWrapper storageWrapper;
        protected int backLog;
        protected double maxRatePeriod;
        protected Measure downloadMeasure;
        protected int piecesNumber;
        protected double snubTime;
        protected DataDelegate measureFunction;
        protected List<SingleDownload> downloads;

        #endregion

        #region Properties

        public int BackLog
        {
            get { return backLog; }
        }

        public Measure DownloadMeasure
        {
            get { return downloadMeasure; }
        }

        public DataDelegate MeasureFunction
        {
            get { return measureFunction; }
        }

        public double MaxRatePeriod
        {
            get { return maxRatePeriod; }
        }

        public List<SingleDownload> Downloads
        {
            get { return downloads; }
        }

        public double SnubTime
        {
            get { return snubTime; }
        }

        public IStorageWrapper StorageWrapper
        {
            get { return storageWrapper; }
        }

        public int PiecesNumber
        {
            get { return piecesNumber; }
        }

        #endregion

        #region Methods

        public abstract SingleDownload MakeDownload(IConnection connection);

        #endregion

    }
}
