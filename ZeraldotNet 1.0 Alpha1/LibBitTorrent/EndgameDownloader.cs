using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public class EndgameDownloader : IDownloader
    {
        public EndgameDownloader(IDownloader oldDownloader)
        {
            Downloader old = (Downloader)oldDownloader;
        }

        #region IDownloader Members

        public ISingleDownload MakeDownload(Connection connection)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
