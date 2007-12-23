using System;
using System.Collections;
using System.Collections.Generic;
using ZeraldotNet.LibBitTorrent.Chokers;
using ZeraldotNet.LibBitTorrent.Connecters;

namespace ZeraldotNet.TestLibBitTorrent.TestUpload
{
    public class DummyChoker : IChoker
    {
        private readonly ArrayList events;

        public DummyChoker(ArrayList events)
        {
            this.events = events;
        }

        #region IChoker Members

        public void CloseConnection(IConnection connection)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public List<IConnection> GetConnections()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Interested(IConnection connection)
        {
            events.Add("interested");
        }

        public void MakeConnection(IConnection connection, int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void MakeConnection(IConnection connection)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void NotInterested(IConnection connection)
        {
            events.Add("not interested");
        }

        #endregion
    }
}
