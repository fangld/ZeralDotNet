using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.TestLibBitTorrent.DummyMessages;

namespace ZeraldotNet.TestLibBitTorrent.TestConnecter
{
    /// <summary>
    /// 测试的连接类
    /// </summary>
    public class DummyConnection
    {
        #region Private Field

        private DummyEncryptedConnection connection;

        private DummyConnecter connecter;

        private bool getAnything;

        private DummyDownload download;

        private DummyUpload upload;

        private DummyMessage message;

        #endregion

        #region Public Properties

        public bool GetAnything
        {
            get { return this.getAnything; }
            set { this.getAnything = value; }
        }

        public DummyDownload Download
        {
            get { return this.download; }
            set { this.download = value; }
        }



        public DummyUpload Upload
        {
            get { return this.upload; }
            set { this.upload = value; }
        }

        #endregion



        public DummyConnection(DummyEncryptedConnection connection, DummyConnecter connecter)
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

        public void SendChoke()
        {
            message = DummyMessageFactory.GetChokeMessage();
            connection.SendMessage(message.Encode());
        }

        public void SendUnchoke()
        {
            message = DummyMessageFactory.GetUnchokeMessage();
            connection.SendMessage(message.Encode());
        }

        public void SendInterested()
        {
            message = DummyMessageFactory.GetInterestedMessage();
            connection.SendMessage(message.Encode());
        }

        public void SendNotInterested()
        {
            message = DummyMessageFactory.GetNotInterestedMessage();
            connection.SendMessage(message.Encode());
        }

        public void SendRequest(int index, int begin, int length)
        {
            message = new DummyRequestMessage(index, begin, length);
            connection.SendMessage(message.Encode());
        }

        public void SendCancel(int index, int begin, int length)
        {
            message = new DummyRequestMessage(index, begin, length);
            connection.SendMessage(message.Encode());
        }

        public void SendPiece(int index, int begin, byte[] piece)
        {
            message = new DummyPieceMessage(index, begin, piece);
            connection.SendMessage(message.Encode());
        }

        public void SendBitfield(bool[] bitfield)
        {
            message = new DummyBitFieldMessage(bitfield);
            connection.SendMessage(message.Encode());
        }

        public void SendHave(int index)
        {
            message = new DummyHaveMessage(index);
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
}
