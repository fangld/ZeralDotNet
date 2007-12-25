using System.Collections.Generic;
using NUnit.Framework;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Downloads;

namespace ZeraldotNet.UnitTest.TestLibBitTorrent.TestEndGameDownloaders
{
    [TestFixture]
    public class TestEndGameDownloader
    {
        /// <summary>
        /// Piece came in no interest lost
        /// </summary>
        [Test]
        public void Test1()
        {
            List<string> events = new List<string>();
            DummyConnection c1 = new DummyConnection(events);
            DummyConnection c2 = new DummyConnection(events);
            List<ActiveRequest> requests = new List<ActiveRequest>();
            List<SingleDownload> downloads = new List<SingleDownload>();
            requests.Add(new ActiveRequest(0,0,2));

            DummyNormalSingleDownload ad1 = new DummyNormalSingleDownload(c1, false, true, new bool[] {true}, requests);
            requests = new List<ActiveRequest>();
            requests.Add(new ActiveRequest(0,4,1));

            DummyNormalSingleDownload ad2 = new DummyNormalSingleDownload(c1, false, true, new bool[] { true }, requests);

            downloads.Add(ad1);
            downloads.Add(ad2);

            DummyStorageWrapper storageWrapper = new DummyStorageWrapper(events);
            DummyNormalDownloader downloader = new DummyNormalDownloader(storageWrapper,1,downloads);
            EndGameDownloader endgameDownloader = new EndGameDownloader(downloader);
            EndGameSingleDownload endgameDownload1 = (EndGameSingleDownload)c1.Download;
            EndGameSingleDownload endgameDownload2 = (EndGameSingleDownload)c2.Download;

            //assert events = [(c1, 'request', 0, 4, 1), (c2, 'request', 0, 0, 2)]
            events.Clear();
            Assert.AreEqual(true, endgameDownload1.GetPiece(0, 4, new byte[] {(byte) 'a'}));

            //assert events = [(s, 'came in', 0, 4, 'a'), (c2, 'cancel', 0, 4, 1)]
            events.Clear();
            DummyConnection c3 = new  DummyConnection(events);
            EndGameSingleDownload endgameDownload3 = (EndGameSingleDownload)endgameDownloader.MakeDownload(c3);
            Assert.AreEqual(0, events.Count);
            endgameDownload3.GetHave(0);

            //assert events = [(c3, 'interested'), (c3, 'request', 0, 0, 2)]
            events.Clear();
            DummyConnection c4 = new DummyConnection(events);
            EndGameSingleDownload endgameDownload4 = (EndGameSingleDownload) endgameDownloader.MakeDownload(c4);
            Assert.AreEqual(0,events.Count);
            
            //assert events = [(c4, 'interested'), (c3, 'request', 0, 0, 2)]
            Assert.AreEqual(1, downloader.DownloadMeasure.TotalLength);
        }

