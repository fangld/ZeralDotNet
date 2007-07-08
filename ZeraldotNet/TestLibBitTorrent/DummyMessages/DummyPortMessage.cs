using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.TestLibBitTorrent.TestConnecter;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Messages;

namespace ZeraldotNet.TestLibBitTorrent.DummyMessages
{
    /// <summary>
    /// Port网络信息类
    /// </summary>
    public class DummyPortMessage : DummyMessage
    {
        #region Private Field

        /// <summary>
        /// DHT监听端口
        /// </summary>
        private ushort port;

        #endregion

        #region Public Properties

        /// <summary>
        /// 访问DHT监听端口
        /// </summary>
        public ushort Port
        {
            get { return port; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public DummyPortMessage(DummyEncryptedConnection encryptedConnection)
            : base(encryptedConnection) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="port">DHT监听端口</param>
        public DummyPortMessage(ushort port)
            : this(null)
        {
            this.port = port;
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 网络信息的编码函数
        /// </summary>
        /// <returns>返回编码后的字节流</returns>
        public override byte[] Encode()
        {
            byte[] result = new byte[3];

            //信息ID为9
            result[0] = (byte)MessageType.Port;

            //写入DHT监听端口
            Globals.UInt16ToBytes(port, result, 1);

            return result;
        }

        /// <summary>
        /// 网络信息的解码函数
        /// </summary>
        /// <param name="buffer">待解码的字节流</param>
        /// <returns>返回是否解码成功</returns>
        public override bool Decode(byte[] buffer)
        {
            //如果信息长度不等于3或者信息ID不为9，则返回false
            if (buffer.Length != BytesLength)
            {
                return false;
            }

            //解码DHT监听端口
            port = Globals.BytesToUInt16(buffer, 1);

            //否则，返回true
            return true;
        }

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public override bool Handle(byte[] buffer)
        {
            bool isDecodeSuccess = this.IsDecodeSuccess(buffer);
            if (isDecodeSuccess)
            {
                //do nothing now
            }
            return isDecodeSuccess;
        }

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public override int BytesLength
        {
            get { return 3; }
        }

        #endregion
    }
}
