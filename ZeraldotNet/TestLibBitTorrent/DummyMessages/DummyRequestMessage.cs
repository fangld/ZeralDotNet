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
    /// Request网络信息类
    /// </summary>
    public class DummyRequestMessage : DummyCancelMessage
    {
        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public DummyRequestMessage(DummyEncryptedConnection encryptedConnection, DummyConnection connection, DummyConnecter connecter)
            : base(encryptedConnection, connection, connecter) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">片断起始位置</param>
        /// <param name="length">片断长度</param>
        public DummyRequestMessage(int index, int begin, int length) 
            : base(index, begin, length) { }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 网络信息的编码函数
        /// </summary>
        /// <returns>返回编码后的字节流</returns>
        public override byte[] Encode()
        {
            return this.Encode(MessageType.Request);
        }

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public override bool Handle(byte[] buffer)
        {
            //如果
            bool isDecodeSuccess = this.IsDecodeSuccess(buffer);
            if (isDecodeSuccess)
            {
                Connection.Upload.GetRequest(index, begin, length);
            }
            return isDecodeSuccess;
        }

        #endregion
    }
}
