using System;
using System.Collections.Generic;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Storages;

namespace ZeraldotNet.TestLibBitTorrent.TestNormalDownloader
{
    public class DummyStorageWrapper : IStorageWrapper
    {
        #region Fields

        private readonly List<List<InactiveRequest>> remaining;
        private readonly List<List<InactiveRequest>> active;

        #endregion

        #region Properties

        public List<List<InactiveRequest>> Active
        {
            get { return active; }
        }

        #endregion

        #region Constructors

        public DummyStorageWrapper(List<List<InactiveRequest>> remaining)
        {
            this.remaining = remaining;
            active = new List<List<InactiveRequest>>();
            active.Add(new List<InactiveRequest>());
        }

        #endregion

        #region IStorageWrapper Members

        public bool DoIHave(int index)
        {
            return remaining[index].Count == 0 && active[index].Count == 0;
        }

        public bool DoIHaveAnything()
        {
            throw new NotImplementedException();
        }

        public bool DoIHaveRequests(int index)
        {
            //Console.WriteLine("{0},{1}",index, remaining.Count);

            if (remaining.Count <= index)
            {
                return false;
            }

            return remaining[index].Count != 0;
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
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public InactiveRequest NewRequest(int index)
        {
            InactiveRequest newRequest = remaining[index][remaining[index].Count - 1];
            remaining[index].RemoveAt(remaining[index].Count - 1);
            active[index].Add(newRequest);
            active[index].Sort();
            return newRequest;
        }

        public void PieceCameIn(int index, long begin, byte[] piece)
        {
            active[index].Remove(new InactiveRequest((int)begin, piece.Length));
        }

        public void RequestLost(int index, InactiveRequest request)
        {
            Console.WriteLine("{0},{1}",index, remaining.Count);
            
            active[index].Remove(request);
            remaining[index].Add(request);
            remaining[index].Sort();
        }

        public void SetHashes(byte[] hash, int index)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}