        /// <summary>
        /// Piece came in lost interested
        /// </summary>
        [Test]
        public void Test2()
        {
            List<string> events = new List<string>();
            DummyConnection c1 = new DummyConnection(events);
            DummyConnection c2 = new DummyConnection(events);
            DummyConnection c3 = new DummyConnection(events);
            DummyConnection c4 = new DummyConnection(events);
            List<ActiveRequest> requests = new List<ActiveRequest>();
            List<SingleDownload> downloads = new List<SingleDownload>();
            requests.Add(new ActiveRequest(0, 0, 2));

            DummyNormalSingleDownload ad1 = new DummyNormalSingleDownload(c1, false, true, new bool[] { true }, requests);
            DummyNormalSingleDownload ad2 = new DummyNormalSingleDownload(c2, false, true, new bool[] { true }, new List<ActiveRequest>());
            DummyNormalSingleDownload ad3 = new DummyNormalSingleDownload(c3, false, false, new bool[] { false }, new List<ActiveRequest>());
            DummyNormalSingleDownload ad4 = new DummyNormalSingleDownload(c4, true, true, new bool[] { true }, new List<ActiveRequest>());
            DummyStorageWrapper storageWrapper = new DummyStorageWrapper(events);

            downloads.Add(ad1);
            downloads.Add(ad2);
            downloads.Add(ad3);
            downloads.Add(ad4);

            DummyNormalDownloader downloader = new DummyNormalDownloader(storageWrapper, 1, downloads);
            EndGameDownloader endgameDownloader = new EndGameDownloader(downloader);
            EndGameSingleDownload endgameDownload1 = (EndGameSingleDownload)c1.Download;
            EndGameSingleDownload endgameDownload2 = (EndGameSingleDownload)c2.Download;
            EndGameSingleDownload endgameDownload3 = (EndGameSingleDownload)c3.Download;
            EndGameSingleDownload endgameDownload4 = (EndGameSingleDownload)c4.Download;

            //assert events = [(c2, 'request', 0, 2, 2)]
            events.Clear();
            Assert.AreEqual(true, endgameDownload1.GetPiece(0, 0, new byte[] { (byte)'a', (byte)'a' }));

            //assert events = [(s, 'came in', 0, 0, 'aa'),(c2, 'cancel', 0, 0, 2), 
            //    (c1, 'not interested'), (c2, 'not interested'), (c4, 'not interested'),
            //    (c1, 'close'), (c2, 'close'), (c4, 'close')]
            events.Clear();
            DummyConnection c5 = new DummyConnection(events);
            EndGameSingleDownload endgameDownload5 = (EndGameSingleDownload) endgameDownloader.MakeDownload(c5);
            Assert.AreEqual(0, events.Count);
            endgameDownload5.GetHave(0);

            //assert events = [(c5, 'close')]
            events.Clear();
            DummyConnection c6 = new DummyConnection(events);
            EndGameSingleDownload endgameDownload6 = (EndGameSingleDownload)endgameDownloader.MakeDownload(c6);
            Assert.AreEqual(0,events.Count);
            endgameDownload6.GetHaveBitField(new bool[] {true});

            //assert events = [(c6, 'close')]
        }

        /// <summary>
        /// Hash fail
        /// </summary>
        [Test]
        public void Test3()
        {
            List<string> events = new List<string>();
            DummyConnection c1 = new DummyConnection(events);
            DummyConnection c2 = new DummyConnection(events);
            DummyConnection c3 = new DummyConnection(events);
            DummyConnection c4 = new DummyConnection(events);
            List<ActiveRequest> requests = new List<ActiveRequest>();
            List<SingleDownload> downloads = new List<SingleDownload>();
            requests.Add(new ActiveRequest(0, 0, 2));

            DummyNormalSingleDownload ad1 = new DummyNormalSingleDownload(c1, false, true, new bool[] { true }, requests);
            DummyNormalSingleDownload ad2 = new DummyNormalSingleDownload(c2, false, true, new bool[] { true }, new List<ActiveRequest>());
            DummyNormalSingleDownload ad3 = new DummyNormalSingleDownload(c3, false, false, new bool[] { false }, new List<ActiveRequest>());
            DummyNormalSingleDownload ad4 = new DummyNormalSingleDownload(c4, true, true, new bool[] { true }, new List<ActiveRequest>());
            DummyStorageWrapper storageWrapper = new DummyStorageWrapper(events);

            List<InactiveRequest> inactiveRequests = new List<InactiveRequest>();
            inactiveRequests.Add(new InactiveRequest(0,4));
            storageWrapper.ExpectFlunk = inactiveRequests;

            downloads.Add(ad1);
            downloads.Add(ad2);
            downloads.Add(ad3);
            downloads.Add(ad4);

            DummyNormalDownloader downloader = new DummyNormalDownloader(storageWrapper, 1, downloads);
            EndGameDownloader endgameDownloader = new EndGameDownloader(downloader);
            EndGameSingleDownload endgameDownload1 = (EndGameSingleDownload)c1.Download;
            EndGameSingleDownload endgameDownload2 = (EndGameSingleDownload)c2.Download;
            EndGameSingleDownload endgameDownload3 = (EndGameSingleDownload)c3.Download;
            EndGameSingleDownload endgameDownload4 = (EndGameSingleDownload)c4.Download;

            //assert events = [(c2, 'request', 0, 0, 2)]
            events.Clear();
            Assert.AreEqual(false, endgameDownload1.GetPiece(0, 0, new byte[] { 0, 0 }));

            //assert events = [(s, 'came in', 0, 0, 'aa'),(c2, 'request', 0, 0, 4)]

            events.Clear();
            endgameDownload4.GetUnchoke();
            //assert events = [(c4, 'request', 0,0,4)]
        }
    }
}