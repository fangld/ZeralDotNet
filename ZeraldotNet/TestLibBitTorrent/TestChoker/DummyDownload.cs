using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Downloads;

namespace ZeraldotNet.TestLibBitTorrent.TestChoker
{
    public class DummyDownload : SingleDownload
    {
        private bool snubbed;

        private DummyConnection connection;

        public DummyDownload(DummyConnection connection)
        {
            this.connection = connection;
        }


        #region ISingleDownload Members

        public override bool Snubbed
        {
            get { return this.snubbed; }
            set { this.snubbed = value; }
        }

        public override double Rate
        {
            get { return this.connection.Value; }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public override void Disconnect()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void GetChoke()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void GetUnchoke()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void GetHave(int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void GetHaveBitField(bool[] have)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override bool GetPiece(int index, int begin, byte[] piece)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
