using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Encrypters;

namespace ZeraldotNet.TestLibBitTorrent.TestConnecter
{
    public class DummyEncryptedConnection : IEncryptedConnection
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

        #region IEncryptedConnection Members


        public bool Closed
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public bool Completed
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void DataCameIn(byte[] bytes)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public byte[] ID
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public string IP
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool IsFlushed
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool IsLocallyInitiated
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public void SendMessage(byte message)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Server()
        {
            throw new Exception("The method or operation is not implemented.");
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

        #endregion
    }
}
