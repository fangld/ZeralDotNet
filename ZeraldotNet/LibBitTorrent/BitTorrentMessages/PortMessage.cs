using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
{
    /// <summary>
    /// Port网络信息类
    /// </summary>
    public class PortMessage : BitTorrentMessage
    {
        #region Private Field

        /// <summary>
        /// DHT监听端口
        /// </summary>
        private ushort port;

        #endregion

        #region Public Properties

        /// <summary>
        /// 访问和设置DHT监听端口
        /// </summary>
        public ushort Port
        {
            get { return port; }
            set { port = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public PortMessage() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="port">DHT监听端口</param>
        public PortMessage(ushort port)
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
            result[0] = (byte)BitTorrentMessageType.Port;

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
            if (buffer.Length != BytesLength || buffer[0] != (byte)BitTorrentMessageType.Port)
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
        public override void Handle()
        {
            throw new Exception("The method or operation is not implemented.");
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
