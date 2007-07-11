using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Connecters;

namespace ZeraldotNet.LibBitTorrent.Downloads
{
    public interface IDownloader
    {
        ISingleDownload MakeDownload(IConnection connection);
    }
}
