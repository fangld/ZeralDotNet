using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.TestLibBitTorrent.TestConnecter
{
    public class DummyEncryptedConnection
    {
        /// <summary>
        /// 记录发生的事件
        /// </summary>
        List<string> events;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="events">记录发生的事件</param>
        public DummyEncryptedConnection(List<string> events)
        {
            this.events = events;
        }

        public void SendMessage(byte[] message)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Message:");
            foreach (byte item in message)
            {
                sb.AppendFormat("0x{0:X2},", item);
            }
            events.Add(sb.ToString());
        }

        public void Close()
        {
        }
    }
}
