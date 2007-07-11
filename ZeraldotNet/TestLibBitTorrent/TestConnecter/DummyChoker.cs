using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Chokers;
using ZeraldotNet.LibBitTorrent.Connecters;

namespace ZeraldotNet.TestLibBitTorrent.TestConnecter
{
    /// <summary>
    /// 测试阻塞类
    /// </summary>
    public class DummyChoker : IChoker
    {
        /// <summary>
        /// 记录发生的事件
        /// </summary>
        List<string> events;

        private List<IConnection> cs;

        public DummyChoker(List<string> events, List<IConnection> cs)
        {
            this.events = events;
            this.cs = cs;
        }
        #region IChoker Members

        public void MakeConnection(IConnection connection)
        {
            events.Add("make");
            cs.Add(connection);
        }

        public void LoseConnection(IConnection connection)
        {
            events.Add("lost");
        }

        public List<IConnection> GetConnections()
        {
            return this.cs;
        }

        public void CloseConnection(ZeraldotNet.LibBitTorrent.Connecters.IConnection connection)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Interested(IConnection connection)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void MakeConnection(IConnection connection, int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void NotInterested(IConnection connection)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
