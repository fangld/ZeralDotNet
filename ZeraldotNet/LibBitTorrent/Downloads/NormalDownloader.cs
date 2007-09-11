using System.Collections.Generic;
using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.PiecePickers;
using ZeraldotNet.LibBitTorrent.Storages;

namespace ZeraldotNet.LibBitTorrent.Downloads
{
    public class NormalDownloader : Downloader
    {
        #region Fields

        private readonly IPiecePicker piecePicker;

        #endregion

        #region Properties

        public IPiecePicker PiecePicker
        {
            get { return piecePicker; }
        }

        #endregion

        #region Constructors

        public NormalDownloader(IStorageWrapper storageWrapper, IPiecePicker piecePicker, int backLog, double maxRatePeriod,
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

        protected NormalDownloader(IStorageWrapper storageWrapper, int pieceNumber, List<SingleDownload> downloads)
        {
            this.storageWrapper = storageWrapper;
            this.backLog = 5;
            this.maxRatePeriod = 50;
            this.piecesNumber = pieceNumber;
            this.downloadMeasure = new Measure(15);
            this.measureFunction = null;
            this.downloads = downloads;
            this.snubTime = 60;
        }

        #endregion

        #region IDownloader Members

        public override SingleDownload MakeDownload(IConnection connection)
        {
            NormalSingleDownload singleDownload = new NormalSingleDownload(this, connection);
            downloads.Add(singleDownload);
            return singleDownload;
        }

        #endregion
    }
}
