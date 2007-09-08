using System.Collections.Generic;
using NUnit.Framework;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Downloads;
using ZeraldotNet.LibBitTorrent.PiecePickers;
using ZeraldotNet.TestLibBitTorrent.TestNormalDownloader;

namespace ZeraldotNet.TestLibBitTorrent.TestNormalDownloader
{
    [TestFixture]
    public class TestNormalDownloader
    {
        /// <summary>
        /// stops at backlog
        /// </summary>
        [Test]
        public void Test1()
        {
            List<List<InactiveRequest>> temp = new List<List<InactiveRequest>>();
            List<InactiveRequest> temp2 = new List<InactiveRequest>();
            temp2.Add(new InactiveRequest(0, 2));
            temp2.Add(new InactiveRequest(2, 2));
            temp2.Add(new InactiveRequest(4, 2));
            temp2.Add(new InactiveRequest(6, 2));
            temp.Add(temp2);

            List<string> events = new List<string>();
            DummyStorageWrapper ds = new DummyStorageWrapper(temp);
            NormalDownloader downloader = new NormalDownloader(ds, new PiecePicker(ds.Active.Count), 2, 15, 1, new Measure(15), 10, null);
            SingleDownload singleDownload = downloader.MakeDownload(new DummyConnection(events));
            Assert.AreEqual(0, events.Count);
            singleDownload.GetChoke();
            singleDownload.GetHave(0);
            events.Clear();
            singleDownload.Disconnect();
        }

        /// <summary>
        /// Get have single
        /// </summary>
        [Test]
        public void Test2()
        {
            List<List<InactiveRequest>> temp = new List<List<InactiveRequest>>();
            List<InactiveRequest> temp2 = new List<InactiveRequest>();
            temp2.Add(new InactiveRequest(0, 2));
            temp.Add(temp2);

            List<string> events = new List<string>();
            DummyStorageWrapper ds = new DummyStorageWrapper(temp);
            NormalDownloader downloader = new NormalDownloader(ds, new PiecePicker(ds.Active.Count), 2, 15, 1, new Measure(15), 10, null);
            SingleDownload singleDownload = downloader.MakeDownload(new DummyConnection(events));

            Assert.AreEqual(0, events.Count);
            singleDownload.GetUnchoke();
            singleDownload.GetHave(0);
            events.Clear();
            singleDownload.Disconnect();
        }

        /// <summary>
        /// Choke clears active
        /// </summary>
        [Test]
        public void Test3()
        {
            List<List<InactiveRequest>> temp = new List<List<InactiveRequest>>();
            List<InactiveRequest> temp2 = new List<InactiveRequest>();
            temp2.Add(new InactiveRequest(0, 2));
            temp.Add(temp2);

            List<string> events = new List<string>();
            DummyStorageWrapper ds = new DummyStorageWrapper(temp);
            NormalDownloader downloader = new NormalDownloader(ds, new PiecePicker(ds.Active.Count), 2, 15, 1, new Measure(15), 10, null);
            SingleDownload singleDownload1 = downloader.MakeDownload(new DummyConnection(events));
            SingleDownload singleDownload2 = downloader.MakeDownload(new DummyConnection(events));

            singleDownload1.GetUnchoke();
            singleDownload1.GetHave(0);
            events.Clear();

            singleDownload2.GetUnchoke();
            singleDownload2.GetHave(0);
            events.Clear();

            singleDownload1.GetChoke();
            events.Clear();

            singleDownload2.GetPiece(0, 0, new byte[] { (byte)'a', (byte)'b' });
        }
    }
}