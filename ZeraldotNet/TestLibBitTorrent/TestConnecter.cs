using System;
using System.IO;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.BitTorrentMessages;

namespace ZeraldotNet.TestLibBitTorrent
{
    public class DummyConnecter
    {
        DummyDownloader downloader;
        private bool rateCapped;

        public bool RateCapped
        {
            get { return this.rateCapped; }
            set { this.rateCapped = value; }
        }

        SchedulerDelegate scheduleFunction;
        Dictionary<DummyConnection, TestConnection> connections;
        PendingDelegate isEverythingPending;
        int piecesNumber;
        DummyChoker choker;
        Measure totalup;
        int maxUploadRate;

        bool endgame;

        private DummyUpload MakeUpload(object c)
        {
            return TestConnecter.MakeUpload(c);
        }

        public DummyConnecter(DummyDownloader downloader, DummyChoker choker, int piecesNumber, PendingDelegate isEverythingpending, Measure totalup, int maxUploadRate, SchedulerDelegate scheduleFunction)
        {
            this.downloader = downloader;
            this.choker = choker;
            this.piecesNumber = piecesNumber;
            this.isEverythingPending = isEverythingpending;
            this.maxUploadRate = maxUploadRate;
            this.scheduleFunction = scheduleFunction;
            this.totalup = totalup;
            this.rateCapped = false;
            this.connections = new Dictionary<DummyConnection, TestConnection>();
            this.endgame = false;
            CheckEndgame();
        }

        public void UpdateUploadRate(int amount)
        {
            totalup.UpdateRate(amount);
            if (maxUploadRate > 0 && totalup.NonUpdatedRate > maxUploadRate)
            {
                rateCapped = true;
                scheduleFunction(new TaskDelegate(Uncap), totalup.TimeUntilRate(maxUploadRate), "Update Upload Rate");
            }
        }

        public void Uncap()
        {
        }

        public int ConnectionsCount
        {
            get { return connections.Count; }
        }

        public void MakeConnection(DummyConnection connection)
        {
            TestConnection conn = new TestConnection(connection, this);
            connections[connection] = conn;
            conn.Upload = MakeUpload(conn);
            conn.Download = downloader.MakeDownload(conn);
            choker.MakeConnection(conn);
        }

        public void LoseConnection(DummyConnection connection)
        {
            TestConnection conn = connections[connection];
            DummyDownload singleDownload = conn.Download;
            connections.Remove(connection);
            singleDownload.Disconnected();
            choker.LoseConnection(conn);
        }

        public void FlushConnection(DummyConnection connection)
        {
            connections[connection].Upload.Flush();
        }

        public void CheckEndgame()
        {
            if (!endgame && isEverythingPending())
            {
                endgame = true;
            }
        }

