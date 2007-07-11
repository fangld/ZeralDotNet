using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Encrypters;
using ZeraldotNet.LibBitTorrent.Connecters;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// Choke网络信息类
    /// </summary>
    public class ChokeMessage : Message
    {
        #region Protected Field

        /// <summary>
        /// 连接类
        /// </summary>
        protected IConnection connection;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public ChokeMessage()
            : this(null, null) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="encryptedConnection">封装连接类</param>
        /// <param name="connection">连接类</param>
        public ChokeMessage(IEncryptedConnection encryptedConnection, IConnection connection)
            : base(encryptedConnection)
        {
            this.connection = connection;
        }

        #endregion


        #region Methods

        /// <summary>
        ///  长度为1的网络信息编码函数
        /// </summary>
        /// <param name="type">网络信息类型</param>
        /// <returns>返回编码后的字节流</returns>
        protected byte[] Encode(MessageType type)
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
            return this.Encode(MessageType.Choke);
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
            //如果解码成功，则choke下载者。
            bool isDecodeSuccess = this.IsDecodeSuccess(buffer);
            if (isDecodeSuccess)
            {
                connection.Download.GetChoke();
            }

            //返回是否解码成功
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
