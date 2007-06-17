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
    }
}
