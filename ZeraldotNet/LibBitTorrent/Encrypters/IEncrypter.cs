using System;
using System.Net;
using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.Encrypters;
using ZeraldotNet.LibBitTorrent.RawServers;

namespace ZeraldotNet.LibBitTorrent.Encrypters
{
    public interface IEncrypter
    {
        void CloseConnection(ISingleSocket singleSocket);
        IConnecter Connecter { get; set; }
        void DataCameIn(ISingleSocket singleSocket, byte[] data);
        byte[] DownloadID { get; set; }
        void FlushConnection(ISingleSocket singleSocket);
        void MakeExternalConnection(ISingleSocket singleSocket);
        int MaxLength { get; set; }
        byte[] MyID { get; set; }
        void Remove(ISingleSocket keySocket);
        void SendKeepAlives();
        void StartConnect(IPEndPoint dns, byte[] id);
    }
}
