using System;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Uploads;

namespace ZeraldotNet.UnitTest.TestLibBitTorrent.TestChokers
{
    public class DummyUpload : IUpload
    {
        private bool interested;

        private bool choked;

        public DummyUpload()
        {
            this.interested = false;
            this.choked = true;
        }

        #region IUpload Members

        public void Choke()
        {
            if (!this.choked)
            {
                this.choked = true;
            }
        }

        public bool Choked
        {
            get { return this.choked; }
            set { this.choked = value; }
        }

        public void Flush()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetCancel(int index, int begin, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetInterested()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetNotInterested()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetRequest(int index, int begin, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool HasQueries
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool Interested
        {
            get { return this.interested; }
            set { this.interested = value; }
        }

        public Measure Measure
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

        public double Rate
        {
            get { return 0; }
        }

        public void Unchoke()
        {
            if (this.choked)
            {
                this.choked = false;
            }
        }

        #endregion
    }
}