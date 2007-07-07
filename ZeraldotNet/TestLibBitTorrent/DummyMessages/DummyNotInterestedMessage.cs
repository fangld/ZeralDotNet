using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.TestLibBitTorrent.TestConnecter;
using ZeraldotNet.LibBitTorrent.Messages;
using ZeraldotNet.LibBitTorrent;

namespace ZeraldotNet.TestLibBitTorrent.DummyMessages
{
    /// <summary>
    /// NotInterested网络信息类
    /// </summary>
    public class DummyNotInterestedMessage : DummyChokeMessage
    {
        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public DummyNotInterestedMessage()
            : this(null, null) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="encryptedConnection">封装连接类</param>
        /// <param name="connection">连接类</param>
        public DummyNotInterestedMessage(DummyEncryptedConnection encryptedConnection, DummyConnection connection)
            : base(encryptedConnection, connection) { }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 网络信息的编码函数
        /// </summary>
        /// <returns>返回编码后的字节流</returns>
        public override byte[] Encode()
        {
            //信息ID为3
            return Encode(MessageType.NotInterested);
        }

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public override bool Handle(byte[] buffer)
        {
            bool isDecodeSuccess = this.IsDecodeSuccess(buffer);
            if (isDecodeSuccess)
            {
                Connection.Upload.GetNotInterested();
            }
            return isDecodeSuccess;
        }

        #endregion
    }
}
