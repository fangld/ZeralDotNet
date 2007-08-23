using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Storages;
using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.PiecePickers;

namespace ZeraldotNet.LibBitTorrent.Downloads
{
    public class NormalDownloader : Downloader
    {
        #region Private Fields

        private IPiecePicker piecePicker;

        #endregion

        #region Public Properties

        public IPiecePicker PiecePicker
        {
            get
            {
                return piecePicker;
            }
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
