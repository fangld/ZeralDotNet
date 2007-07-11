using System;
using System.Collections.Generic;
using ZeraldotNet.LibBitTorrent.Encrypters;

namespace ZeraldotNet.LibBitTorrent.Connecters
{
    public interface IConnecter
    {
        void CheckEndgame();
        void CloseConnection(IEncryptedConnection encryptedConnection);
        ICollection<IConnection> Connections { get; }
        int ConnectionsCount { get; }
        void FlushConnection(IEncryptedConnection encryptedConnection);
        void GetMessage(IEncryptedConnection encryptedConnection, byte[] message);
        void MakeConnection(IEncryptedConnection encryptedConnection);
        int PiecesNumber { get; }
        bool RateCapped { get; }
        void UnCap();
        void UpdateUploadRate(int amount);
    }
}
