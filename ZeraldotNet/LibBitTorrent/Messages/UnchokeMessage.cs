using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// Unchoke网络信息类
    /// </summary>
    public class UnchokeMessage : ChokeMessage
    {
        #region Private Field

        /// <summary>
        /// 连接管理类
        /// </summary>
        private Connecter connecter;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public UnchokeMessage()
            : this(null, null, null) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="encryptedConnection">封装连接类</param>
        /// <param name="connection">连接类</param>
        public UnchokeMessage(EncryptedConnection encryptedConnection, Connection connection, Connecter connecter)
            : base(encryptedConnection, connection) 
        {
            this.connecter = connecter;
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 网络信息的编码函数
        /// </summary>
        /// <returns>返回编码后的字节流</returns>
        public override byte[] Encode()
        {
            //信息ID为1
            return this.Encode(MessageType.Unchoke);
        }

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public override bool Handle(byte[] buffer)
        {
            bool isDecodeSuccess = this.IsDecodeSuccess(buffer);
            if (isDecodeSuccess)
            {
                this.connection.Download.GetUnchoke();
                this.connecter.CheckEndgame();
            }
            return isDecodeSuccess;
        }

        #endregion
    }
}
