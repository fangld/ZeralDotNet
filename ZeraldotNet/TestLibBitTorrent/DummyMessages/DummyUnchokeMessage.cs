using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.TestLibBitTorrent.TestConnecter;
using ZeraldotNet.LibBitTorrent.Messages;

namespace ZeraldotNet.TestLibBitTorrent.DummyMessages
{
    /// <summary>
    /// Unchoke网络信息类
    /// </summary>
    public class DummyUnchokeMessage : DummyChokeMessage
    {
        #region Private Field

        /// <summary>
        /// 连接管理类
        /// </summary>
        private DummyConnecter connecter;

        #endregion

        #region Public Properties

        /// <summary>
        /// 访问和设置连接管理类
        /// </summary>
        public DummyConnecter Connecter
        {
            get { return this.connecter; }
            set { this.connecter = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public DummyUnchokeMessage()
            : this(null, null, null) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="encryptedConnection">封装连接类</param>
        /// <param name="connection">连接类</param>
        public DummyUnchokeMessage(DummyEncryptedConnection encryptedConnection, DummyConnection connection, DummyConnecter connecter)
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
                Connection.Download.GetUnchoke();
                this.connecter.CheckEndgame();
            }
            return isDecodeSuccess;
        }

        #endregion
    }
}
