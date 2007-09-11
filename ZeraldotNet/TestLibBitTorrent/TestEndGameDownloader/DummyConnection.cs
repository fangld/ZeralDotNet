using System;
using System.Collections.Generic;
using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.Downloads;
using ZeraldotNet.LibBitTorrent.Encrypters;
using ZeraldotNet.LibBitTorrent.Uploads;

namespace ZeraldotNet.TestLibBitTorrent.TestEndGameDownloader
{
    public class DummyConnection:IConnection
    {
        private List<string> events;
        private EndGameSingleDownload download;

        public DummyConnection(List<string> events)
        {
            this.events = events;
        }

        #region IConnection Members

        public void Close()
        {
            events.Add("close");
        }

        public SingleDownload Download
        {
            get { return this.download; }
            set { this.download = (EndGameSingleDownload)value; }
        }

        public IEncryptedConnection EncryptedConnection
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool GetAnything
        {
            get { throw new NotImplementedException(); }
        }

        public byte[] ID
        {
            get { throw new NotImplementedException(); }
        }

        public string IP
        {
            get { throw new NotImplementedException(); }
        }

        public bool Flushed
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsLocallyInitiated
        {
            get { throw new NotImplementedException(); }
        }

        public void SendBitfield(bool[] bitfield)
        {
            throw new NotImplementedException();
        }

        public void SendCancel(int index, int begin, int length)
        {
            events.Add(string.Format("cancel:{0},{1},{2}", index, begin, length));
        }

        public void SendChoke()
        {
            throw new NotImplementedException();
        }

        public void SendHave(int index)
        {
            throw new NotImplementedException();
        }

        public void SendInterested()
        {
            events.Add("interested");
        }

        public void SendNotInterested()
        {
            events.Add("not interested");
        }

        public void SendPiece(int index, int begin, byte[] pieces)
        {
            throw new NotImplementedException();
        }

        public void SendPort(ushort port)
        {
            throw new NotImplementedException();
        }

        public void SendRequest(int index, int begin, int length)
        {
            events.Add(string.Format("request:{0},{1},{2}", index, begin, length));
        }

        public void SendUnchoke()
        {
            throw new NotImplementedException();
        }

        public IUpload Upload
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion
    }
}