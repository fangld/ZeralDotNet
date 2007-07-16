using System;
using ZeraldotNet.LibBitTorrent;

namespace ZeraldotNet.LibBitTorrent.Storages
{
    public interface IStorageWrapper
    {
        bool DoIHave(int index);
        bool DoIHaveAnything();
        bool DoIHaveRequests(int index);
        byte[] GetHashes(int index);
        bool[] GetHaveList();
        byte[] GetPiece(int index, int begin, int length);
        bool IsEverythingPending();
        long LeftLength { get; set; }
        InactiveRequest NewRequest(int index);
        void PieceCameIn(int index, long begin, byte[] piece);
        void RequestLost(int index, InactiveRequest request);
        void SetHashed(byte[] hash, int index);
    }
}
