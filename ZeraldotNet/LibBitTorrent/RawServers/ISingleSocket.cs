using System;
using System.Net.Sockets;
using ZeraldotNet.LibBitTorrent.Encrypters;

namespace ZeraldotNet.LibBitTorrent.RawServers
{
    public interface ISingleSocket
    {
        void Close();
        void Close(bool closing);
        IEncrypter Handler { get; set; }
        string IP { get; }
        bool IsConnected { get; set; }
        bool IsFlushed { get; }
        DateTime LastHit { get; set; }
        void ShutDown(SocketShutdown how);
        Socket Socket { get; set; }
        void TryWrite();
        void Write(byte[] bytes);
    }
}
