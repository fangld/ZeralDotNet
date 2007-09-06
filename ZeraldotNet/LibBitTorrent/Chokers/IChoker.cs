using System.Collections.Generic;
using ZeraldotNet.LibBitTorrent.Connecters;

namespace ZeraldotNet.LibBitTorrent.Chokers
{
    public interface IChoker
    {
        void CloseConnection(IConnection connection);
        List<IConnection> GetConnections();
        void Interested(IConnection connection);
        void MakeConnection(IConnection connection, int index);
        void MakeConnection(IConnection connection);
        void NotInterested(IConnection connection);
    }
}
