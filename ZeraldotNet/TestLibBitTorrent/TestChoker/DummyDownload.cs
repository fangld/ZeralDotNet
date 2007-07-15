using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Downloads;

namespace ZeraldotNet.TestLibBitTorrent.TestChoker
{
    public class DummyDownload : ISingleDownload
    {
        private bool snubbed;

        private DummyConnection connection;

        public DummyDownload(DummyConnection connection)
        {
            this.connection = connection;
        }


        #region ISingleDownload Members

        public bool Snubbed
        {
            get { return this.snubbed; }
            set { this.snubbed = value; }
        }

        public double Rate
        {
            get { return this.connection.Value; }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public void Disconnect()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetChoke()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetUnchoke()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetHave(int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetHaveBitField(bool[] have)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool GetPiece(int index, int begin, byte[] piece)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