        public void GetMessage(DummyConnection connection, byte[] message)
        {
            TestConnection conn = connections[connection];
            BitTorrentMessageType firstByte = (BitTorrentMessageType)message[0];

            if (firstByte == BitTorrentMessageType.BitField && conn.GetAnything)
            {
                connection.Close();
                return;
            }

            conn.GetAnything = true;

            if ((firstByte == BitTorrentMessageType.Choke || firstByte == BitTorrentMessageType.Unchoke || 
                firstByte == BitTorrentMessageType.Interested || firstByte == BitTorrentMessageType.NotInterested) 
                && message.Length != 1)
            {
                connection.Close();
                return;
            }

            if (firstByte == BitTorrentMessageType.Choke)
            {
                conn.Download.GetChoke();
            }

            else if (firstByte == BitTorrentMessageType.Unchoke)
            {
                conn.Download.GetUnchoke();
                CheckEndgame();
            }

            else if (firstByte == BitTorrentMessageType.Interested)
            {
                conn.Upload.GetInterested();
            }

            else if (firstByte == BitTorrentMessageType.NotInterested)
            {
                conn.Upload.GetNotInterested();
            }

            else if (firstByte == BitTorrentMessageType.Have)
            {
                if (message.Length != 5)
                {
                    connection.Close();
                    return;
                }

                int index = Globals.BytesToInt32(message, 1);

                if (index >= this.piecesNumber)
                {
                    connection.Close();
                    return;
                }
                conn.Download.GetHave(index);
                CheckEndgame();
            }

            else if (firstByte == BitTorrentMessageType.BitField)
            {
                bool[] booleans = BitField.FromBitField(message, 1, piecesNumber);
                if (booleans == null)
                {
                    connection.Close();
                    return;
                }
                conn.Download.GetHaveBitfield(booleans);
                this.CheckEndgame();
            }

            else if (firstByte == BitTorrentMessageType.Request)
            {
                if (message.Length != 13)
                {
                    connection.Close();
                    return;
                }
                int index = Globals.BytesToInt32(message, 1);
                if (index >= this.piecesNumber)
                {
                    connection.Close();
                    return;
                }
                int begin = Globals.BytesToInt32(message, 5);
                int length = Globals.BytesToInt32(message, 9);
                conn.Upload.GetRequest(index, begin, length);
            }

            else if (firstByte == BitTorrentMessageType.Cancel)
            {

                if (message.Length != 13)
                {
                    connection.Close();
                    return;
                }
                int index = Globals.BytesToInt32(message, 1);
                if (index >= piecesNumber)
                {
                    connection.Close();
                    return;
                }
                int begin = Globals.BytesToInt32(message, 5);
                int length = Globals.BytesToInt32(message, 9);
                conn.Upload.GetCancel(index, begin, length);
            }

            else if (firstByte == BitTorrentMessageType.Piece)
            {
                if (message.Length <= 9)
                {
                    connection.Close();
                    return;
                }

                int index = Globals.BytesToInt32(message, 1);

                if (index >= piecesNumber)
                {
                    connection.Close();
                    return;
                }

                byte[] pieces = new byte[message.Length - 9];
                Globals.CopyBytes(message, 9, pieces);
                int begin = Globals.BytesToInt32(message, 5);
                if (conn.Download.GetPiece(index, begin, pieces))
                {
                    foreach (TestConnection item in connections.Values)
                    {
                        item.SendHave(index);
                    }
                }
                CheckEndgame();

            }
            else
                connection.Close();
        }
    }

    public class DummyDownload
    {
        List<string> events;
        int hit;

        public DummyDownload(List<string> events)
        {
            this.events = events;
            events.Add("make download");
            hit = 0;
        }

        public void Disconnected()
        {
            events.Add("disconnected");
        }

        public void GetChoke()
        {
            events.Add("choke");
        }

        public void GetUnchoke()
        {
            events.Add("unchoke");
        }

        public void GetHave(int i)
        {
            events.Add(string.Format("have:{0}", i));
        }

        public void GetHaveBitfield(bool[] bitfield)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("bitfield:");

            byte[] bitfieldBytes = BitField.ToBitField(bitfield);
            foreach (byte item in bitfieldBytes)
            {
                sb.AppendFormat("0x{0:X2},", item);
            }
            events.Add(sb.ToString());
        }

