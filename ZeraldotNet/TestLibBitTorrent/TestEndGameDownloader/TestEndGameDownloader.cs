using System.Collections.Generic;
using NUnit.Framework;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Downloads;

namespace ZeraldotNet.TestLibBitTorrent.TestEndGameDownloader
{
    [TestFixture]
    public class TestEndGameDownloader
    {
        /// <summary>
        /// Piece came in no interest lost
        /// </summary>
        public void Test1()
        {
            List<string> events = new List<string>();
            DummyConnection c1 = new DummyConnection(events);
            DummyConnection c2 = new DummyConnection(events);
            List<ActiveRequest> requests = new List<ActiveRequest>();
            requests.Add(new ActiveRequest(0,0,2));

            EndGameSingleDownload ad1 = new EndGameSingleDownload();

        }
    }
}