using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Uploads;
using ZeraldotNet.LibBitTorrent;

namespace ZeraldotNet.TestLibBitTorrent.TestConnecter
{
    public class DummyUpload : IUpload
    {
        List<string> events;

        public DummyUpload(List<string> events)
        {
            this.events = events;
            events.Add("make upload");
        }

        #region IUpload Members

        public void Choke()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Choked
        {
            get { throw new Exception("The method or operation is not implemented."); }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public bool HasQueries
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool Interested
        {
            get { throw new Exception("The method or operation is not implemented."); }
            set { throw new Exception("The method or operation is not implemented."); }
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
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public void Unchoke()
        {
            throw new Exception("The method or operation is not implemented.");
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

        #endregion
    }
}
