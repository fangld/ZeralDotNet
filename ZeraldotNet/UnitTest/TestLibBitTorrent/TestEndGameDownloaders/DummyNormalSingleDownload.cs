using System.Collections.Generic;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.Downloads;

namespace ZeraldotNet.UnitTest.TestLibBitTorrent.TestEndGameDownloaders
{
    public class DummyNormalSingleDownload : NormalSingleDownload
    {
        public DummyNormalSingleDownload(IConnection connection, bool choked, bool interested, bool[] have, List<ActiveRequest> requests)
            : base(connection, choked, interested, have, requests) { }
    }
}