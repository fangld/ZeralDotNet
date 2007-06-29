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
        /// 网络信息的解码函数
        /// </summary>
        /// <param name="buffer">待解码的字节流</param>
        /// <returns>返回是否解码成功</returns>
        public override bool Decode(byte[] buffer)
        {
            //信息ID为1
            return this.Decode(buffer, BitTorrentMessageType.Unchoke);
        }

        #endregion
    }
}
