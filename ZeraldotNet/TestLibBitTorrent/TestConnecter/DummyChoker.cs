using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.TestLibBitTorrent.TestConnecter
{
    /// <summary>
    /// 测试阻塞类
    /// </summary>
    public class DummyChoker
    {
        /// <summary>
        /// 记录发生的事件
        /// </summary>
        List<string> events;

        public List<DummyConnection> cs;

        public DummyChoker(List<string> events, List<DummyConnection> cs)
        {
            this.events = events;
            this.cs = cs;
        }

        public void MakeConnection(DummyConnection c)
        {
            events.Add("make");
            cs.Add(c);
        }

        public void LoseConnection(DummyConnection c)
        {
            events.Add("lost");
        }
    }
}
