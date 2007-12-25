using System.Collections.Generic;
using NUnit.Framework;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.Messages;

namespace ZeraldotNet.UnitTest.TestLibBitTorrent.TestConnecters
{
    [TestFixture]
    public class TestConnecter
    {
        /// <summary>
        /// 记录发生的事件
        /// </summary>
        public static List<string> events;

        public static DummyUpload MakeUpload(IConnection connection)
        {
            return new DummyUpload(events);
        }

        public static bool DummyPending()
        {
            return false;
        }

        [Test]
        public void TestOperation()
        {
            events = new List<string>();
            List<IConnection> cs = new List<IConnection>();

            IConnecter co = new DummyConnecter(new DummyDownloader(events), new DummyChoker(events, cs), 3, DummyPending, new Measure(10), 0, null);
            Assert.AreEqual(0, events.Count);
            Assert.AreEqual(0, cs.Count);

            DummyEncryptedConnection dc = new DummyEncryptedConnection(events);
            //建立连接
            co.MakeConnection(dc);
            Assert.AreEqual(1, cs.Count);
            Assert.AreEqual("make upload", events[0]);
            Assert.AreEqual("make StartDownload", events[1]);
            Assert.AreEqual("make", events[2]);

            IConnection cc = cs[0];
            //获得Bitfield网络信息
            co.GetMessage(dc, new byte[] { (byte)MessageType.BitField, 0xC0 });
            Assert.AreEqual("bitfield:0xC0,", events[events.Count - 1]);

            //获得Choke网络信息
            co.GetMessage(dc, new byte[] { (byte)MessageType.Choke });
            Assert.AreEqual("choke", events[events.Count - 1]);

            //获得Unchoke网络信息
            co.GetMessage(dc, new byte[] { (byte)MessageType.Unchoke });
            Assert.AreEqual("unchoke", events[events.Count - 1]);

            //获得Interested网络信息
            co.GetMessage(dc, new byte[] { (byte)MessageType.Interested });
            Assert.AreEqual("interested", events[events.Count - 1]);

            //获得Not Interested网络信息
            co.GetMessage(dc, new byte[] { (byte)MessageType.NotInterested });
            Assert.AreEqual("not interested", events[events.Count - 1]);

            //获得Have网络信息
            co.GetMessage(dc, new byte[] { (byte)MessageType.Have, 0x00, 0x00, 0x00, 0x02 });
            Assert.AreEqual("have:2", events[events.Count - 1]);

            //获得Request网络信息
            co.GetMessage(dc, new byte[] { (byte)MessageType.Request, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x06 });
            Assert.AreEqual("request index:1, begin:5, length:6", events[events.Count - 1]);

            //获得Cancel网络信息
            co.GetMessage(dc, new byte[] { (byte)MessageType.Cancel, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04 });
            Assert.AreEqual("cancel index:2, begin:3, length:4", events[events.Count - 1]);

            //获得Piece网络信息
            co.GetMessage(dc, new byte[] { (byte)MessageType.Piece, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, (byte)'a', (byte)'b', (byte)'c' });
            co.GetMessage(dc, new byte[] { (byte)MessageType.Piece, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, (byte)'d', (byte)'e', (byte)'f' });
            Assert.AreEqual("Message:0x04,0x00,0x00,0x00,0x01,", events[events.Count - 1]);

            co.FlushConnection(dc);
            Assert.AreEqual("flush", events[events.Count - 1]);

            //发送Bitfield网络信息
            cc.SendBitfield(new bool[] { false, true, true });
            Assert.AreEqual("Message:0x05,0x60,", events[events.Count - 1]);

            //发送Interestd网络信息
            cc.SendInterested();
            Assert.AreEqual("Message:0x02,", events[events.Count - 1]);

            //发送Not Interestd网络信息
            cc.SendNotInterested();
            Assert.AreEqual("Message:0x03,", events[events.Count - 1]);

            //发送Choke网络信息
            cc.SendChoke();
            Assert.AreEqual("Message:0x00,", events[events.Count - 1]);

            //发送Unchoke网络信息
            cc.SendUnchoke();
            Assert.AreEqual("Message:0x01,", events[events.Count - 1]);

            //发送Have网络信息
            cc.SendHave(4);
            Assert.AreEqual("Message:0x04,0x00,0x00,0x00,0x04,", events[events.Count - 1]);

            //发送Request网络信息
            cc.SendRequest(0, 2, 1);
            Assert.AreEqual("Message:0x06,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x02,0x00,0x00,0x00,0x01,", events[events.Count - 1]);

            //发送Cancel网络信息
            cc.SendCancel(1, 2, 3);
            Assert.AreEqual("Message:0x08,0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x02,0x00,0x00,0x00,0x03,", events[events.Count - 1]);

            //发送Piece网络信息
            cc.SendPiece(1, 2, new byte[] { (byte)'a', (byte)'b', (byte)'c' });
            Assert.AreEqual("Message:0x07,0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x02,0x61,0x62,0x63,", events[events.Count - 1]);
        }
    }
}