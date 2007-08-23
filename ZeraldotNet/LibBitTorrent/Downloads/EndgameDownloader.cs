using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Connecters;

namespace ZeraldotNet.LibBitTorrent.Downloads
{
    public class EndgameDownloader : Downloader
    {
        #region Fields

        private List<ActiveRequest> requests;

        #endregion

        #region Constructors

        public EndgameDownloader(Downloader oldDownloader)
        {
            NormalDownloader old = (NormalDownloader)oldDownloader;
            this.storageWrapper = old.StorageWrapper;
            this.backLog = old.BackLog;
            this.maxRatePeriod = old.MaxRatePeriod;
            this.piecesNumber = old.PiecesNumber;
            this.downloadMeasure = old.DownloadMeasure;
            this.measureFunction = old.MeasureFunction;
            this.snubTime = old.SnubTime;

            this.downloads = new List<SingleDownload>();
            this.requests = new List<ActiveRequest>();


            foreach (NormalSingleDownload singleDownload in old.Downloads)
            {
                requests.AddRange(singleDownload.Requests);
                downloads.Add(new EndGameSingleDownload(this, null, singleDownload));
            }
        }

        #endregion

        #region IDownloader Members

        public override SingleDownload MakeDownload(IConnection connection)
        {
            EndGameSingleDownload result = new EndGameSingleDownload(this, connection, null);
            downloads.Add(result);
            return result;
        }

        #endregion
    }
}
