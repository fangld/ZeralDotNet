using System;
using System.Text;
using System.Collections.Generic;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Storages;

namespace ZeraldotNet.TestLibBitTorrent.TestEndGameDownloader
{
    public class DummyStorageWrapper:IStorageWrapper
    {
        private List<string> events;
        private List<InactiveRequest> expectFlunk;
        private List<InactiveRequest> requests;

        public List<InactiveRequest> ExpectFlunk
        {
            set { this.expectFlunk = value; }
        }

        public DummyStorageWrapper(List<string> events)
        {
            this.events = events;
            expectFlunk = new List<InactiveRequest>();
            requests = new List<InactiveRequest>();
        }

        #region IStorageWrapper Members

        public bool DoIHave(int index)
        {
            return false;
        }

        public bool DoIHaveAnything()
        {
            throw new NotImplementedException();
        }

        public bool DoIHaveRequests(int index)
        {
            return this.requests.Count != 0;
        }

        public byte[] GetHashes(int index)
        {
            throw new NotImplementedException();
        }

        public bool[] GetHaveList()
        {
            throw new NotImplementedException();
        }

        public byte[] GetPiece(int index, int begin, int length)
        {
            throw new NotImplementedException();
        }

        public bool IsEverythingPending()
        {
            throw new NotImplementedException();
        }

        public long LeftLength
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }



        public InactiveRequest NewRequest(int index)
        {
            InactiveRequest result = requests[requests.Count - 1];
            requests.RemoveAt(requests.Count - 1);
            return result;
        }

        public void PieceCameIn(int index, long begin, byte[] piece)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("came in:{0},{1},", index, begin);
            foreach (byte b in piece)
            {
                sb.AppendFormat("{0:X},", b);
            }
            if (expectFlunk.Count > 0)
            {
                requests = expectFlunk;
            }
            expectFlunk = new List<InactiveRequest>();
        }

        public void RequestLost(int index, InactiveRequest request)
        {
        }

        public void SetHashes(byte[] hash, int index)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}