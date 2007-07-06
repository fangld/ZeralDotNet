using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
{
    /// <summary>
    /// 网络信息基类
    /// </summary>
    public abstract class BitTorrentMessage
    {
        #region Private Field

        /// <summary>
        /// 封装连接类
        /// </summary>
        private EncryptedConnection encryptedConnection;

        #endregion

        #region Public Properties

        /// <summary>
        /// 访问和设置封装连接类
        /// </summary>
        public EncryptedConnection EncryptedConnection
        {
            get { return this.encryptedConnection; }
            set { this.encryptedConnection = value; }
        }

        #endregion

        #region Base Methods

        /// <summary>
        /// 网络信息的编码函数
        /// </summary>
        /// <returns>返回编码后的字节流</returns>
        public abstract byte[] Encode();

        /// <summary>
        /// 网络信息的解码函数
        /// </summary>
        /// <param name="buffer">待解码的字节流</param>
        /// <returns>返回是否解码成功</returns>
        public abstract bool Decode(byte[] buffer);

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public abstract bool Handle(byte[] buffer);

        public bool IsDecodeSuccess(byte[] buffer)
        {
            if (!Decode(buffer))
            {
                this.encryptedConnection.Close();
                return false;
            }
            return true;
        }

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public abstract int BytesLength { get; }

        #endregion
    }
}
