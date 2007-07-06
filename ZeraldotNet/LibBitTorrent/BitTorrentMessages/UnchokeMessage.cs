using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
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

        #region Public Properties

        /// <summary>
        /// 访问和设置连接管理类
        /// </summary>
        public Connecter Connecter
        {
            get { return this.connecter; }
            set { this.connecter = value; }
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
            return this.Encode(BitTorrentMessageType.Unchoke);
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