        public bool GetPiece(int index, int begin, byte[] piece)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("request index:{0}, begin:{1}, piece:", index, begin));
            foreach (byte item in piece)
            {
                sb.AppendFormat("0x{0:X2},", item);
            }

            events.Add(sb.ToString());
            hit += 1;
            return hit > 1;
        }
    }

    public class TestConnection
    {
        public DummyConnection connection;
        DummyConnecter connecter;
        public bool getAnything;

        public bool GetAnything
        {
            get { return this.getAnything; }
            set { this.getAnything = value; }
        }

        private DummyDownload download;

        public DummyDownload Download
        {
            get { return this.download; }
            set { this.download = value; }
        }


        private DummyUpload upload;

        public DummyUpload Upload
        {
            get { return this.upload; }
            set { this.upload = value; }
        }

        private BitTorrentMessage message;

        public TestConnection(DummyConnection connection, DummyConnecter connecter)
        {
            this.connection = connection;
            this.connecter = connecter;
            this.GetAnything = false;
        }

        public string GetIP()
        {
            return "";//connection.get_ip();
        }

        public byte[] GetID()
        {
            return null;//connection.get_id();
        }

        public void close()
        {
            //connection.close();
        }

        public bool IsFlushed()
        {
            //if (connecter.rate_capped)
            return false;
            //return connection.is_flushed();
        }

        public bool IsLocallyInitiated()
        {
            return false; //connection.is_locally_initiated();
        }

        public void SendInterested()
        {
            message = new InterestedMessage();
            connection.SendMessage(message.Encode());
        }

        public void SendNotInterested()
        {
            message = new NotInterestedMessage();
            connection.SendMessage(message.Encode());
        }

        public void SendChoke()
        {
            message = new ChokeMessage();
            connection.SendMessage(message.Encode());
        }

        public void SendUnchoke()
        {
            message = new UnchokeMessage();
            connection.SendMessage(message.Encode());
        }

        public void SendRequest(int index, int begin, int length)
        {
            message = new RequestMessage(index, begin, length);
            connection.SendMessage(message.Encode());
        }

        public void SendCancel(int index, int begin, int length)
        {
            message = new RequestMessage(index, begin, length);
            connection.SendMessage(message.Encode());
        }

        public void SendPiece(int index, int begin, byte[] piece)
        {
            message = new PieceMessage(index, begin, piece);
            connection.SendMessage(message.Encode());
        }

        public void SendBitfield(bool[] bitfield)
        {
            message = new BitFieldMessage(bitfield);
            connection.SendMessage(message.Encode());
        }

        public void SendHave(int index)
        {
            message = new HaveMessage(index);
            connection.SendMessage(message.Encode());
        }

        public DummyUpload GetUpload()
        {
            return upload;
        }

        public DummyDownload GetDownload()
        {
            return download;
        }
    }

    public class DummyUpload
    {
        List<string> events;

        public DummyUpload(List<string> events)
        {
            this.events = events;
            events.Add("make upload");
        }

        public void Flush()
        {
            events.Add("flush");
        }

        public void GetInterested()
        {
            events.Add("interested");
        }

        public void GetNotInterested()
        {
            events.Add("not interested");
        }

        public void GetRequest(int index, int begin, int length)
        {
            events.Add(string.Format("request index:{0}, begin:{1}, length:{2}", index, begin, length));
        }

        public void GetCancel(int index, int begin, int length)
        {
            events.Add(string.Format("cancel index:{0}, begin:{1}, length:{2}", index, begin, length));
        }

    }

    public class DummyDownloader
    {
        List<string> events;

        public DummyDownloader(List<string> events)
        {
            this.events = events;
        }

        public DummyDownload MakeDownload(TestConnection connection)
        {
            return new DummyDownload(events);
        }
    }

    public class DummyConnection
    {
        List<string> events;

        public DummyConnection(List<string> events)
        {
            this.events = events;
        }

        public void SendMessage(byte[] message)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Message:");
            foreach (byte item in message)
            {
                sb.AppendFormat("0x{0:X2},", item);
            }
            events.Add(sb.ToString());
        }

        public void Close()
        {
        }
    }

    public class DummyChoker
    {
        List<string>  events;
        public List<TestConnection> cs;

        public DummyChoker(List<string> events, List<TestConnection> cs)
        {
            this.events = events;
            this.cs = cs;
        }

        public void MakeConnection(TestConnection c)
        {
            events.Add("make");
            cs.Add(c);
        }

        public void LoseConnection(TestConnection c)
        {
            events.Add("lost");
        }
    }


    [TestFixture]
    public class TestConnecter
    {
        public static List<string> events;

        public static DummyUpload MakeUpload(object c)
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
            List<TestConnection> cs = new List<TestConnection>();

            DummyConnecter co = new DummyConnecter(new DummyDownloader(events), new DummyChoker(events, cs), 3, new PendingDelegate(DummyPending), new Measure(10), 0, null);
            Assert.AreEqual(0, events.Count);
            Assert.AreEqual(0, cs.Count);

            DummyConnection dc = new DummyConnection(events);
            co.MakeConnection(dc);
            Assert.AreEqual(1, cs.Count);
            Assert.AreEqual("make upload", events[0]);
            Assert.AreEqual("make download", events[1]);
            Assert.AreEqual("make", events[2]);

            TestConnection cc = cs[0];
            co.GetMessage(dc, new byte[] { (byte)BitTorrentMessageType.BitField, 0xC0 });
            Assert.AreEqual("bitfield:0xC0,", events[events.Count - 1]);

            co.GetMessage(dc, new byte[] { (byte)BitTorrentMessageType.Choke });
            Assert.AreEqual("choke", events[events.Count - 1]);

            co.GetMessage(dc, new byte[] { (byte)BitTorrentMessageType.Unchoke });
            Assert.AreEqual("unchoke", events[events.Count - 1]);

            co.GetMessage(dc, new byte[] { (byte)BitTorrentMessageType.Interested });
            Assert.AreEqual("interested", events[events.Count - 1]);

            co.GetMessage(dc, new byte[] { (byte)BitTorrentMessageType.NotInterested });
            Assert.AreEqual("not interested", events[events.Count - 1]);

            co.GetMessage(dc, new byte[] { (byte)BitTorrentMessageType.Have, 0x00, 0x00, 0x00, 0x02 });
            Assert.AreEqual("have:2", events[events.Count - 1]);

            co.GetMessage(dc, new byte[] { (byte)BitTorrentMessageType.Request, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x06 });
            Assert.AreEqual("request index:1, begin:5, length:6", events[events.Count - 1]);

            co.GetMessage(dc, new byte[] { (byte)BitTorrentMessageType.Cancel, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04 });
            Assert.AreEqual("cancel index:2, begin:3, length:4", events[events.Count - 1]);

            co.GetMessage(dc, new byte[] { (byte)BitTorrentMessageType.Piece, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, (byte)'a', (byte)'b', (byte)'c' });
            co.GetMessage(dc, new byte[] { (byte)BitTorrentMessageType.Piece, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, (byte)'d', (byte)'e', (byte)'f' });
            Assert.AreEqual("Message:0x04,0x00,0x00,0x00,0x01,", events[events.Count - 1]);

            co.FlushConnection(dc);
            Assert.AreEqual("flush", events[events.Count - 1]);

            cc.SendBitfield(new bool[] { false, true, true });
            Assert.AreEqual("Message:0x05,0x60,", events[events.Count - 1]);

            cc.SendInterested();
            Assert.AreEqual("Message:0x02,", events[events.Count - 1]);

            cc.SendNotInterested();
            Assert.AreEqual("Message:0x03,", events[events.Count - 1]);

            cc.SendChoke();
            Assert.AreEqual("Message:0x00,", events[events.Count - 1]);

            cc.SendUnchoke();
            Assert.AreEqual("Message:0x01,", events[events.Count - 1]);

            cc.SendHave(4);
            Assert.AreEqual("Message:0x04,0x00,0x00,0x00,0x04,", events[events.Count - 1]);

            cc.SendRequest(0, 2, 1);
            Assert.AreEqual("Message:0x06,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x02,0x00,0x00,0x00,0x01,", events[events.Count - 1]);

            cc.SendCancel(1, 2, 3);
            Assert.AreEqual("Message:0x06,0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x02,0x00,0x00,0x00,0x03,", events[events.Count - 1]);

            cc.SendPiece(1, 2, new byte[] { (byte)'a', (byte)'b', (byte)'c' });
            Assert.AreEqual("Message:0x07,0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x02,0x61,0x62,0x63,", events[events.Count - 1]);
        }
    }
}