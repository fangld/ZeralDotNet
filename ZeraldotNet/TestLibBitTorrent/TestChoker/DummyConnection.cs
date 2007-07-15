using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.Downloads;
using ZeraldotNet.LibBitTorrent.Uploads;
using ZeraldotNet.LibBitTorrent.Encrypters;

namespace ZeraldotNet.TestLibBitTorrent.TestChoker
{
    public class DummyConnection : IConnection
    {
        private IUpload upload;

        private ISingleDownload download;

        private int value;

        public DummyConnection(int value)
        {
            this.upload = new DummyUpload();
            this.download = new DummyDownload(this);
            this.value = value;
        }

        public int Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        #region IConnection Members

        #region Implement Methods

        #endregion

        public void Close()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public ISingleDownload Download
        {
            get { return this.download; }
            set { this.download = value; }
        }

        public IEncryptedConnection EncryptedConnection
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

        public bool IsFlushed
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool IsLocallyInitiated
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public void SendBitfield(bool[] bitfield)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SendCancel(int index, int begin, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SendChoke()
        {
            throw new Exception("The method or operation is not implemented.");
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
            throw new Exception("The method or operation is not implemented.");
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
            throw new Exception("The method or operation is not implemented.");
        }

        public IUpload Upload
        {
            get { return this.upload; }
            set { this.upload = value; }
        }

        #endregion
    }
}
