using System;
using System.Collections;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Storages;

namespace ZeraldotNet.UnitTest.TestLibBitTorrent.TestUploads
{
    public class DummyStorageWrapper : IStorageWrapper
    {
        private readonly ArrayList events;

        public DummyStorageWrapper(ArrayList events)
        {
            this.events = events;
        }

        #region IStorageWrapper Members

        public bool DoIHave(int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool DoIHaveAnything()
        {
            events.Add("do I have");
            return true;
        }

        public bool DoIHaveRequests(int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public byte[] GetHashes(int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool[] GetHaveList()
        {
            events.Add("get have list");
            return new bool[] { false, true };
        }

        public byte[] GetPiece(int index, int begin, int length)
        {
            events.Add(new object[] { "get piece", index, begin, length });
            if (length == 4)
            {
                return null;
            }

            byte[] result = new byte[length];
            int i;
            for (i = 0; i < result.Length; i++)
            {
                result[i] = (byte)'a';
            }
            return result;

        }

        public bool IsEverythingPending()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public long LeftLength
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

        public InactiveRequest NewRequest(int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void PieceCameIn(int index, long begin, byte[] piece)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void RequestLost(int index, ZeraldotNet.LibBitTorrent.InactiveRequest request)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SetHashes(byte[] hash, int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
