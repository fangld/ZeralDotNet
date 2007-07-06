using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
{
    /// <summary>
    /// Choke网络信息类
    /// </summary>
    public class ChokeMessage : BitTorrentMessage
    {
        #region Private Field

        /// <summary>
        /// 连接类
        /// </summary>
        private Connection connection;

        #endregion

        #region Public Properties

        /// <summary>
        /// 访问和设置连接类
        /// </summary>
        public Connection Connection
        {
            get { return this.connection; }
            set { this.connection = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///  长度为1的网络信息编码函数
        /// </summary>
        /// <param name="type">网络信息类型</param>
        /// <returns>返回编码后的字节流</returns>
        protected byte[] Encode(BitTorrentMessageType type)
        {
            byte[] result = new byte[1];
            result[0] = (byte)type;
            return result;
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 网络信息的编码函数
        /// </summary>
        /// <returns>返回编码后的字节流</returns>
        public override byte[] Encode()
        {
            //信息ID为0
            return this.Encode(BitTorrentMessageType.Choke);
        }

        /// <summary>
        /// 网络信息的解码函数
        /// </summary>
        /// <param name="buffer">待解码的字节流</param>
        /// <returns>返回是否解码成功</returns>
        public override bool Decode(byte[] buffer)
        {
            //如果待解码的字节流长度不为1，则返回false
            if (buffer.Length != BytesLength)
            {
                return false;
            }

            //否则返回true
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public override bool Handle(byte[] buffer)
        {
            bool isDecodeSuccess = this.IsDecodeSuccess(buffer);
            if (isDecodeSuccess)
            {
                connection.Download.GetChoke();
            }
            return isDecodeSuccess;
        }

        /// <summary>
        /// 网络信息的长度
        /// </summary>
        public override int BytesLength
        {
            get { return 1; }
        }

        #endregion
    }
}
