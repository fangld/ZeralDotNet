using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.Downloads;
using ZeraldotNet.LibBitTorrent.Encrypters;
using ZeraldotNet.LibBitTorrent.Uploads;

namespace ZeraldotNet.TestLibBitTorrent.TestUpload
{
    public class DummyConnection : IConnection
    {
        private ArrayList events;

        private bool flushed;

        public DummyConnection(ArrayList events)
        {
            this.events = events;
        }

        #region IConnection Members

        public void Close()
        {
            events.Add("closed");
        }

        public SingleDownload Download
        {
            get { throw new Exception("The method or operation is not implemented."); }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public IEncryptedConnection EncryptedConnection
        {
            get { throw new Exception("The method or operation is not implemented."); }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public bool GetAnything
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public byte[] ID
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public string IP
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool Flushed
        {
            get { return this.flushed; }
            set { this.flushed = value; }
        }

        public bool IsLocallyInitiated
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public void SendBitfield(bool[] bitfield)
        {
            events.Add(new object[] { "bitfield", bitfield });
        }

        public void SendCancel(int index, int begin, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SendChoke()
        {
            events.Add("choke");
        }

        public void SendHave(int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SendInterested()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SendNotInterested()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SendPiece(int index, int begin, byte[] pieces)
        {
            events.Add(new object[] { "piece", index, begin, pieces });
        }

        public void SendPort(ushort port)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SendRequest(int index, int begin, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SendUnchoke()
        {
            events.Add("unchoke");
        }

        public IUpload Upload
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        #endregion
    }
}
