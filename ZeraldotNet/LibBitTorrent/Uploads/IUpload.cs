using System;
using ZeraldotNet.LibBitTorrent;

namespace ZeraldotNet.LibBitTorrent.Uploads
{
    public interface IUpload
    {
        void Choke();
        bool Choked { get; }
        void Flush();
        void GetCancel(int index, int begin, int length);
        void GetInterested();
        void GetNotInterested();
        void GetRequest(int index, int begin, int length);
        bool HasQueries { get; }
        bool Interested { get; }
        Measure Measure { get; set; }
        double Rate { get; }
        void Unchoke();
    }
}
