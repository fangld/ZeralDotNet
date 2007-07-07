using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.TestLibBitTorrent.TestConnecter;
using ZeraldotNet.LibBitTorrent.Messages;

namespace ZeraldotNet.TestLibBitTorrent.DummyMessages
{
    /// <summary>
    /// Interested网络信息类
    /// </summary>
    public class DummyInterestedMessage : DummyChokeMessage
    {
        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public DummyInterestedMessage()
            : this(null, null) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="encryptedConnection">封装连接类</param>
        /// <param name="connection">连接类</param>
        public DummyInterestedMessage(DummyEncryptedConnection encryptedConnection, DummyConnection connection)
            : base(encryptedConnection, connection) { }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 网络信息的编码函数
        /// </summary>
        /// <returns>返回编码后的字节流</returns>
        public override byte[] Encode()
        {
            //信息ID为2
            return Encode(MessageType.Interested);
        }

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public override bool Handle(byte[] buffer)
        {
            //如果解码成功，则interested上传者。
            bool isDecodeSuccess = this.IsDecodeSuccess(buffer);
            if (isDecodeSuccess)
            {
                Connection.Upload.GetInterested();
            }

            //返回是否解码成功
            return isDecodeSuccess;
        }

        #endregion
    }
}
